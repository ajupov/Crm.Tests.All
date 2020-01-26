using System;

namespace Crm.Tests.All.Extensions
{
    public static class ValueWithGuidExtension
    {
        public static string WithGuid(this string value)
        {
            return $"{value}-{Guid.NewGuid()}";
        }
    }
}