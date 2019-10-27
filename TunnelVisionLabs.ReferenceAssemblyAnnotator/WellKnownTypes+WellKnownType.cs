// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private abstract class WellKnownType
        {
            private readonly object _syncObject = new object();
            private TypeReference? _typeReference;

            public TypeReference GetOrCreateTypeReference(ModuleDefinition module, WellKnownTypes wellKnownTypes)
            {
                if (_typeReference is null)
                {
                    lock (_syncObject)
                    {
                        if (_typeReference is null)
                        {
                            _typeReference = GetOrCreateTypeReferenceImpl(module, wellKnownTypes);
                        }
                    }
                }

                return _typeReference;
            }

            protected abstract TypeReference GetOrCreateTypeReferenceImpl(ModuleDefinition module, WellKnownTypes wellKnownTypes);
        }
    }
}
