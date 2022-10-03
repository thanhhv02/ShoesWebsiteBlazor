﻿using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using PoPoy.Api.Data;
using PoPoy.Api.Services.AuthService;
using PoPoy.Shared.Dto;
using PoPoy.Shared.Paging;
using PoPoy.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoPoy.Api.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;
        public OrderService(DataContext context, IAuthService authService)
        {
            _context = context;
        }
        public async Task<List<Order>> GetAllOrders()
        {
            var query = from o in _context.Orders
                        select new { o };

            var orderDetails = await _context.OrderDetails.Join(_context.Orders,
                                                          od => od.OrderIdFromOrder,
                                                          o => o.Id,
                                                          (od, o) => od).ToListAsync();

            return await query.Select(x => new Order()
            {
                Id = x.o.Id,
                UserId = x.o.UserId,
                AddressId = x.o.AddressId,
                OrderDate = x.o.OrderDate,
                TotalPrice = x.o.TotalPrice,
                PaymentMode = x.o.PaymentMode,
                OrderStatus = x.o.OrderStatus,
                OrderDetails = orderDetails
            }).ToListAsync();
        }

        public async Task<List<OrderDetails>> GetOrderDetails(string orderId)
        {
            var orderDetails = await (from od in _context.OrderDetails
                                      join o in _context.Orders on od.OrderIdFromOrder equals o.Id
                                      where od.OrderIdFromOrder == orderId
                                      select od).ToListAsync();

            return orderDetails;
        }

        public async Task<List<Order>> SearchOrder(string searchText)
        {
            var query = from o in _context.Orders
                        where o.UserId.ToString().ToLower().Contains(searchText.ToLower()) ||
                        o.Id.ToLower().Contains(searchText.ToLower())
                        select new { o };

            return await query.Select(x => new Order()
            {
                Id = x.o.Id,
                UserId = x.o.UserId,
                AddressId = x.o.AddressId,
                OrderDate = x.o.OrderDate,
                TotalPrice = x.o.TotalPrice,
                PaymentMode = x.o.PaymentMode,
                OrderStatus = x.o.OrderStatus
            }).ToListAsync();
        }

        public async Task<int> DeleteOrder(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            var orderDetails = await (from od in _context.OrderDetails
                                      join o in _context.Orders on od.OrderIdFromOrder equals o.Id
                                      where od.OrderIdFromOrder == orderId
                                      select od).ToListAsync();
            foreach (var item in orderDetails)
            {
                _context.OrderDetails.Remove(item);
            }
            _context.Orders.Remove(order);

            return await _context.SaveChangesAsync();
        }

        public async Task<PagedList<OrderOverviewResponse>> GetOrders(ProductParameters productParameters, string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(pi => pi.ProductImages)
                .Where(o => o.UserId == Guid.Parse(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderResponse = new List<OrderOverviewResponse>();
            orders.ForEach(o => orderResponse.Add(new OrderOverviewResponse
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                Product = o.OrderDetails.Count > 1 ?
                    $"{o.OrderDetails.First().Product.Title} and" +
                    $" {o.OrderDetails.Count - 1} more..." :
                    o.OrderDetails.First().Product.Title,
                ProductImageUrl = o.OrderDetails.First().Product.ProductImages.FirstOrDefault()?.ImagePath
            }));

            return PagedList<OrderOverviewResponse>.ToPagedList(orderResponse, productParameters.PageNumber, productParameters.PageSize);

        }

        public async Task<ServiceResponse<OrderDetailsResponse>> GetOrderDetailsForClient(string orderId)
        {
            var response = new ServiceResponse<OrderDetailsResponse>();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(oi => oi.ProductImages)
                .Include(o => o.OrderDetails)
                .Where(o => o.Id == orderId)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                response.Success = false;
                response.Message = "Order not found.";
                return response;
            }

            var orderDetailsResponse = new OrderDetailsResponse
            {
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Products = new List<OrderDetailsProductResponse>()
            };

            order.OrderDetails.ForEach(item =>
            orderDetailsResponse.Products.Add(new OrderDetailsProductResponse
            {
                ProductId = item.ProductId,
                ProductSize = item.Size,
                ProductImages = _context.ProductImages.Where(x => x.ProductId == item.ProductId).ToList().Count > 0 ?
                _context.ProductImages.Where(x => x.ProductId == item.ProductId).ToList() : null,
                Quantity = item.Quantity,
                Title = item.Product.Title,
                TotalPrice = (decimal)item.TotalPrice
            }));

            response.Data = orderDetailsResponse;

            return response;
        }
    }
}
