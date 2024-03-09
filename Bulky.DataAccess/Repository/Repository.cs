using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        { 
            _db = db;
            dbSet=_db.Set<T>();
            _db.Products.Include(u => u.category).Include(x=>x.CategoryId);
        }

        public void Add(T entity)
        {
           // _db.Set<T>().Add(entity);
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> Fillter, string? IncludeProperities = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
              query = dbSet;
                
            }
            else
            {
                 query = dbSet.AsNoTracking();
               
            }
            query = query.Where(Fillter);
            if (!string.IsNullOrEmpty(IncludeProperities))
            {
                foreach (var includeProp in IncludeProperities.Split(new char[] { ',' }
                    , StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();

        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? Fillter, string? IncludeProperities=null)
        {
            IQueryable<T> query = dbSet;
            if(Fillter != null)
            {
                query = query.Where(Fillter);

            }
            if (!string.IsNullOrEmpty(IncludeProperities))
            {
                foreach(var includeProp in IncludeProperities.Split(new char[] {','}
                    ,StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
           dbSet.Remove(entity);
        }

        public void RemoveRang(IEnumerable<T> entites)
        {
           dbSet.RemoveRange(entites);

        }
    }
}
