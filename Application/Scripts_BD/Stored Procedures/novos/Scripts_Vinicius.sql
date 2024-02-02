* - Projeto Jockey Bit - *

-- Configura��es e Tradu��es
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'MEIO_PGTO_BITCOIN_ATIVO', 'Ativa pagamento com BitCoin', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'HOME_DASHBOARD_TETO_GANHOS_TOTAIS', 'Define a exibi��o do dashboard Teto de Ganhos Totais', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'HOME_DASHBOARD_TETO_GANHOS_ALAVANCAGEM', 'Define a exibi��o do dashboard Teto de Ganhos Alavancagem', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'HOME_DASHBOARD_TETO_GANHOS_BONUS_EQUIPE_BINARIO', 'Define a exibi��o do dashboard Teto de Ganhos do B�nus Equipe Bin�rio', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'HOME_DASHBOARD_GANHOS_TOTAIS_BONIFICACAO', 'Define a exibi��o do dashboard de Ganhos Totais da Bonificacao', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 2, 'SALDO_DIVIDIDO_JB', 'Ativa o saldo dividido exclusivo da JockeyBit', 'true')
INSERT INTO [ZionJB].[Sistema].[Configuracao] VALUES (7, 1, 'SAQUE_VALOR_MINIMO_BONUS_BINARIO', 'Valor minimo para um saque do saldo do Bonus Binario', '200.00')

INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'CLIQUE_PAGAMENTO_BITCOIN', 'Clique aqui para pagamento com BitCoin.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'CLIQUE_PAGAMENTO_BITCOIN', 'Haga clic aqu� para obtener el pago con BitCoin.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'CLIQUE_PAGAMENTO_BITCOIN', 'Click here for BitCoin payment.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'BITCOIN_DADOS', 'Utilize o QR Code ou copie o endere�o para realizar o pagamento', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'BITCOIN_DADOS', 'Utilice el QR Code o copie la direcci�n para realizar el pago', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'BITCOIN_DADOS', 'Use the QR Code or copy the address to pay', '', GETDATE())

INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'TETO_GANHOS_ALAVANCAGEM', 'Teto de Ganhos Alavancagem', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'TETO_GANHOS_ALAVANCAGEM', 'Ganancias M�ximas de Apalancamiento', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'TETO_GANHOS_ALAVANCAGEM', 'Maximum of Leverage Gains', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'TETO_GANHOS_TOTAIS', 'Teto de Ganhos Totais', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'TETO_GANHOS_TOTAIS', 'Ganancias Totales M�ximas', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'TETO_GANHOS_TOTAIS', 'Maximum Total Earnings', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'TOTAL_GANHOS_BONIFICACAO', 'Total Ganhos de Bonifica��o', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'TOTAL_GANHOS_BONIFICACAO', 'Bonos totales de ganancias', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'TOTAL_GANHOS_BONIFICACAO', 'Total Bonus Gains', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'TETO_GANHOS_BONUS_BINARIO', 'Teto Ganhos B�nus Equipe (Bin�rio)', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'TETO_GANHOS_BONUS_BINARIO', 'Bono por Equipo de Ganancias de Techo (Binario)', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'TETO_GANHOS_BONUS_BINARIO', 'Ceiling Earnings Team Bonus (Binary)', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'BONUS_DIARIO', 'B�nus Di�rio', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'BONUS_DIARIO', 'Bono diario', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'BONUS_DIARIO', 'Daily Bonus', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'SAQUE_SOLICITACAO_ULTIMO_DIA_MES', 'As solicita��es de saque s� podem ser efetuada no �ltimo dia de cada m�s', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'SAQUE_SOLICITACAO_ULTIMO_DIA_MES', 'Las solicitudes de retiro solo se pueden realizar el �ltimo d�a de cada mes.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'SAQUE_SOLICITACAO_ULTIMO_DIA_MES', 'Withdrawal requests can only be made on the last day of each month.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (1, 1, 'INSTRUCOES_SAQUE_JB', '- Liberado para solicita��o todo �ltimo dia de cada m�s.<br/>- Desconto de 5% de taxa por saque.<br/>- Digite a senha do sistema para confirmar o saque.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (2, 1, 'INSTRUCOES_SAQUE_JB', '- Lanzado a pedido el �ltimo d�a de cada mes. <br/>- Descuento del 5% en la tarifa de retiro.<br/>- Introduzca la contrase�a del sistema para confirmar el retiro.', '', GETDATE())
INSERT INTO [ZionJB].[Globalizacao].[Traducao] VALUES (3, 1, 'INSTRUCOES_SAQUE_JB', '- Released on request every last day of each month. <br/>- Discount of 5% withdrawal fee.<br/>- Enter the system password to confirm the withdrawal.', '', GETDATE())

-- Zera ID's das tabelas Produto e ProdutoValor
DBCC CHECKIDENT('[ZionJB].[Loja].[Produto]', RESEED, 0)
DBCC CHECKIDENT('[ZionJB].[Loja].[ProdutoValor]', RESEED, 0)

-- Planos Associa��o
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ADE0001', 1, 1, 'Tips 10 (cinza)', 'Kit Ades�o + Plano Tips 10', 'Tips 10 US$100 + Ades�o US$50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '', 7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ADE0002', 1, 2, 'M�dulo 01 (verde)', 'Kit Ades�o + Plano M�dulo 01', 'M�dulo 01 US$500 + Ades�o US$50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ADE0003', 1, 3, 'M�dulo 10 (marrom)', 'Kit Ades�o + Plano M�dulo 10', 'M�dulo 10 US$5.000 + Ades�o US$50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ADE0004', 1, 4, 'Bot 10 (roxo)', 'Kit Ades�o + Plano Bot 10', 'Bot 10 US$10.000 + Ades�o US$50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '', 7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ADE0005', 1, 5, 'Bot 50 (vermelho)', 'Kit Ades�o + Plano Bot 50', 'Bot 50 US$50.000 + Ades�o US$50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')

-- Planos Upgrade e Acumula��o
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ACU0006', 2, 2, 'Plano M�dulo 01 (verde) ACUMULA��O', 'Plano M�dulo 01 - ACUMULA��O', 'Seu plano atual + M�dulo 01', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('UPG0007', 2, 2, 'Plano M�dulo 01 (verde)', 'Plano M�dulo 01 - UPGRADE', 'Mude seu plano', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ACU0008', 2, 3, 'Plano M�dulo 10 (marrom) ACUMULA��O', 'Plano M�dulo 10 - ACUMULA��O', 'Seu plano atual + M�dulo 10', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('UPG0009', 2, 3, 'Plano M�dulo 10 (marrom)', 'Plano M�dulo 10 - UPGRADE', 'Mude seu plano', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ACU0010', 2, 4, 'Plano Bot 10 (roxo) ACUMULA��O', 'Plano Bot 10 - ACUMULA��O', 'Seu plano atual + Bot 10', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '', 7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('UPG0011', 2, 4, 'Plano Bot 10 (roxo)', 'Plano Bot 10 - UPGRADE', 'Mude seu plano', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '', 7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ACU0012', 2, 5, 'Plano Bot 50 (vermelho) ACUMULA��O', 'Plano Bot 50 - ACUMULA��O', 'Seu plano atual + Bot 50', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '',  7, '', 0, 0, '')
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('UPG0013', 2, 5, 'Plano Bot 50 (vermelho)', 'Plano Bot 50 - UPGRADE', 'Mude seu plano', 0, 0, 9999, 9999, 0, 9999, 1, GETDATE(), GETDATE(), 1, '', 7, '', 0, 0, '')

-- Plano Ativo Mensal
INSERT INTO [ZionJB].[Loja].[Produto] VALUES ('ATM0014', 5, 4, 'ATIVO MENSAL', 'ATIVO MENSAL', 'ATIVO MENSAL', 0, 0, 9999, 9999, 0, 9999, 0, GETDATE(), GETDATE(), 1, 'ATIVO MENSAL', 5, '', 0, 0, '')

-- Valores dos Planos Associa��o
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(1, NULL, '', '', '', '', '', 150, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(2, NULL, '', '', '', '', '', 550, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(3, NULL, '', '', '', '', '', 5050, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(4, NULL, '', '', '', '', '', 10050, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(5, NULL, '', '', '', '', '', 50050, 500, 0, 0)

-- Valores dos Planos Acumula��o
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(6, NULL, '', '', '', '', '', 500, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(8, NULL, '', '', '', '', '', 5000, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(10, NULL, '', '', '', '', '', 10000, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(12, NULL, '', '', '', '', '', 50000, 500, 0, 0)

-- Valores dos Planos Upgrade
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(7, NULL, '', '', '', '', '', 400, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(9, NULL, '', '1', '', '', '', 4900, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(9, NULL, '', '2', '', '', '', 4500, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(11, NULL, '', '1', '', '', '', 9900, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(11, NULL, '', '2', '', '', '', 9500, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(11, NULL, '', '3', '', '', '', 5000, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(13, NULL, '', '1', '', '', '', 49900, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(13, NULL, '', '2', '', '', '', 49500, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(13, NULL, '', '3', '', '', '', 45000, 500, 0, 0)
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(13, NULL, '', '4', '', '', '', 40000, 500, 0, 0)

-- Valor do Plano Ativa��o
INSERT INTO [ZionJB].[Loja].[ProdutoValor]  VALUES(14, NULL, '', '', '', '', '', 30, 0, 0, 0)

-- Acerto de Associa��es
DBCC CHECKIDENT('[ZionJB].[Rede].[Associacao]', RESEED, 0)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (0, 'N�o Associado', 0, NULL)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (1, '10 Tips', 0, NULL)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (2, '01 M�dulo', 0, NULL)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (3, '10 M�dulos', 0, NULL)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (4, 'Bot 10', 0, NULL)
INSERT INTO [ZionJB].[Rede].[Associacao] VALUES (5, 'Bot 50', 0, NULL)

-- B�nus Bin�rio -- N�O USAR
--INSERT INTO [ZionJB].[Financeiro].[Categoria] VALUES (6, 'B�nus Bin�rio')

-- Saque B�nus Bin�rio -- N�O USAR
--INSERT INTO [ZionJB].[Financeiro].[Categoria] VALUES (7, 'Saque B�nus Bin�rio')

-- Conta B�nus Bin�rio
INSERT INTO [ZionJB].[Financeiro].[Conta] VALUES (3, 'B�nus Alavancagem')




