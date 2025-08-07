using AutoMapper;
using Contracts.DTO;
using Contracts.Enums;
using Grpc.Core;
using RestaurantGrpcService;

namespace AdminPanelService.Clients;

public class RestaurantManagementClient(
    RestaurantService.RestaurantServiceClient restaurantClient,
    DishService.DishServiceClient dishClient,
    IMapper mapper,
    ILogger<RestaurantManagementClient> logger)
{
    private readonly RestaurantService.RestaurantServiceClient _restaurantClient = restaurantClient;
    private readonly DishService.DishServiceClient _dishClient = dishClient;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<RestaurantManagementClient> _logger = logger;

    public async Task<RestaurantDto> CreateRestaurantAsync(RestaurantDto restaurant)
    {
        try
        {
            var request = _mapper.Map<RestaurantRequest>(restaurant);
            var response = await _restaurantClient.CreateRestaurantAsync(request);
            return _mapper.Map<RestaurantDto>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant");
            throw;
        }
    }

    public async Task<RestaurantDto?> UpdateRestaurantAsync(RestaurantDto restaurant)
    {
        try
        {
            var request = _mapper.Map<RestaurantRequest>(restaurant);
            var response = await _restaurantClient.UpdateRestaurantAsync(request);
            return _mapper.Map<RestaurantDto>(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating restaurant");
            throw;
        }
    }

    public async Task<bool> DeleteRestaurantAsync(Guid restaurantId)
    {
        try
        {
            var request = new RestaurantIdRequest { RestaurantId = restaurantId.ToString() };
            var response = await _restaurantClient.DeleteRestaurantAsync(request);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting restaurant");
            throw;
        }
    }

    public async Task<IEnumerable<RestaurantDto>> GetAllRestaurantsAsync()
    {
        try
        {
            var response = await _restaurantClient.GetAllRestaurantsAsync(new Google.Protobuf.WellKnownTypes.Empty());
            return _mapper.Map<IEnumerable<RestaurantDto>>(response.Restaurants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all restaurants");
            throw;
        }
    }

    public async Task<IEnumerable<DishCategory>> GetRestaurantCategoriesAsync(Guid restaurantId)
    {
        try
        {
            var request = new RestaurantIdRequest { RestaurantId = restaurantId.ToString() };
            var response = await _restaurantClient.GetRestaurantCategoriesAsync(request);
            return _mapper.Map<IEnumerable<DishCategory>>(response.Categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting restaurant categories");
            throw;
        }
    }

    public async Task<DishDto> CreateDishAsync(DishDto dish)
    {
        try
        {
            var request = _mapper.Map<DishRequest>(dish);
            var response = await _dishClient.CreateDishAsync(request);
            return _mapper.Map<DishDto>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dish");
            throw;
        }
    }

    public async Task<DishDto?> UpdateDishAsync(DishDto dish)
    {
        try
        {
            var request = _mapper.Map<DishRequest>(dish);
            var response = await _dishClient.UpdateDishAsync(request);
            return _mapper.Map<DishDto>(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dish");
            throw;
        }
    }

    public async Task<bool> DeleteDishAsync(Guid dishId)
    {
        try
        {
            var request = new DishIdRequest { DishId = dishId.ToString() };
            var response = await _dishClient.DeleteDishAsync(request);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dish");
            throw;
        }
    }

    public async Task<DishDto?> GetDishByIdAsync(Guid dishId)
    {
        try
        {
            var request = new DishIdRequest { DishId = dishId.ToString() };
            var response = await _dishClient.GetDishByIdAsync(request);
            return _mapper.Map<DishDto>(response);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dish by ID");
            throw;
        }
    }

    public async Task<IEnumerable<DishDto>> GetDishesByNameAsync(string name)
    {
        try
        {
            var request = new DishNameRequest { Name = name };
            var response = await _dishClient.GetDishesByNameAsync(request);
            return _mapper.Map<IEnumerable<DishDto>>(response.Dishes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dishes by name");
            throw;
        }
    }

    public async Task<IEnumerable<DishDto>> GetDishesByRestaurantAsync(Guid restaurantId)
    {
        try
        {
            var request = new RestaurantIdRequest { RestaurantId = restaurantId.ToString() };
            var response = await _dishClient.GetDishesByRestaurantAsync(request);
            return _mapper.Map<IEnumerable<DishDto>>(response.Dishes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dishes by restaurant");
            throw;
        }
    }
}