using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OfficeOpenXml;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Utilities.Extensions;
using TeduCoreApp.Utilities.Helper;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class BillController : BaseController
    {
        private readonly IBillService _billService;

        /// <summary>
        /// The hosting enviroment
        /// </summary>
        private readonly IHostingEnvironment _hostingEnviroment;

        public BillController(IBillService billService, IHostingEnvironment hostingEnvironment)
        {
            _billService = billService;
            _hostingEnviroment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API

        [HttpGet]
        public IActionResult GetById(int id)
        {
            return new OkObjectResult(_billService.GetDetail(id));
        }

        [HttpGet]
        public IActionResult UpdateStatus(int billId, BillStatus billStatus)
        {
            _billService.UpdateStatus(billId, billStatus);
            return new OkResult();
        }

        [HttpPost]
        public IActionResult SaveEntity(BillViewModel billViewModel)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allError = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allError);
            }
            if (billViewModel.Id == 0)
            {
                _billService.Create(billViewModel);
            }
            else
            {
                _billService.Update(billViewModel);
            }
            _billService.Save();
            return new OkObjectResult(billViewModel);
        }

        [HttpGet]
        public IActionResult GetPaymentMethod()
        {
            var enums = ((PaymentMethod[])Enum.GetValues(typeof(PaymentMethod)))
                .Select(c => new EnumModel()
                {
                    Value = (int)c,
                    Name = c.GetDescription()
                });
            return new OkObjectResult(enums.ToList());
        }

        [HttpGet]
        public IActionResult GetAllPaging(string startDate, string endDate, string keyword, int page, int pageSize)
        {
            var model = _billService.GetAllPaging(startDate, endDate, keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetBillStatus()
        {
            var enums = ((BillStatus[])Enum.GetValues(typeof(BillStatus)))
                .Select(c => new EnumModel()
                {
                    Value = (int)c,
                    Name = c.GetDescription()
                });
            return new OkObjectResult(enums.ToList());
        }

        [HttpGet]
        public IActionResult GetSizes()
        {
            return new OkObjectResult(_billService.GetSizes());
        }

        [HttpGet]
        public IActionResult GetColors()
        {
            return new OkObjectResult(_billService.GetColors());
        }

        public IActionResult ExportExcel(int billId)
        {
            string sWebRootFolder = _hostingEnviroment.WebRootPath;
            string sFileName = $"Bill_{billId}.xlsx";
            // Template File
            string templateDocument = Path.Combine(sWebRootFolder, "templates", "BillTemplate.xlsx");

            string url = $"{Request.Scheme}://{Request.Host}/{"export-files"}/{sFileName}";
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, "export-files", sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    // add a new worksheet to the empty workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["TEDUOrder"];
                    // Data Acces, load order header data.
                    var billDetail = _billService.GetDetail(billId);

                    // Insert customer data into template
                    worksheet.Cells[4, 1].Value = "Customer Name: " + billDetail.CustomerName;
                    worksheet.Cells[5, 1].Value = "Address: " + billDetail.CustomerAddress;
                    worksheet.Cells[6, 1].Value = "Phone: " + billDetail.CustomerMobile;
                    // Start Row for Detail Rows
                    int rowIndex = 9;

                    // load order details
                    var orderDetails = _billService.GetBillDetails(billId);
                    int count = 1;
                    foreach (var orderDetail in orderDetails)
                    {
                        // Cell 1, Carton Count
                        worksheet.Cells[rowIndex, 1].Value = count.ToString();
                        // Cell 2, Order Number (Outline around columns 2-7 make it look like 1 column)
                        worksheet.Cells[rowIndex, 2].Value = orderDetail.Product.Name;
                        // Cell 8, Weight in LBS (convert KG to LBS, and rounding to whole number)
                        worksheet.Cells[rowIndex, 3].Value = orderDetail.Quantity.ToString();

                        worksheet.Cells[rowIndex, 4].Value = orderDetail.Price.ToString("N0");
                        worksheet.Cells[rowIndex, 5].Value = (orderDetail.Price * orderDetail.Quantity).ToString("N0");
                        // Increment Row Counter
                        rowIndex++;
                        count++;
                    }
                    decimal total = (decimal)(orderDetails.Sum(x => x.Quantity * x.Price));
                    worksheet.Cells[24, 5].Value = total.ToString("N0");

                    var numberWord = "Total amount (by word): " + TextHelper.ToString(total);
                    worksheet.Cells[26, 1].Value = numberWord;
                    var billDate = billDetail.DateCreated;
                    worksheet.Cells[28, 3].Value = billDate.Day + ", " + billDate.Month + ", " + billDate.Year;

                    package.SaveAs(file); //Save the workbook.
                }
            }
            return new OkObjectResult(url);
        }

        #endregion API
    }
}