using ABC.Models;
using ABC.Services.Storage;

namespace ABC.Services
{
    public class OrderService
    {
        private readonly TableStorageService<OrderEntity> _table;

        // ✅ Constructor with connection string + table name
        public OrderService(string tableConnectionString, string tableName)
        {
            _table = new TableStorageService<OrderEntity>(tableConnectionString, tableName);
        }

        public Task<List<OrderEntity>> GetAllAsync() => _table.GetAllAsync();

        public Task<OrderEntity?> GetByIdAsync(string id) => _table.TryGetAsync("ORDER", id);

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
