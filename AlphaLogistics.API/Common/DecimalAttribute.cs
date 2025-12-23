using Microsoft.EntityFrameworkCore;

namespace Logistics.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DecimalAttribute : PrecisionAttribute
    {
        public DecimalAttribute(byte precision = 18, byte scale = 2) : base(precision, scale)
        {

        }
    }
}
