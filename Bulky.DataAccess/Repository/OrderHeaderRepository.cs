using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader> ,IOrderHeaderRepository
    {
        private  ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db) 
        {
            _db= db;
        }
        

        public void Update(OrderHeader obj)
        {
           _db.orderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string OrderStatus, string? paymentStatus = null)
        {
            var OrderFromDb=_db.orderHeaders.FirstOrDefault(x=> x.Id == id);
            if(OrderFromDb != null)
            {
                OrderFromDb.OrderStatus = OrderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    OrderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string SessionId, string paymentIntentId)
        {
            var OrderFromDb = _db.orderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(SessionId))
            {
                OrderFromDb.SessionId = SessionId;
            }

            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                OrderFromDb.PaymentIntendId = paymentIntentId;
                OrderFromDb.OrderDate = DateTime.Now;
            }

        }
    }
}
