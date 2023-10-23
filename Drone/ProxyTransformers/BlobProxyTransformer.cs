using Drone.Services;
using System.Net.Http.Headers;
using Drone.Options;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace Drone.ProxyTransformers
{
    public class BlobProxyTransformer : HttpTransformer
    {
        /// <summary>
        /// A callback that is invoked prior to sending the proxied request. All HttpRequestMessage
        /// fields are initialized except RequestUri, which will be initialized after the
        /// callback if no value is provided. The string parameter represents the destination
        /// URI prefix that should be used when constructing the RequestUri. The headers
        /// are copied by the base implementation, excluding some protocol headers like HTTP/2
        /// pseudo headers (":authority").
        /// </summary>
        /// <param name="httpContext">The incoming request.</param>
        /// <param name="proxyRequest">The outgoing proxy request.</param>
        /// <param name="destinationPrefix">The uri prefix for the selected destination server which can be used to create
        /// the RequestUri.</param>
        public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
        {
            // Copy all request headers
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

            var azureAdClient = httpContext.RequestServices.GetService<AzureAdClient>();
            var storageAccountOptions = httpContext.RequestServices.GetService<IOptions<StorageAccountOptions>>();
            var requestPath = new PathString(httpContext.Request.Path.ToString().Replace("/video/", "/"));
            proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageAccountOptions.Value.ContainerPath, requestPath, httpContext.Request.QueryString);

            var token = await azureAdClient.GetToken();
            proxyRequest.Headers.Add("x-ms-version", "2020-04-08");
            proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);


            // Suppress the original request header, use the one from the destination Uri.
            proxyRequest.Headers.Host = null;
        }
    }
}
