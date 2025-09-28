using AutoMapper;
using EventManagement.Models;
using EventManagement.ViewModels;
namespace EventManagement.Repositories
{
    public class Helper : Profile
    {
        public Helper()
        {
            CreateMap<User, UserViewModel>().ReverseMap();
            CreateMap<Event, EventViewModel>().ReverseMap();
        }
    }
}
