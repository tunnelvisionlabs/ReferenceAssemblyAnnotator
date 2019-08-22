// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal static partial class EquivalenceComparers
    {
        private sealed class TypeDefinitionEquivalenceComparer : IEqualityComparer<TypeDefinition>
        {
            public static readonly TypeDefinitionEquivalenceComparer Instance = new TypeDefinitionEquivalenceComparer();

            private TypeDefinitionEquivalenceComparer()
            {
            }

            public bool Equals(TypeDefinition x, TypeDefinition y)
            {
                if (x is null || y is null)
                    return ReferenceEquals(x, y);

                if (x.IsNested)
                {
                    if (!y.IsNested || !Equals(x.DeclaringType, y.DeclaringType))
                        return false;
                }
                else if (y.IsNested)
                {
                    return false;
                }

                if (x.Namespace != y.Namespace || x.Name != y.Name)
                    return false;

                if (x.GenericParameters.Count != y.GenericParameters.Count)
                    return false;

                return true;
            }

            public int GetHashCode(TypeDefinition obj)
            {
                if (obj is null)
                    return 0;

                return GetHashCode(obj.DeclaringType)
                    ^ obj.Name.GetHashCode();
            }
        }
    }
}
