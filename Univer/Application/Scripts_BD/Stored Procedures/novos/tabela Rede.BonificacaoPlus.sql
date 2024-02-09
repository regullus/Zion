create table Rede.BonificacaoPlus(
	ID int NOT NULL IDENTITY (1, 1),
	UsuarioID int,
	IsPlus bit,
	UsuarioPlusID int,
	DataCriacao Datetime
	)  ON [PRIMARY]
GO
ALTER TABLE Rede.BonificacaoPlus ADD CONSTRAINT
	PK_Rede_BonificacaoPlus_1 PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


insert into Rede.BonificacaoPlus values (1000,0,null,getdate())