namespace CecilBasedAnnotator
{
    using Mono.Cecil;
    using System.Collections.Generic;

    internal static partial class EquivalenceComparers
    {
        private sealed class ArrayDimensionEqualityComparer : IEqualityComparer<ArrayDimension>
        {
            public static readonly ArrayDimensionEqualityComparer Instance = new ArrayDimensionEqualityComparer();

            private ArrayDimensionEqualityComparer()
            {
            }

            public bool Equals(ArrayDimension x, ArrayDimension y)
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
