// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private class PredefinedType : WellKnownType
        {
            private readonly Type _type;

            public PredefinedType(Type type)
            {
                _type = type;
            }

            protected override TypeReference GetOrCreateTypeReferenceImpl(ModuleDefinition module, WellKnownTypes wellKnownTypes)
            {
                return ResolveRequiredWellKnownType(module, _type);
            }
        }
    }
}
