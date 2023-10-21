using Drone.Options;
using Drone.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Drone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            builder.Services.Configure<StorageAccountOptions>(builder.Configuration.GetSection(StorageAccountOptions.Position));
            builder.Services.Configure<AdOptions>(builder.Configuration.GetSection(AdOptions.Position));

            builder.Services.AddSingleton<PoorMansDb>();

            builder.Services.AddHttpClient<AzureAdClient>();
            builder.Services.AddHttpClient<AzureBlobClient>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}