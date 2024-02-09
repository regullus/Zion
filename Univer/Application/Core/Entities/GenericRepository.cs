using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Abstract;
using Core.Entities;

namespace Core.Entities
{
   public class GenericRepository<T> : IDisposable, IRepository<T> where T : class
   {
      protected DbContext ContextFactory;
      private bool disposed = false;

      public GenericRepository()
      {
         ContextFactory = new YLEVELEntities();
      }

      public IEnumerable<T> ExecWithStoreProcedure(string query, params object[] parameters)
      {
         return ContextFactory.Database.SqlQuery<T>(query, parameters).ToList<T>();
      }

      public IEnumerable<T> ExecWithStoreProcedure(string query)
      {
         return ContextFactory.Database.SqlQuery<T>(query).ToList<T>();
      }

      public IEnumerable<T> GetAll()
      {
          IEnumerable<T> query = ContextFactory.Set<T>().AsQueryable<T>();
         return query;
      }

      public T GetByKey(Expression<Func<T, bool>> predicate)
      {
         var query = ContextFactory.Set<T>().Where(predicate).FirstOrDefault();
         return query;
      }

      public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
      {
          IEnumerable<T> query = ContextFactory.Set<T>().Where(predicate);
         return query;
      }
      
      public int Count(Expression<Func<T, bool>> predicate)
      {
         return ContextFactory.Set<T>().Where(predicate).Count();       }

      public void Add(T entity)
      {
         ContextFactory.Set<T>().Add(entity);
      }

      public void Delete(T entity)
      {
         ContextFactory.Set<T>().Remove(entity);
      }

      public void Edit(T entity)
      {
         var entry = ContextFactory.Entry<T>(entity);
         if (entry.State != System.Data.Entity.EntityState.Added)
            entry.State = System.Data.Entity.EntityState.Modified;
      }

      public void Save()
      {
         ContextFactory.SaveChanges();
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!this.disposed)
         {
            if (disposing)
            {
               ContextFactory.Dispose();
            }
         }
         this.disposed = true;
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
   }
}
