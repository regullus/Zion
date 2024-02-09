using Core.Entities;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Rede
{
    public class PontosBinarioRepository : PersistentRepository<Entities.PontosBinario>
    {
        private DbContext _context;
        public PontosBinarioRepository(DbContext context)
            : base(context)
        {
            _context = context;
        }

        public IEnumerable<Entities.PontosBinario> GetByUsuario(int usuarioID, DateTime? dataInicial = null, DateTime? dataFinal = null)
        {
            var pontosBinario = base.GetByExpression(b => b.UsuarioID == usuarioID);

            if (dataInicial.HasValue)
            {
                pontosBinario = pontosBinario.Where(b => b.DataReferencia >= dataInicial.Value);
            }

            if (dataFinal.HasValue)
            {
                pontosBinario = pontosBinario.Where(b => b.DataReferencia <= dataFinal.Value);
            }

            return pontosBinario;
        }

        public double AcumuladoDireita(int idUsuario)
        {
            double retorno = 0;
            var pontosBinario = this.GetByExpression(x => x.UsuarioID == idUsuario && x.Lado == 1).Sum(x => x.ValorCripto);
            if (pontosBinario != null)
            {            
                retorno += (double)pontosBinario;           
            }
            else
            {
                retorno = 0;
            }

            return retorno;
        }

        public double AcumuladoEsquerda(int idUsuario)
        {
            double retorno = 0;
            var pontosBinario = this.GetByExpression(x => x.UsuarioID == idUsuario && x.Lado == 0).Sum(x => x.ValorCripto);
            if (pontosBinario != null)
            {               
                retorno += (double)pontosBinario;   
            }
            else
            {
                retorno = 0;
            }

            return retorno;
        }
    }
}

