// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System;
    using Mono.Cecil;

    internal partial class WellKnownTypes
    {
        private sealed class DoesNotReturnAttributeProvidedType : ProvidedAttributeType
        {
            public DoesNotReturnAttributeProvidedType()
                : base("System.Diagnostics.CodeAnalysis", "DoesNotReturnAttribute")
            {
            }

            protected override void ImplementAttribute(ModuleDefinition module, TypeDefinition attribute, WellKnownTypes wellKnownTypes, CustomAttributeFactory attributeFactory)
            {
                attribute.AddDefaultConstructor(wellKnownTypes.TypeSystem);
                attribute.CustomAttributes.Add(attributeFactory.AttributeUsage(AttributeTargets.Method, inherited: false));
            }
        }
    }
}
