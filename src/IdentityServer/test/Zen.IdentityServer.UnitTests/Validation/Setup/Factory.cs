// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer.UnitTests.Common;
using Zen.IdentityServer.Configuration;
using Zen.IdentityServer.Models;
using Zen.IdentityServer.Services;
using Zen.IdentityServer.Services.Default;
using Zen.IdentityServer.Stores;
using Zen.IdentityServer.Stores.Serialization;
using Zen.IdentityServer.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer.UnitTests.Validation.Setup;

internal static class Factory
{
    public static IClientStore CreateClientStore()
    {
        return new InMemoryClientStore(TestClients.Get());
    }

    public static TokenRequestValidator CreateTokenRequestValidator(
        IdentityServerOptions? options = null,
        IResourceStore? resourceStore = null,
        IAuthorizationCodeStore? authorizationCodeStore = null,
        IRefreshTokenStore? refreshTokenStore = null,
        IResourceOwnerPasswordValidator? resourceOwnerValidator = null,
        IProfileService? profile = null,
        IDeviceCodeValidator? deviceCodeValidator = null,
        IEnumerable<IExtensionGrantValidator>? extensionGrantValidators = null,
        ICustomTokenRequestValidator? customRequestValidator = null,
        ITokenValidator? tokenValidator = null,
        IRefreshTokenService? refreshTokenService = null,
        IResourceValidator? resourceValidator = null)
    {
        options ??= TestIdentityServerOptions.Create();

        resourceStore ??= new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis(), TestScopes.GetScopes());

        resourceOwnerValidator ??= new TestResourceOwnerPasswordValidator();

        profile ??= new TestProfileService();
        
        deviceCodeValidator ??= new TestDeviceCodeValidator();

        customRequestValidator ??= new DefaultCustomTokenRequestValidator();

        ExtensionGrantValidator aggregateExtensionGrantValidator;
        if (extensionGrantValidators == null)
        {
            aggregateExtensionGrantValidator = new ExtensionGrantValidator(new[] { new TestGrantValidator() }, TestLogger.Create<ExtensionGrantValidator>());
        }
        else
        {
            aggregateExtensionGrantValidator = new ExtensionGrantValidator(extensionGrantValidators, TestLogger.Create<ExtensionGrantValidator>());
        }

        authorizationCodeStore ??= CreateAuthorizationCodeStore();

        refreshTokenStore ??= CreateRefreshTokenStore();

        resourceValidator ??= CreateResourceValidator(resourceStore);
        
        tokenValidator ??= CreateTokenValidator(refreshTokenStore: refreshTokenStore, profile: profile);

        refreshTokenService ??= CreateRefreshTokenService(
                refreshTokenStore,
                profile);

        return new TokenRequestValidator(
            options,
            authorizationCodeStore,
            resourceOwnerValidator,
            profile,
            deviceCodeValidator,
            aggregateExtensionGrantValidator,
            customRequestValidator,
            resourceValidator,
            resourceStore,
            tokenValidator,
            refreshTokenService,
            new TestEventService(), 
            new StubClock(), 
            TestLogger.Create<TokenRequestValidator>());
    }

    private static IRefreshTokenService CreateRefreshTokenService(IRefreshTokenStore store, IProfileService profile)
    {
        var service = new DefaultRefreshTokenService(
            store,
            profile,
            new StubClock(),
            TestLogger.Create<DefaultRefreshTokenService>());

        return service;
    }

    internal static IResourceValidator CreateResourceValidator(IResourceStore? store = null)
    {
        store = store ?? new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis(), TestScopes.GetScopes());
        return new DefaultResourceValidator(store, new DefaultScopeParser(TestLogger.Create<DefaultScopeParser>()), TestLogger.Create<DefaultResourceValidator>());
    }

    internal static ITokenCreationService CreateDefaultTokenCreator(IdentityServerOptions? options = null)
    {
        return new DefaultTokenCreationService(
            new StubClock(),
            new DefaultKeyMaterialService(new IValidationKeysStore[] { },
                new ISigningCredentialStore[] { new InMemorySigningCredentialsStore(TestCert.LoadSigningCredentials()) }),
            options ?? TestIdentityServerOptions.Create(),
            TestLogger.Create<DefaultTokenCreationService>());
    }

    public static DeviceAuthorizationRequestValidator CreateDeviceAuthorizationRequestValidator(
        IdentityServerOptions? options = null,
        IResourceStore? resourceStore = null,
        IResourceValidator? resourceValidator = null)
    {
        options ??= TestIdentityServerOptions.Create();
        
        resourceStore ??= new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis(), TestScopes.GetScopes());

        resourceValidator ??= CreateResourceValidator(resourceStore);


        return new DeviceAuthorizationRequestValidator(
            options,
            resourceValidator,
            TestLogger.Create<DeviceAuthorizationRequestValidator>());
    }

    public static AuthorizeRequestValidator CreateAuthorizeRequestValidator(
        IdentityServerOptions? options = null,
        IResourceStore? resourceStore = null,
        IClientStore? clients = null,
        IProfileService? profile = null,
        ICustomAuthorizeRequestValidator? customValidator = null,
        IRedirectUriValidator? uriValidator = null,
        IResourceValidator? resourceValidator = null,
        JwtRequestValidator? jwtRequestValidator = null,
        IJwtRequestUriHttpClient? jwtRequestUriHttpClient = null)
    {
        options ??= TestIdentityServerOptions.Create();

        resourceStore ??= new InMemoryResourcesStore(TestScopes.GetIdentity(), TestScopes.GetApis(), TestScopes.GetScopes());

        clients ??= new InMemoryClientStore(TestClients.Get());

        customValidator ??= new DefaultCustomAuthorizeRequestValidator();

        uriValidator ??= new StrictRedirectUriValidator();

        resourceValidator ??= CreateResourceValidator(resourceStore);

        jwtRequestValidator ??= new JwtRequestValidator("https://identityserver", new LoggerFactory().CreateLogger<JwtRequestValidator>());

        jwtRequestUriHttpClient ??= new DefaultJwtRequestUriHttpClient(new HttpClient(new NetworkHandler(new Exception("no jwt request uri response configured"))), options, new LoggerFactory());


        var userSession = new MockUserSession();

        return new AuthorizeRequestValidator(
            options,
            clients,
            customValidator,
            uriValidator,
            resourceValidator,
            userSession,
            jwtRequestValidator,
            jwtRequestUriHttpClient,
            TestLogger.Create<AuthorizeRequestValidator>());
    }

    public static TokenValidator CreateTokenValidator(
        IReferenceTokenStore? store = null, 
        IRefreshTokenStore? refreshTokenStore = null,
        IProfileService? profile = null, 
        IdentityServerOptions? options = null,
        ISystemClock? clock = null)
    {
        options ??= TestIdentityServerOptions.Create();

        profile ??= new TestProfileService();

        store ??= CreateReferenceTokenStore();

        clock = clock ?? new StubClock();

        refreshTokenStore ??= CreateRefreshTokenStore();

        var clients = CreateClientStore();
        var context = new MockHttpContextAccessor(options);
        var logger = TestLogger.Create<TokenValidator>();

        var keyInfo = new SecurityKeyInfo
        {
            Key = TestCert.LoadSigningCredentials().Key,
            SigningAlgorithm = "RS256"
        };

        var validator = new TokenValidator(
            clients: clients,
            clock: clock,
            profile: profile,
            referenceTokenStore: store,
            refreshTokenStore: refreshTokenStore,
            customValidator: new DefaultCustomTokenValidator(),
                keys: new DefaultKeyMaterialService(new[] { new InMemoryValidationKeysStore(new[] { keyInfo }) }, Enumerable.Empty<ISigningCredentialStore>()),
            logger: logger,
            options: options,
            context: context);

        return validator;
    }

    public static IDeviceCodeValidator CreateDeviceCodeValidator(
        IDeviceFlowCodeService service,
        IProfileService? profile = null,
        IDeviceFlowThrottlingService? throttlingService = null,
        ISystemClock? clock = null)
    {
        profile = profile ?? new TestProfileService();
        throttlingService = throttlingService ?? new TestDeviceFlowThrottlingService();
        clock = clock ?? new StubClock();
        
        var validator = new DeviceCodeValidator(service, profile, throttlingService, clock, TestLogger.Create<DeviceCodeValidator>());

        return validator;
    }

    public static IClientSecretValidator CreateClientSecretValidator(
        IClientStore? clients = null,
        SecretParser? parser = null,
        SecretValidator? validator = null,
        IdentityServerOptions? options = null)
    {
        options = options ?? TestIdentityServerOptions.Create();

        clients ??= new InMemoryClientStore(TestClients.Get());

        if (parser == null)
        {
            var parsers = new List<ISecretParser>
            {
                new BasicAuthenticationSecretParser(options, TestLogger.Create<BasicAuthenticationSecretParser>()),
                new PostBodySecretParser(options, TestLogger.Create<PostBodySecretParser>())
            };

            parser = new SecretParser(parsers, TestLogger.Create<SecretParser>());
        }

        if (validator == null)
        {
            var validators = new List<ISecretValidator>
            {
                new HashedSharedSecretValidator(TestLogger.Create<HashedSharedSecretValidator>()),
                new PlainTextSharedSecretValidator(TestLogger.Create<PlainTextSharedSecretValidator>())
            };

            validator = new SecretValidator(new StubClock(), validators, TestLogger.Create<SecretValidator>());
        }

        return new ClientSecretValidator(clients, parser, validator, new TestEventService(), TestLogger.Create<ClientSecretValidator>());
    }

    public static IAuthorizationCodeStore CreateAuthorizationCodeStore()
    {
        return new DefaultAuthorizationCodeStore(new InMemoryPersistedGrantStore(),
            new PersistentGrantSerializer(),
            new DefaultHandleGenerationService(),
            TestLogger.Create<DefaultAuthorizationCodeStore>());
    }
    
    public static IRefreshTokenStore CreateRefreshTokenStore()
    {
        return new DefaultRefreshTokenStore(new InMemoryPersistedGrantStore(),
            new PersistentGrantSerializer(),
            new DefaultHandleGenerationService(),
            TestLogger.Create<DefaultRefreshTokenStore>());
    }
    
    public static IReferenceTokenStore CreateReferenceTokenStore()
    {
        return new DefaultReferenceTokenStore(new InMemoryPersistedGrantStore(),
            new PersistentGrantSerializer(),
            new DefaultHandleGenerationService(),
            TestLogger.Create<DefaultReferenceTokenStore>());
    }

    public static IDeviceFlowCodeService CreateDeviceCodeService()
    {
        return new DefaultDeviceFlowCodeService(new InMemoryDeviceFlowStore(), new DefaultHandleGenerationService());
    }
    
    public static IUserConsentStore CreateUserConsentStore()
    {
        return new DefaultUserConsentStore(new InMemoryPersistedGrantStore(),
            new PersistentGrantSerializer(),
            new DefaultHandleGenerationService(),
            TestLogger.Create<DefaultUserConsentStore>());
    }
}