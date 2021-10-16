using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos
{
    public class FilterParameter
    {
        const int maxPageSize = 30;
        public string SearchText { get; set; }
        private int _pageNumber = 1;
        public int PageNumber 
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                _pageNumber = (value < 1) ? 1 : value;
            }
        }
        private int _pageSize = 10;
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

        internal bool CanSearchText
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SearchText);
            }
        }
    }

    public class QuestionFilterParams : FilterParameter
    {
        public string FilterText { get; set; }
    }
}
