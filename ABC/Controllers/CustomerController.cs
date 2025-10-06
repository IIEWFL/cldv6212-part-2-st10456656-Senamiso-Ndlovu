using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ABC.Services;
using ABC.Models;
using ABCRetailFunctions.Services;

namespace ABC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FunctionService _functionService;

        public CustomerController(FunctionService functionService)
        {
            _functionService = functionService ?? throw new ArgumentNullException(nameof(functionService));
        }

        // GET: Customer/Index
        public async Task<IActionResult> Index()
        {
            var customers = await _functionService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customer/Details/{rowKey}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var customer = await _functionService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerEntity customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var success = await _functionService.AddCustomerAsync(customer);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to create customer.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Edit/{rowKey}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var customer = await _functionService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/{rowKey}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerEntity customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            if (string.IsNullOrEmpty(id) || id != customer.RowKey)
                return BadRequest();

            var success = await _functionService.UpdateCustomerAsync(id, customer);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update customer.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Delete/{rowKey}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var customer = await _functionService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customer/Delete/{rowKey}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var success = await _functionService.DeleteCustomerAsync(id);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to delete customer.");
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}