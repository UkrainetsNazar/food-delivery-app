using System.Text;
using System.Text.Json;
using Contracts.Events;
using Grpc.Core;
using Order.API.Entities;
using Order.API.Interfaces;
using OrderGrpcService;
using RabbitMQ.Client;
using RestaurantGrpcService;

namespace Order.API.Services;

public class OrderService(
    IOrderRepository orderRepository,
    DishService.DishServiceClient restaurantClient,
    IConnection rabbitConnection) : OrderGrpcService.OrderService.OrderServiceBase
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly DishService.DishServiceClient _restaurantClient = restaurantClient;
    private readonly IConnection _rabbitConnection = rabbitConnection;

    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var order = new Entities.Order
        {
            UserId = Guid.Parse(request.UserId),
            Status = "Pending",
            PaymentStatus = "Unpaid"
        };

        foreach (var item in request.Items)
        {
            var dish = await _restaurantClient.GetDishByIdAsync(new DishIdRequest { DishId = item.DishId });

            order.Items.Add(new OrderItem
            {
                DishId = Guid.Parse(item.DishId),
                RestaurantId = Guid.Parse(dish.RestaurantId),
                DishName = dish.Name,
                Price = (decimal)dish.Price,
                Quantity = item.Quantity
            });
        }

        await _orderRepository.AddAsync(order);

        // 🔹 Публікуємо подію в RabbitMQ
        using var channel = _rabbitConnection.CreateModel();
        channel.QueueDeclare("order_created", durable: true, exclusive: false, autoDelete: false);

        var evt = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalPrice
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

        channel.BasicPublish(exchange: "", routingKey: "order_created", basicProperties: null, body: body);

        return MapOrder(order);
    }

    public override async Task<OrderResponse> GetOrderById(GetOrderByIdRequest request, ServerCallContext context)
    {
        var order = await _orderRepository.GetByIdAsync(Guid.Parse(request.OrderId))
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        return MapOrder(order);
    }

    public override async Task<OrdersResponse> GetOrdersByUser(GetOrdersByUserRequest request, ServerCallContext context)
    {
        var orders = await _orderRepository.GetByUserIdAsync(Guid.Parse(request.UserId));

        var response = new OrdersResponse();
        response.Orders.AddRange(orders.Select(MapOrder));
        return response;
    }

    public override async Task<OrderResponse> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
    {
        var order = await _orderRepository.GetByIdAsync(Guid.Parse(request.OrderId))
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        if (!string.IsNullOrEmpty(request.Status))
            order.Status = request.Status;

        if (!string.IsNullOrEmpty(request.PaymentStatus))
            order.PaymentStatus = request.PaymentStatus;

        await _orderRepository.UpdateAsync(order);

        return MapOrder(order);
    }

    private static OrderResponse MapOrder(Entities.Order order)
    {
        return new OrderResponse
        {
            OrderId = order.Id.ToString(),
            UserId = order.UserId.ToString(),
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            TotalPrice = (double)order.TotalPrice,
            CreatedAt = order.CreatedAt.ToString("O"),
            Items = { order.Items.Select(i => new OrderItemResponse
            {
                DishId = i.DishId.ToString(),
                DishName = i.DishName,
                RestaurantId = i.RestaurantId.ToString(),
                Price = (double)i.Price,
                Quantity = i.Quantity,
                Total = (double)i.Total
            }) }
        };
    }
}