// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal static partial class EquivalenceComparers
    {
        public static IEqualityComparer<MethodDefinition> MethodDefinition
            => MethodDefinitionEquivalenceComparer.Instance;

        public static IEqualityComparer<MethodReturnType> MethodReturnType
            => MethodReturnTypeEquivalenceComparer.Instance;

        public static IEqualityComparer<ParameterDefinition> ParameterDefinition
            => ParameterDefinitionEquivalenceComparer.Instance;

        public static IEqualityComparer<PropertyDefinition> PropertyDefinition
            => PropertyDefinitionEquivalenceComparer.Instance;

        public static IEqualityComparer<TypeDefinition> TypeDefinition
            => TypeDefinitionEquivalenceComparer.Instance;

        public static IEqualityComparer<TypeReference> TypeReference
            => TypeReferenceEquivalenceComparer.Instance;
    }
}
