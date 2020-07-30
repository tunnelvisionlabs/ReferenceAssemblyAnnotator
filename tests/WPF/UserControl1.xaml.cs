using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace WPF
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

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
