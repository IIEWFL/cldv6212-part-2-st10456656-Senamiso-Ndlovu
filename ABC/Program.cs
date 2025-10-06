using ABC.Services;
using ABC.Services.Storage;
using ABCRetailFunctions.Services;

namespace ABC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Retrieve the connection string from configuration
            var storageConnectionString = builder.Configuration.GetConnectionString("StorageConnectionString")
                ?? throw new InvalidOperationException("Storage connection string not found in configuration.");

            // Register Blob Storage for product images
            var blobService = new BlobStorageService(storageConnectionString, "product-photos");
            builder.Services.AddSingleton(blobService);

            // Register Table-based services (aligned with constructor signatures)
            builder.Services.AddSingleton(new CustomerService(storageConnectionString, "Customer"));
            builder.Services.AddSingleton(new ProductService(storageConnectionString, "Product", blobService));
            builder.Services.AddSingleton(new OrderService(storageConnectionString, "Order"));

            // Register other Azure storage services (Queue + FileShare)
            builder.Services.AddSingleton(new QueueStorageService(storageConnectionString, "abcretail-audit-log"));
            builder.Services.AddSingleton(new FileShareStorageService(storageConnectionString, "abcretail-fileshare"));

            //Register the function service 
            builder.Services.AddHttpClient<FunctionService>();
            builder.Services.AddSingleton<FunctionService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Order}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
