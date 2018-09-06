using AutoMapper;
using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;

namespace TeduCoreApp.Application.Implementations
{
    public class ProductCategoryService : IProductCategoryService
    {
        private IProductCategoryRepository productCategoryRepository;
        private IUnitOfWork unitOfWork;
        public ProductCategoryService(IProductCategoryRepository productCategoryRepository, IUnitOfWork unitOfWork)
        {
            this.productCategoryRepository = productCategoryRepository;
            this.unitOfWork = unitOfWork;
        }
        public ProductCategoryViewModel Add(ProductCategoryViewModel productCategoryVm)
        {
            try
            {
                var productCategory = Mapper.Map<ProductCategoryViewModel, ProductCategory>(productCategoryVm);
                productCategoryRepository.Add(productCategory);
                return productCategoryVm;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Delete(int id) => productCategoryRepository.Remove(id);

        public List<ProductCategoryViewModel> GetAll() => productCategoryRepository.FindAll().OrderBy(x => x.ParentId).ProjectTo<ProductCategoryViewModel>().ToList();

        public List<ProductCategoryViewModel> GetAll(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                return productCategoryRepository.FindAll(x => x.Name.Contains(keyword) || x.Description.Contains(keyword))
                    .OrderBy(x => x.ParentId).ProjectTo<ProductCategoryViewModel>().ToList();
            }
            else
            {
                return productCategoryRepository.FindAll().OrderBy(x => x.ParentId).ProjectTo<ProductCategoryViewModel>().ToList();
            }
        }

        public List<ProductCategoryViewModel> GetAllByParentId(int parentId)
        {
            return productCategoryRepository.FindAll(x => x.Status == Status.Active
            && x.ParentId == parentId).ProjectTo<ProductCategoryViewModel>().ToList();
        }

        public ProductCategoryViewModel GetById(int id)
        {
            return Mapper.Map<ProductCategory, ProductCategoryViewModel>(productCategoryRepository.FindById(id));
        }

        public List<ProductCategoryViewModel> GetHomeCategories(int top)
        {
            var categories = productCategoryRepository
                .FindAll(x => x.HomeFlag == true, c => c.Products)
                  .OrderBy(x => x.HomeOrder)
                  .Take(top).ProjectTo<ProductCategoryViewModel>();

            return categories.ToList();
        }

        public void ReOrder(int sourceId, int targetId)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            unitOfWork.Commit();
        }

        public void Update(ProductCategoryViewModel productCategoryVm)
        {
            throw new NotImplementedException();
        }

        public void UpdateParentId(int sourceId, int targetId, Dictionary<int, int> items)
        {
            throw new NotImplementedException();
        }
    }
}
