using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;


namespace MediaPortal.GUI.Video
{
    class TSRVodTrailers
    {
        public static string TempXML = string.Empty;

        public static string[] MenuName = new string[20];
        public static string[] MenuURL = new string[20];	// strings for all menu items
        public static string[] SubMenuName = new string[200];
        public static string[] SubMenuURL = new string[200];	// strings for all sub menu items

        public static bool menuview = false; // bools for reminding which view the user is in
        public static bool submenuview = false;

        public static void GetMenu()
        {
            TrailersUtility TU = new TrailersUtility();
            TU.GetWebPage("http://www.tsr.ch/xml/electron_libre.xml", out TempXML);
            if (TempXML == null || TempXML == string.Empty)
                return;
            //TempXML = TempXML.Replace("UTF-8", "windows-1250");

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(TempXML);
            XmlNodeList MenuNames = XmlDoc.SelectNodes("/base/feeds/feed/nom");
            XmlNodeList MenuSources = XmlDoc.SelectNodes("/base/feeds/feed/source");
            if (MenuNames == null)
                return;
            for (int i = 0; i < MenuNames.Count; i++)
            {
                MenuName[i] = MenuNames[i].InnerText;
                Byte[] encodedBytes = Encoding.ASCII.GetBytes(MenuSources[i].InnerText);
                MenuURL[i] = Encoding.ASCII.GetString(encodedBytes);
            }


        }
        public static void GetSubMenu(string url, string speed, string nmbOfResults)
        {
            //empty old sub menu
            Array.Clear(SubMenuName, 0, 200);
            Array.Clear(SubMenuURL, 0, 200);
            Regex rgx = new Regex("&nmbOfResults=\\d");
            TrailersUtility TU = new TrailersUtility();
            TU.GetWebPage(url, out TempXML);
            if (TempXML == null || TempXML == string.Empty)
                return;
            //TempXML = TempXML.Replace("UTF-8", "windows-1250");

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(TempXML);
            XmlNodeList MenuNames = XmlDoc.SelectNodes("/sujets/sujet/titre");
            XmlNodeList MenuEmissions = XmlDoc.SelectNodes("/sujets/sujet/nomEmission");
            //Il faut que je demande le 1er de chaque noeud pour être sur d'en avoir qu'un.
            XmlNodeList MenuSources = XmlDoc.SelectNodes("/sujets/sujet/file"+speed+"kWmv[1]");
            if (MenuSources == null)
                return;
            for (int i = 0; i < MenuNames.Count; i++)
            {
                SubMenuName[i] = MenuNames[i].InnerText + " " + MenuEmissions[i].InnerText;
                Byte[] encodedBytes = Encoding.ASCII.GetBytes(MenuSources[i].InnerText);
                SubMenuURL[i] = Encoding.ASCII.GetString(encodedBytes);
                if (nmbOfResults != "-1")
                {                    
                    SubMenuURL[i] = rgx.Replace(SubMenuURL[i], nmbOfResults);
                }
            }


        }
    }
}
