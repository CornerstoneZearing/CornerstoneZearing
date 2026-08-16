-- ------------------------------------
-- Parent Pages (nested page hierarchy)
-- ------------------------------------

ALTER TABLE [Pages] ADD [ParentPageID] [uniqueidentifier] NULL
GO

ALTER TABLE [Pages] WITH CHECK 
ADD CONSTRAINT [FK_Pages_Pages_ParentPageID] 
FOREIGN KEY ([ParentPageID]) REFERENCES [Pages]([PageID])
GO

DROP INDEX [IX_Pages_UrlSlug] ON [Pages]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Pages_ParentPageID_UrlSlug] ON [Pages]([ParentPageID] ASC, [UrlSlug] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO