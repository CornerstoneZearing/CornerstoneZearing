-- -----------------------
-- Slideshow Slides Schema
-- -----------------------

CREATE TABLE [SlideshowSlides] (
	[SlideshowSlideID] [uniqueidentifier] NOT NULL DEFAULT (NEWID()),
	[Name] [nvarchar](260) NOT NULL,
	[StoredFileName] [nvarchar](300) NOT NULL,
	[AltText] [nvarchar](500) NOT NULL DEFAULT (''),
	[Link] [nvarchar](260) NOT NULL DEFAULT (''),
	[IsActive] [bit] NOT NULL DEFAULT (1),
	[DisplayOrder] [int] NOT NULL DEFAULT (0),
	[DateCreated] [datetime2](7) NOT NULL,
	[DateModified] [datetime2](7) NOT NULL,
CONSTRAINT [PK_SlideshowSlides] PRIMARY KEY CLUSTERED ([SlideshowSlideID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
GO