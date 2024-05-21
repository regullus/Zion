use univerDev
go
--Delete Rede.TabuleiroLog 
/*
Update Usuario.usuario set senha = 'YWbWJ3CI34EG1FB1eU6Ryg==' where id>2586
Update Autenticacao set PasswordHash = 'AFdh14qX3cC2KR5OLsCnYPVunVMzwAKzr2Z0r8Z0+oRYezufFNVs+WM+y4tWXMsk4w==', SecurityStamp = '95c37d77-4bab-4b44-827f-b7a72a2a73db' where id>2586
*/

--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and apelido like 'nali24'

Declare @UsuarioID int, @BoardID int, @TabuleiroID int, @PatrocinadorID int

Set @UsuarioID = 5579
Set @PatrocinadorID = 5579
Set @BoardID = 1
Set @TabuleiroID = 1

--****************** Usuario ******************
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id = @UsuarioID

--****************** Inidcados ******************
--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = 5578

--****************** TabuleiroUsuario ******************
--Select * from Rede.TabuleiroUsuario where usuarioID = @UsuarioID and BoardID in (1,2)

--Exec spC_TabuleiroUsuario @UsuarioID=@UsuarioID

--****************** lOG ******************
Select * from  Rede.TabuleiroLog where UsuarioID = @UsuarioID

--****************** REGRAS ******************
--Exec spC_TabuleiroUsuarioRule @UsuarioID=@UsuarioID
--Exec spC_TabuleiroMasterRule @UsuarioID=@UsuarioID, @BoardID=1

--****************** tabuleiro ******************
--Select * from  Rede.Tabuleiro where ID = @TabuleiroID

--****************** Usuario esta nos tabuleiros: ******************

--Select * From Rede.Tabuleiro Where (Master = @UsuarioID Or CoordinatorDir = @UsuarioID Or IndicatorDirSup = @UsuarioID Or IndicatorDirInf = @UsuarioID Or DonatorDirSup1 = @UsuarioID Or DonatorDirSup2 = @UsuarioID Or DonatorDirInf1 = @UsuarioID Or DonatorDirInf2 = @UsuarioID Or CoordinatorEsq = @UsuarioID Or IndicatorEsqSup = @UsuarioID Or IndicatorEsqInf = @UsuarioID Or DonatorEsqSup1 = @UsuarioID Or DonatorEsqSup2 = @UsuarioID Or DonatorEsqInf1 = @UsuarioID Or DonatorEsqInf2 = @UsuarioID) and StatusID =1




