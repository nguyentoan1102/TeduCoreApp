using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using TeduCoreApp.Authorization;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService userService;
        private readonly IAuthorizationService authorizationService;

        public UserController(IUserService userService, IAuthorizationService authorizationService)
        {
            this.userService = userService;
            this.authorizationService = authorizationService;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            else
            {
                await userService.DeleteAsync(id);
                return new OkObjectResult(id);
            }
        }

        public IActionResult GetAllAsync()
        {
            var model = userService.GetAllAsync();
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = userService.GetAllPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var model = await userService.GetById(id);
            return new OkObjectResult(model);
        }

        public async Task<IActionResult> Index()
        {
            var result = await authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            if (result.Succeeded == false)
                return new RedirectResult("/Admin/Login/Index");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(AppUserViewModel userVm)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                if (userVm.Id == null)
                {
                    await userService.AddAsync(userVm);
                }
                else
                {
                    await userService.UpdateAsync(userVm);
                }
                return new OkObjectResult(userVm);
            }
        }
    }
}