create table Usuario.Qualificacao(
	ID int NOT NULL IDENTITY (1, 1),
	UsuarioID int,
	QualificadorDireitaID int null,
	QualificadorEsquerdaID int null,
	DataQualificacao datetime null,

	)  ON [PRIMARY]
GO
ALTER TABLE Usuario.Qualificacao ADD CONSTRAINT
	PK_Usuario_Qualificacao_1 PRIMARY KEY CLUSTERED 
	(
	ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


