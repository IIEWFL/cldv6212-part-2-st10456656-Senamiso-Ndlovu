#nullable enable
using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ABCRetailFunctions.Services
{
    public class OrderService
    {
        private readonly TableStorageService<OrderEntity> _table;
        private readonly ILogger<OrderService> _logger;

        public OrderService(string tableConnectionString, string tableName, ILogger<OrderService> logger)
        {
            _table = new TableStorageService<OrderEntity>(tableConnectionString, tableName);
            _logger = logger;
        }

        public async Task<List<OrderEntity>> GetAllOrdersAsync()
        {
            try
            {
                return await _table.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching all orders: {ex.Message}");
                return new List<OrderEntity>();
            }
        }

        public Task<OrderEntity?> GetByIdAsync(string id)
        {
            return _table.TryGetAsync("ORDER", id);
        }

        public async Task AddAsync(OrderEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            entity.PartitionKey = "ORDER";

            await _table.AddAsync(entity);
        }

        public async Task UpdateAsync(OrderEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.PartitionKey = "ORDER";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                throw new ArgumentException("Order RowKey is required for update.");

            await _table.UpdateAsync(entity);
        }

        public Task DeleteAsync(string id) => _table.DeleteAsync("ORDER", id);
    }
}