
use Univer
go
delete rede.TabuleiroUsuario where id > 49
delete rede.Tabuleiro where id > 7
delete rede.TabuleiroLog
delete rede.TabuleiroNivel where id> 49

update Rede.Tabuleiro 
Set
   Master = 2580,
   CoodinatorDir = 2581, 
   CoodinatorEsq = 2582,
   IndicatorDirSup = 2583,
   IndicatorDirInf = 2584,
   DonatorDirSup1 = NULL,
   DonatorDirSup2 = NULL,
   DonatorDirInf1 = NULL,
   DonatorDirInf2 = NULL,
   IndicatorEsqSup = 2585,
   IndicatorEsqInf = 2586,
   DonatorEsqSup1 = NULL,
   DonatorEsqSup2 = NULL,
   DonatorEsqInf1 = NULL,
   DonatorEsqInf2 = NULL,
   StatusID = 1,
   DataFim = NULL
Where 
   id >= 1 and ID < = 7

update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'Master', MasterID = 2580 Where id in (1,8,15,22,29,36,43)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'CordinatorDir', MasterID = 2580 Where id in (2,9,16,23,30,37,44)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'CordinatorEsq', MasterID = 2580 Where id in (3,10,17,24,31,38,45)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'IndicatorDirSup', MasterID = 2580 Where id in (4,11,18,25,32,39,46)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'IndicatorDirInf', MasterID = 2580 Where id in (5,12,19,26,33,40,47)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'IndicatorEsqSup', MasterID = 2580 Where id in (6,13,20,27,34,41,48)
update Rede.TabuleiroUsuario Set StatusID = 1, DataFim = null, Posicao = 'IndicatorEsqInf', MasterID = 2580 Where id in (7,14,21,28,35,42,49)
update Rede.TabuleiroUsuario Set TabuleiroID = BoardID, Ciclo = 1 Where id <= 49

update rede.TabuleiroNivel set DataFim = null, StatusID = 2, Observacao = 'Usuário do Sistema'
go

Select * from rede.TabuleiroUsuario -- where StatusID = 1 order by UsuarioID
Select * from Rede.TabuleiroNivel
Select * from rede.Tabuleiro --where StatusID = 1
Select * from Rede.TabuleiroLog







