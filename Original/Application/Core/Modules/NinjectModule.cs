using System;
using System.Collections.Generic;
using System.Data.Entity;
//using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modules
{
    public class NinjectModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<Entities.YLEVELEntities>();
        }
    }
}
