
using System;

namespace Base.Helper
{
    public static class EnumHelper
    {
        public static int ToInt(Enum @enum) => (int)Convert.ChangeType(@enum, typeof(int));
    }
}
