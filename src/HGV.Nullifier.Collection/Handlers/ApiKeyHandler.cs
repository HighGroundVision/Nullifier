using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Nullifier.Collection.Handlers
{
   public class ApiKeyHandler : DelegatingHandler
    {
        private readonly string ApiKey;

        public ApiKeyHandler()
        {
            ApiKey = Environment.GetEnvironmentVariable("SteamApiKey");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var builder = new UriBuilder(request.RequestUri);

            if (string.IsNullOrEmpty(builder.Query))
                builder.Query = $"key={ApiKey}";
            else
                builder.Query = $"{builder.Query}&key={ApiKey}";

            request.RequestUri = builder.Uri;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
