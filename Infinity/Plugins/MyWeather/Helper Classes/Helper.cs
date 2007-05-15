using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

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

        /// <summary>
        /// this will take the CityProviderInfo Object and turn it to a
        /// City object by adding empty data
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static City CityInfoToCityObject(CitySetupInfo info)
        {
            return new City(info);
        }

        /// <summary>
        /// this will create a new list of cities, that
        /// already hold the providerinformation
        /// </summary>
        /// <param name="cpiList"></param>
        /// <returns></returns>
        public static List<City> CityInfoListToCityObjectList(List<CitySetupInfo> cpiList)
        {
            List<City> cList = new List<City>();
            foreach (CitySetupInfo cpi in cpiList)
            {
                cList.Add(new City(cpi));
            }
            return cList;
        }

        /// <summary>
        /// encode a given string to an URL
        /// </summary>
        /// <param name="instring"></param>
        /// <returns></returns>
        public static string UrlEncode(string instring)
        {
            StringReader strRdr = new StringReader(instring);
            StringWriter strWtr = new StringWriter();
            int charValue = strRdr.Read();
            while (charValue != -1)
            {
                if (((charValue >= 48) && (charValue <= 57)) // 0-9
                  || ((charValue >= 65) && (charValue <= 90)) // A-Z
                  || ((charValue >= 97) && (charValue <= 122))) // a-z
                {
                    strWtr.Write((char)charValue);
                }
                else if (charValue == 32)  // Space
                {
                    strWtr.Write("+");
                }
                else
                {
                    strWtr.Write("%{0:x2}", charValue);
                }

                charValue = strRdr.Read();
            }

            return strWtr.ToString();
        }
 
    }
}
