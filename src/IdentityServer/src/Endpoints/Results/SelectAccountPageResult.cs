// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zen.IdentityServer.Configuration;
using Zen.IdentityServer.Extensions;
using Zen.IdentityServer.Hosting;
using Zen.IdentityServer.Models;
using Zen.IdentityServer.Stores;
using Zen.IdentityServer.Validation;

namespace Zen.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for select account page
/// </summary>
/// <seealso cref="Zen.IdentityServer.Hosting.IEndpointResult" />
public class SelectAccountPageResult : IEndpointResult
{
    private readonly ValidatedAuthorizeRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="System.ArgumentNullException">request</exception>
    public SelectAccountPageResult(ValidatedAuthorizeRequest request)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
    }

    internal SelectAccountPageResult(
        ValidatedAuthorizeRequest request,
        IdentityServerOptions options,
        IAuthorizationParametersMessageStore? authorizationParametersMessageStore = null)
        : this(request)
    {
        _options = options;
        _authorizationParametersMessageStore = authorizationParametersMessageStore;
    }

    private IdentityServerOptions _options;
    private IAuthorizationParametersMessageStore? _authorizationParametersMessageStore;

    private void Init(HttpContext context)
    {
        _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        _authorizationParametersMessageStore = _authorizationParametersMessageStore ?? context.RequestServices.GetService<IAuthorizationParametersMessageStore>();
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns></returns>
    public async Task ExecuteAsync(HttpContext context)
    {
        Init(context);

        var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeCallback;
        if (_authorizationParametersMessageStore != null)
        {
            var msg = new Message<IDictionary<string, string[]>>(_request.Raw.ToFullDictionary());
            var id = await _authorizationParametersMessageStore.WriteAsync(msg);
            returnUrl = returnUrl.AddQueryString(Constants.AuthorizationParamsStore.MessageStoreIdParameterName, id);
        }
        else
        {
            returnUrl = returnUrl.AddQueryString(_request.Raw.ToQueryString());
        }

        var selectAccountUrl = _options.UserInteraction.SelectAccountUrl;
        if (!selectAccountUrl.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
        }

        var url = selectAccountUrl.AddQueryString(_options.UserInteraction.SelectAccountReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}