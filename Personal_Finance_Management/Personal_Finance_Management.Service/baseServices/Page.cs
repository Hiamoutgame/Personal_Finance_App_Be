using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Personal_Finance_Management.Service.baseServices
{
    public class Page<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new();

    }
    public class PaginationMetadata
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}