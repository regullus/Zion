using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Financeiro
{
    public class SaqueStatusRepository : PersistentRepository<Entities.SaqueStatus>
    {
        private DbContext _context;
        public SaqueStatusRepository(DbContext context)
            : base(context)
        {
            this._context = context;
        }

        public IEnumerable<Entities.SaqueStatus> GetBySaque(int saqueID)
        {
            return base.GetByExpression(s => s.SaqueID == saqueID);
        }

        public IEnumerable<Entities.SaqueStatus> GetByStatus(int saqueID, Entities.SaqueStatus.TodosStatus status)
        {
            return base.GetByExpression(s => s.SaqueID == saqueID && s.StatusID == (int)status);
        }

        public Entities.SaqueStatus GetByUltimoStatus(int saqueID)
        {
            return base.GetByExpression(s => s.SaqueID == saqueID && s.Ultimo == true).FirstOrDefault();
        }

        public List<Entities.SaqueStatus>  GetListByUltimoStatus(Entities.SaqueStatus.TodosStatus status)
        {
            return base.GetByExpression(s => s.StatusID == (int)status && s.Ultimo == true).ToList();
        }

        public bool GravaSaqueStatus(Entities.SaqueStatus saqueStatusNew)
        {
            using (var tranSaqueStatus = _context.Database.BeginTransaction())
            {
                try
                {
                    var saqueStatusOld = GetByUltimoStatus(saqueStatusNew.SaqueID);
                    if (saqueStatusOld != null)
                    {
                        saqueStatusOld.Ultimo = false;
                        Save(saqueStatusOld);
                    }

                    saqueStatusNew.Ultimo = true;
                    Save(saqueStatusNew);

                    tranSaqueStatus.Commit();
                    return true;
                }
                catch
                {
                    tranSaqueStatus.Rollback();
                    return false;
                }          
            }
        }

    }
}
