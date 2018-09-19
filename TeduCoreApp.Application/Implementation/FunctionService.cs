using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.System;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Application.Implementation
{
    public class FunctionService : IFunctionService
    {
        private readonly IFunctionRepository functionReponsitory;

        public FunctionService(IFunctionRepository functionReponsitory)
        {
            this.functionReponsitory = functionReponsitory;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public Task<List<FunctionViewModel>> GetAll()
        {
            return functionReponsitory.FindAll().ProjectTo<FunctionViewModel>().ToListAsync();
        }

        public List<FunctionViewModel> GetAllByPermission(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}