using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Utilities.Helper;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProductQuantityService _productQuantityService;
        private readonly IProductImageService _productImageService;
        private readonly IWolePriceService _wolePriceService;

        public ProductController(IProductService productService, IProductCategoryService productCategoryService, IHostingEnvironment hostingEnvironment, IProductQuantityService productQuantityService, IProductImageService productImageService, IWolePriceService wolePriceService)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
            _hostingEnvironment = hostingEnvironment;
            _productQuantityService = productQuantityService;
            _productImageService = productImageService;
            _wolePriceService = wolePriceService;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region AJAX API

        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var model = _productCategoryService.GetAll();
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var model = _productService.GetAll();
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(int? categoryId, string keyword, int page, int pageSize)
        {
            var model = _productService.GetAllPaging(categoryId, keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var model = _productService.GetById(id);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            else
            {
                _productService.Delete(id);
                _productService.Save();
                return new OkObjectResult(id);
            }
        }

        public IActionResult SaveEntity(ProductViewModel productVm)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                productVm.SeoAlias = TextHelper.ToUnsignString(productVm.Name);
                if (productVm.Id == 0)
                {
                    productVm.DateCreated = DateTime.Now;
                    _productService.Add(productVm);
                }
                else
                {
                    productVm.DateModified = DateTime.Now;
                    _productService.Update(productVm);
                }
                _productService.Save();
                return new OkObjectResult(productVm);
            }
        }

        [HttpPost]
        public IActionResult SaveQuantities(int productId, List<ProductQuantityViewModel> quantities)
        {
            _productQuantityService.AddQuantity(productId, quantities);
            _productQuantityService.Save();
            return new OkObjectResult(quantities);
        }

        [HttpPost]
        public IActionResult ImportExcel(IList<IFormFile> files, int categoryId)
        {
            if (files != null && files.Count > 0)
            {
                var file = files[0];
                var filename = ContentDispositionHeaderValue
                                   .Parse(file.ContentDisposition)
                                   .FileName
                                   .Trim('"');
                string folder = _hostingEnvironment.WebRootPath + $@"\uploaded\excels";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string filePath = Path.Combine(folder, filename);
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _productService.ImportExcel(filePath, categoryId);
                _productService.Save();
                return new OkObjectResult(filePath);
            }
            return new NoContentResult();
        }

        [HttpPost]
        public IActionResult ExportExcel()
        {
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string directory = Path.Combine(sWebRootFolder, "export-files");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string sFileName = $"Product_{DateTime.Now:yyyyMMddhhmmss}.xlsx";
            string fileUrl = $"{Request.Scheme}://{Request.Host}/export-files/{sFileName}";
            FileInfo file = new FileInfo(Path.Combine(directory, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            var products = from p in _productService.GetAll()
                           select new
                           {
                               p.Id,
                               p.Name,
                               p.CategoryId,
                               p.Image,
                               p.Price,
                               p.PromotionPrice,
                               p.OriginalPrice,
                               p.Description,
                               p.Content,
                               p.HomeFlag,
                               p.HotFlag,
                               p.ViewCount,
                               p.Tags,
                               p.Unit,
                               ProductCategory = p.ProductCategory.Name,
                               p.SeoPageTitle,
                               p.SeoAlias,
                               p.SeoKeywords,
                               p.SeoDescription,
                               DateCreate = p.DateCreated.ToString("dd/MM/yyyy"),
                               DateModified = p.DateModified.ToString("dd/MM/yyyy"),
                               p.Status
                           };
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells["A1"].LoadFromCollection(products.ToList(), true, TableStyles.Light1);
                worksheet.Cells.AutoFitColumns();
                package.Save(); //Save the workbook.
            }
            return new OkObjectResult(fileUrl);
        }

        [HttpGet]
        public IActionResult GetQuantities(int productId)
        {
            var quantities = _productQuantityService.GetQuantities(productId);
            return new OkObjectResult(quantities);
        }

        //Image
        [HttpGet]
        public IActionResult GetImages(int productId)
        {
            var images = _productImageService.GetImages(productId);
            return new OkObjectResult(images);
        }

        [HttpPost]
        public IActionResult SaveImages(int productId, string[] images)
        {
            _productImageService.AddImages(productId, images);
            _productService.Save();
            return new OkObjectResult(images);
        }

        //WholePrice
        [HttpPost]
        public IActionResult SaveWholePrice(int productId, List<WholePriceViewModel> wholePrices)
        {
            _wolePriceService.AddWholePrice(productId, wholePrices);
            _productService.Save();
            return new OkObjectResult(wholePrices);
        }

        [HttpGet]
        public IActionResult GetWholePrices(int productId)
        {
            var wholePrices = _wolePriceService.GetWholePrices(productId);
            return new OkObjectResult(wholePrices);
        }

        #endregion AJAX API
    }
}