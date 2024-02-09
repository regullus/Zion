SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description:  Gera registros de Bonificacao residual (pagamentos de ativo mensal)
-- =============================================================================================

IF OBJECT_ID('dbo.sp_JB_GeraBonusResidual', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[sp_JB_GeraBonusResidual];
GO

Create Proc [dbo].[sp_JB_GeraBonusResidual]   (@baseDateIni varchar(8), @baseDateFim varchar(8))
AS
BEGIN
    BEGIN TRY
        SET NOCOUNT ON;

        DECLARE @RegraID_Residual INT = 2
		DECLARE @CategoriaBonificacaoID INT = 23     -- Bônus Residual - Atividade Mensal 
		DECLARE @Qtde_Max_Niveis INT = 100
		DECLARE @Tipo_Produto_Ativo_Mensal INT = 5
		DECLARE @Bonus_Qtde_Maxima_Niveis_Pagos	INT = 7
		DECLARE @Valor_Bonus_Por_Nivel float = 3

		DECLARE @dataInicio datetime;
		DECLARE @dataFim datetime;

		SET @dataInicio = CAST(@baseDateIni + ' 00:00:00' as datetime2);
		SET @dataFim = CAST(@baseDateFim + ' 23:59:59' as datetime2);

		declare @CicloID int
		set @CicloID = (select ID from Rede.Ciclo where @dataFim between DataInicial and DataFinal)
		
		IF(EXISTS(SELECT 1 FROM Rede.BonificacaoExecucao where CategoriaBonusID = @CategoriaBonificacaoID and DataExecucao = cast(@dataFim as date)))
		BEGIN
			print 'este bonus ja foi executado para este dia. Altere a data do processamento, ou remova a trava em Rede.BonificacaoExecucao'
			RETURN
		END

		
        CREATE TABLE #TAtvMensal
        (
            UsuarioID INT,
            DataAtivacao DATETIME,
            Pontos FLOAT
        );
		
		CREATE TABLE #TempNiveis
		(
			UsuarioID INT,
			DiretoID INT,
			NivelDireto INT,
			AtivoMensal BIT,
			ValorBonus FLOAT
		);		

        INSERT INTO #TAtvMensal
        SELECT Ped.UsuarioID AS [UsuarioID],
               MIN(PPAGST.Data) AS [DataAtivacao],
               SUM(Item.ValorUnitario) AS [Pontos]
        FROM Loja.Pedido Ped (NOLOCK)
            INNER JOIN Loja.PedidoItem Item (NOLOCK)
                ON Item.PedidoID = Ped.ID
            INNER JOIN Loja.Produto PRd (NOLOCK)
                ON PRd.ID = Item.ProdutoID
			INNER JOIN Loja.PedidoPagamento PPAG ON Ped.ID = PPAG.PedidoID
			INNER JOIN Loja.PedidoPagamentoStatus PPAGST ON PPAG.ID = PPAGST.PedidoPagamentoID
							
            INNER JOIN Usuario.Usuario Usr (NOLOCK)
                ON Usr.ID = Ped.UsuarioID
        WHERE PRd.TipoID IN ( @Tipo_Produto_Ativo_Mensal ) /*ativo mensal*/
			AND PPAGST.StatusID = 3 /*pago*/
              AND PPAGST.Data BETWEEN @dataInicio AND @dataFim 
              AND ISNULL(UPPER(Ped.Observacoes),'') NOT LIKE 'CANCELAD%'
        GROUP BY Ped.UsuarioID
        --HAVING SUM(Item.VQ) >= 200; /*não há minimo de pontos para ativo mensal*/

		
		INSERT INTO #TempNiveis
		SELECT ATV.UsuarioID,
			   Uplines.UserID,
			   Uplines.Nivel,
			   0,
			   0
		FROM #TAtvMensal ATV
			CROSS APPLY [fn_Get_Uplines_Diretos](ATV.UsuarioID, @Qtde_Max_Niveis) Uplines;
		
		
		/*remove quem nao esta ativo*/
		UPDATE #TempNiveis
		SET AtivoMensal = 1
		FROM #TempNiveis
			INNER JOIN Usuario.AtivacaoMensal Atv
				ON #TempNiveis.DiretoID = Atv.UsuarioID
		WHERE Atv.CicloID = @CicloID;		
		
		
		UPDATE #TempNiveis SET ValorBonus = 0 where AtivoMensal = 0
		
		UPDATE #TempNiveis SET ValorBonus = 0 from #TempNiveis 
		inner join Usuario.Usuario U on #TempNiveis.usuarioID = U.ID 
		where U.StatusID = 2 And U.NivelAssociacao > 0
		and U.RecebeBonus = 1
		and U.Bloqueado = 0
		and U.DataRenovacao >= getdate()

		
        SELECT DISTINCT
            #TAtvMensal.UsuarioID AS [UsuarioID],
            Upl.DiretoID AS [Upline],
            DENSE_RANK() OVER (PARTITION BY #TAtvMensal.UsuarioID ORDER BY Upl.NivelDireto ASC) AS [Nivel],
            CASE
                WHEN AtvMes.ID IS NOT NULL THEN
                    DENSE_RANK() OVER (PARTITION BY #TAtvMensal.UsuarioID,
                                                    CASE
                                                        WHEN AtvMes.ID IS NOT NULL THEN
                                                            1
                                                        ELSE
                                                            0
                                                    END
                                       ORDER BY Upl.NivelDireto ASC
                                      )
                ELSE
                    NULL
            END AS [NivelPago],
			0 as ValorBonus
        INTO #TUpline
        FROM #TAtvMensal
            INNER JOIN #TempNiveis Upl (NOLOCK)
                ON Upl.UsuarioID = #TAtvMensal.UsuarioID
            INNER JOIN Usuario.Usuario UsUp
                ON Upl.DiretoID = UsUp.ID
            LEFT JOIN Usuario.AtivacaoMensal AtvMes (NOLOCK)
                ON AtvMes.UsuarioID = Upl.DiretoID
                   AND AtvMes.CicloID = @CicloID

				   
		update #TUpline set ValorBonus = @Valor_Bonus_Por_Nivel 
		where NivelPago IS NOT NULL
		and Nivel <= @Bonus_Qtde_Maxima_Niveis_Pagos
		

				   
        DECLARE C_REGRAITEM CURSOR FOR
        SELECT Nivel,
               RegraItem.ID,
               Regra,
               ClassificacaoID
        FROM Rede.RegraItem (NOLOCK)
        WHERE RegraID = @RegraID_Residual
              AND Ativo = 1
        ORDER BY Nivel DESC,
                 ClassificacaoID ASC;

        DECLARE @Nivel INT,
                @RegraItemID INT,
                @Regra VARCHAR(128),
                @ClassificacaoID INT;

        OPEN C_REGRAITEM;
        FETCH NEXT FROM C_REGRAITEM
        INTO @Nivel,
             @RegraItemID,
             @Regra,
             @ClassificacaoID;

        WHILE (@@FETCH_STATUS = 0)
        BEGIN

            BEGIN TRANSACTION;

            INSERT INTO Rede.Bonificacao
            (
                CategoriaID,
                UsuarioID,
                ReferenciaID,
                StatusID,
                Data,
                Valor,
                PedidoID,
                Descricao,
                RegraItemID,
                CicloID
            )
            SELECT @CategoriaBonificacaoID AS [CategoriaID],
                   Upl.Upline AS [UsuarioID],
                   T.UsuarioID AS [ReferenciaID],
                   0 AS [StatusID],
                   T.DataAtivacao AS [Data],
                   CONVERT(DECIMAL(18, 2), @Regra) AS [Valor],
                   NULL AS [PedidoID],
                   '[NM1]' + (ISNULL(
                              (
                                  SELECT UsuN1.Nome
                                  FROM #TempNiveis UplN1 (NOLOCK)
                                      INNER JOIN Usuario.Usuario UsuN1 (NOLOCK)
                                          ON UsuN1.ID = UplN1.DiretoID
                                      INNER JOIN Usuario.Usuario Pat (NOLOCK)
                                          ON Pat.ID = UplN1.DiretoID
                                  WHERE UplN1.UsuarioID = T.UsuarioID
                                        AND Pat.PatrocinadorDiretoID = Upl.Upline
                              ),
                              (
                                  SELECT Nome FROM Usuario.Usuario (NOLOCK) WHERE ID = Upl.Upline
                              )
                                    )
                             ) + '|[N]' + CONVERT(VARCHAR, Upl.Nivel) + '|[NP]' + CONVERT(VARCHAR, Upl.NivelPago)
                   + '|[PT]' + CONVERT(VARCHAR, CONVERT(INT, ROUND(T.Pontos, 0))) + '|[NM]' + Base.Nome AS [Descricao],
                   @RegraItemID AS [RegraItemID],
                   @CicloID AS [CicloID]
            FROM #TAtvMensal T
                INNER JOIN Usuario.Usuario Base (NOLOCK)
                    ON Base.ID = T.UsuarioID
                INNER JOIN #TUpline Upl
                    ON Upl.UsuarioID = T.UsuarioID
                LEFT JOIN Usuario.UsuarioClassificacao UsrClf
                    ON UsrClf.UsuarioID = Upl.Upline
                       AND UsrClf.CicloID = @CicloID
                LEFT JOIN Rede.Classificacao Clf
                    ON Clf.Nivel = UsrClf.NivelClassificacao
            WHERE Upl.NivelPago IS NOT NULL
                  AND Upl.Nivel = (@Nivel * -1)

            COMMIT TRANSACTION;

            FETCH NEXT FROM C_REGRAITEM
            INTO @Nivel,
                 @RegraItemID,
                 @Regra,
                 @ClassificacaoID;
        END;

        CLOSE C_REGRAITEM;
        DEALLOCATE C_REGRAITEM;


        -- Remove todas as tabelas temporárias
        DROP TABLE #TUpline;
        DROP TABLE #TAtvMensal;
		DROP TABLE #TempNiveis
		
		insert into Rede.BonificacaoExecucao values (@CategoriaBonificacaoID, cast(@dataFim as date), getdate())

    END TRY
    BEGIN CATCH
        IF @@Trancount > 0
            ROLLBACK TRANSACTION;

        DECLARE @error INT,
                @message VARCHAR(4000),
                @xstate INT;
        SELECT @error = ERROR_NUMBER(),
               @message = ERROR_MESSAGE();
        RAISERROR('Erro na execucao de sp_JB_GeraBonusResidual: %d: %s', 16, 1, @error, @message) WITH SETERROR;
    END CATCH;
END;

GO
