USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spPontuacaoPosicao]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spPontuacaoPosicao]
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
      @AntFetch int

   set @Total = 0

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
         if(@ValorDiaPernaDireita <= @ValorDiaPernaEsquerda)
         Begin
            set @Total = @Total + @ValorDiaPernaDireita 
         End
         Else
         Begin
            set @Total = @Total + @ValorDiaPernaEsquerda
         End

         Fetch Next From curPontos Into @ValorDiaPernaDireita, @ValorDiaPernaEsquerda,@DataInicio
         -- Para ver se nao é fim do loop
         Select @AntFetch = @@fetch_status    

      End -- While
   End -- @@CURSOR_ROWS

   Close curPontos
   Deallocate curPontos

   Select @Total Pontos
End -- Sp









GO
