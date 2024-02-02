﻿using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Usuario
{
    public class DocumentoRepository : PersistentRepository<Entities.Documento>
    {

        public DocumentoRepository(DbContext context)
            : base(context)
        {
        }

    }
}
