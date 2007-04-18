using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MyWeather
{
    public class Helper
    {
        /// <summary>
        /// dll Import to check Internet Connection
        /// </summary>
        /// <param name="Description"></param>
        /// <param name="ReservedValue"></param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// check if we have an Internetconnection
        /// </summary>
        /// <param name="code"></param>
        /// <returns>true if Internetconnection is available</returns>
        public static bool IsConnectedToInternet(ref int code)
        {
            return InternetGetConnectedState(out code, 0);
        }

    }
}
