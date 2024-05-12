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
    public class OrderDetailRepository : Repository<OrderDetailModel>, IOrderDetailRepository  //remember set<T> in Repo class
    {
        private ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetailModel orderDetailModel)
        {
            _context.orderDetails.Update(orderDetailModel);
        }
    }
}
