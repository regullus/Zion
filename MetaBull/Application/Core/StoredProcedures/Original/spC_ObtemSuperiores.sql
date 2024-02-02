use ClubeVantagens
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_ObtemSuperiores'))
   Drop Procedure spC_ObtemSuperiores
Go

delete [Usuario].[Superiores]
go

/*

drop table [Usuario].[Superiores]
--Tabela fisica com comportamento de temporaria
CREATE TABLE [Usuario].[Superiores](
	[ID] [int] NOT NULL,
	[IDSuperior] [int] NOT NULL,
	[Nivel] [int] NULL
) ON [PRIMARY]
CREATE INDEX UsuSupID ON [Usuario].[Superiores] (ID)
CREATE INDEX UsuSupIDNivel ON [Usuario].[Superiores] (ID,Nivel)

*/

Create  Proc [dbo].[spC_ObtemSuperiores]
   @id int,
   @idSuperiores int,
   @NivelFilho int,
   @MaxNivel int,
   @ExibeNivel int null

As
Begin
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on
   declare @Data datetime,
           @DataOntem datetime,
           @Continua int

   Select @Continua = 1
      
   if(@NivelFilho = 1) 
   Begin
     If Exists (Select 'x' From Usuario.Superiores Where ID = @id and nivel = @MaxNivel)
      Begin
         Set @Continua = 0
      End
      Else
      Begin
        delete Usuario.Superiores  where ID = @id
      End
   End

   --Select @NivelFilho , @MaxNivel         
   if(@NivelFilho <= @MaxNivel and @Continua = 1)
   Begin  
     if exists ( --Verifica se superior é o mesmo q o afiliado
     Select 
        'ok'
      from 
         usuario.Usuario    usu
      where 
         usu.id = @idSuperiores and
         usu.id <> PatrocinadorDiretoID and
         usu.StatusID = 2
     )
     Begin
         -- Obtem Primeiro nivel
         Insert into Usuario.Superiores
         Select 
            @id ID,
            usu.PatrocinadorDiretoID IDSuperior,
            @NivelFilho Nivel
         from 
            usuario.Usuario    usu
         where 
            usu.id = @idSuperiores and
            usu.StatusID = 2 

        --  ******* Inicio Cursor *******
         Declare
            @cID int,
            @IDSuperior int,
            @Nivel int,
            @ProximoNivel int,
            @query nvarchar(255),
            @AntFetch int

         --Cursor
         Declare
            curRegistro 
         Cursor Local For
         Select 
            IDSuperior,
            Nivel
         From 
            Usuario.Superiores
         Where 
            ID = @id And
            Nivel = @NivelFilho 
         Order By 
            ID

         Open curRegistro

         if (@NivelFilho <= @MaxNivel)
         If (Select @@CURSOR_ROWS) <> 0
            Begin
               Fetch Next From curRegistro Into 
                  @IDSuperior,
                  @Nivel

               Select @AntFetch = @@fetch_status
               While @AntFetch = 0
               Begin
                  set @ProximoNivel = @Nivel + 1;

                  if(@ProximoNivel <= @MaxNivel)
                  Begin
                     --Recursiva
                     Exec spC_ObtemSuperiores @id, @IDSuperior, @ProximoNivel, @MaxNivel, @ExibeNivel
                  End

                  --Proxima linha do cursor
                  Fetch Next From curRegistro Into 
                     @IDSuperior,
                     @Nivel
    
                  -- Para ver se nao é fim do loop
                  Select @AntFetch = @@fetch_status         
               End -- While

            End -- @@CURSOR_ROWS
      
         Close curRegistro
         Deallocate curRegistro

      -- ******* Fim Cursor *******

      End --not exists
   End -- if @NivelFilho <= @MaxNivel

   if(@NivelFilho = 1 and @ExibeNivel >-1) 
   Begin
      Select 
         ID,
         IDSuperior,
         Nivel
      From 
         Usuario.Superiores 
      where 
         ID = @id And
         nivel = Case  
                    When @ExibeNivel > 0 Then @ExibeNivel
                    When @ExibeNivel = -1 then -1
                    Else nivel
         End
         and nivel <= @MaxNivel 
      Order by
         Nivel,
         IDSuperior
   End
 

End -- Sp

go
Grant Exec on spC_ObtemSuperiores To public
go

--Exec spC_ObtemSuperiores @id=26,@idSuperiores=26,@NivelFilho=1,@MaxNivel=6, @ExibeNivel = 0

--Exec spC_ObtemSuperiores @id=1233,@idSuperiores=1233,@NivelFilho=1,@MaxNivel=10, @ExibeNivel = 0
 
--select * from Usuario.Usuario where login = 'cdvbrazil1'

--Exec spC_ObtemSuperiores @id=26,@idSuperiores=26,@NivelFilho=1,@MaxNivel=1, @ExibeNivel = 1