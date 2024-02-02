create table Rede.BonificacaoExecucao(
	ID int NOT NULL IDENTITY (1, 1),
	CategoriaBonusID int,
	DataExecucao date,
	DataCriacao Datetime
	)  ON [PRIMARY]
GO
ALTER TABLE Rede.BonificacaoExecucao ADD CONSTRAINT
	PK_Rede_BonificacaoExecucao_1 PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


