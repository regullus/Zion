create table Rede.ConfiguracaoBonusDiario(
	ID int NOT NULL IDENTITY (1, 1),
	DataReferencia datetime NOT NULL,
	Valor float(53) NOT NULL,
	IsPercentual bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Rede.ConfiguracaoBonusDiario ADD CONSTRAINT
	PK_Rede_ConfiguracaoBonusDiario_1 PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


