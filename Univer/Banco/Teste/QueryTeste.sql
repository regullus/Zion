use univerDev
go

--Delete Rede.TabuleiroLog 
/*
Update Usuario.usuario set senha = 'YWbWJ3CI34EG1FB1eU6Ryg==' where id>2586
Update Autenticacao set PasswordHash = 'AFdh14qX3cC2KR5OLsCnYPVunVMzwAKzr2Z0r8Z0+oRYezufFNVs+WM+y4tWXMsk4w==', SecurityStamp = '95c37d77-4bab-4b44-827f-b7a72a2a73db' where id>2586
*/

--Select id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id = 2584

Declare @UsuarioID int, @BoardID int, @TabuleiroID int, @PatrocinadorID int, @MasterID int

Select @UsuarioID = id from usuario.usuario where id > 2500 and apelido = 'cloacir010'

Set @PatrocinadorID = @UsuarioID
Set @BoardID = 1
Set @TabuleiroID = 154
--Set @UsuarioID = 4820

--****************** Usuario ******************
Select 'Usuario' Query, id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id = @UsuarioID

--****************** Tempo Usuario ******************
--Select 'Tempo' Query, * from Rede.TabuleiroUltimoAcesso where UsuarioID = @UsuarioID

--****************** TabuleiroUsuario ******************
Select @TabuleiroID = TabuleiroID, @MasterID = MasterID from Rede.TabuleiroUsuario where usuarioID = @UsuarioID and BoardID = @BoardID
--Select 'TabuleiroUsuario' Query, @TabuleiroID TabuleiroID, @MasterID MasterID 
--Select 'TabuleiroUsuario' Query, * from Rede.TabuleiroUsuario where usuarioID = @UsuarioID and BoardID = @BoardID
--Select 'TabuleiroUsuario' Query, * from Rede.TabuleiroUsuario where usuarioID = @UsuarioID and BoardID in (1,2,3,4)

--****************** TabuleiroUsuario COMO MASTER ******************
--Select 'TabuleiroUsuario' Query, * from Rede.TabuleiroUsuario where MasterID = @UsuarioID and BoardID = @BoardID

--****************** Usuario esta nos tabuleiros: ******************
--Select 'Tabuleiro' Query, * From Rede.Tabuleiro Where (Master = @UsuarioID Or CoordinatorDir = @UsuarioID Or IndicatorDirSup = @UsuarioID Or IndicatorDirInf = @UsuarioID Or DonatorDirSup1 = @UsuarioID Or DonatorDirSup2 = @UsuarioID Or DonatorDirInf1 = @UsuarioID Or DonatorDirInf2 = @UsuarioID Or CoordinatorEsq = @UsuarioID Or IndicatorEsqSup = @UsuarioID Or IndicatorEsqInf = @UsuarioID Or DonatorEsqSup1 = @UsuarioID Or DonatorEsqSup2 = @UsuarioID Or DonatorEsqInf1 = @UsuarioID Or DonatorEsqInf2 = @UsuarioID) and StatusID = 1
--Select count(*) From Rede.Tabuleiro Where (Master = @UsuarioID Or CoordinatorDir = @UsuarioID Or IndicatorDirSup = @UsuarioID Or IndicatorDirInf = @UsuarioID Or DonatorDirSup1 = @UsuarioID Or DonatorDirSup2 = @UsuarioID Or DonatorDirInf1 = @UsuarioID Or DonatorDirInf2 = @UsuarioID Or CoordinatorEsq = @UsuarioID Or IndicatorEsqSup = @UsuarioID Or IndicatorEsqInf = @UsuarioID Or DonatorEsqSup1 = @UsuarioID Or DonatorEsqSup2 = @UsuarioID Or DonatorEsqInf1 = @UsuarioID Or DonatorEsqInf2 = @UsuarioID) and StatusID =2 and BoardID = 1

--****************** tabuleiro ******************
--Select * from  Rede.Tabuleiro where ID = @TabuleiroID

--****************** lOG ******************
Select 'LOG' Query, * from  Rede.TabuleiroLog where UsuarioID = @UsuarioID order By id Desc

--****************** Inidcados ******************
--Select 'indicado', id,login, apelido, PatrocinadorDiretoID from usuario.usuario where id > 2500 and PatrocinadorDiretoID = @UsuarioID
--Exec spC_TabuleiroUsuario @UsuarioID=@UsuarioID

--****************** REGRAS ******************
--Exec spC_TabuleiroUsuarioRule @UsuarioID=@UsuarioID
--Exec spC_TabuleiroMasterRule @UsuarioID=@UsuarioID, @BoardID=1

