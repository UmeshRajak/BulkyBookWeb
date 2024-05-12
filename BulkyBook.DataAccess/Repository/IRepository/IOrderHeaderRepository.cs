using BulkyBook.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeaderModel>
    {
        void Update(OrderHeaderModel orderHeaderModel);
        void UpdateStatus(int orderheaderId, string orderstatus, string? paymentstatus=null);
        void UpdateStripePaymentID(int orderheaderId, string sessionId, string paymentIntentId);
    }
}
