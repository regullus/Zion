Use MinersBits
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_RE_GeraBonusClassificacao'))
   Drop Procedure spDG_RE_GeraBonusClassificacao
go

-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 25/05/2017
-- Description: Gera registros de Bonificacao de Classificacao e 
-- =============================================================================================

Create PROCEDURE [dbo].[spDG_RE_GeraBonusClassificacao]
   @baseDate varchar(8) null

AS
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

   -- Paga bonus so de segunda a Sexta-feira
   --if (DATEPART(weekday, @dataInicio) in (2,3,4,5,6))
   --Begin
   --    GOTO DONE;
   --End
      
   Create Table #tBonus
   (
     UsuarioID          int,
	  NivelClassificacao int,
     NewClassificacao   int,
     VlrAcumulado       float,
	  VlrBonu            float,   
   );

   -- Obtem o bonus dos Usuarios Quatificados
   Insert Into #tBonus
   Select  
     U.ID, 
     U.NivelClassificacao, 
     0,
     Case 
       When UP.AcumuladoDireita > UP.AcumuladoEsquerda
       Then UP.AcumuladoEsquerda
       Else UP.AcumuladoDireita
     End,
     0.0 
   From Usuario.Usuario U (nolock)
     OUTER APPLY (Select Top 1  AcumuladoEsquerda, AcumuladoDireita
                  From Rede.Posicao  p (nolock) 
                  Where U.ID = P.UsuarioID
                  Order By DataFim desc) As UP
   Where U.StatusID = 2  -- ativos e pagos 
     and U.GeraBonus = 1
     and U.DataValidade >= @dataInicio --so recebe se estiver ativo 

   -- Remove os Usuarios com Pontos igual a Zero
   Delete #tBonus
   Where VlrAcumulado = 0;

   -- Atualiza o valor do Bonus e a Nova Classificacao 
   Update #tBonus
   Set VlrBonu = UC.ValorBonus,
       NewClassificacao = UC.Nivel
   From #tBonus T (nolock)
     OUTER APPLY (Select Top 1 Nivel, ValorBonus
                  From Rede.Classificacao  C (nolock) 
                  Where C.Pontos <= T.VlrAcumulado 
                  Order By Pontos desc) As UC;

   -- Remove os Usuarios que já estão na nova classificacao
   Delete #tBonus
   Where NivelClassificacao = NewClassificacao;

   -- Atualiza Posicao
   Declare
      @UsuarioID          int,	 
      @NewClassificacao   int, 
	   @VlrBonu            float,   

      @AntFetch int

   Declare
      curLocal
   Cursor For
   Select 
      UsuarioID,	 
      NewClassificacao, 
	   VlrBonu  
   From
      #tBonus 

   Open curLocal

   If (Select @@CURSOR_ROWS) <> 0
   Begin
      Fetch Next From curLocal Into  @UsuarioID, @NewClassificacao, @VlrBonu    
      Select @AntFetch = @@fetch_status

      While @AntFetch = 0
      Begin
         BEGIN TRANSACTION

         -- Atualiza Usuariocom o novo nivel de Classificacao
         Update Usuario.Usuario
         Set NivelClassificacao = @NewClassificacao 
         Where ID = @UsuarioID;

         -- Gera UsuarioClassificacao
         Insert Into Usuario.UsuarioClassificacao
            (UsuarioID, NivelClassificacao, Data)
		   Values
            (@UsuarioID, @NewClassificacao, dbo.GetDateZion() );
		   
         -- Gera Bonificacao
		   if (@VlrBonu > 0)
		   Begin
            Insert Into Rede.Bonificacao 
               (CategoriaID, UsuarioID, ReferenciaID, StatusID, Data, Valor)
            Values
               (8, @UsuarioID, 0, 0, @dataFim, dbo.TruncZion(@VlrBonu, 8));    
		   End
            
         COMMIT TRANSACTION    

         Fetch Next From curLocal Into  @UsuarioID, @NewClassificacao, @VlrBonu 
         Select @AntFetch = @@fetch_status  -- Para ver se nao é fim do loop        
      End -- While
   End -- @@CURSOR_ROWS

   Close curLocal
   Deallocate curLocal

   drop table #tBonus

   DONE:

END TRY

BEGIN CATCH     
   DECLARE @error int, @message varchar(4000), @xstate int;
   SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE();
   RAISERROR ('Erro na execucao de spDG_RE_GeraBonusClassificacao: %d: %s', 16, 1, @error, @message) WITH SETERROR;
END CATCH  

--exec spDG_RE_GeraBonusClassificacao null

--Select * from delete Usuario.UsuarioClassificacao
--Select * from Rede.Bonificacao order by id desc
-- delete Rede.Bonificacao where id >= 1425 
-- Select * from Usuario.Usuario where NivelClassificacao > 0



