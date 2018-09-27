using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Data.EF.Repositories
{
    public class ProductQuantityReponsitory : EFRepository<ProductQuantity, int>, IProductQuatityReponsitory
    {
        public ProductQuantityReponsitory(AppDbContext context) : base(context)
        {
        }
    }
}