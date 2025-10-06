using ABC.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization; // Added for CultureInfo
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetailFunctions.Services
{
    public class FunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;
        private readonly ILogger<FunctionService> _logger;

        public FunctionService(HttpClient httpClient, IConfiguration configuration, ILogger<FunctionService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _functionBaseUrl = configuration["AzureFunctionBaseUrlProd"] ?? throw new InvalidOperationException("Azure Functions Base URL is missing");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation($"FunctionService initialized with base URL: {_functionBaseUrl}");
        }

        // --- Customer Methods ---
        public async Task<List<CustomerEntity>> GetAllCustomersAsync()
        {
            try
            {
                _logger.LogInformation($"Fetching all customers from {_functionBaseUrl}/api/customers");
                var response = await _httpClient.GetFromJsonAsync<List<CustomerEntity>>($"{_functionBaseUrl}/api/customers");
                _logger.LogInformation($"Successfully fetched {response?.Count ?? 0} customers");
                return response ?? new List<CustomerEntity>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Failed to fetch customers: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<CustomerEntity?> GetByIdAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in GetByIdAsync");
                return null;
            }

            try
            {
                _logger.LogInformation($"Fetching customer with RowKey: {rowKey}");
                var response = await _httpClient.GetFromJsonAsync<CustomerEntity>($"{_functionBaseUrl}/api/customers/{rowKey}");
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Failed to fetch customer with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> AddCustomerAsync(CustomerEntity customer)
        {
            if (customer == null)
            {
                _logger.LogWarning("Customer is null in AddCustomerAsync");
                return false;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(customer.CustomerName ?? string.Empty), "CustomerName");
                content.Add(new StringContent(customer.CustomerEmail ?? string.Empty), "CustomerEmail");

                _logger.LogInformation($"Creating customer: {customer.CustomerName}");
                var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/customers", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully created customer: {customer.CustomerName}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to create customer: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error creating customer: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(string rowKey, CustomerEntity customer)
        {
            if (string.IsNullOrWhiteSpace(rowKey) || customer == null)
            {
                _logger.LogWarning("RowKey or customer is null in UpdateCustomerAsync");
                return false;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(customer.CustomerName ?? string.Empty), "CustomerName");
                content.Add(new StringContent(customer.CustomerEmail ?? string.Empty), "CustomerEmail");

                _logger.LogInformation($"Updating customer with RowKey: {rowKey}");
                var response = await _httpClient.PutAsync($"{_functionBaseUrl}/api/customers/{rowKey}", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully updated customer with RowKey: {rowKey}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to update customer: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error updating customer with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in DeleteCustomerAsync");
                return false;
            }

            try
            {
                var requestUrl = $"{_functionBaseUrl}/api/customers/{rowKey}";
                _logger.LogInformation($"Deleting customer with RowKey: {rowKey}");
                var response = await _httpClient.DeleteAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully deleted customer with RowKey: {rowKey}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to delete customer: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error deleting customer with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // --- Product Methods ---
        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation($"Fetching all products from {_functionBaseUrl}/api/products");
                var response = await _httpClient.GetFromJsonAsync<List<ProductEntity>>($"{_functionBaseUrl}/api/products");
                _logger.LogInformation($"Successfully fetched {response?.Count ?? 0} products");
                return response ?? new List<ProductEntity>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Failed to fetch products: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductEntity?> GetProductByIdAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in GetProductByIdAsync");
                return null;
            }

            try
            {
                _logger.LogInformation($"Fetching product with RowKey: {rowKey}");
                var response = await _httpClient.GetFromJsonAsync<ProductEntity>($"{_functionBaseUrl}/api/products/{rowKey}");
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Failed to fetch product with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> AddProductAsync(ProductEntity product, Stream? imageStream)
        {
            if (product == null)
            {
                _logger.LogWarning("Product is null in AddProductAsync");
                return false;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(product.ProductName ?? string.Empty), "ProductName");
                content.Add(new StringContent(product.ProductPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty), "ProductPrice");
                content.Add(new StringContent(product.ProductDescription ?? string.Empty), "ProductDescription");

                if (imageStream != null)
                {
                    var fileContent = new StreamContent(imageStream);
                    content.Add(fileContent, "imageFile", product.ProductImageUrl ?? "product-image.jpg");
                }

                _logger.LogInformation($"Creating product: Name={product.ProductName}, Price={product.ProductPrice}, Description={product.ProductDescription}, Image={(imageStream != null ? product.ProductImageUrl : "none")}");
                var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/products", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully created product: {product.ProductName}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to create product: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(string rowKey, ProductEntity product, Stream? imageStream)
        {
            if (string.IsNullOrWhiteSpace(rowKey) || product == null)
            {
                _logger.LogWarning("RowKey or product is null in UpdateProductAsync");
                return false;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(product.ProductName ?? string.Empty), "ProductName");
                content.Add(new StringContent(product.ProductPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty), "ProductPrice");
                content.Add(new StringContent(product.ProductDescription ?? string.Empty), "ProductDescription");

                if (imageStream != null)
                {
                    var fileContent = new StreamContent(imageStream);
                    content.Add(fileContent, "imageFile", product.ProductImageUrl ?? "product-image.jpg");
                }

                _logger.LogInformation($"Updating product with RowKey: {rowKey}");
                var response = await _httpClient.PutAsync($"{_functionBaseUrl}/api/products/{rowKey}", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully updated product with RowKey: {rowKey}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to update product: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error updating product with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in DeleteProductAsync");
                return false;
            }

            try
            {
                var requestUrl = $"{_functionBaseUrl}/api/products/{rowKey}";
                _logger.LogInformation($"Deleting product with RowKey: {rowKey}");
                var response = await _httpClient.DeleteAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully deleted product with RowKey: {rowKey}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to delete product: StatusCode={response.StatusCode}, Reason={errorContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error deleting product with RowKey {rowKey}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        // --- Order Methods ---
        public async Task<List<OrderEntity>> GetAllOrdersAsync()
        {
            var requestUrl = $"{_functionBaseUrl}/api/orders";
            _logger.LogInformation($"Attempting to fetch all orders from: {requestUrl}");

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                _logger.LogInformation($"Received response with status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"HTTP request failed with status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {errorContent}");
                    return new List<OrderEntity>(); // Fallback to empty list
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response content: {content}");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var orders = JsonSerializer.Deserialize<List<OrderEntity>>(content, options) ?? new List<OrderEntity>();
                _logger.LogInformation($"Successfully fetched {orders.Count} orders");
                return orders;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error fetching orders from {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new List<OrderEntity>(); // Fallback to avoid crash
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error for {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new List<OrderEntity>(); // Fallback for malformed response
            }
        }

        public async Task<OrderEntity?> GetOrderByIdAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in GetOrderByIdAsync");
                return null;
            }

            var requestUrl = $"{_functionBaseUrl}/api/orders/{rowKey}";
            _logger.LogInformation($"Fetching order with RowKey: {rowKey} from {requestUrl}");

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                _logger.LogInformation($"Received response with status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"HTTP request failed with status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {errorContent}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response content for RowKey {rowKey}: {content}");
                return JsonSerializer.Deserialize<OrderEntity>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error fetching order with RowKey {rowKey} from {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error for RowKey {rowKey} from {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> AddOrderAsync(OrderEntity order)
        {
            if (order == null)
            {
                _logger.LogWarning("Order is null in AddOrderAsync");
                return false;
            }

            var requestUrl = $"{_functionBaseUrl}orders"; // Adjusted to match Azure Functions route
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("CustomerId", order.CustomerId ?? string.Empty),
            new KeyValuePair<string, string>("ProductId", order.ProductId ?? string.Empty),
            new KeyValuePair<string, string>("Quantity", order.Quantity.ToString()),
            new KeyValuePair<string, string>("OrderDate", order.OrderDate.ToString("o")),
            new KeyValuePair<string, string>("TotalPrice", order.TotalPrice.ToString(CultureInfo.InvariantCulture)),
            new KeyValuePair<string, string>("Status", order.Status ?? "Pending")
        });

                _logger.LogInformation($"Queuing create order for customer: {order.CustomerId} to {requestUrl}");
                var response = await _httpClient.PostAsync(requestUrl, content);
                _logger.LogInformation($"Received response with status: {response.StatusCode} for order creation");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create order: StatusCode={response.StatusCode}, Reason={response.ReasonPhrase}, Content={errorContent}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error queuing create order to {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(string rowKey, OrderEntity order)
        {
            if (string.IsNullOrWhiteSpace(rowKey) || order == null)
            {
                _logger.LogWarning("RowKey or order is null in UpdateOrderAsync");
                return false;
            }

            var requestUrl = $"{_functionBaseUrl}/api/orders/{rowKey}";
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("CustomerId", order.CustomerId ?? string.Empty),
                    new KeyValuePair<string, string>("ProductId", order.ProductId ?? string.Empty),
                    new KeyValuePair<string, string>("Quantity", order.Quantity.ToString()),
                    new KeyValuePair<string, string>("OrderDate", order.OrderDate.ToString("o")),
                    new KeyValuePair<string, string>("TotalPrice", order.TotalPrice.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("Status", order.Status ?? "Pending")
                });

                _logger.LogInformation($"Queuing update order with RowKey: {rowKey} to {requestUrl}");
                var response = await _httpClient.PutAsync(requestUrl, content);
                _logger.LogInformation($"Received response with status: {response.StatusCode} for order update");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update order: StatusCode={response.StatusCode}, Reason={response.ReasonPhrase}, Content={errorContent}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error queuing update order with RowKey {rowKey} to {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                _logger.LogWarning("RowKey is null or empty in DeleteOrderAsync");
                return false;
            }

            var requestUrl = $"{_functionBaseUrl}/api/orders/{rowKey}";
            _logger.LogInformation($"Queuing delete order with RowKey: {rowKey} to {requestUrl}");

            try
            {
                var response = await _httpClient.DeleteAsync(requestUrl);
                _logger.LogInformation($"Received response with status: {response.StatusCode} for order deletion");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to delete order: StatusCode={response.StatusCode}, Reason={response.ReasonPhrase}, Content={errorContent}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error queuing delete order with RowKey {rowKey} to {requestUrl}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}