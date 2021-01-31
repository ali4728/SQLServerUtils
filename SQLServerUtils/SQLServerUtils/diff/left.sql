SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Applications](
	[nvcApplicationName] [nvarchar](128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[uidAppID] [uniqueidentifier] NOT NULL,
	[fAttributes] [bigint] NOT NULL
) ON [PRIMARY]

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') AND name = N'CIX_Applications')
DROP INDEX [CIX_Applications] ON [dbo].[Applications] WITH ( ONLINE = OFF )
SET ANSI_PADDING ON

CREATE UNIQUE CLUSTERED INDEX [CIX_Applications] ON [dbo].[Applications]
(
	[nvcApplicationName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') AND name = N'IX_Applications')
DROP INDEX [IX_Applications] ON [dbo].[Applications]
SET ANSI_PADDING ON

CREATE NONCLUSTERED INDEX [IX_Applications] ON [dbo].[Applications]
(
	[uidAppID] ASC,
	[nvcApplicationName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
