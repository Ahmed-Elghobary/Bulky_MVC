using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T- Category
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? Fillter=null, string? IncludeProperities = null);
        T Get(Expression<Func<T, bool>> Fillter, string? IncludeProperities = null,bool tracked=false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRang(IEnumerable<T> entites);
    }
}
