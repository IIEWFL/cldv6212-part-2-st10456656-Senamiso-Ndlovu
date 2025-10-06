using ABC.Models;
using ABC.Services.Storage;

namespace ABC.Services
{
    public class ProductService
    {
        private readonly TableStorageService<ProductEntity> _table;
        private readonly BlobStorageService _blobService;

        // Constructor with connection string + table name + blob storage
        public ProductService(string tableConnectionString, string tableName, BlobStorageService blobService)
        {
            _table = new TableStorageService<ProductEntity>(tableConnectionString, tableName);
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public Task<List<ProductEntity>> GetAllProductsAsync() => _table.GetAllAsync();

        public Task<ProductEntity?> GetByIdAsync(string id) => _table.TryGetAsync("PRODUCT", id);

        public async Task AddAsync(ProductEntity entity, Stream? imageStream = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.RowKey))
                entity.RowKey = Guid.NewGuid().ToString();

            entity.PartitionKey = "PRODUCT";

            // Upload product image if provided
            if (imageStream != null && !string.IsNullOrWhiteSpace(entity.ProductImageUrl))
            {
                await _blobService.UploadImageAsync(imageStream, entity.ProductImageUrl);
            }

            await _table.AddAsync(entity);
        }

        public async Task UpdateAsync(ProductEntity entity, Stream? imageStream = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.PartitionKey = "PRODUCT";
            if (string.IsNullOrWhiteSpace(entity.RowKey))
                throw new ArgumentException("Product RowKey is required for update.");

            if (imageStream != null && !string.IsNullOrWhiteSpace(entity.ProductImageUrl))
            {
                await _blobService.UploadImageAsync(imageStream, entity.ProductImageUrl);
            }

            await _table.UpdateAsync(entity);
        }

        public Task DeleteAsync(string id) => _table.DeleteAsync("PRODUCT", id);
    }
}
