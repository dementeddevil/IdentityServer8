// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


#pragma warning disable 1591

namespace Zen.IdentityServer.Stores.Serialization
{
    public class ClaimLite
    {
        public required string Type { get; set; }
        public required string Value { get; set; }
        public required string ValueType { get; set; }
    }
}