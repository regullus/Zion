drop  PROC [dbo].[spOC_US_ObtemAvisos] 
go
CREATE PROC [dbo].[spOC_US_ObtemAvisos] @UsuarioID INT 
AS
BEGIN

	select A.*, AT.Nome as TipoNome from Usuario.Aviso A inner join Usuario.AvisoTipo AT on A.TipoID = AT.ID
	left join Usuario.AvisoLido AL on A.ID = AL.AvisoID and AL.UsuarioID =  @UsuarioID
	where A.UsuarioIDs = null or A.UsuarioIDs = @UsuarioID


	select * from Usuario.AvisoLido
END;

GO
