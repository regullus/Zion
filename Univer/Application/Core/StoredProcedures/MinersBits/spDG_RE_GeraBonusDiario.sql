Use MinersBits
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusDiario'))
   Drop Procedure spDG_RE_GeraBonusDiario
go

-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 25/05/2017
-- Description: Gera registros de Bonificacao Diario 
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusDiario]
   @baseDate varchar(8) null

AS
BEGIN
   BEGIN TRY
   
      set nocount on

      DECLARE @dataInicio datetime;
      DECLARE @dataFim    datetime;
      
      if (@baseDate is null)
      Begin 
         SET @dataInicio = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(CONVERT(VARCHAR(8), dbo.GetDateZion()-1, 112) + ' 23:59:59' as datetime2);
      End
      Else
      Begin
         SET @dataInicio = CAST(@baseDate + ' 00:00:00' as datetime2);
         SET @dataFim    = CAST(@baseDate + ' 23:59:59' as datetime2);
      End

      -- Gera bonus so de segunda a Sexta-feira
      if (DATEPART(weekday, @dataInicio) not in (2,3,4,5,6))
      Begin
          GOTO DONE;
      End
      
      Create Table #tBonus
      (
        UsuarioID        int,
	     NivelAssociacao  int,
        DataValidade     datetime,  
	     VlrBonusDia      float,      
        AcumuladoDia     float,
        AcumuladoGanho   float,     
        LimiteDiario     float,
        LimiteAssociacao float
      );

       Create Table #tBonusAcumuladoDia
      (
        UsuarioID   int,	     
	     vlrBonusDia float
      );

      Declare @fatorBonus float = ISNULL((Select convert(float, Dados) From Sistema.Configuracao (nolock) Where Chave = 'FATOR_BONUS_DIARIO') , 0.0);

      -- Obtem o bonus dos Usuarios Quatificados
      Insert Into #tBonus
      Select 
        U.ID, 
        U.NivelAssociacao, 
        U.DataValidade, 
        dbo.TruncZion((PV.Valor * @fatorBonus),8), 
        0,
        0,
        0,
        0
      From Usuario.Usuario U (nolock)
        INNER JOIN Loja.Produto PR (nolock) ON U.NivelAssociacao = PR.NivelAssociacao
        INNER JOIN Loja.ProdutoValor PV (nolock) ON PV.ProdutoID = PR.ID
      Where U.StatusID = 2  -- ativos e pagos 
        and U.GeraBonus = 1
        and U.DataValidade >= @dataInicio --so recebe se estiver ativo 
        and PR.TipoID IN (1);

      -- Obtem o Bonus Acumulado do Dia
      Insert Into #tBonusAcumuladoDia
      Select 
        B.UsuarioID, 
        dbo.TruncZion(Sum(B.Valor),8)
      From Rede.Bonificacao B (nolock)
      where B.Data BETWEEN @dataInicio and @dataFim
      Group By B.UsuarioID; 

      -- Calcula o novo Acumulado do Dia
      Update #tBonus
      Set AcumuladoDia = A.VlrBonusDia
      From #tBonus T , 
           #tBonusAcumuladoDia A
      where T.UsuarioID = A.UsuarioID;

      -- Obtem o Acumulado do usuario (Total de Ganho)
      Update #tBonus
      Set AcumuladoGanho = UG.AcumuladoGanho       
      From #tBonus T, 
           Usuario.UsuarioGanho UG (nolock)
      where T.UsuarioID    = UG.UsuarioID
        and T.DataValidade = UG.DataFim;

      -- Obtem o Limite de Ganho do Dia
      Update #tBonus
      Set LimiteDiario = ALG.Valor
      From #tBonus T,
           Rede.AssociacaoLimiteGanho ALG (nolock)
      where ALG.NivelAssociacao = T.NivelAssociacao
        and ALG.TipoID = 3; -- Tipo Diario

      -- Obtem o Limite de Ganho da Associacao
      Update #tBonus
      Set LimiteAssociacao = ALG.Valor
      From #tBonus T,
           Rede.AssociacaoLimiteGanho ALG (nolock)
      where ALG.NivelAssociacao = T.NivelAssociacao
        and ALG.TipoID = 1; -- Tipo Associacao

      -- Verifica o Limite Diario de Bonus
      Update #tBonus 
      Set VlrBonusDia = (LimiteDiario - AcumuladoDia) 
      Where VlrBonusDia + AcumuladoDia > LimiteDiario;

      -- Delete os Usuarios que não tem Bonus
      Delete #tBonus
      From #tBonus     
      Where VlrBonusDia <= 0;

      -- Verifica o Limite de Ganho da Associacao
      Update #tBonus 
      Set VlrBonusDia = (LimiteAssociacao - AcumuladoGanho) 
      Where VlrBonusDia + AcumuladoGanho > LimiteAssociacao;

       -- Delete os Usuarios que não tem Bonus
      Delete #tBonus
      From #tBonus     
      Where VlrBonusDia <= 0;

      BEGIN TRANSACTION

      -- Gera Bonificacao
      Insert Into Rede.Bonificacao 
         (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
      Select 
         '21' as Categoria, UsuarioID, 0, 0, @dataFim, dbo.TruncZion(VlrBonusDia, 8) 
      From #tBonus;    
   
       -- Atualiza Total de Ganho de Associacao
      Update  Usuario.UsuarioGanho
      Set AcumuladoGanho =  dbo.TruncZion((UG.AcumuladoGanho + T.VlrBonusDia),8)      
      From #tBonus T, 
           Usuario.UsuarioGanho UG
      where T.UsuarioID    = UG.UsuarioID
        and T.DataValidade = UG.DataFim;

      COMMIT TRANSACTION

      -- Select * From #tBonus Where VlrBonusNew > 0

      drop table #tBonus
      drop table #tBonusAcumuladoDia

      DONE:

   END TRY

   BEGIN CATCH     
      DECLARE @error int, @message varchar(4000), @xstate int;
      SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
      RAISERROR ('Erro na execucao de spDG_RE_GeraBonusDiario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
   END CATCH  
END

--exec spDG_RE_GeraBonusDiario null
-- Select * from Rede.Bonificacao order by id desc


