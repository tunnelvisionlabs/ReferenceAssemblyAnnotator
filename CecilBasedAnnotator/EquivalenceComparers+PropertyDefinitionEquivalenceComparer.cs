namespace CecilBasedAnnotator
{
    using Mono.Cecil;
    using System.Collections.Generic;

    internal static partial class EquivalenceComparers
    {
        private sealed class PropertyDefinitionEquivalenceComparer : IEqualityComparer<PropertyDefinition>
        {
            public static readonly PropertyDefinitionEquivalenceComparer Instance = new PropertyDefinitionEquivalenceComparer();

            private PropertyDefinitionEquivalenceComparer()
            {
            }

            public bool Equals(PropertyDefinition x, PropertyDefinition y)
            {
                if (x is null || y is null)
                    return ReferenceEquals(x, y);

                if (x.Name != y.Name)
                    return false;

                // We intentionally allow the right hand side add accessors, but not remove or change them
                if (x.GetMethod is object && !MethodDefinition.Equals(x.GetMethod, y.GetMethod))
                    return false;

                if (x.SetMethod is object && !MethodDefinition.Equals(x.SetMethod, y.SetMethod))
                    return false;

                return true;
            }

            public int GetHashCode(PropertyDefinition obj)
            {
                if (obj is null)
                    return 0;

                return obj.Name.GetHashCode();
            }
        }
    }
}
