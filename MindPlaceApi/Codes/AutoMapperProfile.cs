using System;
using System.Linq;
using AutoMapper;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using static MindPlaceApi.Codes.AppHelper;

namespace MindPlaceApi.Codes
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<NewUserDto, AppUser>();
            CreateMap<AppUser, AbbrvUser>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} { src.LastName}"));
            CreateMap<AppUser, UserResponseDto>()
             .ForMember(dest => dest.NoOfReferrals, opt => opt.MapFrom(src => src.Referrals.Count))
             .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(r => r.Role.Name).ToList()))
             .ForMember(dest => dest.Followers, opt =>
             {
                 opt.MapFrom(src => src.UserRoles.Any(r => r.Role.Name.Contains("Patient")) ?
                                                 src.RelationshipWithProfessionals
                                                    .Where(rwf => rwf.Status == FollowStatus.CONFIRMED.ToString())
                                                    .Select(rwp => rwp.Professional).ToList() :
                                                    src.RelationshipWithPatients.Select(rwp => rwp.Patient).ToList());
             })
             .ForMember(dest => dest.Following, opt =>
             {
                 opt.MapFrom(src => src.UserRoles.Any(r => r.Role.Name.Contains("Patient")) ?
                                                   src.RelationshipWithProfessionals.Select(rwp => rwp.Professional).ToList() :
                                                   src.RelationshipWithPatients.Where(rwf => rwf.Status == FollowStatus.CONFIRMED.ToString())
                                                                               .Select(rwp => rwp.Patient)
                                                                               .ToList());
             });
             
            CreateMap<QualificationDto, Qualification>();
            CreateMap<Qualification, QualificationResponseDto>();
            CreateMap<QuestionDto, Question>();
            CreateMap<Question, QuestionResponseDto>()
                .ForMember(dest => dest.User, opt => opt.Condition(src => src.User != null))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.NoOfLikes, opt => opt.MapFrom(src => src.Likes.Count));
            CreateMap<Question, ForumQuestionResponseDto>()
                .ForMember(dest => dest.User, opt => opt.Condition(src => src.User != null))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.NoOfLikes, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuestionTags.Select(t => t.Tag.Name).ToList()));
            CreateMap<Question, ForumPostResponseDto>()
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.NoOfLikes, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuestionTags.Select(t => t.Tag.Name).ToList()))
                .ForMember(dest => dest.TopComments, opt => opt.MapFrom(src => src.Comments.OrderByDescending(c => c.CreatedOn).Take(10).ToList()));
            CreateMap<CommentDto, Comment>();
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.User, opt => opt.Condition(src => src.User != null));
            CreateMap<Comment, AbbrvCommentResponseDto>()
                .ForMember(dest => dest.User, opt => opt.Condition(src => src.User != null));
            CreateMap<Notification, NotificationResponseDto>()
                .ForPath(dest => dest.Creator.Username, opt => opt.MapFrom(src => src.CreatedBy.UserName))
                .ForPath(dest => dest.Creator.FullName,
                           opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} { src.CreatedBy.LastName}"))
                .ForPath(dest => dest.Creator.ImageUrl, opt => opt.MapFrom(src => src.CreatedBy.ImageUrl));
            //.ForMember(dest => dest.CreatedOn, opt =>
            //{
            //    opt.MapFrom(src => TimeZoneInfo.ConvertTimeFromUtc(src.CreatedOn,
            //                                                TimeZoneInfo.FindSystemTimeZoneById(src.CreatedFor.TimeZone)));
            //});
            CreateMap<Tag, TagResponseDto>();
            CreateMap<TagDto, Tag>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TagName));
            CreateMap<WorkExperience, WorkExperienceResponseDto>();
            CreateMap<WorkExperienceDto, WorkExperience>();
        }
    }
}