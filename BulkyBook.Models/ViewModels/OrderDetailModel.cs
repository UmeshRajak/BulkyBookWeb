using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class OrderDetailModel
    {
        [Key]
        public int OrderDetailId { get; set; }
        public int OrderHeaderId { get; set; }
        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        public OrderHeaderModel orderHeaderModel { get; set; }
        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public ProductModel productModel { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }
    }
}
