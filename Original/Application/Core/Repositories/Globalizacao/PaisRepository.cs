using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Globalizacao
{
    public class PaisRepository : CachedRepository<Entities.Pais>
    {

        public PaisRepository(DbContext context)
            : base(context)
        {
        }

        public IEnumerable<Entities.Pais> GetDisponiveis()
        {
            return cachedRepository.Where(p => p.Disponivel).OrderBy(p => p.Nome).ToList(); ;
        }

        public IEnumerable<Entities.Pais> GetByBloco(int blocoID)
        {
            return cachedRepository.Where(p => p.BlocoID == blocoID);
        }

        public Entities.Pais GetBySigla(string siglaComnpleta)
        {
            var split = siglaComnpleta.Split('-');
            var sigla = "BR";

            if (split.Any())
            {
                sigla = split[split.Length - 1];
            }
            try
            {
                return cachedRepository.FirstOrDefault(p => p.Disponivel && p.Sigla == sigla);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Entities.Pais GetPadrao()
        {
            return cachedRepository.FirstOrDefault(p => p.Disponivel && p.Padrao);
        }

        /// <summary>
        /// Obtem sigla dado o nome do país
        /// </summary>
        /// <param name="nome">nome do país</param>
        /// <returns>sigla</returns>
        public string GetSigla(string nome)
        {
            string sigla = "BR";

            if (!String.IsNullOrEmpty(nome))
            {
                nome = nome.ToLower();
                var pais = cachedRepository.FirstOrDefault(p => p.Disponivel && p.Nome.ToLower() == nome);
                if (pais != null)
                {
                    sigla = pais.Sigla;
                }
            }

            return sigla != null ? sigla : "BR";
        }

        /// <summary>
        /// Obtem id dado o nome
        /// </summary>
        /// <param name="nome">nome do pais</param>
        /// <returns>id</returns>
        public int GetID(string nome)
        {
            nome = nome.ToLower();
            var pais = cachedRepository.FirstOrDefault(e => e.Nome.ToLower() == nome);
            int PaisID = 0;

            if (pais != null)
            {
                PaisID = pais.ID;
            }

            return PaisID;
        }

    }
}
