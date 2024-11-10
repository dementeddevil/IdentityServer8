// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer8.AspNetIdentity
{
    internal class Decorator<TService>
    {
        public TService? Instance { get; set; }

        public Decorator(TService instance)
        {
            Instance = instance;
        }
    }

    internal class Decorator<TService, TImplementation> : Decorator<TService>
        where TImplementation : class, TService
    {
        public Decorator(TImplementation instance) : base(instance)
        {
        }
    }

    internal class DisposableDecorator<TService> : Decorator<TService>, IDisposable
    {
        public DisposableDecorator(TService instance) : base(instance)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                (Instance as IDisposable)?.Dispose();
                Instance = default;
            }
        }
    }
}
