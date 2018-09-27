using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;

namespace TeduCoreApp.Application.Implementation
{
    public class ProductQuantityService : IProductQuantityService
    {
        private readonly IProductQuatityReponsitory productQuantityRepository;
        private readonly IUnitOfWork unitOfWork;

        public ProductQuantityService(IProductQuatityReponsitory productQuantityRepository, IUnitOfWork unitOfWork)
        {
            this.productQuantityRepository = productQuantityRepository;
            this.unitOfWork = unitOfWork;
        }

        public void AddQuantity(int productId, List<ProductQuantityViewModel> quantities)
        {
            productQuantityRepository.RemoveMultiple(productQuantityRepository.FindAll(x => x.ProductId == productId).ToList());
            foreach (var item in quantities)
            {
                productQuantityRepository.Add(new ProductQuantity()
                {
                    ProductId = productId,
                    ColorId = item.ColorId,
                    SizeId = item.SizeId,
                    Quantity = item.Quantity
                });
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public List<ProductQuantityViewModel> GetQuantities(int productId)
        {
            return productQuantityRepository.FindAll(x => x.ProductId == productId).ProjectTo<ProductQuantityViewModel>().ToList();
        }

        public void Save()
        {
            unitOfWork.Commit();
        }
    }
}