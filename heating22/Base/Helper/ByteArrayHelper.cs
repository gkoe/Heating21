using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Helper
{
    public static class ByteArrayHelper
    {
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            byte[] byteArray = new byte[16];
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                string hexByte = hexString.Substring(i * 2, 2);
                byteArray[i] = Convert.ToByte(hexByte, 16);
            }
            return byteArray;
        }


    }
}
