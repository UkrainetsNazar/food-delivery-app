using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;
using RestaurantGrpcService;
using Google.Protobuf.WellKnownTypes;
using Contracts.Enums;

namespace Restaurant.API.Services;

public class DishService(RestaurantDbContext context) : RestaurantGrpcService.DishService.DishServiceBase
{
    private readonly RestaurantDbContext _context = context;

    public override async Task<DishResponse> CreateDish(DishRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var dish = new Dish
        {
            Id = Guid.Parse(request.Id),
            Name = request.Name,
            Description = request.Description,
            Category = (DishCategory)request.Category,
            ImageUrl = request.ImageUrl,
            Price = (decimal)request.Price,
            RestaurantId = restaurantId
        };

        await _context.Dishes.AddAsync(dish);
        await _context.SaveChangesAsync();

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

    public override async Task<DishResponse> UpdateDish(DishRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var existingDish = await _context.Dishes.FindAsync(dishId);
        if (existingDish == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Dish not found"));
        }

        existingDish.Name = request.Name;
        existingDish.Description = request.Description;
        existingDish.Category = (DishCategory)request.Category;
        existingDish.ImageUrl = request.ImageUrl;
        existingDish.Price = (decimal)request.Price;

        await _context.SaveChangesAsync();

        return new DishResponse
        {
            Id = existingDish.Id.ToString(),
            Name = existingDish.Name,
            Description = existingDish.Description ?? string.Empty,
            Category = (Common.DishCategory)existingDish.Category,
            ImageUrl = existingDish.ImageUrl ?? string.Empty,
            Price = (double)existingDish.Price,
            RestaurantId = existingDish.RestaurantId.ToString()
        };
    }

    public override async Task<BoolValue> DeleteDish(DishIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.DishId, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var dish = await _context.Dishes.FindAsync(dishId);
        if (dish == null)
        {
            return new BoolValue { Value = false };
        }

        _context.Dishes.Remove(dish);
        await _context.SaveChangesAsync();

        return new BoolValue { Value = true };
    }

    public override async Task<DishResponse> GetDishById(DishIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.DishId, out var dishId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid dish ID format"));
        }

        var dish = await _context.Dishes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Dish not found"));
        }

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

    public override async Task<DishListResponse> GetDishesByName(DishNameRequest request, ServerCallContext context)
    {
        var dishes = await _context.Dishes
            .AsNoTracking()
            .Where(d => EF.Functions.ILike(d.Name!, $"%{request.Name}%"))
            .ToListAsync();

        var response = new DishListResponse();
        response.Dishes.AddRange(dishes.Select(d => new DishResponse
        {
            Id = d.Id.ToString(),
            Name = d.Name,
            Description = d.Description ?? string.Empty,
            Category = (Common.DishCategory)d.Category,
            ImageUrl = d.ImageUrl ?? string.Empty,
            Price = (double)d.Price,
            RestaurantId = d.RestaurantId.ToString()
        }));

        return response;
    }

    public override async Task<DishListResponse> GetDishesByRestaurant(RestaurantIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RestaurantId, out var restaurantId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid restaurant ID format"));
        }

        var dishes = await _context.Dishes
            .AsNoTracking()
            .Where(d => d.RestaurantId == restaurantId)
            .ToListAsync();

        var response = new DishListResponse();
        response.Dishes.AddRange(dishes.Select(d => new DishResponse
        {
            Id = d.Id.ToString(),
            Name = d.Name,
            Description = d.Description ?? string.Empty,
            Category = (Common.DishCategory)d.Category,
            ImageUrl = d.ImageUrl ?? string.Empty,
            Price = (double)d.Price,
            RestaurantId = d.RestaurantId.ToString()
        }));

        return response;
    }
}