using ABCRetailFunctions.Models;
using ABCRetailFunctions.Services.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Services
{
    public class ProductService
    {
        private readonly TableStorageService<ProductEntity> _table;
        private readonly BlobStorageService _blob;
        private readonly ILogger<ProductService> _logger;

        public ProductService(string tableConnectionString, string tableName, BlobStorageService blobService, ILogger<ProductService> logger)
        {
            _table = new TableStorageService<ProductEntity>(tableConnectionString, tableName);
            _blob = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            var products = await _table.GetAllAsync();
            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ProductImageBlobName))
                {
                    try
                    {
                        product.ProductImageUrl = _blob.GetImageSasUri(product.ProductImageBlobName, 1440); // 24 hours
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to generate SAS URL for blob {product.ProductImageBlobName}: {ex.Message}");
                    }
                }
            }
            return products;
        }

        public async Task<ProductEntity?> GetByIdAsync(string id)
        {
            var product = await _table.TryGetAsync("PRODUCT", id);
            if (product != null && !string.IsNullOrEmpty(product.ProductImageBlobName))
            {
                try
                {
                    product.ProductImageUrl = _blob.GetImageSasUri(product.ProductImageBlobName, 1440);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to generate SAS URL for blob {product.ProductImageBlobName}: {ex.Message}");
                }
            }
            return product;
        }

        public async Task<bool> AddAsync(ProductEntity entity, Stream? imageStream = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            try
            {
                if (string.IsNullOrWhiteSpace(entity.RowKey))
                    entity.RowKey = Guid.NewGuid().ToString();
                entity.PartitionKey = "PRODUCT";

                if (imageStream != null)
                {
                    var blobName = $"{entity.RowKey}{Path.GetExtension(entity.ProductName ?? ".jpg")}";
                    await _blob.UploadImageAsync(imageStream, blobName);
                    entity.ProductImageBlobName = blobName;
                    entity.ProductImageUrl = _blob.GetImageSasUri(blobName, 1440);
                }

                await _table.AddAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ProductEntity entity, Stream? imageStream = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(entity.RowKey)) throw new ArgumentException("Product RowKey is required for update.");

            try
            {
                var existing = await GetByIdAsync(entity.RowKey);
                if (existing != null && imageStream != null && !string.IsNullOrEmpty(existing.ProductImageBlobName))
                {
                    await _blob.DeleteImageAsync(existing.ProductImageBlobName);
                }

                if (imageStream != null)
                {
                    var blobName = $"{entity.RowKey}{Path.GetExtension(entity.ProductName ?? ".jpg")}";
                    await _blob.UploadImageAsync(imageStream, blobName);
                    entity.ProductImageBlobName = blobName;
                    entity.ProductImageUrl = _blob.GetImageSasUri(blobName, 1440);
                }
                else if (existing != null)
                {
                    entity.ProductImageBlobName = existing.ProductImageBlobName;
                    entity.ProductImageUrl = existing.ProductImageUrl;
                }

                await _table.UpdateAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product {entity.RowKey}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var existing = await GetByIdAsync(id);
                if (existing != null && !string.IsNullOrEmpty(existing.ProductImageBlobName))
                {
                    await _blob.DeleteImageAsync(existing.ProductImageBlobName);
                }
                await _table.DeleteAsync("PRODUCT", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product {id}: {ex.Message}");
                return false;
            }
        }
    }
}