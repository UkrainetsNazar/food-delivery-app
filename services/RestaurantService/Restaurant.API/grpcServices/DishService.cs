using Grpc.Core;
using Restaurant.API.Domain.Entities;
using RestaurantGrpcService;
using Google.Protobuf.WellKnownTypes;
using Contracts.Enums;
using Restaurant.API.Domain.Services;

namespace Restaurant.API.grpcServices;

public class DishService(DishDomainService dishDomainService) : RestaurantGrpcService.DishService.DishServiceBase
{
    private readonly DishDomainService _dishDomainService = dishDomainService;

    public override async Task<DishResponse> CreateDish(DishRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var dish = new Dish
        {
            Id = Guid.TryParse(request.Id, out var parsedId) ? parsedId : Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Category = (DishCategory)request.Category,
            ImageUrl = request.ImageUrl,
            Price = (decimal)request.Price,
            RestaurantId = restaurantId
        };

        var createdDish = await _dishDomainService.CreateDishAsync(dish);

        return MapDishToResponse(createdDish);
    }

    public override async Task<DishResponse> UpdateDish(DishRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var dishToUpdate = new Dish
        {
            Id = dishId,
            Name = request.Name,
            Description = request.Description,
            Category = (DishCategory)request.Category,
            ImageUrl = request.ImageUrl,
            Price = (decimal)request.Price,
            RestaurantId = Guid.TryParse(request.RestaurantId, out var rid) ? rid : Guid.Empty
        };

        var updatedDish = await _dishDomainService.UpdateDishAsync(dishToUpdate)
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Dish not found"));
        
        return MapDishToResponse(updatedDish);
    }

    public override async Task<BoolValue> DeleteDish(DishIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.DishId, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var result = await _dishDomainService.DeleteDishAsync(dishId);

        return new BoolValue { Value = result };
    }

    public override async Task<DishResponse> GetDishById(DishIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.DishId, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var dish = await _dishDomainService.GetDishByIdAsync(dishId);

        if (dish == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Dish not found"));

        return MapDishToResponse(dish);
    }

    public override async Task<DishListResponse> GetDishesByName(DishNameRequest request, ServerCallContext context)
    {
        var dishes = await _dishDomainService.GetDishesByNameAsync(request.Name);

        var response = new DishListResponse();
        response.Dishes.AddRange(dishes.Select(MapDishToResponse));
        return response;
    }

    public override async Task<DishListResponse> GetDishesByRestaurant(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var dishes = await _dishDomainService.GetDishesByRestaurantAsync(restaurantId);

        var response = new DishListResponse();
        response.Dishes.AddRange(dishes.Select(MapDishToResponse));
        return response;
    }

    private static DishResponse MapDishToResponse(Dish dish)
    {
        return new DishResponse
        {
            Id = dish.Id.ToString(),
            Name = dish.Name,
            Description = dish.Description ?? string.Empty,
            Category = (Common.DishCategory)dish.Category,
            ImageUrl = dish.ImageUrl ?? string.Empty,
            Price = (double)dish.Price,
            RestaurantId = dish.RestaurantId.ToString()
        };
    }
}