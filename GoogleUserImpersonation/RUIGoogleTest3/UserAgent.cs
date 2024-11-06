using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Pet.RUIGoogle
{

    public static class UserAgent
    {

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkGetSessionOption(
            int dwOption, StringBuilder pBuffer, int dwBufferLength, out int pdwBufferLength, int dwReserved);
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(
            int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        const int URLMON_OPTION_USERAGENT = 0x10000001;
        const int URLMON_OPTION_USERAGENT_REFRESH = 0x10000002;
        const int URLMON_OPTION_URL_ENCODING = 0x10000004;

        public static string Value
        {
            get
            {
                StringBuilder builder = new StringBuilder(512);
                int returnLength;
                UrlMkGetSessionOption(URLMON_OPTION_USERAGENT, builder, builder.Capacity, out returnLength, 0);
                string value = builder.ToString();
                return value;
            }
            //set
            //{
            //    UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, value, value.Length, 0);
            //}
        }
    }

}
