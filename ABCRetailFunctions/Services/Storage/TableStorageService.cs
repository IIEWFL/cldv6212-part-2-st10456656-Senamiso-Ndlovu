using Azure.Data.Tables;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Services.Storage
{
    public class TableStorageService<T> where T : class, ITableEntity, new()
    {
        //defined table client
        private readonly TableClient _tableClient;

        //initialise the constructor
        public TableStorageService(string storageConnectionString, string tableName)
        {
            var TableServiceClient = new TableServiceClient(storageConnectionString);
            _tableClient = TableServiceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        // Get all entities
        public async Task<List<T>> GetAllAsync()
        {
            var results = new List<T>();
            await foreach (var entity in _tableClient.QueryAsync<T>())
            {
                results.Add(entity);
            }
            return results;
        }

        // Get entity by PK + RK
        public async Task<T?> GetAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Insert
        public async Task AddAsync(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        // Update
        public async Task UpdateAsync(T entity)
        {
            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        // Delete
        public async Task DeleteAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<T?> TryGetAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey)) throw new ArgumentException(nameof(partitionKey));
            if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException(nameof(rowKey));

            try
            {
                var resp = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return resp.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
    }
}
