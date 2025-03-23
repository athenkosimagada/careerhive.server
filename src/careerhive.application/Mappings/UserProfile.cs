using AutoMapper;
using careerhive.application.DTOs.Response;
using careerhive.application.Request;
using careerhive.domain.Entities;

namespace careerhive.application.Mappings;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserInfoResponseDto>();
        CreateMap<UpdateUserInfoRequestDto, User>();
        CreateMap<Job, JobResponseDto>();
        CreateMap<User, UserResponseDto>();

        CreateMap<SavedJob, JobResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SavedJobDetails.Id))
           .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.SavedJobDetails.Title))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.SavedJobDetails.Description))
           .ForMember(dest => dest.ExternalLink, opt => opt.MapFrom(src => src.SavedJobDetails.ExternalLink))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.SavedJobDetails.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.SavedJobDetails.UpdatedAt))
           .ForMember(dest => dest.PostedByUserId, opt => opt.MapFrom(src => src.SavedJobDetails.PostedByUserId))
           .ForMember(dest => dest.PostedByUserId, opt => opt.MapFrom(src => src.SavedByUserId))
           .ForMember(dest => dest.PostedBy, opt => opt.MapFrom(src => src.SavedBy));
    }
}
