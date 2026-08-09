-- ---------------------------
-- Editor.js Changes for Pages 
-- ---------------------------

ALTER TABLE [Pages] ADD [ContentJson] [nvarchar](max) NULL
GO

EXEC sp_rename 'Pages.Content', 'ContentHtml', 'COLUMN'
GO