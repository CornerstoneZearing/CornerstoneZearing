/*
    Cornerstone Zearing
    Initial database schema
*/

CREATE TABLE dbo.Roles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    [Name] NVARCHAR(256)   NULL,
    NormalizedName NVARCHAR(256)   NULL,
    ConcurrencyStamp NVARCHAR(MAX)  NULL,
    [Description] NVARCHAR(256)   NULL
)
GO

CREATE UNIQUE INDEX IX_Roles_NormalizedName
ON dbo.Roles (NormalizedName) 
WHERE NormalizedName IS NOT NULL
GO

CREATE TABLE dbo.Users (
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
ON dbo.Users (NormalizedUserName)
WHERE NormalizedUserName IS NOT NULL
GO

CREATE INDEX IX_Users_NormalizedEmail 
ON dbo.Users (NormalizedEmail)
GO

CREATE TABLE dbo.RoleClaims (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoleClaims PRIMARY KEY,
    RoleId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_RoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_RoleClaims_RoleId
ON dbo.RoleClaims (RoleId)
GO

CREATE TABLE dbo.UserClaims (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserClaims PRIMARY KEY,
    UserId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_UserClaims_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserClaims_UserId
ON dbo.UserClaims (UserId)
GO

CREATE TABLE dbo.UserLogins
(
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId INT NOT NULL,
    CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_UserLogins_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserLogins_UserId
ON dbo.UserLogins (UserId)
GO

CREATE TABLE dbo.UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE
)
GO

CREATE INDEX IX_UserRoles_RoleId
ON dbo.UserRoles (RoleId)
GO

CREATE TABLE dbo.UserTokens
(
    UserId INT NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
)
GO

CREATE TABLE dbo.Media
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

CREATE TABLE dbo.Documents
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

CREATE TABLE dbo.PostCategories
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
ON dbo.PostCategories (Slug)
GO

CREATE TABLE dbo.SermonCategories (
    SermonCategoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SermonCategories PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE UNIQUE INDEX IX_SermonCategories_Slug 
ON dbo.SermonCategories (Slug)
GO

CREATE TABLE dbo.Sidebars (
    SidebarID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sidebars PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    ContentJson NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

CREATE TABLE dbo.Pages (
    PageID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pages PRIMARY KEY,
    ParentPageID INT NULL,
    SidebarID INT NULL,
    FeaturedMediaID INT NULL,
    Title NVARCHAR(250) NOT NULL,
    Slug NVARCHAR(250) NOT NULL,
    Template NVARCHAR(100) NULL,
    ContentJson NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    [Status] INT NOT NULL,
    MetaTitle NVARCHAR(200) NULL,
    MetaDescription NVARCHAR(500) NULL,
    SortOrder INT NOT NULL,
    ShowInNavigation BIT NOT NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL,
    CONSTRAINT FK_Pages_Pages FOREIGN KEY (ParentPageID) REFERENCES dbo.Pages (PageID),
    CONSTRAINT FK_Pages_Sidebars FOREIGN KEY (SidebarID) REFERENCES dbo.Sidebars (SidebarID),
    CONSTRAINT FK_Pages_Media FOREIGN KEY (FeaturedMediaID) REFERENCES dbo.Media (MediaID)
)
GO

CREATE UNIQUE INDEX IX_Pages_Slug
ON dbo.Pages (Slug);
GO

CREATE TABLE dbo.Posts (
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
    CONSTRAINT FK_Posts_PostCategories FOREIGN KEY (PostCategoryID) REFERENCES dbo.PostCategories (PostCategoryID),
    CONSTRAINT FK_Posts_Media FOREIGN KEY (FeaturedMediaID) REFERENCES dbo.Media (MediaID)
)
GO

CREATE UNIQUE INDEX IX_Posts_Slug
ON dbo.Posts (Slug)
GO

CREATE TABLE dbo.Sermons (
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
    CONSTRAINT FK_Sermons_SermonCategories FOREIGN KEY (SermonCategoryID) REFERENCES dbo.SermonCategories (SermonCategoryID),
    CONSTRAINT FK_Sermons_Documents FOREIGN KEY (FeaturedDocumentID) REFERENCES dbo.Documents (DocumentID),
    CONSTRAINT FK_Sermons_Media FOREIGN KEY (FeaturedMediaID) REFERENCES dbo.Media (MediaID)
)
GO

CREATE TABLE dbo.[Events] (
    EventID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    Title NVARCHAR(250) NOT NULL,
    StartDateTime DATETIME NOT NULL,
    EndDateTime DATETIME NOT NULL,
    Location NVARCHAR(100) NULL,
    Description NVARCHAR(MAX) NULL,
    Private BIT NOT NULL,
    RecurrenceRule NVARCHAR(500) NULL,
    RecurrenceEndDate DATETIME NULL,
    RecurrenceExceptions NVARCHAR(MAX) NULL,
    DateCreated DATETIME NOT NULL,
    DateModified DATETIME NOT NULL
)
GO

INSERT INTO dbo.Roles (Name, NormalizedName, ConcurrencyStamp, Description)
VALUES (N'Administrator', N'ADMINISTRATOR', NEWID(), N'Full access to every area of the admin.');
GO


DECLARE @AdministratorRoleId INT = (
    SELECT Id FROM dbo.Roles WHERE NormalizedName = N'ADMINISTRATOR'
);

INSERT INTO dbo.RoleClaims (RoleId, ClaimType, ClaimValue)
VALUES
    (@AdministratorRoleId, N'permission', N'Pages.View'),
    (@AdministratorRoleId, N'permission', N'Pages.Create'),
    (@AdministratorRoleId, N'permission', N'Pages.Edit'),
    (@AdministratorRoleId, N'permission', N'Pages.Delete'),
    (@AdministratorRoleId, N'permission', N'Posts.View'),
    (@AdministratorRoleId, N'permission', N'Posts.Create'),
    (@AdministratorRoleId, N'permission', N'Posts.Edit'),
    (@AdministratorRoleId, N'permission', N'Posts.Delete'),
    (@AdministratorRoleId, N'permission', N'Sidebars.View'),
    (@AdministratorRoleId, N'permission', N'Sidebars.Create'),
    (@AdministratorRoleId, N'permission', N'Sidebars.Edit'),
    (@AdministratorRoleId, N'permission', N'Sidebars.Delete'),
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

INSERT INTO dbo.Users (
    UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, IsActive
)
VALUES (
    N'admin@cornerstonezearing.org', N'ADMIN@CORNERSTONEZEARING.ORG',
    N'admin@cornerstonezearing.org', N'ADMIN@CORNERSTONEZEARING.ORG', 1,
    N'AQAAAAEAACcQAAAAEBf4SBRAOx/vb2jH3mnHVTkgBYRXhoWeAkhMtq7+LIxeNQ486zOjf6aGhhbO3DJ2bQ==',
    CONVERT(NVARCHAR(MAX), NEWID()), CONVERT(NVARCHAR(MAX), NEWID()),
    0, 0, 1, 0,
    N'Site', N'Administrator', 1
)
GO

DECLARE @AdministratorRoleId INT = (
    SELECT Id 
    FROM dbo.Roles 
    WHERE NormalizedName = N'ADMINISTRATOR'
);

DECLARE @AdminUserId INT = (
    SELECT Id 
    FROM dbo.Users 
    WHERE NormalizedEmail = N'ADMIN@CORNERSTONEZEARING.ORG'
);

INSERT INTO dbo.UserRoles (UserId, RoleId)
VALUES (@AdminUserId, @AdministratorRoleId);
GO