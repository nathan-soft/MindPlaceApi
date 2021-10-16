using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LifeLongApi.Dtos
{
    public class PaginationFilter
    {
        const int maxPageSize = 50;
        private int _pageSize = 10;
        public string SearchText { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        internal int SkipCount => (PageNo - 1) * PageSize ;

       
        //public string SortField { get; set; }
        //example: ?Username:asc, StartDate:desc
        internal bool CanSearch
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SearchText);
            }
        }
    }
}
