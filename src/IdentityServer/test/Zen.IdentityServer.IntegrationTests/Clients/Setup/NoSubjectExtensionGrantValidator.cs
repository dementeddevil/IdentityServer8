﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Zen.IdentityServer.Models;
using Zen.IdentityServer.Validation;

namespace IdentityServer.IntegrationTests.Clients.Setup;

public class NoSubjectExtensionGrantValidator : IExtensionGrantValidator
{
    public Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var credential = context.Request.Raw.Get("custom_credential");

        if (credential != null)
        {
            context.Result = new GrantValidationResult();
        }
        else
        {
            // custom error message
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
        }

        return Task.CompletedTask;
    }

    public string GrantType => "custom.nosubject";
}