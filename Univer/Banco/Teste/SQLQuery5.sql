SELECT
            tn.ID,
            tn.UsuarioID,
            tn.BoardID,
            boa.Nome as BoardNome,
            boa.Cor as BoardCor,
            tab.TabuleiroID as TabuleiroID,
            tab.Posicao,
            tn.DataInicio,
            tn.DataFim,
            tn.StatusID,
            tn.Observacao
        FROM 
            Rede.TabuleiroNivel tn,
            Rede.TabuleiroBoard boa,
            Rede.TabuleiroUsuario tab
        WHERE
            tn.UsuarioID = 2591 and
            tn.StatusID = 2 and
            tn.BoardID = boa.id and
            tn.UsuarioID = tab.UsuarioID and
            tn.BoardID = tab.boardID and
            tab.StatusID = 1
        Order By 
            StatusID

        SELECT
            tn.ID,
            tn.UsuarioID,
            tn.BoardID,
            boa.Nome as BoardNome,
            boa.Cor as BoardCor,
            tab.TabuleiroID as TabuleiroID,
            tab.Posicao,
            tn.DataInicio,
            tn.DataFim,
            tn.StatusID,
            tn.Observacao
        FROM 
            Rede.TabuleiroNivel tn,
            Rede.TabuleiroBoard boa,
            Rede.TabuleiroUsuario tab
        WHERE
            tn.UsuarioID = 2591 and
            tn.StatusID = 2 and
            tn.BoardID = boa.id and
            tn.UsuarioID = tab.UsuarioID and
            tn.BoardID = tab.boardID and
            tab.StatusID = 1
        Order By 
            StatusID


--2591 Enzo

Select * from Rede.TabuleiroNivel Where USUARIOID in (2591,2595)

Select * from Rede.TabuleiroUsuario Where USUARIOID in (2591)

Select * from Rede.TabuleiroUsuario Where USUARIOID in (2595)