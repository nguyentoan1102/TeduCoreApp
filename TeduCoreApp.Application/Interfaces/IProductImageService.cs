using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Application.ViewModels;
using TeduCoreApp.Application.ViewModels.Product;

namespace TeduCoreApp.Application.Interfaces
{
    public interface IProductImageService
    {
        List<ProductImageViewModel> GetImages(int productId);

        void AddImages(int productId, string[] images);
    }
}