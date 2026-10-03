/*
    Cornerstone Zearing
    Page slugs are unique per parent page rather than globally
*/

DROP INDEX IX_Pages_Slug ON dbo.Pages
GO

CREATE UNIQUE INDEX IX_Pages_ParentPageID_Slug
ON dbo.Pages (ParentPageID, Slug)
GO
