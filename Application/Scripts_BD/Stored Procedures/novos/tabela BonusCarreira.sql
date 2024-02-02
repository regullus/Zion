create table Rede.ConfiguracaoBonusCarreira(
	ID int NOT NULL IDENTITY (1, 1),
	DataReferencia datetime NOT NULL,
	CarreiraID int,
	Valor float(53) NOT NULL,
	QuantidadePessoas int	
	)  ON [PRIMARY]
GO
ALTER TABLE Rede.ConfiguracaoBonusCarreira ADD CONSTRAINT
	PK_Rede_ConfiguracaoBonusCarreira_1 PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


