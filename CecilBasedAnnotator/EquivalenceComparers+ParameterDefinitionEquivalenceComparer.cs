// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CecilBasedAnnotator
{
    using Mono.Cecil;
    using System.Collections.Generic;

    internal static partial class EquivalenceComparers
    {
        private sealed class ParameterDefinitionEquivalenceComparer : IEqualityComparer<ParameterDefinition>
        {
            public static readonly ParameterDefinitionEquivalenceComparer Instance = new ParameterDefinitionEquivalenceComparer();

            private ParameterDefinitionEquivalenceComparer()
            {
            }

            public bool Equals(ParameterDefinition x, ParameterDefinition y)
            {
                if (x is null || y is null)
                    return ReferenceEquals(x, y);

                return TypeReference.Equals(x.ParameterType, y.ParameterType);
            }

            public int GetHashCode(ParameterDefinition obj)
            {
                if (obj is null)
                    return 0;

                return TypeReference.GetHashCode(obj.ParameterType);
            }
        }
    }
}
