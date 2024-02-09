using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class ValidacaoCadastroRepository
    {
        DbContext _context;

        public ValidacaoCadastroRepository(DbContext context)
        {
            _context = context;
        }


        public void SalvarStatus(int usuarioID, bool ok)
        {
            try
            {
                _context.Database.SqlQuery<bool>("EXEC sp_ValidacaoCadastro_Insert @ID, @Ok",
                    new SqlParameter("@ID", SqlDbType.Int) { Value = usuarioID },
                    new SqlParameter("@Ok", SqlDbType.Bit) { Value = ok }
                    ).FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
        }

        public bool ObtemStatus(int usuarioID)
        {
            try
            {
                var count =  _context.Database.SqlQuery<int>("EXEC sp_ValidacaoCadastroUsuario_Select @ID",
                    new SqlParameter("@ID", SqlDbType.Int) { Value = usuarioID }
                    ).FirstOrDefault();

                return count > 0;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
