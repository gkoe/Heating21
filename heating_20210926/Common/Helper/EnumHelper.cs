using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public static class EnumHelper
    {
        public static int ToInt(Enum @enum) => (int)Convert.ChangeType(@enum, typeof(int));
    }
}
