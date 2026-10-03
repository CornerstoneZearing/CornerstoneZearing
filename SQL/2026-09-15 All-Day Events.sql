/*
    Cornerstone Zearing
    Support for all-day events
*/

ALTER TABLE dbo.[Events] ADD AllDay BIT NOT NULL CONSTRAINT DF_Events_AllDay DEFAULT (0)
GO