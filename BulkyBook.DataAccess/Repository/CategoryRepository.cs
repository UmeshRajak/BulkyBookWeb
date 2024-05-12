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
    public class CategoryRepository : Repository<CategoryModel>, ICategoryRepository  //remember set<T> in Repo class
    {
        private ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(CategoryModel cm)
        {
            _context.Categories.Update(cm);
        }
    }
}
