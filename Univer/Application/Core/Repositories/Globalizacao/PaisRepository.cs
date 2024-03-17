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

        public Entities.Pais GetBySigla(string sigla)
        {
            Entities.Pais pais = null;
            
            try
            {
                if(sigla == "en-US")
                {
                    return cachedRepository.FirstOrDefault(p => p.ID == 476); //USA
                }
                else if(sigla =="es-ES") 
                {
                    return cachedRepository.FirstOrDefault(p => p.ID == 449); //Spain
                }
                else
                {
                    return cachedRepository.FirstOrDefault(p => p.ID == 1); //Brazil
                }
            }
            catch (Exception)
            {
                pais = GetPadrao();
                return pais;
            }
        }

        public Entities.Pais GetPadrao()
        {
            try
            {
                return cachedRepository.FirstOrDefault(p => p.ID == 476); //USA
            }
            catch (Exception)
            {
                return null;
            }
            
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
