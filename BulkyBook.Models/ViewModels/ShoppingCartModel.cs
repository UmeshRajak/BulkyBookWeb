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
    public class ShoppingCartModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public ProductModel productModel { get; set; }

        [Range(0, 1000, ErrorMessage ="Please Enter the value between 1-1000")]
        public int Count { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser applicationUser { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }
}
// access id from class name like ApplicationUserId