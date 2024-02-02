USE [ZionJB]
GO
/****** Object:  StoredProcedure [dbo].[spOC_FI_RelatorioSaldoResumo]    Script Date: 19/07/2019 06:33:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[spOC_FI_RelatorioSaldoResumo]
   @DataIni datetime,
   @DataFim datetime,
   @login varchar(100) = null,
   @PorAssinatura bit = 0

As
Begin

   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on

   declare @usuarioID int = 0
   declare @Assinatura nvarchar(max) = ''

   IF @login <> ''
 BEGIN

  SELECT 
   @UsuarioID = ID,
   @Assinatura = Assinatura
  FROM usuario.usuario (nolock) 
  WHERE login = @Login

 -- IF @PorAssinatura = 1
 --  BEGIN

 --   SELECT 
 --    @Assinatura = Assinatura 
 --   FROM usuario.usuario (nolock) 
 --   WHERE login  = @Login

 --  END 

 END


   Declare 
	@Qtd_Total int,
	@Vlr_Liquido decimal,
	@Vlr_Total_Pago_BTC decimal,
	@Vlr_Total_Pago_MAN decimal,
	@Vlr_Total_Aviso decimal,
	@Vlr_Total_Estornado decimal,
	@Vlr_Total_Cancelado decimal,
	@Vlr_Total_Processando decimal,
	@Vlr_Total_Aprovado decimal

Declare @StatusID INT

SELECT @Qtd_Total = count(1),
 @Vlr_Liquido = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
	(
	(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
	(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
	)

  -- Select 
  --    @Qtd_Total = count(*)
  -- From 
  --    Financeiro.Saque fs (nolock),
  --    Financeiro.Saquestatus fss (nolock),
  -- Usuario.Usuario u
  -- Where 
  --    fs.[id] = fss.[saqueid] 
  -- and fs.[UsuarioID] = u.[ID] 
  --    and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
  --    and 1 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus (nolock) fssx Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC) 
  -- AND
  --   (
  --(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
  --(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
  --) 


  -- --Valor total Em Aberto
  -- Select 
  --    @Vlr_Liquido = sum(fs.liquido)
  -- From 
  --    Financeiro.Saque fs (nolock),
  --    Financeiro.Saquestatus fss (nolock),
  -- Usuario.Usuario u
  -- Where 
  --    FS.[id] = FSS.[saqueid]
  -- and fs.[UsuarioID] = u.[ID] 
  --    and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
  --    and 1 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock) Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
  --    AND
  --   (
  --(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
  --(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
  --) 

SET @StatusID = 8;
WITH UltimoStatusSaque AS (
select distinct  SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK)
) 
SELECT distinct @Vlr_Total_Pago_BTC = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1	
WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)

   -- Valor Total pago com bitcoin
  -- Select 
  --   @Vlr_Total_Pago_BTC = sum(fs.liquido)
  -- From 
  --    Financeiro.Saque fs (nolock),
  --    Financeiro.Saquestatus fss (nolock),
  -- Usuario.Usuario u
  -- Where 
  --    FS.[id] = FSS.[saqueid]
  -- and fs.[UsuarioID] = u.[ID] 
  --    and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
  --    and 8 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock) Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
  -- AND
  --   (
  --(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
  --(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
  --) 

SET @StatusID = 99
SET @Vlr_Total_Pago_MAN = 0

--WITH UltimoStatusSaque AS (
--select SAQUEID, STATUSID,
--	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
--from FINANCEIRO.SAQUESTATUS (NOLOCK)
--) 
--SELECT @Qtd_Total = count(1),
-- @Vlr_Total_Pago_MAN = sum(FS.Total)
--FROM FINANCEIRO.SAQUE (NOLOCK) FS 
--INNER JOIN FINANCEIRO.SAQUESTATUS FSS ON FSS.SaqueID = FS.ID 
--					AND FSS.STATUSID = @StatusID
--INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
--INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
--							AND US.STATUSID = @StatusID
--							and US.Ordem = 1

--WHERE FS.ID = FSS.SAQUEID AND FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
--AND FS.DATA < @DataFim
--AND
--    (
--(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
--(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
--)

 -- Valor Total Pago Manualmente
 --Select 
 -- @Vlr_Total_Pago_MAN = sum(fs.[liquido])
 --From 
 -- Financeiro.Saque fs (nolock),
 -- Financeiro.Saquestatus fss (nolock),
 --  Usuario.Usuario u (nolock) 
 --Where 
 -- FS.[id] = FSS.[saqueid]
 --  and fs.[UsuarioID] = u.[ID] 
 -- and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
 -- and 4 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
 --  AND
 --    (
 -- (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
 -- (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
 -- ) 

SET @StatusID = 9;
WITH UltimoStatusSaque AS (
select distinct  SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK)
) 
SELECT @Vlr_Total_Aviso = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1

WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)

   -- Valor Total aviso
 --Select
 --   @Vlr_Total_Aviso = sum(fs.liquido)
 --From 
 --   Financeiro.Saque fs (nolock),
 --   Financeiro.Saquestatus fss (nolock),
 --  Usuario.Usuario u (nolock) 
 --Where 
 --   FS.[id] = FSS.[saqueid]
 --  and fs.[UsuarioID] = u.[ID] 
 --   and fs.[data] >= @DataFim and fs.[data] < @DataFim --PARAMETRO
 --   and 9 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
 --  AND
 --    (
 -- (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
 -- (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
 -- ) 

SET @StatusID = 5;
WITH UltimoStatusSaque AS (
select distinct  SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK)
) 
SELECT @Vlr_Total_Estornado = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1

WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)
 ---- Valor Total Estornado
 --Select 
 --  @Vlr_Total_Estornado = sum(fs.liquido)
 --From 
 --  Financeiro.Saque fs (nolock),
 --  Financeiro.Saquestatus fss (nolock),
 --  Usuario.Usuario u (nolock) 
 --Where 
 --  FS.[id] = FSS.[saqueid]
 --  and fs.[UsuarioID] = u.[ID] 
 --  and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
 --  and 5 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
 --  AND
 --    (
 -- (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
 -- (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
 -- ) 

SET @StatusID = 4;
WITH UltimoStatusSaque AS (
select distinct SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY FSS.SAQUEID order by FSS.DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK) FSS
inner join FINANCEIRO.SAQUE (NOLOCK) FS  ON FS.ID = FSS.SaqueID
WHERE FS.DATA >= @DataIni
AND FS.DATA < @DataFim
) 
SELECT @Vlr_Total_Cancelado = sum(fs.total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1

WHERE FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)
 -- Valor Total Cancelado
 --Select 
 --  @Vlr_Total_Cancelado = sum(fs.liquido)
 --From 
 --  Financeiro.Saque fs (nolock) ,
 --  Financeiro.Saquestatus fss (nolock) ,
 --  Usuario.Usuario u (nolock) 
 --Where 
 --  FS.[id] = FSS.[saqueid]
 --  and fs.[UsuarioID] = u.[ID] 
 --  and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
 --  and 4 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
 --  AND
 --    (
 -- (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
 -- (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
 -- ) 


SET @StatusID = 2;
WITH UltimoStatusSaque AS (
select distinct  SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK)
) 
SELECT @Vlr_Total_Processando = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1

WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)
-- -- Valor Total Processando
--Select
--   @Vlr_Total_Processando = sum(fs.liquido) 
-- From 
--   Financeiro.Saque fs (nolock) ,
--   Financeiro.Saquestatus fss (nolock) ,
--   Usuario.Usuario u (nolock) 
-- Where 
--   FS.[id] = FSS.[saqueid]
--   and fs.[UsuarioID] = u.[ID] 
--   and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
--   and 2 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
--   AND
--     (
--  (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
--  (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
--  ) 

SET @StatusID = 6;
WITH UltimoStatusSaque AS (
select distinct  SAQUEID, STATUSID,
	RANK()  over ( PARTITION BY SAQUEID order by DATA desc) Ordem
from FINANCEIRO.SAQUESTATUS (NOLOCK)
) 
SELECT @Vlr_Total_Aprovado = sum(FS.Total)
FROM FINANCEIRO.SAQUE (NOLOCK) FS 
INNER JOIN USUARIO.USUARIO (NOLOCK) U ON U.ID = FS.USUARIOID 
INNER JOIN UltimoStatusSaque US ON US.SAQUEID = FS.ID
							AND US.STATUSID = @StatusID
							and US.Ordem = 1

WHERE FS.USUARIOID = U.ID AND FS.USUARIOID = U.ID AND FS.DATA >= @DataIni
AND FS.DATA < @DataFim
AND
    (
(@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
(@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
)
 -- Valor Total Aprovado
 --Select 
 --  @Vlr_Total_Aprovado = sum(fs.liquido) 
 --From 
 --  Financeiro.Saque fs (nolock) ,
 --  Financeiro.Saquestatus fss (nolock) ,
 --  Usuario.Usuario u (nolock) 
 --Where 
 --  FS.[id] = FSS.[saqueid]
 --  and fs.[UsuarioID] = u.[ID] 
 --  and fs.[data] >= @DataIni and fs.[data] < @DataFim --PARAMETRO
 --  and 6 = (select Top 1 fssx.[statusid]  From Financeiro.Saquestatus fssx (nolock)  Where fssx.[saqueid] = fs.[id] order by fssx.[DATA] DESC)
 --     AND
 --    (
 -- (@PorAssinatura = 0 AND (@UsuarioID = 0 OR u.ID = @UsuarioID)) OR
 -- (@PorAssinatura = 1 AND u.Assinatura not like (@Assinatura + '%'))
 -- ) 

Select
 ISNULL(@Qtd_Total,  0) QuantidadeTotal,
   ISNULL(@Vlr_Liquido , 0.0)  Valorliquido,
   ISNULL(@Vlr_Total_Pago_BTC , 0.0) ValorTotalPagoBTC,
   ISNULL(@Vlr_Total_Pago_MAN , 0.0)  ValorTotalPagoMAN,
   ISNULL(@Vlr_Total_Aviso , 0.0) ValorTotalAviso,
   ISNULL(@Vlr_Total_Estornado , 0.0)  ValorTotalEstornado,
   ISNULL(@Vlr_Total_Cancelado , 0.0)  ValorTotalCancelado,
   ISNULL(@Vlr_Total_Processando , 0.0) ValorTotalProcessando,
   ISNULL(@Vlr_Total_Aprovado , 0.0) ValorTotalAprovado

End -- Sp

GO
