-- ------------
-- Pages Schema
-- ------------

CREATE TABLE [Pages] (
	[PageID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[TemplateName] [nvarchar](100) NOT NULL,
	[UrlSlug] [nvarchar](200) NOT NULL,
	[MetaTitle] [nvarchar](200) NULL,
	[MetaDescription] [nvarchar](500) NULL,
	[DateCreated] [datetime2](7) NOT NULL,
	[DateModified] [datetime2](7) NOT NULL,
	[Status] [int] NOT NULL,
CONSTRAINT [PK_Pages] PRIMARY KEY CLUSTERED ([PageID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indices

CREATE UNIQUE NONCLUSTERED INDEX [IX_Pages_UrlSlug] ON [Pages]([UrlSlug] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO