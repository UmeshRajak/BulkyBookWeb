using BulkyBook.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetailModel>
    {
        void Update(OrderDetailModel orderDetailModel);
    }
}
