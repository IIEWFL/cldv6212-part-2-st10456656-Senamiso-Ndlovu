using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ABC.Models;
using ABCRetailFunctions.Services;
using Microsoft.Extensions.Logging;

namespace ABC.Controllers
{
    public class OrderController : Controller
    {
        private readonly FunctionService _functionService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(FunctionService functionService, ILogger<OrderController> logger)
        {
            _functionService = functionService ?? throw new ArgumentNullException(nameof(functionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(string search = null)
        {
            try
            {
                var orders = await _functionService.GetAllOrdersAsync();
                _logger.LogInformation($"Retrieved {orders?.Count ?? 0} orders from function.");
                if (orders == null || orders.Count == 0)
                {
                    _logger.LogInformation("No orders found or GetAllOrdersAsync returned empty.");
                    return View(new List<OrderViewModel>());
                }
                var viewModels = await MapToViewModel(orders);

                if (!string.IsNullOrEmpty(search))
                {
                    viewModels = viewModels.Where(v =>
                        (v.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (v.ProductName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                return View(viewModels);
            }
            catch (HttpRequestException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError($"Error loading orders. Status: {ex.StatusCode}, Message: {innerMessage}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while loading orders: {innerMessage}");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError($"Unexpected error loading orders: {innerMessage}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while loading orders: {innerMessage}");
            }
        }

        public async Task<IActionResult> Create()
        {
            return View(new OrderViewModel { OrderDate = DateTimeOffset.Now, Status = "Pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel viewModel)
        {
            _logger.LogInformation("Attempting to create a new order. Received model: {@Model}", viewModel);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState is invalid. Errors: {@Errors}", errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error); // Display all validation errors
                }
                return View(viewModel);
            }

            var order = MapToEntity(viewModel);
            try
            {
                _logger.LogInformation("Calling AddOrderAsync with order: {@Order}", order);
                var success = await _functionService.AddOrderAsync(order);
                if (!success)
                {
                    _logger.LogWarning($"AddOrderAsync failed for order {order.RowKey}. Check function logs.");
                    ModelState.AddModelError("", "Failed to create order. Verify function endpoint and logs.");
                    return View(viewModel);
                }
                _logger.LogInformation($"Successfully created order with RowKey: {order.RowKey}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"An error occurred while creating the order: {ex.Message}");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");
            try
            {
                var order = await _functionService.GetOrderByIdAsync(id);
                if (order == null) return NotFound("Order not found.");
                var viewModel = await MapToViewModel(order);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading order details for ID {id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while loading order details.");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");
            try
            {
                var order = await _functionService.GetOrderByIdAsync(id);
                if (order == null) return NotFound("Order not found.");
                var viewModel = await MapToViewModel(order);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading edit for order ID {id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while loading the edit page.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, OrderViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);
            if (string.IsNullOrEmpty(id) || id != viewModel.RowKey) return BadRequest("Invalid ID.");
            var order = MapToEntity(viewModel);
            try
            {
                var success = await _functionService.UpdateOrderAsync(id, order);
                if (!success)
                {
                    ModelState.AddModelError("", "Failed to update order.");
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order ID {id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while updating the order.");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");
            try
            {
                var order = await _functionService.GetOrderByIdAsync(id);
                if (order == null) return NotFound("Order not found.");
                var viewModel = await MapToViewModel(order);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading delete for order ID {id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while loading the delete page.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");
            try
            {
                var success = await _functionService.DeleteOrderAsync(id);
                if (!success)
                {
                    _logger.LogWarning($"Failed to delete order with ID {id}.");
                    return StatusCode(500, "Failed to delete order.");
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting order ID {id}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string newStatus)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newStatus))
                return BadRequest("ID and new status required.");
            try
            {
                var order = await _functionService.GetOrderByIdAsync(id);
                if (order == null) return NotFound("Order not found.");
                order.Status = newStatus;
                var success = await _functionService.UpdateOrderAsync(id, order);
                if (!success)
                {
                    _logger.LogWarning($"Failed to update status for order {id}.");
                    return StatusCode(500, "Failed to update status.");
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating status for order {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while updating status.");
            }
        }

        private async Task<List<OrderViewModel>> MapToViewModel(List<OrderEntity> orders)
        {
            var viewModels = new List<OrderViewModel>();
            foreach (var order in orders ?? new List<OrderEntity>())
            {
                viewModels.Add(await MapToViewModel(order));
            }
            return viewModels;
        }

        private async Task<OrderViewModel> MapToViewModel(OrderEntity order)
        {
            var viewModel = new OrderViewModel
            {
                PartitionKey = order?.PartitionKey,
                RowKey = order?.RowKey,
                CustomerId = order?.CustomerId,
                ProductId = order?.ProductId,
                Quantity = order?.Quantity ?? 0,
                OrderDate = order?.OrderDate ?? DateTimeOffset.Now,
                TotalPrice = order?.TotalPrice ?? 0,
                Status = order?.Status ?? "Pending"
            };

            if (!string.IsNullOrEmpty(viewModel.CustomerId))
            {
                var customer = await _functionService.GetByIdAsync(viewModel.CustomerId);
                viewModel.CustomerName = customer?.CustomerName ?? "Unknown Customer";
            }
            if (!string.IsNullOrEmpty(viewModel.ProductId))
            {
                var product = await _functionService.GetProductByIdAsync(viewModel.ProductId);
                viewModel.ProductName = product?.ProductName ?? "Unknown Product";
            }

            return viewModel;
        }

        private OrderEntity MapToEntity(OrderViewModel viewModel)
        {
            return new OrderEntity
            {
                PartitionKey = viewModel.PartitionKey ?? "ORDER",
                RowKey = viewModel.RowKey ?? Guid.NewGuid().ToString(),
                CustomerId = viewModel.CustomerId,
                ProductId = viewModel.ProductId,
                Quantity = viewModel.Quantity,
                OrderDate = viewModel.OrderDate,
                TotalPrice = viewModel.TotalPrice,
                Status = viewModel.Status
            };
        }
    }
}