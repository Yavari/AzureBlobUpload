using System.Diagnostics;
using System.Net;
using Drone.Options;
using Drone.ProxyTransformers;
using Drone.Services;
using Drone.Services.AzureBlob;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Yarp.ReverseProxy.Forwarder;

namespace Drone
{
    public class Program
    {
        private const long MaxRequestSizeInMb = 1000;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddReverseProxy();

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                // if don't set default value is: 30 MB
                options.Limits.MaxRequestBodySize = (MaxRequestSizeInMb + 1) * 1024 * 1024;
            });

            builder.Services.Configure<StorageAccountOptions>(                builder.Configuration.GetSection(StorageAccountOptions.Position));
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
            SetupEndpoints(app);
            app.Run();
        }

        private static void SetupEndpoints(WebApplication app)
        {
            // Configure our own HttpMessageInvoker for outbound calls for proxy operations
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
                ConnectTimeout = TimeSpan.FromSeconds(15),
            });

            // Setup our own request transform class
            var transformer = new BlobProxyTransformer(); 
            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapForwarder("/video/{**catch-all}", "https://whatisthis.com", requestOptions, transformer, httpClient);
            });
        }
    }
}