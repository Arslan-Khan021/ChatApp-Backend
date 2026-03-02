using AutoMapper;
using ChatApp.DTOs.Auth;
using ChatApp.Entities;

namespace ChatApp.Mappers
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<User,AuthResponse>();
        }
    }
}
