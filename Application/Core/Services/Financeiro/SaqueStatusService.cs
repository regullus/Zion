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
    public class SaqueStatusService
    {
        private SaqueStatusRepository saqueStatusRepository;

        public SaqueStatusService(DbContext context)
        {
            saqueStatusRepository = new SaqueStatusRepository(context);
        }

        public Entities.SaqueStatus GetByUltimoStatus(int saqueID)
        {
            return this.saqueStatusRepository.GetByUltimoStatus(saqueID);
        }

        public bool GravaSaqueStatus(SaqueStatus saqueStatus)
        {
            return this.saqueStatusRepository.GravaSaqueStatus(saqueStatus);
        }

        public List<SaqueStatus> ObterSaques()
        {
            return this.saqueStatusRepository.GetListByUltimoStatus(SaqueStatus.TodosStatus.Aprovado).OrderBy(p => p.ID).ToList();
        }
    }
}
