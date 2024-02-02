using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Abstract
{
   public interface IRepository<T> where T : class
   {
      IEnumerable<T> ExecWithStoreProcedure(string query, params object[] parameters);
      IEnumerable<T> ExecWithStoreProcedure(string query);
      IEnumerable<T> GetAll();
      T GetByKey(Expression<Func<T, bool>> predicate);
      IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);
      void Add(T entity);
      void Delete(T entity);
      void Edit(T entity);
      void Save();
   }
}
