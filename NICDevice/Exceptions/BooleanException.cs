using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NICDevice.Exceptions
{
    public static class BooleanException
    {
        public static void ThrowIfFalse(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
        public static void ThrowIfTrue(bool condition, string message)
        {
            if (condition)
            {
                throw new Exception(message);
            }
        }
        public static void ThrowIfNull(object @object, string message)
        {
            if (@object is null)
            {
                throw new Exception(message);
            }
        }
    }
}
