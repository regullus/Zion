Use MinersBits
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spDG_FI_GeraSaques'))
   Drop Procedure spDG_FI_GeraSaques
go

-- =============================================================================================
-- Author.....: Adamastor
-- Create date: 24/05/2017
-- Description: Gera registros de saque automaticos
-- =============================================================================================

Create Proc [dbo].[spDG_FI_GeraSaques]

AS
BEGIN
   Create Table #tSaque
   (
      UsuarioID int,
      Valor float ,
	   Bitcoin nvarchar(max),
	   ContaID int 
   );
	
   Declare @valorMinimo float = ISNULL((Select convert(float, Dados) From Sistema.Configuracao (nolock) Where Chave = 'VALOR_MINIMO_SAQUE') , 0.0);
   Declare @valorTaxa   float = ISNULL((Select convert(float, Dados) From Sistema.Configuracao (nolock) Where Chave = 'VALOR_TAXA_SAQUE'  ) , 0.0);
   Declare @valorFee    float = ISNULL((Select convert(float, Dados) From Sistema.Configuracao (nolock) Where Chave = 'VALOR_FEE'  ) , 0.0);

   -- Obtem os Usuarios e valores para saque
   Insert Into #tSaque
   Select U.ID, Round(Sum(L.Valor),8,1), null , 0
   From Usuario.Usuario U (nolock)
      INNER JOIN Financeiro.Lancamento L (nolock) ON u.id = L.UsuarioID
   Where U.StatusID = 2 /*ativos e pagos*/
     and U.Bloqueado = 0
   Group By U.ID
   Having Round(Sum(L.Valor),8,1) >= @valorMinimo;

   -- Obtem a conta para credito
   Update #tSaque
   Set Bitcoin = C.Bitcoin ,
	    ContaID = C.ID
   From #tSaque T,
      Financeiro.ContaDeposito C (nolock)
   where C.IDTipoConta = 2
     and C.IDUsuario = T.UsuarioID
     and LEN(RTRIM(LTRIM(C.Bitcoin))) > 0;  

   -- Remove os Usuario que não tem conta
   Delete #tSaque 
   Where Bitcoin IS NULL 
   
   -- Gera Solicitacao de saque (Saque, SaqueStatus e Lancamento)
   Declare
      @NewID int,
	   @BancoID int;

   Declare
      @UsuarioID int,
      @Valor float,
	   @Bitcoin nvarchar(max),
	   @ContaID int,

      @AntFetch int;

   Declare
      curLocal
   Cursor For
   Select 
      UsuarioID, Valor, Bitcoin,  ContaID
   From
      #tSaque
   Order by UsuarioID

   Open curLocal

   If (Select @@CURSOR_ROWS) <> 0
   Begin
      Fetch Next From curLocal Into @UsuarioID , @Valor ,@Bitcoin, @ContaID
      Select @AntFetch = @@fetch_status

      While @AntFetch = 0
      Begin   	   
	    --BEGIN TRANSACTION

	      set @BancoID = ISNULL((Select top 1 id From Usuario.Banco (nolock) Where UsuarioID = @UsuarioID), 0)
	      if (@BancoID = 0)
	      Begin
	         Insert Into Usuario.Banco (UsuarioID, Dados,Principal) Values (@UsuarioID,'', 1)
	         Select @BancoID = SCOPE_IDENTITY();
	      End
	  	      
		   Insert Into Financeiro.Saque
            (UsuarioID, BancoID,MoedaID,Total,Taxas,Impostos,Liquido,IR,INSS,Bitcoin,Fee,Data)
		   Values
            (@UsuarioID, @BancoID, 6, @Valor, (@valorFee + @valorTaxa), 0, (@Valor - @valorFee - @valorTaxa), 0  ,0 , @Bitcoin, @valorFee, dbo.GetDateZion() )

		   Select @NewID = SCOPE_IDENTITY();

         Insert Into Financeiro.SaqueStatus
            (SaqueID, AdministradorID , StatusID, Data)
         Values
            (@NewID, null, 1 , dbo.GetDateZion() );

         Insert Into Financeiro.Lancamento
            (UsuarioID ,ContaID  ,TipoID ,CategoriaID ,ReferenciaID   ,Valor  ,Descricao  ,DataCriacao  ,DataLancamento)
         Values
            (@UsuarioID,  1, 7 , 9 ,@NewID, -@Valor, 'Solicitação de Saque', dbo.GetDateZion(), dbo.GetDateZion() )

		--COMMIT TRANSACTION

         Fetch Next From curLocal Into @UsuarioID , @Valor ,@Bitcoin, @ContaID
         Select @AntFetch = @@fetch_status -- Para ver se nao é fim do loop        
      End -- While
   End -- @@CURSOR_ROWS

   Close curLocal
   Deallocate curLocal

   Drop Table #tSaque
END

go
   Grant Exec on spDG_FI_GeraSaques To public
go


--Drop Table #tSaque;
--Exec spDG_FI_GeraSaques






