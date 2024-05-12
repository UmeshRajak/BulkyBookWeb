using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbset = _context.Set<T>();             //dbset takes everything of set model class
        }

        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeproperties = null, bool tracked = false)
        {
            if (tracked)
            {
                IQueryable<T> query = dbset;
                query = query.Where(filter);
                if (!string.IsNullOrEmpty(includeproperties))
                {
                    foreach (var includeProp in includeproperties
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }
                return query.FirstOrDefault();
            }
            else
            {
                IQueryable<T> query = dbset.AsNoTracking();
                query = query.Where(filter);
                if (!string.IsNullOrEmpty(includeproperties))
                {
                    foreach (var includeProp in includeproperties
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }
                return query.FirstOrDefault();
            }
            

        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeproperties = null)
        {
            IQueryable<T> query = dbset;
            if(filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach (var includeProp in includeproperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }
    }
}
