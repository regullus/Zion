Use Nextter
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('uspMG_US_GeraClassificacaoUsuario'))
   Drop Procedure uspMG_US_GeraClassificacaoUsuario
go

Create PROCEDURE [dbo].[uspMG_US_GeraClassificacaoUsuario]
   @baseDate varchar(8) = null

AS
-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 22/03/2018
-- Description: Gera registros de Classificacao e Reconhecimento por usuario
-- =============================================================================================

BEGIN TRY
   
   set nocount on

   Declare 
      @dataProcess datetime,
      @dataInicio  datetime,
      @dataFim     datetime;
      
   if (@baseDate is null)
   Begin 
      SET @baseDate = CONVERT(VARCHAR(8), dbo.GetDateZion(), 112);
   End;

   Set @dataProcess = CAST(CONVERT(VARCHAR(8), DATEADD(MM,-0,DATEADD(DD,-DAY(@baseDate)+1,@baseDate)) , 112) + ' 00:00:00' as datetime);
   Set @dataInicio  = CAST(CONVERT(VARCHAR(8), DATEADD(MM,-1,DATEADD(DD,-DAY(@baseDate)+1,@baseDate)) , 112) + ' 00:00:00' as datetime);
   Set @dataFim     = CAST(CONVERT(VARCHAR(8), DATEADD(DD,-DAY(@baseDate),@baseDate)                  , 112) + ' 23:59:59' as datetime);

   -- Executa de Segunda a Sexta-feira
   --if (DATEPART(weekday, @dataInicio) in (2,3,4,5,6))
   --Begin
   --    GOTO DONE;
   --End

   -- Evita duplicidade de classificação caso a rotina seja executada mais de uma vez no mes
   If Exists (Select Top(1) 'Ok' From Usuario.UsuarioClassificacao (nolock) Where Data = @dataProcess)
   Begin
       GOTO DONE;
   End;
      
   Create Table #TUsuarios  
   (          
      ID             Int,
      Qtde           Int,
      Volume         Float,
      LinhasQualif   Int,
	  Reconhecimento Int
   );  

   -- Monta a rede para a analise
   Select 
      P.ID  as UsuPaiID,
      F.ID  as UsuFilhoID
   Into 
      #TRede
   From 
      Usuario.Usuario P (nolock),
      Usuario.Usuario F (nolock)
   Where F.Assinatura like P.Assinatura + '%' 
     and F.Assinatura  > P.Assinatura 
     and F.GeraBonus = 1 -- Somente ususarios que geram bonus 1= sim, 0 = nao  
     and F.Bloqueado = 0 -- somente usuarios que nao estejam bloqueados 1= sim, 0 = nao 
     and F.DataValidade >= @dataFim; -- so recebe se estiver com ativo mensal pago    	 
	  	  
   -- Apura a quantidade de associados filhos ativos
   Insert Into 
      #TUsuarios  	 
   Select 
      R.UsuPaiID        as ID,
      Count(R.UsuPaiID) as Qtde,
      0                 as Volume,
      0                 as LinhasQualif,
	  0                 as Reconhecimento
   From 
      #TRede R	 
   Group By 
      R.UsuPaiID;

   -- Apura o volume de Ativo Mensal pagos no mes        
   Select            
      R.UsuPaiID    as ID,                   
      Sum(PV.Valor) as Volume	 
   Into 
      #TVolume           
   From   	     	
      #TRede R,
      Loja.pedido P (nolock),   
      Loja.PedidoItem PI (nolock),   
      Loja.Produto PR (nolock),          
      Loja.ProdutoValor PV (nolock),       
      Loja.PedidoPagamento PP (nolock),   
      Loja.PedidoPagamentoStatus PS (nolock)     
   Where P.ID = PI.PedidoID       
     and PI.ProdutoID = PR.ID  
     and PR.TipoID IN (5)  -- Ativo Mensal  
     and PI.ProdutoID = PV.ProdutoID  
     and P.UsuarioID = R.UsuFilhoID   
     and P.ID = PP.PedidoID  
     and PP.ID = PS.PedidoPagamentoID  
     and PS.StatusID = 3 -- somente pedidos pagos                          
     and PS.Data BETWEEN @dataInicio AND @dataFim -- pega somente pedidos do mes 
   Group By 
     R.UsuPaiID;

   -- Atualiza Volume na tabela temporaria de usuario (principal)
   Update
      #TUsuarios
   Set 
      Volume = V.Volume
   From 
      #TUsuarios U,
      #TVolume   V
   Where U.ID = V.ID;

   -- Busca a ultima Classificação dos usuarios
   Select 
      C.ID , 
      C.Data as Data, 
      C.NivelClassificacao as NivelClassificacao,
      C.NivelReconhecimento as NivelReconhecimento
   Into 
      #TUltimaClassificacao
   From 
      Usuario.UsuarioClassificacao C (nolock) 
   Where 
      C.Data = @dataInicio;

   -- Atualiza o Reconhecimento na tabela temporaria de usuario (principal)
   Update
      #TUsuarios
   Set 
      Reconhecimento = C.NivelReconhecimento
   From 
      #TUsuarios U,
      #TUltimaClassificacao C
   Where U.ID = C.ID;
	
   -- Apura a quantidade de linhas Qualificadas  	 
   Select 
      R.UsuPaiID                  as ID,
      Max(C.NivelClassificacao)   as NivelClassificacao,		 
      Count(C.NivelClassificacao) as Qtde,
      Max(C.NivelClassificacao) * 1000000 + Count(C.NivelClassificacao) as Faixa
   Into 
      #TLinhas
   From 
      #TRede R,
      #TUltimaClassificacao C
   Where R.UsuFilhoID = C.ID	  	    
   Group By 
      R.UsuPaiID;

   -- Atualiza Linhas Qualificadas na tabela temporaria de usuario (principal)
   Update
      #TUsuarios
   Set 
      LinhasQualif = L.Faixa
   From 
      #TUsuarios U,
      #TLinhas   L
   Where U.ID = L.ID;
	 
   -- Classifica Usuario por quantidade de Cadastrados
   Select
      T.ID,
      'Quantidade' as Tipo,
      Case
         When T.Qtde >=  10 and T.Qtde <  20 THEN 1
         When T.Qtde >=  20 and T.Qtde <  30 THEN 2
         When T.Qtde >=  30 and T.Qtde <  50 THEN 3
         When T.Qtde >=  50 and T.Qtde <  80 THEN 4
         When T.Qtde >=  80 and T.Qtde < 100 THEN 5
         When T.Qtde >= 100 and T.Qtde < 200 THEN 6				
         When T.Qtde >= 200                  THEN 7				
         Else 0
      End as NivelClassificacao,
	  T.Reconhecimento as Reconhecimento
   Into 
      #TUsuarioClassificacao
   From 
      #TUsuarios T;

   -- Classifica Usuario por volume de venda
   Insert Into 
      #TUsuarioClassificacao
   Select
      T.ID,
      'Volume' as Tipo,
      Case
         When T.Volume >=   50000.00 and T.Volume <  100000.00 THEN 1
         When T.Volume >=  100000.00 and T.Volume <  250000.00 THEN 2
         When T.Volume >=  250000.00 and T.Volume <  500000.00 THEN 3
         When T.Volume >=  500000.00 and T.Volume < 1000000.00 THEN 4
         When T.Volume >= 1000000.00 and T.Volume < 2000000.00 THEN 5
         When T.Volume >= 2000000.00 and T.Volume < 3000000.00 THEN 6				
         When T.Volume >= 3000000.00                           THEN 7				
         Else 0
      End as NivelClassificacao,
	  T.Reconhecimento as Reconhecimento	  
   From 
      #TUsuarios T;

   -- Classifica Usuario por Linhas Qualificadas
   Insert Into 
      #TUsuarioClassificacao
   Select
      T.ID,
      'Linha' as Tipo,
      Case
         When T.LinhasQualif >= 1000001 and T.LinhasQualif < 1000002 THEN 2
         When T.LinhasQualif >= 1000002 and T.LinhasQualif < 2000002 THEN 3
         When T.LinhasQualif >= 2000002 and T.LinhasQualif < 2000003 THEN 4
         When T.LinhasQualif >= 2000003 and T.LinhasQualif < 2000004 THEN 5
         When T.LinhasQualif >= 2000004 and T.LinhasQualif < 3000004 THEN 6			
         When T.LinhasQualif >= 3000004                              THEN 7				
         Else 0
      End as NivelClassificacao,
	  T.Reconhecimento as Reconhecimento
   From 
      #TUsuarios T;

   -- Inclui 0 Nivel de Classificao do usuario
   Insert Into 
      Usuario.UsuarioClassificacao
         (UsuarioID,
          Data,
          NivelClassificacao,
          NivelReconhecimento)
      Select 
         T.ID,
         @dataProcess as Data,
         Min(T.NivelClassificacao),
         T.Reconhecimento as Reconhecimento
      From
         #TUsuarioClassificacao T
      Group By
         T.ID;

   -- Apura e grava o novo Reconhecimento do Usuario
   Declare
      @AntFetch  Int,
	  @AntID Int,
	  @AntUsuarioID Int,
	  @AntReconhecimento int,
	  @NovoReconhecimento int,
	  @Niveis NVARCHAR(10);

   --Cursor
   Declare
	  @curID Int,
	  @curUsuarioID Int,
      @curData DateTime,
      @curNivelClassificacao  Int,
      @curNivelReconhecimento Int;

   Declare 
      @dataProcess6M datetime = CAST(CONVERT(VARCHAR(8), DATEADD(MM,-6,DATEADD(DD,-DAY(@dataProcess)+1,@dataProcess)) , 112) + ' 00:00:00' as datetime);

   Declare
      CursorUsuClass
   Cursor For
      Select 
	     UC.ID,
         UC.UsuarioID,
         UC.Data,
         UC.NivelClassificacao,
         UC.NivelReconhecimento
      From 
	     Usuario.UsuarioClassificacao UC (nolock)  
	  where UC.Data BETWEEN @dataProcess6M AND @dataProcess
      Order by
	     UC.UsuarioID,
		 Uc.Data desc;
      
   Open CursorUsuClass;

   If (Select @@CURSOR_ROWS) > 0
   Begin
      Fetch Next From CursorUsuClass Into @curID, @curUsuarioID, @curData, @curNivelClassificacao, @curNivelReconhecimento;
      Set @AntFetch = @@fetch_status;

      While @AntFetch = 0
      Begin
	     Set @AntUsuarioID = @curUsuarioID;
		 Set @AntID = @curID;
		 Set @AntReconhecimento = @curNivelReconhecimento;
		 Set @Niveis = '';

	     While ( @AntFetch = 0 and @AntUsuarioID = @curID )
         Begin
		    Set @Niveis = @Niveis + @curNivelClassificacao;
            
            Fetch Next From CursorUsuClass Into @curID, @curUsuarioID, @curData, @curNivelClassificacao, @curNivelReconhecimento;            
            Set @AntFetch = @@fetch_status;   -- Para ver se nao é fim do Cursor     
         End; -- 2 While

	
		 Set @NovoReconhecimento = 0;

	     If (@AntReconhecimento = 6) 
		 Begin
			 Set @Niveis = Left(@Niveis + '000000', 6);

			 If (dbo.ValidaCarcacterZion(@Niveis, '7') = 0)
			 Begin
			    Set @NovoReconhecimento = 7;
			 End;
	     End;

		 If (@AntReconhecimento = 5) 
		 Begin
			 Set @Niveis = Left(@Niveis + '000000', 6);

			 If (dbo.ValidaCarcacterZion(@Niveis, '67') = 0)
			 Begin
			    Set @NovoReconhecimento = 6;
			 End;
	     End;

		 If (@AntReconhecimento = 4) 
		 Begin
			 Set @Niveis = Left(@Niveis + '000000', 4);

			 If (dbo.ValidaCarcacterZion(@Niveis, '567') = 0)
			 Begin
			    Set @NovoReconhecimento = 5;
			 End;
	     End;

		 If (@AntReconhecimento = 3) 
		 Begin
			 Set @Niveis = Left(@Niveis + '000000', 3);

			 If (dbo.ValidaCarcacterZion(@Niveis, '4567') = 0)
			 Begin
			    Set @NovoReconhecimento = 4;
			 End;
	     End;
	    
		 If (@AntReconhecimento = 2) 
		 Begin
			 Set @Niveis = Left(@Niveis + '000000', 2);

			 If (dbo.ValidaCarcacterZion(@Niveis, '34567') = 0)
			 Begin
			    Set @NovoReconhecimento = 3;
			 End;
	     End;

		 If (@AntReconhecimento = 1) 
         Begin
			 Set @Niveis = Left(@Niveis + '000000', 1);

			 If (dbo.ValidaCarcacterZion(@Niveis, '234567') = 0)
			 Begin
			    Set @NovoReconhecimento = 2;
			 End;
	     End;

		 If (@AntReconhecimento = 0) 
         Begin
			 Set @Niveis = Left(@Niveis + '000000', 1);

			 If (dbo.ValidaCarcacterZion(@Niveis, '1234567') = 0)
			 Begin
			    Set @NovoReconhecimento = 1;
			 End;
	     End;

		 -- Alter o novo Reconhacimento do Usuario
		 if (@NovoReconhecimento > 0)
		 Begin		 	 
		    Update Usuario.UsuarioClassificacao
		    Set NivelReconhecimento = @NovoReconhecimento
		    Where ID = @AntID;
		 End;
      End -- 1 While
   End -- @@CURSOR_ROWS

   Close CursorUsuario
   Deallocate CursorUsuario
    
   Drop Table #TUsuarios;
   Drop Table #TVolume;
   Drop TaBle #TUltimaClassificacao;
   Drop Table #TLinhas;
   Drop Table #TRede;
   Drop Table #TUsuarioClassificacao;

   DONE:

END TRY

BEGIN CATCH     
   DECLARE @error int, @message varchar(4000), @xstate int;
   SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
   RAISERROR ('Erro na execucao de uspMG_US_GeraClassificacaoUsuario: %d: %s', 16, 1, @error, @message) WITH SETERROR;
END CATCH  

--exec uspMG_US_GeraClassificacaoUsuario null
--exec uspMG_US_GeraClassificacaoUsuario '20180310'

-- Atual             Mes Analise  Validos        Resto
-- ----------------  -----------  -------------  -------------
-- Reconhecimento 7  Não Analisa	 	  
-- Reconhecimento 6  9.9.9.9.9.9  7              0,1,2,3,4,5,6
-- Reconhecimento 5    9.9.9.9.9  6,7            0,1,2,3,4,5
-- Reconhecimento 4      9.9.9.9  5,6,7          0,1,2,3,4
-- Reconhecimento 3        9.9.9  4,5,6,7        0,1,2,3
-- Reconhecimento 2          9.9  3,4,5,6,7      0,1,2
-- Reconhecimento 1            9  2,3,4,5,6,7    0,1
-- Reconhecimento 0            9  1,2,3,4,5,6,7  0

