using AutoMapper;
using ContactAppMinimalAPI.Entity;

namespace ContactAppMinimalAPI
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Contact, ContactDTO>().ReverseMap();
        }
    }
}
