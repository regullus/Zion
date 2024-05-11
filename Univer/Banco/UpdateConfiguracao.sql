use UniverDev
go
declare @chave nvarchar(255), @TextoP nvarchar(255), @TextoE nvarchar(255), @TextoI nvarchar(255)
select @chave = 'INDICADOS'
Select * from Globalizacao.Traducao where chave like '%' + @chave + '%'

delete Globalizacao.Traducao  where chave = @chave

select 
    @TextoP = 'Indicados',
	@TextoE = 'Nominados',
	@TextoI = 'Nominees'
	
--update Globalizacao.Traducao set Texto = @TextoP where chave = @chave and IdiomaID = 1
--update Globalizacao.Traducao set Texto = @TextoE where chave = @chave and IdiomaID = 2
--update Globalizacao.Traducao set Texto = @TextoI where chave = @chave and IdiomaID = 3

insert into  Globalizacao.Traducao values (1,1,@chave,@TextoP,'',getdate())
insert into  Globalizacao.Traducao values (2,1,@chave,@TextoE,'',getdate())
insert into  Globalizacao.Traducao values (3,1,@chave,@TextoI,'',getdate())

Select * from Globalizacao.Traducao where chave = @chave

-----------CONFIGURACAO------------
use UniverDev
go
declare @chaveC nvarchar(255), @dados nvarchar(255)
select @chaveC = 'ADM_HOME_DASHBOARD_USUARIOS_ASSOCIACAO'
select @dados = 'false'
--delete Sistema.Configuracao where chave = @chaveC 
select * from Sistema.Configuracao where chave like '%' + @chaveC + '%'

--delete sistema.Configuracao where chave = @chaveC
update Sistema.Configuracao set Dados = @dados where chave = @chaveC
--insert into Sistema.Configuracao values (1,1,@chaveC,'',@dados)

select * from Sistema.Configuracao where chave = @chaveC

--select * from Sistema.Configuracao where dados like  '%http%'

--dev -> prod
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/office//Home/MTRetorno' where Dados = 'https://dev.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global' where Dados = 'https://dev.universol.global'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/' where Dados = 'https://dev.universol.global/'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/office//Home/MTRetorno' where Dados = 'https://dev.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/account/loginexterno' where Dados = 'https://dev.universol.global/account/loginexterno'

--prod -> dev
--update Sistema.Configuracao set Dados = 'https://dev.universol.global/office//Home/MTRetorno' where Dados = 'https://universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://dev.universol.global' where Dados = 'https://donator.universol.global'
--update Sistema.Configuracao set Dados = 'https://dev.universol.global/' where Dados = 'https://donator.universol.global/'
--update Sistema.Configuracao set Dados = 'https://dev.universol.global/office//Home/MTRetorno' where Dados = 'https://universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://dev.universol.global/account/loginexterno' where Dados = 'https://donator.universol.global/account/loginexterno'

--dev -> homol
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/office//Home/MTRetorno' where Dados = 'https://dev.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global' where Dados = 'https://dev.universol.global'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/' where Dados = 'https://dev.universol.global/'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/office//Home/MTRetorno' where Dados = 'https://dev.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/account/loginexterno' where Dados = 'https://dev.universol.global/account/loginexterno'

--homol -> prod
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/office//Home/MTRetorno' where Dados = 'https://homol.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global' where Dados = 'https://homol.universol.global'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/' where Dados = 'https://homol.universol.global/'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/office//Home/MTRetorno' where Dados = 'https://homol.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://donation.universol.global/account/loginexterno' where Dados = 'https://homol.universol.global/account/loginexterno'

--prod -> homol
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/office//Home/MTRetorno' where Dados = 'https://donation.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global' where Dados = 'https://donation.universol.global'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/' where Dados = 'https://donation.universol.global/'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/office//Home/MTRetorno' where Dados = 'https://donation.universol.global/office//Home/MTRetorno'
--update Sistema.Configuracao set Dados = 'https://homol.universol.global/account/loginexterno' where Dados = 'https://donation.universol.global/account/loginexterno'



