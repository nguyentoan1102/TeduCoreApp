using System;
using System.Collections.Generic;
using System.Text;

namespace TeduCoreApp.Utilities.Dtos
{
    public class PagedResultBase
    {
        public int CurentPage { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage => (CurentPage - 1) * PageSize + 1;
        public int LastRowOnPage => Math.Min(CurentPage * PageSize, RowCount);

        public int PageCount => (int)Math.Ceiling((double)RowCount / PageSize);
    }
}