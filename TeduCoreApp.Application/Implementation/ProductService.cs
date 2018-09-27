using AutoMapper;
using AutoMapper.QueryableExtensions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.EF.Repositories;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Constants;
using TeduCoreApp.Utilities.Dtos;
using TeduCoreApp.Utilities.Helper;

namespace TeduCoreApp.Application.Implementation
{
    public class ProductService : IProductService, IProductImageService, IWolePriceService
    {
        private readonly IProductRepository productRepository;
        private readonly IUnitOfWork unitOfWork;

        private ITagRepository tagRepository;
        private readonly IWolePriceRepository wolePriceRepository;
        private readonly IProductTagRepository productTagRepository;
        private readonly IProductImagesRepository productImagesRepository;

        public ProductService(IProductRepository productRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, IProductTagRepository productTagRepository, IProductImagesRepository productImagesRepository, IWolePriceRepository wolePriceRepository)
        {
            this.productRepository = productRepository;
            this.tagRepository = tagRepository;
            this.productTagRepository = productTagRepository;
            this.unitOfWork = unitOfWork;
            this.productImagesRepository = productImagesRepository;
            this.wolePriceRepository = wolePriceRepository;
        }

        public List<ProductViewModel> GetAll()
        {
            return productRepository.FindAll(x => x.ProductCategory).ProjectTo<ProductViewModel>().ToList();
        }

        public ProductViewModel Add(ProductViewModel productVm)
        {
            List<ProductTag> productTags = new List<ProductTag>();
            if (!string.IsNullOrEmpty(productVm.Tags))
            {
                string[] tags = productVm.Tags.Split(',');
                foreach (string t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!tagRepository.FindAll(x => x.Id == tagId).Any())
                    {
                        Tag tag = new Tag
                        {
                            Id = tagId,
                            Name = t,
                            Type = CommonConstants.ProductTag
                        };
                        tagRepository.Add(tag);
                    }
                    ProductTag productTag = new ProductTag
                    {
                        TagId = tagId
                    };
                    productTags.Add(productTag);
                }
                var product = Mapper.Map<ProductViewModel, Product>(productVm);
                foreach (var productTag in productTags)
                {
                    product.ProductTags.Add(productTag);
                }
                productRepository.Add(product);
            }
            return productVm;
        }

        public void Delete(int id)
        {
            productRepository.Remove(id);
        }

        public void Update(ProductViewModel productVm)
        {
            List<ProductTag> productTags = new List<ProductTag>();

            if (!string.IsNullOrEmpty(productVm.Tags))
            {
                string[] tags = productVm.Tags.Split(',');
                foreach (string t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!tagRepository.FindAll(x => x.Id == tagId).Any())
                    {
                        Tag tag = new Tag
                        {
                            Id = tagId,
                            Name = t,
                            Type = CommonConstants.ProductTag
                        };
                        tagRepository.Add(tag);
                    }
                    productTagRepository.RemoveMultiple(productTagRepository.FindAll(x => x.Id == productVm.Id).ToList());
                    ProductTag productTag = new ProductTag
                    {
                        TagId = tagId
                    };
                    productTags.Add(productTag);
                }
            }

            var product = Mapper.Map<ProductViewModel, Product>(productVm);
            foreach (var productTag in productTags)
            {
                product.ProductTags.Add(productTag);
            }
            productRepository.Update(product);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public PagedResult<ProductViewModel> GetAllPaging(int? categoryId, string keyword, int page, int pageSize)
        {
            var query = productRepository.FindAll(x => x.Status == Status.Active);
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

        public void Save()
        {
            unitOfWork.Commit();
        }

        public ProductViewModel GetById(int id)
        {
            return Mapper.Map<Product, ProductViewModel>(productRepository.FindById(id));
        }

        public void ImportExcel(string filePath, int categoryId)
        {
            using (var pakage = new ExcelPackage(new System.IO.FileInfo(filePath)))
            {
                ExcelWorksheet workSheet = pakage.Workbook.Worksheets[1];
                Product product;
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    product = new Product
                    {
                        CategoryId = categoryId,

                        Name = workSheet.Cells[i, 1].Value.ToString(),

                        Description = workSheet.Cells[i, 2].Value.ToString()
                    };

                    decimal.TryParse(workSheet.Cells[i, 3].Value.ToString(), out var originalPrice);
                    product.OriginalPrice = originalPrice;

                    decimal.TryParse(workSheet.Cells[i, 4].Value.ToString(), out var price);
                    product.Price = price;
                    decimal.TryParse(workSheet.Cells[i, 5].Value.ToString(), out var promotionPrice);

                    product.PromotionPrice = promotionPrice;
                    product.Content = workSheet.Cells[i, 6].Value.ToString();
                    product.SeoKeywords = workSheet.Cells[i, 7].Value.ToString();

                    product.SeoDescription = workSheet.Cells[i, 8].Value.ToString();
                    bool.TryParse(workSheet.Cells[i, 9].Value.ToString(), out var hotFlag);

                    product.HotFlag = hotFlag;
                    bool.TryParse(workSheet.Cells[i, 10].Value.ToString(), out var homeFlag);
                    product.HomeFlag = homeFlag;

                    product.Status = Status.Active;

                    productRepository.Add(product);
                }
            }
        }

        public List<ProductImageViewModel> GetImages(int productId)
        {
            return productImagesRepository.FindAll(x => x.ProductId == productId)
                .ProjectTo<ProductImageViewModel>().ToList();
        }

        public void AddImages(int productId, string[] images)
        {
            productImagesRepository.RemoveMultiple(productImagesRepository.FindAll(x => x.ProductId == productId).ToList());
            foreach (var image in images)
            {
                productImagesRepository.Add(new ProductImage()
                {
                    Path = image,
                    ProductId = productId,
                    Caption = string.Empty
                });
            }
        }

        public void AddWholePrice(int productId, List<WholePriceViewModel> wholePrices)
        {
            wolePriceRepository.RemoveMultiple(wolePriceRepository.FindAll(x => x.ProductId == productId).ToList());
            foreach (var item in wholePrices)
            {
                wolePriceRepository.Add(new WholePrice()

                {
                    ProductId = productId,
                    FromQuantity = item.FromQuantity,
                    ToQuantity = item.ToQuantity,
                    Price = item.Price
                });
            }
        }

        /// <summary>
        /// Gets the whole prices.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public List<WholePriceViewModel> GetWholePrices(int productId) => wolePriceRepository.FindAll(x => x.ProductId == productId).ProjectTo<WholePriceViewModel>().ToList();
    }
}