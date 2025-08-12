using AutoMapper;
using Contracts.DTO;
using RestaurantGrpcService;

namespace AdminPanelService.Mappings;

public class RestaurantManagementProfile : Profile
{
    public RestaurantManagementProfile()
    {
        CreateMap<RestaurantRequest, RestaurantDto>().ReverseMap();
        CreateMap<RestaurantResponse, RestaurantDto>().ReverseMap();

        CreateMap<DishRequest, DishDto>().ReverseMap();
        CreateMap<DishResponse, DishDto>().ReverseMap();
    }
}
