using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Sample1
{
    public class Class1
    {
        public static void M(SqlConnection connection, [AllowNull] string? x)
        {
            if (!string.IsNullOrEmpty(x))
            {
                x.ToString();
            }
            
            _ = EqualityComparer<string?>.Default.GetHashCode(x);
        }
    }
}
