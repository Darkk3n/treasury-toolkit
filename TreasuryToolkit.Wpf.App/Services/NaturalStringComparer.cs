using System.Runtime.InteropServices;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Explorer-style ordering ("2.pdf" before "10.pdf"), as v1 did with StrCmpLogicalW. Source files must be
    /// processed in this order so pages line up with the grid rows.
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string>
    {
        public static readonly NaturalStringComparer Instance = new();

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public int Compare(string x, string y) => StrCmpLogicalW(x, y);
    }
}
