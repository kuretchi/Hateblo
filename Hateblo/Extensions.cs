using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Hateblo
{
    internal static class Extensions
    {
        internal static readonly XNamespace _atomNamespace
            = "http://www.w3.org/2005/Atom";

        internal static readonly XNamespace _appNamespace
            = "http://www.w3.org/2007/app";

        internal static readonly XNamespace _hatenaNamespace
            = "http://www.hatena.ne.jp/info/xmlns#";

        internal static TimeSpan Max(TimeSpan val1, TimeSpan val2)
            => val1 > val2 ? val1 : val2;
    }
}
