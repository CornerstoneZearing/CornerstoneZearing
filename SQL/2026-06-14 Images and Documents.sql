-- ---------------------------
-- Images and Documents Schema
-- ---------------------------

CREATE TABLE [MediaDocuments] (
	[MediaDocumentID] [uniqueidentifier] NOT NULL,
	[OriginalFileName] [nvarchar](260) NOT NULL,
	[StoredFileName] [nvarchar](300) NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[DateUploaded] [datetime2](7) NOT NULL,
	[UploadedByUserID] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_MediaDocuments] PRIMARY KEY CLUSTERED ([MediaDocumentID] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
GO

CREATE TABLE [MediaImages] (
	[MediaImageID] [uniqueidentifier] NOT NULL,
	[OriginalFileName] [nvarchar](260) NOT NULL,
	[StoredFileName] [nvarchar](300) NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[FileSize] [bigint] NOT NULL,
	[AltText] [nvarchar](500) NOT NULL,
	[DateUploaded] [datetime2](7) NOT NULL,
	[UploadedByUserID] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_MediaImages] PRIMARY KEY CLUSTERED ([MediaImageID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
GO