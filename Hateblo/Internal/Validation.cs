using System;
using System.Collections.Generic;
using System.Text;

namespace Hateblo.Internal
{
    internal static class Validation
    {
        internal static void NotNull(object obj, string paramName)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName, "Value cannot be null.");
        }

        internal static void NotNullOrEmpty(string value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName, "Value cannot be null.");

            if (value == "")
                throw new ArgumentException($"Value cannot be empty.", paramName);
        }

        internal static void Requires(bool condition, string paramName, string message = null)
        {
            if (!condition)
                throw new ArgumentException(message, paramName);
        }

        internal static void InRange<T>(T value, T minValue, T maxValue, string paramName, string minValueName = null, string maxValueName = null)
            where T : IComparable<T>
        {
            var message
                = $"{paramName} is "
                + $"less than {minValueName ?? minValue.ToString()} or "
                + $"greater than {maxValueName ?? maxValue.ToString()}.";

            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(paramName, message);
        }
    }
}
