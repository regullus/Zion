using DomainExtension.Entities.Interfaces;
using DomainExtension.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public abstract class CachedRepository<T> where T : class, IPersistentEntity
    {

        private static PersistentRepository<T> _repository;

        private static List<T> _cachedRepository;
        internal static List<T> cachedRepository
        {
            get
            {
                if (_cachedRepository == null)
                {
                    try
                    {
                        _cachedRepository = _repository.GetAll().ToList();
                    }
                    catch (Exception)
                    {
                        _cachedRepository = null;
                    }

                }
                return _cachedRepository;
            }
        }

        public CachedRepository()
        {
        }

        public CachedRepository(DbContext context)
        {
            _repository = new PersistentRepository<T>(context);
        }

        public void Delete(int id)
        {
            var item = cachedRepository.FirstOrDefault(i => i.ID == id);
            if (item != null)
            {
                cachedRepository.Remove(item);
            }
            _repository.Delete(id);
        }

        public void Delete(T entity)
        {
            var item = cachedRepository.FirstOrDefault(i => i.ID == entity.ID);
            if (item != null)
            {
                cachedRepository.Remove(item);
            }
            _repository.Delete(entity);
        }

        public T Get(int id)
        {
            return cachedRepository.FirstOrDefault(i => i.ID == id);
        }

        public IQueryable<T> GetAll()
        {
            try
            {
                return cachedRepository.AsQueryable();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IQueryable<T> GetByExpression(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return cachedRepository.AsQueryable().Where(expression);
        }

        public void Save(T entity)
        {
            _repository.Save(entity);
            var item = cachedRepository.FirstOrDefault(i => i.ID == entity.ID);
            if (item != null)
            {
                var index = cachedRepository.IndexOf(item);
                cachedRepository[index] = entity;
            }
            else
            {
                cachedRepository.Add(entity);
            }
        }

        public static void ClearCache()
        {
            _cachedRepository = null;
        }

    }
}
