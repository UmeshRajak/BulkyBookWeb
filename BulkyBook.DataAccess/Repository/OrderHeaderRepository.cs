using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeaderModel>, IOrderHeaderRepository  //remember set<T> in Repo class
    {
        private ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeaderModel orderHeaderModel)
        {
            _context.OrderHeaders.Update(orderHeaderModel);
        }

        public void UpdateStatus(int orderheaderId, string orderstatus, string? paymentstatus = null)
        {
            var OrderFromDb = _context.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderheaderId);
            if (OrderFromDb != null)
            {
                OrderFromDb.OrderStatus = orderstatus;
                if (!string.IsNullOrEmpty(paymentstatus))
                {
                    OrderFromDb.PaymentStatus = paymentstatus;
                }
            }
        }

        public void UpdateStripePaymentID(int orderheaderId, string sessionId, string paymentIntentId)
        {
            var OrderFromDb = _context.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderheaderId);

            if (!string.IsNullOrEmpty(sessionId))
            {
                OrderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                OrderFromDb.PaymentIntentId = paymentIntentId;
                OrderFromDb.PaymentDate = DateTime.Now;
            }
        }


    }      
}
