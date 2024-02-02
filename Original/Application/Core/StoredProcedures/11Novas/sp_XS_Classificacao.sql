SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_XS_Classificacao]
AS
BEGIN

Select PontosBinario.UsuarioID,
 Sum(Case When ( PontosBinario.Lado = 0 ) Then PontosBinario.Pontos Else 0 End) PtosEsquerda,
 Sum(Case When ( PontosBinario.Lado = 1 ) Then PontosBinario.Pontos Else 0 End) PtosDireita
Into #PtosCicloAtual
From Rede.PontosBinario (Nolock)
Group BY PontosBinario.UsuarioID

Select Posicao.UsuarioID,
 ValorPernaEsquerda = ValorPernaEsquerda + PtosEsquerda,
 ValorPernaDireita = ValorPernaDireita + PtosDireita
Into #RedePosicao
From Rede.Posicao
Inner Join #PtosCicloAtual Ptos On Ptos.UsuarioID = Posicao.UsuarioID

Select Posicao.UsuarioID,
 Case When (ValorPernaEsquerda < ValorPernaDireita) Then ValorPernaEsquerda Else ValorPernaDireita End PernaMenor
Into #UsuariosClassificacao
From #RedePosicao Posicao
Where Posicao.ValorPernaEsquerda > 0
 And Posicao.ValorPernaDireita > 0

Select Clf.UsuarioID,
 Classificacao.Nivel,
 Rank() Over (Partition By Clf.UsuarioID Order By Classificacao.Nivel Desc) Ordem
Into #ClasfificaoMaxima
From #UsuariosClassificacao Clf
Inner Join Rede.Classificacao (nolock) On Classificacao.Pontos <= Clf.PernaMenor

-- Checar se tem um direto em cada perna c 1 nivel abaixo do dele, caso for nivel acima de bronze
-- Lado esquerdo
Select Usuario.ID, Diretos.ID QualificadorEsquedo,
 rank() over (partition by Diretos.ID order by Diretos.DataCriacao) Ordem
Into #QualificadorEsquerdo
From Usuario.Usuario (nolock)
Inner Join #UsuariosClassificacao Clf On Clf.UsuarioID = Usuario.ID
Inner Join #ClasfificaoMaxima ClfMax On ClfMax.UsuarioID = Usuario.ID
									And ClfMax.Ordem = 1
Inner join Usuario.Usuario (nolock) Diretos on Diretos.PatrocinadorDiretoID = Usuario.ID
					and (Diretos.NivelAssociacao = ClfMax.Nivel - 1 Or ClfMax.Nivel = 0)
					and Diretos.Assinatura <> ''
Where Usuario.ID > 2588
 and Usuario.Assinatura <> ''
 and left(Diretos.Assinatura, len(Usuario.Assinatura) + 1) = Usuario.Assinatura + '0'

 -- Lado direito
Select Usuario.ID, Diretos.ID QualificadorDireito,
 rank() over (partition by Diretos.ID order by Diretos.DataCriacao) Ordem
Into #QualificadorDireito
From Usuario.Usuario (nolock)
Inner Join #UsuariosClassificacao Clf On Clf.UsuarioID = Usuario.ID
Inner Join #ClasfificaoMaxima ClfMax On ClfMax.UsuarioID = Usuario.ID
									And ClfMax.Ordem = 1
Inner join Usuario.Usuario (nolock) Diretos on Diretos.PatrocinadorDiretoID = Usuario.ID
					and (Diretos.NivelAssociacao = ClfMax.Nivel - 1 Or ClfMax.Nivel = 0)
					and Diretos.Assinatura <> ''
Where Usuario.ID > 2588
 and Usuario.Assinatura <> ''
 and left(Diretos.Assinatura, len(Usuario.Assinatura) + 1) = Usuario.Assinatura + '1'

Update Usuario.Usuario
Set NivelClassificacao = Clf.Nivel
From Usuario.Usuario
Inner Join #ClasfificaoMaxima Clf On Clf.UsuarioID = Usuario.ID
Where Clf.Ordem = 1

Insert Into Usuario.UsuarioClassificacao (UsuarioID, NivelClassificacao, Data, NivelReconhecimento, CicloID)
Select Clf.UsuarioID, Clf.Nivel, dbo.GetDateZion(), 0, Null
From #ClasfificaoMaxima Clf
Where Clf.Ordem = 1

END
GO