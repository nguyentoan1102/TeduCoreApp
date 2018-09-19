using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductReponsitory productReponsitory;

        public ProductService(IProductReponsitory productReponsitory)
        {
            this.productReponsitory = productReponsitory;
        }

        public List<ProductViewModel> GetAll()
        {
            return productReponsitory.FindAll(x => x.ProductCategory).ProjectTo<ProductViewModel>().ToList();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public PagedResult<ProductViewModel> GetAllPaging(int? categoryId, string keyword, int page, int pageSize)
        {
            var query = productReponsitory.FindAll(x => x.Status == Status.Active);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }
            int totalRow = query.Count();
            query = query.OrderByDescending(x => x.DateCreated)
                .Skip((page - 1) * pageSize).Take(pageSize);
            List<ProductViewModel> data = query.ProjectTo<ProductViewModel>().ToList();
            var pagingationSet = new PagedResult<ProductViewModel>()
            {
                Results = data,
                CurentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };
            return pagingationSet;
        }
    }
}