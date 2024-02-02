Use ??????????
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_ObtemDiretos'))
   Drop Procedure spC_ObtemDiretos
go

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Gera registro de Afiliados ate o nivel informado retornando os dados
-- =============================================================================================

Create  Proc [dbo].[spC_ObtemDiretos]
   @id int,
   @idUsuario int,
   @NivelPai int,
   @MaxNivel int,
   @ExibeNivel int null

As
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   declare @Data datetime,
           @DataOntem datetime,
           @Continua int

   Select @Data = dbo.GetDateZion(), @Continua = 1
   
   --Verifica se h� dados da ultima hora
   set @DataOntem = DATEADD(minute,-59,@Data); 
      
   if(@NivelPai = 1 and @Continua = 1) 
   Begin
     If Exists (Select 'x' From Usuario.Afiliados (nolock) Where ID = @id and Data > @DataOntem)
      Begin
         Set @Continua = 0
      End
      else
      Begin
         delete Usuario.Afiliados where id = @id
         set @idUsuario = @id
      End
   End
         
   if(@NivelPai <= @MaxNivel and @Continua = 1)
   Begin  
     if exists ( --Verifica se h� afiliados para o dado patrocinador
     Select 
        'ok'
      from 
         usuario.Usuario (nolock) usu
      where 
         usu.PatrocinadorDiretoID = @idUsuario and
         usu.StatusID = 2 and --Ativos
         usu.id not in (Select IdAfiliado from Usuario.Afiliados where ID = @id And Nivel = @NivelPai) --Verifica se afiliado j� foi incluido na tabela
     )
     Begin

         -- Obtem Primeiro nivel
         Insert into Usuario.Afiliados
         Select 
            @id ID,
            usu.ID IdAfiliado,
            usu.[Login],
            usu.Nome,
            usu.PatrocinadorDiretoID,
            null LoginPatrocinadorDireto,
            null NomePatrocinadorDireto,
            usu.DataAtivacao,
            usu.DataValidade,
            usu.DerramamentoID,
            null AssociacaoID,
            null NomeAssociacao,
            @Data,
            @NivelPai Nivel
         from 
            usuario.Usuario    usu
         where 
            usu.PatrocinadorDiretoID = @idUsuario and
            usu.StatusID = 2 and --Ativos
            usu.id not in (Select IdAfiliado from Usuario.Afiliados (nolock) where ID = @id And Nivel = @NivelPai)

         Update
            afi
         Set
            afi.LoginPatrocinadorDireto = usu.Login,
            afi.NomePatrocinadorDireto = usu.Nome
         From
            Usuario.Afiliados afi,
            Usuario.Usuario   usu
         Where
            afi.PatrocinadorDiretoID = usu.id and
            afi.ID = @id

         Update
            afi
         Set
            afi.AssociacaoID = usu.NivelAssociacao,
            afi.NomeAssociacao = ass.Nome
         From
            Usuario.Afiliados afi,
            Usuario.UsuarioAssociacao usu,
            Rede.Associacao ass           
         Where
            afi.IDAfiliado = usu.UsuarioID and
            ass.ID = usu.NivelAssociacao and
            afi.ID = @id 

         -- ******* Inicio Cursor *******
         Declare
            @cID int,
            @IDAfiliado int,
            @Login nvarchar(50),
            @PatrocinadorDiretoID int,
            @DataAtivacao datetime,
            @ProdutoID int,
            @Pack nvarchar(255),
            @Nivel int,
            @ProximoNivel int,
            @query nvarchar(255),
            @AntFetch int

         --Cursor
         Declare
            curRegistro 
         Cursor Local For
         Select 
            IDAfiliado,
            Nivel
         From 
            Usuario.Afiliados (nolock)
         Where 
            ID = @id And
            Nivel = @NivelPai 
         Order By 
            ID

         Open curRegistro

         if (@NivelPai <= @MaxNivel)
         If (Select @@CURSOR_ROWS) <> 0
            Begin
               Fetch Next From curRegistro Into 
                  @IDAfiliado,
                  @Nivel

               Select @AntFetch = @@fetch_status
               While @AntFetch = 0
               Begin
                  set @ProximoNivel = @Nivel + 1;

                  if(@ProximoNivel <= @MaxNivel)
                  Begin
                     --Recursiva
                     --select @ProximoNivel
                     Exec spC_ObtemDiretos @id, @IDAfiliado, @ProximoNivel, @MaxNivel, @ExibeNivel
                  End

                  --Proxima linha do cursor
                  Fetch Next From curRegistro Into 
                     @IDAfiliado,
                     @Nivel
    
                  -- Para ver se nao � fim do loop
                  Select @AntFetch = @@fetch_status         
               End -- While

            End -- @@CURSOR_ROWS
      
         Close curRegistro
         Deallocate curRegistro

      -- ******* Fim Cursor *******

      End --not exists
   End -- if @NivelPai <= @MaxNivel

   if(@NivelPai = 1 and @ExibeNivel >-1) 
   Begin
      Select 
         ID,
         IDAfiliado,
         [Login],
         Nome,
         PatrocinadorDiretoID,
         LoginPatrocinadorDireto,
         NomePatrocinadorDireto,
         DataAtivacao,
         DataValidade,
         DerramamentoID,
         AssociacaoID,
         NomeAssociacao,
         Data,
         Nivel
      From 
         Usuario.Afiliados (nolock)
      where 
         ID = @id And
         nivel = Case  
                    When @ExibeNivel > 0 And @ExibeNivel <= @ExibeNivel Then @ExibeNivel
                    When @ExibeNivel = -1 then -1
                    Else nivel
         End
      Order by
         Nivel,
         IdAfiliado
   End
 

End -- Sp

go
Grant Exec on spC_ObtemDiretos To public
go

--Exec spC_ObtemDiretos @id=26,@idUsuario=26,@NivelPai=1,@MaxNivel=6, @ExibeNivel = 0

--select * from Usuario.Usuario where login = 'cdvbrazil1'

--Exec spC_ObtemDiretos @id=26,@idUsuario=26,@NivelPai=1,@MaxNivel=1, @ExibeNivel = 1