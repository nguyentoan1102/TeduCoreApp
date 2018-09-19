using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Data.EF.Repositories
{
    public class ProductReponsitory : EFRepository<Product, int>, IProductReponsitory
    {
        public ProductReponsitory(AppDbContext context) : base(context)
        {
        }
    }
}