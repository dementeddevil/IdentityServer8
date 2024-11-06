// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer8.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer8.Extensions;
using System.Text.Json.Serialization;
using IdentityServer8.ResponseHandling;

namespace IdentityServer8.Endpoints.Results
{
    internal class TokenErrorResult : IEndpointResult
    {
        public TokenErrorResponse Response { get; }

        public TokenErrorResult(TokenErrorResponse error)
        {
            if (error.Error.IsMissing()) throw new ArgumentNullException(nameof(error.Error), "Error must be set");

            Response = error;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                Error = Response.Error,
                ErrorDescription = Response.ErrorDescription,
                
                Custom = Response.Custom
            };

            await context.Response.WriteJsonAsync(dto);
        }

        internal class ResultDto
        {
            [JsonPropertyName("error")]
            public string Error { get; set; }

            [JsonPropertyName("error_description")]
            public string? ErrorDescription { get; set; }

            [JsonExtensionData]
            [JsonPropertyName("custom")]
            public Dictionary<string, object> Custom { get; set; }
        }    
    }
}
