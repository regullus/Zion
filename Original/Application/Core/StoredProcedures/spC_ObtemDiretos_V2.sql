USE [SRMRACCO]
GO
/****** Object:  StoredProcedure [dbo].[spC_ObtemDiretos_V2]    Script Date: 12/07/2017 12:12:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Gera registro de Afiliados ate o nivel informado retornando os dados
-- =============================================================================================

-- spC_ObtemDiretos_V2 1000, 1000, 1, 7, 1

ALTER  Proc [dbo].[spC_ObtemDiretos_V2]
   @id int,
   @idUsuario int,
   @NivelPai int,
   @MaxNivel int,
   @ExibeNivel int = null,
   @UsuarioAtivo int = null
As
BEGIN

   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   declare @Data datetime,
           @DataOntem datetime,
           @Continua int,
		   @ClassificacaoInicial varchar(256)

   Select @Data = dbo.GetDateZion(), @Continua = 1
   
   --Verifica se há dados da ultima hora
   set @DataOntem = DATEADD(minute,-59,@Data); 
      
   if(@NivelPai = 1 and @Continua = 1) 
   Begin
     If Exists (Select 'x' From Usuario.Afiliados_V2 (nolock) Where ID = @id and Data > @DataOntem)
      Begin
         Set @Continua = 0
      End
      else
      Begin
         delete Usuario.Afiliados_V2 where id = @id
         set @idUsuario = @id
      End
   End
         
   if(@NivelPai <= @MaxNivel and @Continua = 1)
   Begin  
     if exists ( --Verifica se há afiliados para o dado patrocinador
     Select 
        'ok'
      from 
         usuario.Usuario (nolock) usu
      where 
         usu.PatrocinadorDiretoID = @idUsuario and
         (@UsuarioAtivo is null or
         [dbo].GetStatusAtivoUsuario(usu.ID) = @UsuarioAtivo) and
         --usu.StatusID = 2 and --Ativos
         usu.id not in (Select IdAfiliado from Usuario.Afiliados_V2 where ID = @id And Nivel = @NivelPai) --Verifica se afiliado já foi incluido na tabela
     )
     Begin

		Select Top 1 @ClassificacaoInicial = Nome
		From Rede.Classificacao (nolock)
		Where Nivel = 0

         -- Obtem Primeiro nivel
         Insert into Usuario.Afiliados_V2
         Select 
            @id ID,
            usu.ID IdAfiliado,
            usu.[Login],
            usu.Nome,
            usu.PatrocinadorDiretoID,
            null LoginPatrocinadorDireto,
            null NomePatrocinadorDireto,
            0 Ativo,
			0 VP,
			0 VPP,
			0 VPE,
			0 VPM,
			0 VT,
			0 VQ,
            null ClassificacaoID,
            @ClassificacaoInicial NomeClassificacao,
            @Data,
            @NivelPai Nivel
         from 
            usuario.Usuario    usu
         where 
            usu.PatrocinadorDiretoID = @idUsuario and
			usu.ID <> @idUsuario and -- Retira TOPO
            (@UsuarioAtivo is null or
			[dbo].GetStatusAtivoUsuario(usu.ID) = @UsuarioAtivo) and
            usu.id not in (Select IdAfiliado from Usuario.Afiliados_V2 (nolock) where ID = @id And Nivel = @NivelPai)

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
            afi.ClassificacaoID = usu.NivelClassificacao,
            afi.NomeClassificacao = ass.Nome
         From
            Usuario.Afiliados_V2 afi,
            Usuario.UsuarioClassificacao usu,
            Rede.Classificacao ass           
         Where
            afi.IDAfiliado = usu.UsuarioID and
            ass.ID = usu.NivelClassificacao and
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
                     Exec spC_ObtemDiretos_V2 @id, @IDAfiliado, @ProximoNivel, @MaxNivel, @ExibeNivel, @UsuarioAtivo
                  End

                  --Proxima linha do cursor
                  Fetch Next From curRegistro Into 
                     @IDAfiliado,
                     @Nivel
    
                  -- Para ver se nao é fim do loop
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
         Ativo,
         VP,
         VPP,
         VPE,
		 VPM,
		 VT,
		 VQ,
         NomeClassificacao,
         Data,
         Nivel,
		ISNULL(STUFF((
			SELECT 
				', ' + CAST(UE.ExternoID AS NVARCHAR)
			FROM 
				Usuario.Externo UE (NOLOCK)
			WHERE 
				UE.UsuarioID = IDAfiliado
			FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)') 
		 ,1 ,2 , ''), '') AS ExternoIDs
      From 
         Usuario.Afiliados_V2 (nolock)
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


