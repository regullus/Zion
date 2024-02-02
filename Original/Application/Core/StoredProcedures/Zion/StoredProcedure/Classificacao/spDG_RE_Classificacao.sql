Use Zion
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_Classificacao'))
   Drop Procedure spDG_RE_Classificacao
go

-- =============================================================================================
-- Author.....: 
-- Create date: 24/08/2020
-- Description: Gera registro de Classificação dos usuarios baseadoem seus pontos binarios
-- =============================================================================================

Create  Proc [dbo].[spDG_RE_Classificacao]
 
As
BEGIN
    BEGIN TRY
   
        --Necessario para o entity reconhecer retorno de select com tabela temporaria
        Set FMTONLY OFF
        SET NOCOUNT ON

        DECLARE 
            @UsuarioRootID INT = 1000;

        Select 
            PB.UsuarioID,
            Sum(Case When ( PB.Lado = 0 ) Then PB.Pontos Else 0 End) PtosEsquerda,
            Sum(Case When ( PB.Lado = 1 ) Then PB.Pontos Else 0 End) PtosDireita
        Into 
            #PontosCicloAtual
        From 
            Rede.PontosBinario (NOLOCK) PB
        Group BY 
            PB.UsuarioID;
 
        Select 
            P.UsuarioID,
            ValorPernaEsquerda = ValorPernaEsquerda + PtosEsquerda,
            ValorPernaDireita  = ValorPernaDireita  + PtosDireita
        Into 
            #RedePosicao
        From 
            Rede.Posicao                 P (NOLOCK)
            Inner Join #PontosCicloAtual T On T.UsuarioID = P.UsuarioID;

        Select Posicao.UsuarioID,
            Case When (ValorPernaEsquerda < ValorPernaDireita) 
                Then ValorPernaEsquerda 
                Else ValorPernaDireita 
            End PernaMenor
        Into 
            #UsuariosClassificacao
        From 
            #RedePosicao Posicao
        Where 
            Posicao.ValorPernaEsquerda > 0
        And Posicao.ValorPernaDireita > 0

        Select 
            Clf.UsuarioID,
            Classificacao.Nivel,
            Rank() Over (Partition By Clf.UsuarioID Order By Classificacao.Nivel Desc) Ordem
        Into 
            #ClasfificaoMaxima
        From 
            #UsuariosClassificacao Clf
            Inner Join Rede.Classificacao (NOLOCK) On Classificacao.Pontos <= Clf.PernaMenor;

        -- Checar se tem um direto em cada perna c 1 nivel abaixo do dele, caso for nivel acima de bronze
        -- Lado esquerdo
        Select 
            Usuario.ID, 
            Diretos.ID QualificadorEsquedo,
            rank() over (partition by Diretos.ID order by Diretos.DataCriacao) Ordem
        Into 
            #QualificadorEsquerdo
        From 
            Usuario.Usuario (           NOLOCK)
            Inner Join #UsuariosClassificacao   Clf     On Clf.UsuarioID = Usuario.ID
            Inner Join #ClasfificaoMaxima       ClfMax  On ClfMax.UsuarioID = Usuario.ID 
                                                           And ClfMax.Ordem = 1
            Inner join Usuario.Usuario (NOLOCK) Diretos on Diretos.PatrocinadorDiretoID = Usuario.ID
    					                                   and (Diretos.NivelAssociacao = ClfMax.Nivel - 1 Or ClfMax.Nivel = 0)
    					                                   and Diretos.Assinatura <> ''
        Where 
            Usuario.ID > @UsuarioRootID
        and Usuario.Assinatura <> ''
        and left(Diretos.Assinatura, len(Usuario.Assinatura) + 1) = Usuario.Assinatura + '0';

        -- Lado direito
        Select 
            Usuario.ID, 
            Diretos.ID QualificadorDireito,
            rank() over (partition by Diretos.ID order by Diretos.DataCriacao) Ordem
        Into 
            #QualificadorDireito
        From 
            Usuario.Usuario            (nolock)
            Inner Join #UsuariosClassificacao   Clf     On Clf.UsuarioID = Usuario.ID
            Inner Join #ClasfificaoMaxima       ClfMax  On ClfMax.UsuarioID = Usuario.ID 
    									                   And ClfMax.Ordem = 1
            Inner join Usuario.Usuario (nolock) Diretos on Diretos.PatrocinadorDiretoID = Usuario.ID
		    			                                   and (Diretos.NivelAssociacao = ClfMax.Nivel - 1 Or ClfMax.Nivel = 0)
	    				                                   and Diretos.Assinatura <> ''
        Where 
            Usuario.ID > @UsuarioRootID
        and Usuario.Assinatura <> ''
        and left(Diretos.Assinatura, len(Usuario.Assinatura) + 1) = Usuario.Assinatura + '1';

        Update 
            Usuario.Usuario
        Set 
            NivelClassificacao = Clf.Nivel
        From 
            Usuario.Usuario
            Inner Join #ClasfificaoMaxima Clf On Clf.UsuarioID = Usuario.ID
        Where 
            Clf.Ordem = 1;

        Insert Into Usuario.UsuarioClassificacao (
            UsuarioID, 
            NivelClassificacao, 
            Data, 
            NivelReconhecimento, 
            CicloID
        )
        Select 
            Clf.UsuarioID, 
            Clf.Nivel, 
            dbo.GetDateZion(), 
            0, 
            Null
        From 
            #ClasfificaoMaxima Clf
        Where 
            Clf.Ordem = 1;
    END TRY
    BEGIN CATCH
        IF @@Trancount > 0
            ROLLBACK TRANSACTION;

        DECLARE @error INT, @message VARCHAR(4000), @xstate INT;
        SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
        RAISERROR('Erro na execucao de spDG_RE_Classificacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
    END CATCH;

END -- Sp

go
Grant Exec on spDG_RE_Classificacao To public
go

