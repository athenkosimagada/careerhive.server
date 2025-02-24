using AutoMapper;
using careerhive.application.DTOs;
using careerhive.application.DTOs.Request;
using careerhive.application.DTOs.Response;
using careerhive.domain.Entities;

namespace careerhive.application.Mappings;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserInfoDto>();
        CreateMap<UpdateUserInfoRequestDto, User>();
        CreateMap<Job, JobResponseDto>();
        CreateMap<User, UserResponseDto>();
    }
}
