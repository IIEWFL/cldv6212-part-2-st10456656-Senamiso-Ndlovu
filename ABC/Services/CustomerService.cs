using ABC.Models;
using ABC.Services.Storage;

namespace ABC.Services
{
    public class CustomerService
    {
        private readonly TableStorageService<CustomerEntity> _table;

        // Constructor with connection string + table name
        public CustomerService(string tableConnectionString, string tableName)
        {
            _table = new TableStorageService<CustomerEntity>(tableConnectionString, tableName);
        }

        public Task<List<CustomerEntity>> GetAllCustomersAsync() => _table.GetAllAsync();

        public Task<CustomerEntity?> GetByIdAsync(string id) => _table.TryGetAsync("CUSTOMER", id);

        public async Task AddAsync(CustomerEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            entity.PartitionKey = "CUSTOMER";

            await _table.AddAsync(entity);
        }

        public async Task UpdateAsync(CustomerEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.PartitionKey = "CUSTOMER";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                throw new ArgumentException("Customer RowKey is required for update.");

            await _table.UpdateAsync(entity);
        }

        public Task DeleteAsync(string id) => _table.DeleteAsync("CUSTOMER", id);
    }
}
