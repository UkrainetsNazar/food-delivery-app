using Grpc.Core;
using RestaurantGrpcService;
using Google.Protobuf.WellKnownTypes;
using Restaurant.API.Domain.Entities;
using Restaurant.API.Domain.Services;

namespace Restaurant.API.grpcServices;

public class RestaurantService(RestaurantDomainService domainService) : RestaurantGrpcService.RestaurantService.RestaurantServiceBase
{
    private readonly RestaurantDomainService _domainService = domainService;

    public override async Task<RestaurantResponse> CreateRestaurant(RestaurantRequest request, ServerCallContext context)
    {
        var restaurant = new SupportedRestaurant
        {
            Id = Guid.TryParse(request.Id, out var id) ? id : Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber
        };

        var created = await _domainService.CreateRestaurantAsync(restaurant);

        return MapToResponse(created);
    }

    public override async Task<RestaurantResponse> UpdateRestaurant(RestaurantRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));

        var restaurant = new SupportedRestaurant
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber
        };

        var updated = await _domainService.UpdateRestaurantAsync(restaurant)
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Restaurant not found"));
        
        return MapToResponse(updated);
    }

    public override async Task<BoolValue> DeleteRestaurant(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));

        var result = await _domainService.DeleteRestaurantAsync(id);

        return new BoolValue { Value = result };
    }

    public override async Task<RestaurantListResponse> GetAllRestaurants(Empty request, ServerCallContext context)
    {
        var restaurants = await _domainService.GetAllRestaurantsAsync();

        var response = new RestaurantListResponse();
        response.Restaurants.AddRange(restaurants.Select(MapToResponse));
        return response;
    }

    public override async Task<DishCategoryListResponse> GetRestaurantCategories(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));

        var categories = await _domainService.GetRestaurantCategoriesAsync(id);

        var response = new DishCategoryListResponse();
        response.Categories.AddRange(categories.Select(c => (Common.DishCategory)c));
        return response;
    }

    private static RestaurantResponse MapToResponse(SupportedRestaurant r)
        => new()
        {
            Id = r.Id.ToString(),
            Name = r.Name,
            Description = r.Description ?? string.Empty,
            Address = r.Address,
            PhoneNumber = r.PhoneNumber
        };
}