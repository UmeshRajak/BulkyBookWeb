using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CompanyRepository :Repository<CompanyModel>, ICompanyRepository
    {
        private ApplicationDbContext _context;
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(CompanyModel cm)
    {
        _context.companies.Update(cm);
    }

}
}
