using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
	public class OrderVM
	{
		public OrderHeaderModel orderHeaderModel { get; set; }
		public IEnumerable<OrderDetailModel> orderDetailModels { get; set; }
	}
}
