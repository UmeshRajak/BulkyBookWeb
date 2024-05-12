using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class ProductVM
    {
        public ProductModel productModel { get; set; }
        
        [ValidateNever]
        public IEnumerable<SelectListItem> categoryList { get; set; }
    }
}
