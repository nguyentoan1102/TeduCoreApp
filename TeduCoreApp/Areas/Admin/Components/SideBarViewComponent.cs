using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.System;
using TeduCoreApp.Extensions;
using TeduCoreApp.Utilities.Constants;

namespace TeduCoreApp.Areas.Admin.Components
{
    public class SideBarViewComponent : ViewComponent
    {
        private readonly IFunctionService functionService;

        public SideBarViewComponent(IFunctionService functionService)
        {
            this.functionService = functionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var roles = ((ClaimsPrincipal)User).GetSpecificClaim("Roles");
            List<FunctionViewModel> functions;
            if (roles.Split(";").Contains(CommonConstants.AppRole.AdminRole))
            {
                functions = await functionService.GetAll(string.Empty);
            }
            else
            {
                //TODO: Get by permission
                functions = await functionService.GetAll(string.Empty);
            }
            return View(functions);
        }
    }
}