use ClubeVantagens
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spPontuacaoPosicao'))
   Drop Procedure spPontuacaoPosicao
Go

Create Proc [dbo].[spPontuacaoPosicao]
   @idUsuario int
As
Begin
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   
   --******** Inicio Cursor *********
   Declare
      @ValorDiaPernaDireita decimal(10,2),
	  @ValorDiaPernaEsquerda decimal(10,2),
	  @DataInicio datetime,
      @Total decimal(10,2),
      @TotalD decimal(10,2),
      @TotalE decimal(10,2),
      @AntFetch int

   set @TotalD = 0
   set @TotalE = 0

   Declare
      curPontos
   Cursor For
   Select
      ValorDiaPernaDireita,
      ValorDiaPernaEsquerda,
      DataInicio
   From 
      rede.Posicao
   Where
      UsuarioID = @idUsuario
   Order by 
      DataInicio   
      
   Open curPontos
   If (Select @@CURSOR_ROWS) <> 0
   Begin
      Fetch Next From curPontos Into @ValorDiaPernaDireita, @ValorDiaPernaEsquerda,@DataInicio
      Select @AntFetch = @@fetch_status
      While @AntFetch = 0
      Begin

         --if(@ValorDiaPernaDireita <= @ValorDiaPernaEsquerda)
         --Begin
         --   set @Total = @Total + @ValorDiaPernaDireita 
         --End
         --Else
         --Begin
         --   set @Total = @Total + @ValorDiaPernaEsquerda
         --End

         set @TotalD = @TotalD + @ValorDiaPernaDireita 
         set @TotalE = @TotalE + @ValorDiaPernaEsquerda

         Fetch Next From curPontos Into @ValorDiaPernaDireita, @ValorDiaPernaEsquerda,@DataInicio

         -- Para ver se nao é fim do loop
         Select @AntFetch = @@fetch_status    

      End -- While
   End -- @@CURSOR_ROWS

   Close curPontos
   Deallocate curPontos
   
   --Verifica qual o valor menor e exibe
   if(@TotalD <= @TotalE)
   Begin
      set @Total = @TotalD
   End
   Else
   Begin
      set @Total = @TotalE
   End
   
   Select @Total Pontos
End -- Sp

go
Grant Exec on spPontuacaoPosicao To public
go

Exec spPontuacaoPosicao @idusuario=30
--Exec spPontuacaoPosicao @id=72


