using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class ShoppingCartVm
    {
        public IEnumerable<ShoppingCartModel> ShoppingCartList { get; set; }

        public OrderHeaderModel orderHeaderModel { get; set; }

    }
}
