using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Hateblo
{
    internal static class Category
    {
        internal static IEnumerable<string> FromXElement(XElement xElement)
        {
            return xElement.Elements(Extensions._atomNamespace + "category")
                .Attributes("term")
                .Select(a => a.Value);
        }
    }
}
