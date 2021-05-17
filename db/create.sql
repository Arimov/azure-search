CREATE TABLE [dbo].[BookMetaData](
	[blobPath] [varchar](255) NOT NULL,
	[title] [nvarchar](255) NULL,
	[autorId] [int] NULL,
	[themeId] [int] NULL,
	[price] [money] NULL,
	[countPages] [int] NULL,
	[rating] [decimal](15, 2) NULL,
 CONSTRAINT [PK_BookMetaData] PRIMARY KEY CLUSTERED 
(
	[blobPath] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
 
 CREATE TABLE [dbo].[BookAuthors](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[authorName] [nvarchar](100) NOT NULL,
	[firstName] [nvarchar](50) NULL,
	[lastName] [nvarchar](50) NULL,
	[birth] [date] NULL,
 CONSTRAINT [PK_BookAuthors] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[BookThemes](
	[id] [int] NOT NULL,
	[theme] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BookThemes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO