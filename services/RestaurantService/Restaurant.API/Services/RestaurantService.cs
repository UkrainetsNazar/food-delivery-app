using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;
using RestaurantGrpcService;
using Google.Protobuf.WellKnownTypes;

namespace Restaurant.API.Services;

public class RestaurantService(RestaurantDbContext context) : RestaurantGrpcService.RestaurantService.RestaurantServiceBase
{
    private readonly RestaurantDbContext _context = context;

    public override async Task<RestaurantResponse> CreateRestaurant(RestaurantRequest request, ServerCallContext context)
    {
        var restaurant = new SupportedRestaurant
        {
            Id = Guid.Parse(request.Id),
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber
        };

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        return new RestaurantResponse
        {
            Id = restaurant.Id.ToString(),
            Name = restaurant.Name,
            Description = restaurant.Description ?? string.Empty,
            Address = restaurant.Address,
            PhoneNumber = restaurant.PhoneNumber
        };
    }

    public override async Task<RestaurantResponse> UpdateRestaurant(RestaurantRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var existingRestaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (existingRestaurant == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Restaurant not found"));
        }

        existingRestaurant.Name = request.Name;
        existingRestaurant.Description = request.Description;
        existingRestaurant.Address = request.Address;
        existingRestaurant.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync();

        return new RestaurantResponse
        {
            Id = existingRestaurant.Id.ToString(),
            Name = existingRestaurant.Name,
            Description = existingRestaurant.Description ?? string.Empty,
            Address = existingRestaurant.Address,
            PhoneNumber = existingRestaurant.PhoneNumber
        };
    }

    public override async Task<BoolValue> DeleteRestaurant(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var restaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (restaurant == null)
        {
            return new BoolValue { Value = false };
        }

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        return new BoolValue { Value = true };
    }

    public override async Task<RestaurantListResponse> GetAllRestaurants(Empty request, ServerCallContext context)
    {
        var restaurants = await _context.Restaurants.AsNoTracking().ToListAsync();

        var response = new RestaurantListResponse();
        response.Restaurants.AddRange(restaurants.Select(r => new RestaurantResponse
        {
            Id = r.Id.ToString(),
            Name = r.Name,
            Description = r.Description ?? string.Empty,
            Address = r.Address,
            PhoneNumber = r.PhoneNumber
        }));

        return response;
    }

    public override async Task<DishCategoryListResponse> GetRestaurantCategories(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var categories = await _context.Dishes
            .AsNoTracking()
            .Where(d => d.RestaurantId == restaurantId)
            .Select(d => d.Category)
            .Distinct()
            .ToListAsync();

        var response = new DishCategoryListResponse();
        response.Categories.AddRange(categories.Select(c => (Common.DishCategory)c));

        return response;
    }
}