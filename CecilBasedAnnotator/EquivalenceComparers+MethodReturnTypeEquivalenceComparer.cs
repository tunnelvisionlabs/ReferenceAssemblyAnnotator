namespace CecilBasedAnnotator
{
    using Mono.Cecil;
    using System.Collections.Generic;

    internal static partial class EquivalenceComparers
    {
        private sealed class MethodReturnTypeEquivalenceComparer : IEqualityComparer<MethodReturnType>
        {
            public static readonly MethodReturnTypeEquivalenceComparer Instance = new MethodReturnTypeEquivalenceComparer();

            private MethodReturnTypeEquivalenceComparer()
            {
            }

            public bool Equals(MethodReturnType x, MethodReturnType y)
            {
                if (x is null || y is null)
                    return ReferenceEquals(x, y);

                return TypeReference.Equals(x.ReturnType, y.ReturnType);
            }

            public int GetHashCode(MethodReturnType obj)
            {
                if (obj is null)
                    return 0;

                return TypeReference.GetHashCode(obj.ReturnType);
            }
        }
    }
}
