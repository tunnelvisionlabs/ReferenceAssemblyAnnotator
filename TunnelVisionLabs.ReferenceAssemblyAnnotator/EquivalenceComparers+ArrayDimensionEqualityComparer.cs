// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace TunnelVisionLabs.ReferenceAssemblyAnnotator
{
    using System.Collections.Generic;
    using Mono.Cecil;

    internal static partial class EquivalenceComparers
    {
        private sealed class ArrayDimensionEqualityComparer : IEqualityComparer<ArrayDimension>
        {
            public static readonly ArrayDimensionEqualityComparer Instance = new ArrayDimensionEqualityComparer();

            private ArrayDimensionEqualityComparer()
            {
            }

#pragma warning disable CS8614 // In CLI builds: Nullability of reference types in type of parameter 'x' of 'bool ArrayDimensionEqualityComparer.Equals(ArrayDimension x, ArrayDimension y)' doesn't match implicitly implemented member 'bool IEqualityComparer<ArrayDimension>.Equals(ArrayDimension x, ArrayDimension y)'.
            public bool Equals(ArrayDimension x, ArrayDimension y)
#pragma warning restore CS8614
            {
                return x.LowerBound == y.LowerBound
                    && x.UpperBound == y.UpperBound;
            }

            public int GetHashCode(ArrayDimension obj)
            {
                return obj.LowerBound.GetHashCode()
                    ^ obj.UpperBound.GetHashCode();
            }
        }
    }
}
