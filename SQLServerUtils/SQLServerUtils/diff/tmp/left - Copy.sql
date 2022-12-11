SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- TESTing
CREATE TABLE [dbo].[BizTalkDBVersion](
	[BizTalkDBName] [nvarchar](64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[DatabaseMajor] [int] NOT NULL,
	[DatabaseMinor] [int] NOT NULL,
	[DatabaseBuildNumber] [int] NOT NULL,
	[DatabaseRevision] [int] NOT NULL,
	[ProductMajor] [int] NOT NULL,
	[ProductMinor] [int] NOT NULL,
	[ProductBuildNumber] [int] NOT NULL,
	[ProductRevision] [int] NOT NULL,
	[ProductLanguage] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Description] [nvarchar](256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Modified] [datetime] NULL
) ON [PRIMARY]
SELECT GETDATE()
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BizTalkDBVersion]') AND name = N'BizTalkDBVersion_unique_key')
ALTER TABLE [dbo].[BizTalkDBVersion] DROP CONSTRAINT [BizTalkDBVersion_unique_key]
SET ANSI_PADDING ON

ALTER TABLE [dbo].[BizTalkDBVersion] ADD  CONSTRAINT [BizTalkDBVersion_unique_key] UNIQUE NONCLUSTERED 
(
	[BizTalkDBName] ASC,
	[ProductMajor] ASC,
	[ProductMinor] ASC,
	[ProductBuildNumber] ASC,
	[ProductRevision] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
