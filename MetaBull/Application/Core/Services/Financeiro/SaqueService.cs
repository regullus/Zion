using Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Repositories.Financeiro;
using System.Data.Entity;

namespace Core.Services.Financeiro
{
    public class SaqueService
    {
        private SaqueRepository saqueRepository;
        private SaqueStatusRepository saqueStatusRepository;

        public SaqueService(DbContext context)
        {
            saqueRepository = new SaqueRepository(context);
            saqueStatusRepository = new SaqueStatusRepository(context);
        }

        public Saque ObterSaquePorId(int id)
        {
            return this.saqueRepository.Get(id);
        }

        public List<Saque> ObterSaques()
        {
            return this.saqueRepository.GetByUltimoStatus(SaqueStatus.TodosStatus.Aprovado).OrderBy(p => p.ID).ToList();
        }

        public void SaqueRecebido(List<Saque> saques)
        {
            saques.ForEach(p =>
            {
                SaqueRecebido(p);
            });
        }

        public void SaqueRecebido(Saque saque)
        {
            SaqueStatus saqueStatus = new SaqueStatus();
            saqueStatus.Data = App.DateTimeZion;
            saqueStatus.SaqueID = saque.ID;
            saqueStatus.StatusID = (int)Core.Entities.SaqueStatus.TodosStatus.Processando;
            saqueStatus.Ultimo = true;

            saqueStatusRepository.GravaSaqueStatus(saqueStatus);
        }
    }
}
