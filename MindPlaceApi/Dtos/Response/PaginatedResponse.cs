using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Dtos.Response
{
    public class PaginatedResponse<U> 
        where U : class
    {
        public List<U> Data { get; set; }
        public PaginationMetaData Meta { get; private set; }


        public PaginatedResponse(List<U> items, int totalCount, int pageNumber, int pageSize)
        {
            Data = items;

            var metaInfo = new PaginationMetaData()
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            Meta = metaInfo;
            
        }

        public static async Task<PaginatedResponse<U>> ToPagedListAsync<T>(IQueryable<T> source, int pageNumber, int pageSize, IMapper _mapper)
            where T : class
        {
            var count = source.Count();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            //map data/items
            var data = _mapper.Map<List<U>>(items);
            return new PaginatedResponse<U>(data, count, pageNumber, pageSize);
        }
    }

    public struct PaginationMetaData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
