using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class CategoryModel
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string? CategoryName { get; set; }
        [Range(1, 100)]
        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }
    }
}
