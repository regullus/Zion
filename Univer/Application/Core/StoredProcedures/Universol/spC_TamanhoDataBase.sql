use UniverDev
go
If Exists (Select 'Sp' From sysobjects Where id = object_id('spC_TamanhoDataBase'))
   Drop Procedure spC_TamanhoDataBase
go

Create  Proc [dbo].[spC_TamanhoDataBase]

As
-- =============================================================================================
-- Author.....: 
-- Create date: 
-- Description: Obtem niveis no tabuleito de um usuario
-- =============================================================================================
BEGIN
   --Necessario para o entity reconhecer retorno de select com tabela temporaria
   Set FMTONLY OFF
   Set nocount on
   
   SELECT 
          database_name = DB_NAME(database_id)
        , log_size_mb = CAST(SUM(CASE WHEN type_desc = 'LOG' THEN size END) * 8. / 1024 AS DECIMAL(18,2))
        , row_size_mb = CAST(SUM(CASE WHEN type_desc = 'ROWS' THEN size END) * 8. / 1024 AS DECIMAL(18,2))
        , total_size_mb = CAST(SUM(size) * 8. / 1024 AS DECIMAL(18,2))
    Into #temp
    FROM 
        sys.master_files WITH(NOWAIT)
    -- where database_id = DB_ID('Nomedoseubanco') -- Caso queira filtrar um DB específico
    GROUP BY database_id
    order by row_size_mb desc

    Select 
        SUM(total_size_mb) Total_mb
    From
        #temp
    
    Select 
        *
    From
        #temp
End -- Sp

go
Grant Exec on spC_TamanhoDataBase To public
go


Exec spC_TamanhoDataBase


