using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MindPlaceApi.Codes;
using MindPlaceApi.Data.Repositories;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindPlaceApi.Services
{
    public interface ITagService
    {
        Task<ServiceResponse<TagResponseDto>> CreateTagAsync(TagDto tag);
        Task<ServiceResponse<List<TagResponseDto>>> ListTagsAsync();
    }

    public class TagService : ITagService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TagService(IHttpContextAccessor httpContextAccessor, IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<ServiceResponse<List<TagResponseDto>>> ListTagsAsync()
        {
            var tags = await _repositoryWrapper.Tag.GetAll().ToListAsync();

            var sr = new ServiceResponse<List<TagResponseDto>>();
            sr.Code = 200;
            sr.Data = _mapper.Map<List<TagResponseDto>>(tags);
            sr.Success = true;
            return sr;
        }

        public async Task<ServiceResponse<TagResponseDto>> CreateTagAsync(TagDto tag)
        {
            //create new tag
            var newTag = _mapper.Map<Tag>(tag);
            newTag.CreatedBy = _httpContextAccessor.HttpContext.GetUsernameOfCurrentUser();
            //add to context...
            await _repositoryWrapper.Tag.InsertAsync(newTag);
            //save to db
            await _repositoryWrapper.SaveChangesAsync();

            var sr = new ServiceResponse<TagResponseDto>();
            sr.Data = _mapper.Map<TagResponseDto>(newTag);
            return sr;
        }
    }
}
