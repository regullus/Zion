drop table [Usuario].[Pontos]
GO
CREATE TABLE [Usuario].[Pontos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioID] [int] NOT NULL,
	[CicloID] [int] NULL,
	[PontosE] [int] NOT NULL,
	[PontosD] [int] NOT NULL,
	DataReferencia datetime,
	DataCriacao Datetime
 CONSTRAINT [PK_Usuario_Pontos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

