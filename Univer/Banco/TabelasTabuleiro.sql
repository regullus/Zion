USE [univerDev]
GO
DROP TABLE [Rede].[TabuleiroLog]
DROP TABLE [Rede].[TabuleiroUsuario]
DROP TABLE [Rede].[Tabuleiro]
DROP TABLE [Rede].[TabuleiroBoard]
GO
/****** Object:  Table [Rede].[Tabuleiro]    Script Date: 4/19/2024 1:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Rede].[Tabuleiro](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BoardID] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[Master] [int] NOT NULL,
	[CoordinatorDir] [int] NULL,
	[IndicatorDirSup] [int] NULL,
	[IndicatorDirInf] [int] NULL,
	[DonatorDirSup1] [int] NULL,
	[DonatorDirSup2] [int] NULL,
	[DonatorDirInf1] [int] NULL,
	[DonatorDirInf2] [int] NULL,
	[CoordinatorEsq] [int] NULL,
	[IndicatorEsqSup] [int] NULL,
	[IndicatorEsqInf] [int] NULL,
	[DonatorEsqSup1] [int] NULL,
	[DonatorEsqSup2] [int] NULL,
	[DonatorEsqInf1] [int] NULL,
	[DonatorEsqInf2] [int] NULL,
	[DataInicio] [int] NULL,
	[DataFim] [int] NULL,
 CONSTRAINT [PK_Estacao] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Rede].[TabuleiroBoard]    Script Date: 4/19/2024 1:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Rede].[TabuleiroBoard](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](50) NOT NULL,
	[Cor] [nvarchar](50) NOT NULL,
	[CorTexto] [nvarchar](50) NOT NULL,
	[GroupID] [int] NOT NULL,
	[DataInicial] [datetime] NOT NULL,
	[DataFinal] [datetime] NOT NULL,
	[Ativo] [bit] NOT NULL,
	[Licenca] [decimal](10, 2) NOT NULL,
	[Transferencia] [decimal](10, 2) NOT NULL,
	[indicados] [int] NULL,
 CONSTRAINT [PK_Board] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Rede].[TabuleiroLog]    Script Date: 4/19/2024 1:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Rede].[TabuleiroLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioID] [int] NOT NULL,
	[UsuarioPaiID] [int] NOT NULL,
	[BoardID] [int] NOT NULL,
	[Data] [int] NOT NULL,
	[Mensagem] [nvarchar](255) NOT NULL,
	[Debug] [nvarchar](max) NULL,
 CONSTRAINT [PK_TabuleiroLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Rede].[TabuleiroUsuario]    Script Date: 4/19/2024 1:49:09 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Rede].[TabuleiroUsuario](
	[UsuarioID] [int] NOT NULL,
	[BoardID] [int] NOT NULL,
	[TabuleiroID] [int] NULL,
	[StatusID] [int] NOT NULL,
	[MasterID] [int] NOT NULL,
	[InformePag] [bit] NOT NULL,
	[UsuarioIDPag] [int] NULL,
	[Ciclo] [int] NOT NULL,
	[Posicao] [nvarchar](100) NOT NULL,
	[PagoMaster] [bit] NOT NULL,
	[InformePagSistema] [bit] NOT NULL,
	[PagoSistema] [bit] NOT NULL,
	[DireitaFechada] [bit] NOT NULL,
	[EsquerdaFechada] [bit] NOT NULL,
	[DataInicio] [datetime] NOT NULL,
	[DataFim] [int] NULL,
	[Debug] [nvarchar](255) NULL,
 CONSTRAINT [PK_TabuleiroUsuario_1] PRIMARY KEY CLUSTERED 
(
	[UsuarioID] ASC,
	[BoardID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [Rede].[Tabuleiro] ON 

INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (1, 1, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240206, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (2, 2, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240207, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (3, 3, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240208, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (4, 4, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240209, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (5, 5, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240210, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (6, 6, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240211, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (7, 7, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240212, NULL)
INSERT [Rede].[Tabuleiro] ([ID], [BoardID], [StatusID], [Master], [CoordinatorDir], [IndicatorDirSup], [IndicatorDirInf], [DonatorDirSup1], [DonatorDirSup2], [DonatorDirInf1], [DonatorDirInf2], [CoordinatorEsq], [IndicatorEsqSup], [IndicatorEsqInf], [DonatorEsqSup1], [DonatorEsqSup2], [DonatorEsqInf1], [DonatorEsqInf2], [DataInicio], [DataFim]) VALUES (8, 8, 1, 2580, 2581, 2583, 2584, NULL, NULL, NULL, NULL, 2582, 2585, 2586, NULL, NULL, NULL, NULL, 20240206, NULL)
SET IDENTITY_INSERT [Rede].[Tabuleiro] OFF
GO
SET IDENTITY_INSERT [Rede].[TabuleiroBoard] ON 

INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (1, N'MERCURIO', N'grey-card', N'grey-gallery', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(10.00 AS Decimal(10, 2)), CAST(25.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (2, N'SATURNO', N'green-card', N'blue-chambray', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(20.00 AS Decimal(10, 2)), CAST(75.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (3, N'MARTE', N'red-card', N'red-soft', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(40.00 AS Decimal(10, 2)), CAST(225.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (4, N'JUPITER', N'pink-card', N'purple-seance', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(60.00 AS Decimal(10, 2)), CAST(675.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (5, N'VENUS', N'orange-card', N'yellow-gold', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(100.00 AS Decimal(10, 2)), CAST(2000.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (6, N'URANO', N'blue-card', N'blue-steel', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(150.00 AS Decimal(10, 2)), CAST(6000.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (7, N'TERRA', N'turquoise-card', N'blue-soft', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(300.00 AS Decimal(10, 2)), CAST(18000.00 AS Decimal(10, 2)), 1)
INSERT [Rede].[TabuleiroBoard] ([ID], [Nome], [Cor], [CorTexto], [GroupID], [DataInicial], [DataFinal], [Ativo], [Licenca], [Transferencia], [indicados]) VALUES (8, N'SOL', N'gold-card', N'red-haze', 1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2050-01-01T00:00:00.000' AS DateTime), 1, CAST(800.00 AS Decimal(10, 2)), CAST(54000.00 AS Decimal(10, 2)), 1)
SET IDENTITY_INSERT [Rede].[TabuleiroBoard] OFF
GO
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 1, 1, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 0, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 2, 2, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 3, 3, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 4, 4, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 5, 5, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 6, 6, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 7, 7, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2580, 8, 8, 1, 2580, 1, NULL, 1, N'Master', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 1, 1, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 2, 2, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 3, 3, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 4, 4, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 5, 5, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 6, 6, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 7, 7, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2581, 8, 8, 1, 2580, 1, NULL, 1, N'CoordinatorDir', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 1, 1, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 2, 2, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 3, 3, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 4, 4, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 5, 5, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 6, 6, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 7, 7, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2582, 8, 8, 1, 2580, 1, NULL, 1, N'CoordinatorEsq', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 1, 1, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 2, 2, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 3, 3, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 4, 4, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 5, 5, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 6, 6, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 7, 7, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2583, 8, 8, 1, 2580, 1, NULL, 1, N'IndicatorDirSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 1, 1, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 2, 2, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 3, 3, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 4, 4, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 5, 5, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 6, 6, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 7, 7, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2584, 8, 8, 1, 2580, 1, NULL, 1, N'IndicatorDirInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 1, 1, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 2, 2, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 3, 3, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 4, 4, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 5, 5, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 6, 6, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 7, 7, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2585, 8, 8, 1, 2580, 1, NULL, 1, N'IndicatorEsqSup', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 1, 1, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 2, 2, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 3, 3, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 4, 4, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 5, 5, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 6, 6, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 7, 7, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
INSERT [Rede].[TabuleiroUsuario] ([UsuarioID], [BoardID], [TabuleiroID], [StatusID], [MasterID], [InformePag], [UsuarioIDPag], [Ciclo], [Posicao], [PagoMaster], [InformePagSistema], [PagoSistema], [DireitaFechada], [EsquerdaFechada], [DataInicio], [DataFim], [Debug]) VALUES (2586, 8, 8, 1, 2580, 1, NULL, 1, N'IndicatorEsqInf', 1, 1, 1, 0, 0, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, NULL)
GO
/****** Object:  Index [IX_TabuleiroUsuario]    Script Date: 4/19/2024 1:49:09 PM ******/
CREATE NONCLUSTERED INDEX [IX_TabuleiroUsuario] ON [Rede].[TabuleiroUsuario]
(
	[UsuarioID] ASC,
	[BoardID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [Rede].[Tabuleiro]  WITH CHECK ADD  CONSTRAINT [FK_Estacao_Board] FOREIGN KEY([BoardID])
REFERENCES [Rede].[TabuleiroBoard] ([ID])
GO
ALTER TABLE [Rede].[Tabuleiro] CHECK CONSTRAINT [FK_Estacao_Board]
GO
ALTER TABLE [Rede].[Tabuleiro]  WITH CHECK ADD  CONSTRAINT [FK_Estacao_Status] FOREIGN KEY([StatusID])
REFERENCES [Rede].[Status] ([ID])
GO
ALTER TABLE [Rede].[Tabuleiro] CHECK CONSTRAINT [FK_Estacao_Status]
GO
ALTER TABLE [Rede].[TabuleiroBoard]  WITH CHECK ADD  CONSTRAINT [FK_Board_Group] FOREIGN KEY([GroupID])
REFERENCES [Rede].[Group] ([ID])
GO
ALTER TABLE [Rede].[TabuleiroBoard] CHECK CONSTRAINT [FK_Board_Group]
GO
ALTER TABLE [Rede].[TabuleiroLog]  WITH CHECK ADD  CONSTRAINT [FK_TabuleiroLog_Usuario] FOREIGN KEY([UsuarioID])
REFERENCES [Usuario].[Usuario] ([ID])
GO
ALTER TABLE [Rede].[TabuleiroLog] CHECK CONSTRAINT [FK_TabuleiroLog_Usuario]
GO
ALTER TABLE [Rede].[TabuleiroUsuario]  WITH CHECK ADD  CONSTRAINT [FK_TabuleiroUsuario_Tabuleiro] FOREIGN KEY([TabuleiroID])
REFERENCES [Rede].[Tabuleiro] ([ID])
GO
ALTER TABLE [Rede].[TabuleiroUsuario] CHECK CONSTRAINT [FK_TabuleiroUsuario_Tabuleiro]
GO
ALTER TABLE [Rede].[TabuleiroUsuario]  WITH CHECK ADD  CONSTRAINT [FK_TabuleiroUsuario_TabuleiroBoard] FOREIGN KEY([BoardID])
REFERENCES [Rede].[TabuleiroBoard] ([ID])
GO
ALTER TABLE [Rede].[TabuleiroUsuario] CHECK CONSTRAINT [FK_TabuleiroUsuario_TabuleiroBoard]
GO
