using System;
using System.Collections.Generic;
using System.Text;

namespace Hateblo.Internal
{
    internal static class ThrowHelper
    {
        internal static void ReportBug(string message = "")
        {
            throw new NotImplementedException("Please report this as a bug. Additional information: " + message);
        }
    }
}
