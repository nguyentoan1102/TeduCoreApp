using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Data.EF.Repositories
{
    public class WolePriceRepository : EFRepository<WholePrice, int>, IWolePriceRepository
    {
        public WolePriceRepository(AppDbContext context) : base(context)
        {
        }
    }
}