// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    internal static partial class EquivalenceComparers
    {
        private sealed class MethodDefinitionEquivalenceComparer : IEqualityComparer<MethodDefinition?>
        {
            public static readonly MethodDefinitionEquivalenceComparer Instance = new MethodDefinitionEquivalenceComparer();

            private MethodDefinitionEquivalenceComparer()
            {
            }

            public bool Equals(MethodDefinition? x, MethodDefinition? y)
            {
                if (x is null || y is null)
                    return ReferenceEquals(x, y);

                if ((x.Attributes & MethodAttributes.MemberAccessMask) != (y.Attributes & MethodAttributes.MemberAccessMask))
                    return false;

                if (x.IsStatic != y.IsStatic)
                    return false;

                if (x.Name != y.Name)
                    return false;

                if (x.GenericParameters.Count != y.GenericParameters.Count)
                    return false;

                if (x.Parameters.Count != y.Parameters.Count)
                    return false;

                if (!MethodReturnType.Equals(x.MethodReturnType, y.MethodReturnType))
                    return false;

                if (!x.Parameters.SequenceEqual(y.Parameters, ParameterDefinition))
                    return false;

                return true;
            }

            public int GetHashCode(MethodDefinition? obj)
            {
                if (obj is null)
                    return 0;

                return obj.Name.GetHashCode()
                    ^ obj.Parameters.Count;
            }
        }
    }
}
