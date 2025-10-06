using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ABC.Models;
using ABCRetailFunctions.Services;
using Microsoft.Extensions.Logging;

namespace ABC.Controllers
{
    public class ProductController : Controller
    {
        private readonly FunctionService _functionService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(FunctionService functionService, ILogger<ProductController> logger)
        {
            _functionService = functionService ?? throw new ArgumentNullException(nameof(functionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionService.GetAllProductsAsync();
                var viewModels = MapToViewModel(products);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading products index: {ex.Message}");
                return StatusCode(500, "An error occurred while loading products.");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");

            try
            {
                var product = await _functionService.GetProductByIdAsync(id);
                if (product == null) return NotFound("Product not found.");
                var viewModel = MapToViewModel(product);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading product details for ID {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while loading product details.");
            }
        }

        public IActionResult Create()
        {
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var product = MapToEntity(viewModel);
            Stream? imageStream = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                imageStream = imageFile.OpenReadStream();
                product.ProductImageUrl = imageFile.FileName; // Store filename as placeholder
            }

            try
            {
                var success = await _functionService.AddProductAsync(product, imageStream);
                if (!success)
                {
                    ModelState.AddModelError("", "Failed to create product.");
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the product.");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");

            try
            {
                var product = await _functionService.GetProductByIdAsync(id);
                if (product == null) return NotFound("Product not found.");
                var viewModel = MapToViewModel(product);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading edit for product ID {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while loading the edit page.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductViewModel viewModel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(viewModel);
            if (string.IsNullOrEmpty(id) || id != viewModel.RowKey) return BadRequest("Invalid ID.");

            var product = MapToEntity(viewModel);
            Stream? imageStream = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                imageStream = imageFile.OpenReadStream();
                product.ProductImageUrl = imageFile.FileName; // Update filename
            }

            try
            {
                var success = await _functionService.UpdateProductAsync(id, product, imageStream);
                if (!success)
                {
                    ModelState.AddModelError("", "Failed to update product.");
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product ID {id}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the product.");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required.");

            try
            {
                var product = await _functionService.GetProductByIdAsync(id);
                if (product == null) return NotFound("Product not found.");
                var viewModel = MapToViewModel(product);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading delete for product ID {id}: {ex.Message}");
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
                var success = await _functionService.DeleteProductAsync(id);
                if (!success) return StatusCode(500, "Failed to delete product.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product ID {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }

        // Mapping helpers
        private List<ProductViewModel> MapToViewModel(List<ProductEntity> products)
        {
            var viewModels = new List<ProductViewModel>();
            foreach (var product in products ?? new List<ProductEntity>())
            {
                viewModels.Add(MapToViewModel(product));
            }
            return viewModels;
        }

        private ProductViewModel MapToViewModel(ProductEntity product)
        {
            return new ProductViewModel
            {
                PartitionKey = product?.PartitionKey,
                RowKey = product?.RowKey,
                ProductName = product?.ProductName,
                ProductPrice = product?.ProductPrice,
                ProductDescription = product?.ProductDescription,
                ProductImage = product?.ProductImageUrl,
                ImageSasUrl = product?.ProductImageBlobName // Use existing SAS URL if set by service
            };
        }

        private ProductEntity MapToEntity(ProductViewModel viewModel)
        {
            return new ProductEntity
            {
                PartitionKey = viewModel.PartitionKey ?? "PRODUCT",
                RowKey = viewModel.RowKey ?? Guid.NewGuid().ToString(),
                ProductName = viewModel.ProductName,
                ProductPrice = viewModel.ProductPrice,
                ProductDescription = viewModel.ProductDescription,
                ProductImageUrl = viewModel.ProductImage,
                ProductImageBlobName = null // Reset SAS URL; service should regenerate if needed
            };
        }
    }
}