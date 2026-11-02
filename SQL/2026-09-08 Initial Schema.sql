/*
    Cornerstone Zearing
    Initial database schema
*/

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    [Name] NVARCHAR(256)   NULL,
    NormalizedName NVARCHAR(256)   NULL,
    ConcurrencyStamp NVARCHAR(MAX)  NULL,
    [Description] NVARCHAR(256)   NULL
)
GO

CREATE UNIQUE INDEX IX_Roles_NormalizedName
ON Roles (NormalizedName) 
WHERE NormalizedName IS NOT NULL
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    IsActive BIT NOT NULL
)
GO

CREATE UNIQUE INDEX IX_Users_NormalizedUserName
ON Users (NormalizedUserName)
WHERE NormalizedUserName IS NOT NULL
GO

CREATE INDEX IX_Users_NormalizedEmail 
ON Users (NormalizedEmail)
GO

CREATE TABLE RoleClaims (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoleClaims PRIMARY KEY,
    RoleId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_RoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES Roles (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_RoleClaims_RoleId
ON RoleClaims (RoleId)
GO

CREATE TABLE UserClaims (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserClaims PRIMARY KEY,
    UserId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_UserClaims_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserClaims_UserId
ON UserClaims (UserId)
GO

CREATE TABLE UserLogins
(
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId INT NOT NULL,
    CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_UserLogins_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserLogins_UserId
ON UserLogins (UserId)
GO

CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserRoles_RoleId
ON UserRoles (RoleId)
GO

CREATE TABLE UserTokens
(
    UserId INT NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
)
GO

CREATE TABLE Media
(
    MediaID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Media PRIMARY KEY,
    OriginalFileName NVARCHAR(250) NOT NULL,
    StoredFileName NVARCHAR(250) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    AltText NVARCHAR(MAX) NULL,
    Width INT NOT NULL,
    Height INT NOT NULL,
    SizeBytes INT NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE TABLE Documents
(
    DocumentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Documents PRIMARY KEY,
    OriginalFileName NVARCHAR(250) NOT NULL,
    StoredFileName NVARCHAR(250) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    SizeBytes INT NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE TABLE PostCategories
(
    PostCategoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PostCategories PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE UNIQUE INDEX IX_PostCategories_Slug 
ON PostCategories (Slug)
GO

CREATE TABLE SermonCategories (
    SermonCategoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SermonCategories PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE UNIQUE INDEX IX_SermonCategories_Slug 
ON SermonCategories (Slug)
GO

CREATE TABLE ContentBlocks (
    ContentBlockID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContentBlocks PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    ContentJson NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE TABLE Posts (
    PostID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Posts PRIMARY KEY,
    PostCategoryID INT NULL,
    FeaturedMediaID INT NULL,
    Title NVARCHAR(250) NOT NULL,
    Slug NVARCHAR(250) NOT NULL,
    Summary NVARCHAR(500) NULL,
    ContentJson NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
    MetaTitle NVARCHAR(200) NULL,
    MetaDescription NVARCHAR(500) NULL,
    Tags NVARCHAR(500) NULL,
    DatePublished DATETIME NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL,
    CONSTRAINT FK_Posts_PostCategories FOREIGN KEY (PostCategoryID) REFERENCES PostCategories (PostCategoryID),
    CONSTRAINT FK_Posts_Media FOREIGN KEY (FeaturedMediaID) REFERENCES Media (MediaID)
)
GO

CREATE UNIQUE INDEX IX_Posts_Slug
ON Posts (Slug)
GO

CREATE TABLE Sermons (
    SermonID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sermons PRIMARY KEY,
    SermonCategoryID INT NOT NULL,
    FeaturedDocumentID INT NULL,
    FeaturedMediaID INT NULL,
    Title NVARCHAR(250) NOT NULL,
    SermonDate DATETIME NOT NULL,
    ContentJson NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    Speaker NVARCHAR(100) NULL,
    [Status] INT NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL,
    CONSTRAINT FK_Sermons_SermonCategories FOREIGN KEY (SermonCategoryID) REFERENCES SermonCategories (SermonCategoryID),
    CONSTRAINT FK_Sermons_Documents FOREIGN KEY (FeaturedDocumentID) REFERENCES Documents (DocumentID),
    CONSTRAINT FK_Sermons_Media FOREIGN KEY (FeaturedMediaID) REFERENCES Media (MediaID)
)
GO

CREATE TABLE [Events] (
    EventID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    Title NVARCHAR(250) NOT NULL,
    StartDateTime DATETIME NOT NULL,
    EndDateTime DATETIME NOT NULL,
    [Location] NVARCHAR(100) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Private] BIT NOT NULL,
    AllDay BIT NOT NULL,
    RecurrenceRule NVARCHAR(500) NULL,
    RecurrenceEndDate DATETIME NULL,
    RecurrenceExceptions NVARCHAR(MAX) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

INSERT INTO Roles ([Name], NormalizedName, ConcurrencyStamp, Description)
VALUES (N'Administrator', N'ADMINISTRATOR', NEWID(), N'Full access to every area of the admin.');
GO

DECLARE @AdministratorRoleId INT = (
    SELECT Id FROM Roles WHERE NormalizedName = N'ADMINISTRATOR'
);

INSERT INTO RoleClaims (RoleId, ClaimType, ClaimValue)
VALUES
    (@AdministratorRoleId, N'permission', N'Posts.View'),
    (@AdministratorRoleId, N'permission', N'Posts.Create'),
    (@AdministratorRoleId, N'permission', N'Posts.Edit'),
    (@AdministratorRoleId, N'permission', N'Posts.Delete'),
    (@AdministratorRoleId, N'permission', N'ContentBlocks.View'),
    (@AdministratorRoleId, N'permission', N'ContentBlocks.Create'),
    (@AdministratorRoleId, N'permission', N'ContentBlocks.Edit'),
    (@AdministratorRoleId, N'permission', N'ContentBlocks.Delete'),
    (@AdministratorRoleId, N'permission', N'Media.View'),
    (@AdministratorRoleId, N'permission', N'Media.Upload'),
    (@AdministratorRoleId, N'permission', N'Media.Edit'),
    (@AdministratorRoleId, N'permission', N'Media.Delete'),
    (@AdministratorRoleId, N'permission', N'Documents.View'),
    (@AdministratorRoleId, N'permission', N'Documents.Upload'),
    (@AdministratorRoleId, N'permission', N'Documents.Edit'),
    (@AdministratorRoleId, N'permission', N'Documents.Delete'),
    (@AdministratorRoleId, N'permission', N'Events.View'),
    (@AdministratorRoleId, N'permission', N'Events.Create'),
    (@AdministratorRoleId, N'permission', N'Events.Edit'),
    (@AdministratorRoleId, N'permission', N'Events.Delete'),
    (@AdministratorRoleId, N'permission', N'Sermons.View'),
    (@AdministratorRoleId, N'permission', N'Sermons.Create'),
    (@AdministratorRoleId, N'permission', N'Sermons.Edit'),
    (@AdministratorRoleId, N'permission', N'Sermons.Delete'),
    (@AdministratorRoleId, N'permission', N'Categories.Manage'),
    (@AdministratorRoleId, N'permission', N'Users.Manage'),
    (@AdministratorRoleId, N'permission', N'Roles.Manage');
GO

INSERT INTO Users (
    UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, IsActive
)
VALUES (
    N'ryan@cornerstonezearing.org', N'RYAN@CORNERSTONEZEARING.ORG',
    N'ryan@cornerstonezearing.org', N'RYAN@CORNERSTONEZEARING.ORG', 1,
    N'AQAAAAEAACcQAAAAEBf4SBRAOx/vb2jH3mnHVTkgBYRXhoWeAkhMtq7+LIxeNQ486zOjf6aGhhbO3DJ2bQ==',
    CONVERT(NVARCHAR(MAX), NEWID()), CONVERT(NVARCHAR(MAX), NEWID()),
    0, 0, 1, 0,
    N'Site', N'Administrator', 1
)
GO

DECLARE @AdministratorRoleId INT = (
    SELECT Id 
    FROM Roles 
    WHERE NormalizedName = N'ADMINISTRATOR'
);

DECLARE @AdminUserId INT = (
    SELECT Id 
    FROM Users 
    WHERE NormalizedEmail = N'RYAN@CORNERSTONEZEARING.ORG'
);

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminUserId, @AdministratorRoleId);
GO