USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_FI_RelatorioSaldo]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....  : 
-- Create date  : 
-- Description  : Exibe Relatório de Saldos   
-- Modified by  : Vinicius Castro
-- Modified date: 30/11/2017
-- =============================================================================================

CREATE PROC [dbo].[spOC_FI_RelatorioSaldo]
   @DataIni datetime,
   @DataFim datetime,
   @Status int,
   @login varchar(100) = null,
   @PorAssinatura bit = 0

As
Begin
   
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   declare @loginCompara nvarchar(max) = null
   declare @usuarioID int = 0
   declare @Assinatura nvarchar(max) = ''

   IF @login <> ''
	BEGIN

	    SET @loginCompara = @login
	 
		SELECT 
			@UsuarioID = ID 
		FROM usuario.usuario (nolock) 
		WHERE login  = @Login

		IF @PorAssinatura = 1
			BEGIN

				SELECT 
					@Assinatura = Assinatura 
				FROM usuario.usuario (nolock) 
				WHERE login  = @Login

			END 

	END
	
   Select 
     S.ID AS Codigo,
     S.USUARIOID AS UsuarioID,
     SS.DATA AS Data,
     S.DATA AS DataS,
     S.Total AS Valor,
     SS.StatusID as Status,
     M.Simbolo as Moeda,
     CD.Bitcoin, 
     MP.Descricao TipoPagamento, 
     CD.ID ContaDepositoID 
   INTO 
      #SAQUE 
   From 
     FINANCEIRO.Saque S (nolock)
     INNER JOIN
        FINANCEIRO.SaqueStatus SS (nolock) ON S.ID = SS.SaqueID
     INNER JOIN 
        GLOBALIZACAO.MOEDA M (nolock) ON M.ID = S.MoedaID
     LEFT JOIN 
        Financeiro.ContaDeposito CD (nolock) ON CD.IDUsuario = S.USUARIOID 
     INNER JOIN 
        Financeiro.MeioPagamento MP (nolock) ON CD.IDMeioPagamento = MP.ID 
	INNER JOIN 
		Usuario.Usuario U (nolock) ON S.UsuarioID = U.ID
   Where
     S.DATA >= @DataIni AND 
     S.DATA < @DataFim AND
	 U.Datavalidade > getdate() AND
	 SS.StatusID = @Status AND
     u.Bloqueado = 0 AND
   	 (
		(@PorAssinatura = 0 AND (@UsuarioID = 0 OR U.ID = @UsuarioID)) OR
		(@PorAssinatura = 1 AND U.Assinatura not like (@Assinatura + '%'))
	 ) 

   Select 
      ISNULL(S.Codigo, 0) as Codigo,
      U.PaisID,
      U.DataValidade,
      U.Login as Login,
      U.ID as UsuarioId,
      S.DataS AS Data,
      S.Valor as Valor,
      S.Status as Status,
      ISNULL(S.Moeda, 'R$') as Moeda,
      S.Bitcoin as Bitcoin,
      S.TipoPagamento as TipoPagamento,
      S.ContaDepositoID as ContaDepositoID,
      (Select sum(fl2.[Valor]) from financeiro.Lancamento fl2 (nolock) where U.ID = fl2.[UsuarioID] and fl2.[CategoriaID] IN (13, 14, 15, 16) and fl2.datalancamento < @DataFim) as Ganho,
      isnull((Select sum(fl2.[Valor]) from financeiro.Lancamento fl2 (nolock) where U.ID = fl2.[UsuarioID] and fl2.[CategoriaID] = 7 and fl2.[Valor]> 0  and fl2.datalancamento < @DataFim and fl2.[Referenciaid] <> 1),0) as transferencias,
      isnull(S.Valor,0) * -(1) as saques
    Into 
      #Temp
    FROM
	  #SAQUE S
	  RIGHT JOIN Usuario.Usuario U (nolock) on U.ID = S.UsuarioID
	  WHERE U.Login = ISNULL(@loginCompara, U.Login)

   GROUP BY
      U.ID,
      S.Codigo,
      U.PaisID,
      U.DataValidade,
      S.UsuarioID,
      U.Login,
      S.DataS,
      S.Valor,
      S.Status,
      S.Moeda,
      S.Bitcoin,
      S.TipoPagamento,
      S.ContaDepositoID
   Order by 
      S.Valor 


   Select 
      t.Codigo Codigo,
      p.nome Pais,
      es.nome Estado,
      c.nome Cidade,
	  t.Moeda Moeda,
      t.login Login,
      ISNULL(CONVERT(varchar, t.data, 103), '') Data,
      t.bitcoin Bitcoin,
      ISNULL(t.ganho, 0) Ganho,
      ISNULL(t.saques, 0) * -(1) Saques,
      t.transferencias Transferencias,
	  ISNULL(t.ganho, 0) + ISNULL(t.saques, 0) Valor
   from 
      #temp t,
      Globalizacao.Pais p (nolock),
      usuario.endereco e (nolock),
      globalizacao.cidade c (nolock),
      globalizacao.estado es (nolock)
   where 
      t.paisid = p.id
      and t.usuarioid = e.usuarioid
      and e.cidadeid = c.id
      and e.estadoid = es.id
	  and ISNULL(t.ganho, 0) + ISNULL(t.saques, 0) > 0
   group by
      t.Codigo,
      p.nome,
      es.nome,
      c.nome,
	  t.moeda,
      t.login,
      t.data,
      t.valor,
      t.bitcoin,
      t.ganho,
      t.saques,
      t.transferencias
   order by
      t.ganho,t.Valor

   IF OBJECT_ID('tempdb..#TEMPORARIA2') IS NOT NULL
      Begin
          DROP TABLE #TEMPORARIA2
      End

   IF OBJECT_ID('tempdb..#TEMPORARIA2') IS NOT NULL
      Begin
          DROP TABLE #Temp
      End

End -- Sp


GO
