use UniverDev
go
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = '__MigrationHistory') Begin drop table dbo.__MigrationHistory End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'Autenticacao') Begin drop table dbo.Autenticacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoGrupo') Begin drop table dbo.AutenticacaoGrupo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoGrupoRegra') Begin drop table dbo.AutenticacaoGrupoRegra End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoRegra') Begin drop table dbo.AutenticacaoRegra End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoUsuarioGrupo') Begin drop table dbo.AutenticacaoUsuarioGrupo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoUsuarioLogin') Begin drop table dbo.AutenticacaoUsuarioLogin End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoUsuarioPermissao') Begin drop table dbo.AutenticacaoUsuarioPermissao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'dbo' and t.name = 'AutenticacaoUsuarioRegra') Begin drop table dbo.AutenticacaoUsuarioRegra End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Arbitragem') Begin drop table Financeiro.Arbitragem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'ArbitragemPeriodo') Begin drop table Financeiro.ArbitragemPeriodo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'BitCoinSafe') Begin drop table Financeiro.BitCoinSafe End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'BlockchainLog') Begin drop table Financeiro.BlockchainLog End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Boleto') Begin drop table Financeiro.Boleto End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'CartaoCredito') Begin drop table Financeiro.CartaoCredito End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Categoria') Begin drop table Financeiro.Categoria End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'CategoriaTipo') Begin drop table Financeiro.CategoriaTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Conta') Begin drop table Financeiro.Conta End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'ContaDeposito') Begin drop table Financeiro.ContaDeposito End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'ContaOperacao') Begin drop table Financeiro.ContaOperacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Instituicao') Begin drop table Financeiro.Instituicao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'IR') Begin drop table Financeiro.IR End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Lancamento') Begin drop table Financeiro.Lancamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'LancamentoTipo') Begin drop table Financeiro.LancamentoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'MeioPagamento') Begin drop table Financeiro.MeioPagamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'Saque') Begin drop table Financeiro.Saque End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'SaqueStatus') Begin drop table Financeiro.SaqueStatus End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'SplitCripto') Begin drop table Financeiro.SplitCripto End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Financeiro' and t.name = 'TipoConta') Begin drop table Financeiro.TipoConta End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Bloco') Begin drop table Globalizacao.Bloco End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Cidade') Begin drop table Globalizacao.Cidade End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Estado') Begin drop table Globalizacao.Estado End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Filial') Begin drop table Globalizacao.Filial End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Idioma') Begin drop table Globalizacao.Idioma End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Moeda') Begin drop table Globalizacao.Moeda End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'MoedaCotacao') Begin drop table Globalizacao.MoedaCotacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'MoedaCotacaoTipo') Begin drop table Globalizacao.MoedaCotacaoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Pais') Begin drop table Globalizacao.Pais End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'Traducao') Begin drop table Globalizacao.Traducao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Globalizacao' and t.name = 'TraducaoSecao') Begin drop table Globalizacao.TraducaoSecao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Integracao' and t.name = 'Motivacard') Begin drop table Integracao.Motivacard End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Integracao' and t.name = 'MovimentoConta') Begin drop table Integracao.MovimentoConta End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Agendamento') Begin drop table Loja.Agendamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'AgendamentoItem') Begin drop table Loja.AgendamentoItem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Armazem') Begin drop table Loja.Armazem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Cupom') Begin drop table Loja.Cupom End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Departamento') Begin drop table Loja.Departamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'EstoqueMovimento') Begin drop table Loja.EstoqueMovimento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'EstoqueSaldo') Begin drop table Loja.EstoqueSaldo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Pedido') Begin drop table Loja.Pedido End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoItem') Begin drop table Loja.PedidoItem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoItemStatus') Begin drop table Loja.PedidoItemStatus End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoItemStatusEntrega') Begin drop table Loja.PedidoItemStatusEntrega End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoPagamento') Begin drop table Loja.PedidoPagamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoPagamentoStatus') Begin drop table Loja.PedidoPagamentoStatus End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'PedidoTaxa') Begin drop table Loja.PedidoTaxa End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Produto') Begin drop table Loja.Produto End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoCategoria') Begin drop table Loja.ProdutoCategoria End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoDepartamento') Begin drop table Loja.ProdutoDepartamento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoItem') Begin drop table Loja.ProdutoItem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoOpcao') Begin drop table Loja.ProdutoOpcao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoTipo') Begin drop table Loja.ProdutoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'ProdutoValor') Begin drop table Loja.ProdutoValor End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Loja' and t.name = 'Taxa') Begin drop table Loja.Taxa End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Associacao') Begin drop table Rede.Associacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'AssociacaoLimiteGanho') Begin drop table Rede.AssociacaoLimiteGanho End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Board') Begin drop table Rede.Board End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Bonificacao') Begin drop table Rede.Bonificacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Ciclo') Begin drop table Rede.Ciclo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Classificacao') Begin drop table Rede.Classificacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'ClassificacaoVML') Begin drop table Rede.ClassificacaoVML End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'ConfiguracaoBonusCarreira') Begin drop table Rede.ConfiguracaoBonusCarreira End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'ConfiguracaoBonusDiario') Begin drop table Rede.ConfiguracaoBonusDiario End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Group') Begin drop table Rede.[Group] End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Pontos') Begin drop table Rede.Pontos End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'PontosBinario') Begin drop table Rede.PontosBinario End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Posicao') Begin drop table Rede.Posicao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'RedeMigracao') Begin drop table Rede.RedeMigracao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Regra') Begin drop table Rede.Regra End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'RegraItem') Begin drop table Rede.RegraItem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Status') Begin drop table Rede.Status End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Tabuleiro') Begin drop table Rede.Tabuleiro End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'TabuleiroLog') Begin drop table Rede.TabuleiroLog End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'TabuleiroNivel') Begin drop table Rede.TabuleiroNivel End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'TabuleiroUsuario') Begin drop table Rede.TabuleiroUsuario End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'UplineCiclo') Begin drop table Rede.UplineCiclo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Rede' and t.name = 'Upline_Ciclo') Begin drop table Rede.Upline_Ciclo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Administrador') Begin drop table Sistema.Administrador End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Arquivo') Begin drop table Sistema.Arquivo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'ArquivoSecao') Begin drop table Sistema.ArquivoSecao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'ArquivoTipo') Begin drop table Sistema.ArquivoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Ativo') Begin drop table Sistema.Ativo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Configuracao') Begin drop table Sistema.Configuracao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'ConfiguracaoCategoria') Begin drop table Sistema.ConfiguracaoCategoria End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'ConfiguracaoTipo') Begin drop table Sistema.ConfiguracaoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Empresa') Begin drop table Sistema.Empresa End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Integracao') Begin drop table Sistema.Integracao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Log') Begin drop table Sistema.Log End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Paginacao') Begin drop table Sistema.Paginacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'Suporte') Begin drop table Sistema.Suporte End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Sistema' and t.name = 'SuporteMensagem') Begin drop table Sistema.SuporteMensagem End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Afiliados') Begin drop table Usuario.Afiliados End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'AtivacaoMensal') Begin drop table Usuario.AtivacaoMensal End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Aviso') Begin drop table Usuario.Aviso End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'AvisoLido') Begin drop table Usuario.AvisoLido End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'AvisoTipo') Begin drop table Usuario.AvisoTipo End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Banco') Begin drop table Usuario.Banco End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Complemento') Begin drop table Usuario.Complemento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Documento') Begin drop table Usuario.Documento End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Endereco') Begin drop table Usuario.Endereco End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'LogAcesso') Begin drop table Usuario.LogAcesso End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'LoginExterno') Begin drop table Usuario.LoginExterno End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Pontos') Begin drop table Usuario.Pontos End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'PontosLinha') Begin drop table Usuario.PontosLinha End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Qualificacao') Begin drop table Usuario.Qualificacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Sincronizacao') Begin drop table Usuario.Sincronizacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Superiores') Begin drop table Usuario.Superiores End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'Usuario') Begin drop table Usuario.Usuario End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'UsuarioAssociacao') Begin drop table Usuario.UsuarioAssociacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'UsuarioClassificacao') Begin drop table Usuario.UsuarioClassificacao End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'UsuarioDerramamentoLog') Begin drop table Usuario.UsuarioDerramamentoLog End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'UsuarioGanho') Begin drop table Usuario.UsuarioGanho End;
if exists (select 'existe' from sys.tables t where schema_name(t.schema_id) = 'Usuario' and t.name = 'UsuarioStatus') Begin drop table Usuario.UsuarioStatus End;