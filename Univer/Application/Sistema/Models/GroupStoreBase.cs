using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;


namespace Sistema.Models
{
    public class GroupStoreBase
    {
        public DbContext Context
        {
            get;
            private set;
        }


        public DbSet<AutenticacaoGrupo> DbEntitySet
        {
            get;
            private set;
        }


        public IQueryable<AutenticacaoGrupo> EntitySet
        {
            get
            {
                return this.DbEntitySet;
            }
        }


        public GroupStoreBase(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<AutenticacaoGrupo>();
        }


        public void Create(AutenticacaoGrupo entity)
        {
            this.DbEntitySet.Add(entity);
        }


        public void Delete(AutenticacaoGrupo entity)
        {
            this.DbEntitySet.Remove(entity);
        }


        public virtual Task<AutenticacaoGrupo> GetByIdAsync(object id)
        {
            return this.DbEntitySet.FindAsync(new object[] { id });
        }


        public virtual AutenticacaoGrupo GetById(object id)
        {
            return this.DbEntitySet.Find(new object[] { id });
        }


        public virtual void Update(AutenticacaoGrupo entity)
        {
            if (entity != null)
            {
                this.Context.Entry<AutenticacaoGrupo>(entity).State = EntityState.Modified;
            }
        }
    }
}