/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// This class will hold all text used in the application
	/// The text is loaded for the current language from
	/// the file language/[language]/strings.xml
	/// </summary>
	public class GUILocalizeStrings
	{
        const string LanguageDirectory = @"language\";
        private static string[] m_Languages = null;

    static System.Collections.Generic.Dictionary<int, string> m_mapStrings = new System.Collections.Generic.Dictionary<int, string>();

    // singleton. Dont allow any instance of this class
    private GUILocalizeStrings()
    {
    }


		/// <summary>
		/// Clean up.
		/// just delete all text
		/// </summary>
    static public void Dispose()
    {
      m_mapStrings.Clear();
    }

		/// <summary>
		/// Load the text from the strings.xml file
		/// </summary>
		/// <param name="strFileName">filename to string.xml for current language</param>
		/// <param name="map">on return this map will contain all texts loaded</param>
		/// <param name="bDetermineNumberOfChars">
		/// when true this function will determine the total number of characters needed for this language.
		/// This is later on used by the font classes to cache those characters
		/// when false this function wont determine the total number of characters needed for this language.
		/// </param>
		/// <returns></returns>
    static bool LoadMap(string strFileName, ref System.Collections.Generic.Dictionary<int, string> map, bool bDetermineNumberOfChars)
    {
		bool isPrefixEnabled = true;

		using(MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			isPrefixEnabled = reader.GetValueAsBool("general", "myprefix", true);

			if (strFileName==null) return false;
			if (strFileName==String.Empty) return false;
			if (map==null) return false;
      map.Clear();
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(strFileName);
        if (doc.DocumentElement==null) return false;
        string strRoot=doc.DocumentElement.Name;
        if (strRoot!="strings") return false;
        if (bDetermineNumberOfChars==true)
        {
          int iChars=255;
          XmlNode nodeChars = doc.DocumentElement.SelectSingleNode("/strings/characters");
          if (nodeChars!=null)
          {
            if (nodeChars.InnerText!=null && nodeChars.InnerText.Length>0)
            {
              try
              {
                iChars=Convert.ToInt32(nodeChars.InnerText);
                if (iChars < 255) iChars=255;
              }
              catch(Exception)
              {
                iChars=255;
              }
              GUIGraphicsContext.CharsInCharacterSet=iChars;
            }
          }
        }
        XmlNodeList list=doc.DocumentElement.SelectNodes("/strings/string");
        foreach (XmlNode node in list)
        {
			StringBuilder builder = new StringBuilder();

			int    iCode       =(int)System.Int32.Parse(node.SelectSingleNode("id").InnerText);

			XmlAttribute prefix = node.Attributes["Prefix"];

			if(isPrefixEnabled && prefix != null)
				builder.Append(prefix.Value);

			builder.Append(node.SelectSingleNode("value").InnerText);

			XmlAttribute suffix = node.Attributes["Suffix"];

			if(isPrefixEnabled && suffix != null)
				builder.Append(suffix.Value);

			map[iCode]=builder.ToString();
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("exception loading language {0} err:{1} stack:{2}", strFileName, ex.Message,ex.StackTrace);
        return false;
      }
    }

		/// <summary>
		/// Public method to load the text from a strings/xml file into memory
		/// </summary>
		/// <param name="strFileName">Contains the filename+path for the string.xml file</param>
		/// <returns>
		/// true when text is loaded
		/// false when it was unable to load the text
		/// </returns>
		static public bool	Load(string strFileName)
		{
			if (strFileName==null) return false;
			if (strFileName==String.Empty) return false;
      System.Collections.Generic.Dictionary<int, string> mapEnglish = new System.Collections.Generic.Dictionary<int, string>();
      m_mapStrings.Clear();
			Log.Write("  load localized strings from:{0}", strFileName);
			// load the text for the current language
			LoadMap(strFileName,ref m_mapStrings,true);
			//load the text for the english language
			LoadMap(@"language\English\strings.xml",ref mapEnglish,false);

			// check if current language contains an entry for each textline found
			// in the english version

      Dictionary<int, string>.KeyCollection keyColl = mapEnglish.Keys;
      foreach (int key in keyColl)
      {
        if (!m_mapStrings.ContainsKey(key))
        {
          //if current language does not contain a translation for this text
          //then use the english variant
          m_mapStrings[key] = mapEnglish[key];
          Log.Write("language file:{0} is missing entry for id:{1} text:{2}", strFileName, key, (string)mapEnglish[key]);
        }
      }
			mapEnglish=null;
			return true;
		} 

		/// <summary>
		/// Get the translation for a given id
		/// </summary>
		/// <param name="dwCode">id of text</param>
		/// <returns>
		/// string containing the translated text
		/// </returns>
		static public string  Get(int dwCode)
		{
			if (m_mapStrings.ContainsKey(dwCode))
			{
				return (string)m_mapStrings[dwCode];
			}
			return "";
		}
		
		static public void LocalizeLabel(ref string strLabel)
		{
			if (strLabel==null) strLabel=String.Empty;
			if (strLabel == "-")	strLabel = "";
			if (strLabel == "")		return;
			// This can't be a valid string code if the first character isn't a number.
			// This check will save us from catching unnecessary exceptions.
			if (!char.IsNumber(strLabel, 0))
				return;
			try
			{
				int dwLabelID = System.Int32.Parse(strLabel);
				strLabel = GUILocalizeStrings.Get(dwLabelID);
			}
			catch (FormatException)
			{
			}
		}

		public static void Clear()
		{
			m_mapStrings.Clear();
		}

        public static string LocalSupported()
        {
            if (m_Languages == null)
            {
                LoadLanguages();
            }

            int numb = LanguageNumb(CultureInfo.CurrentCulture.EnglishName);

            if (numb == -1 && CultureInfo.CurrentCulture.Parent.EnglishName != null)
            {
                numb = LanguageNumb(CultureInfo.CurrentCulture.Parent.EnglishName);
            }

            if (numb >= 0)
                return m_Languages[numb];
            else
                return "English";

        }

        private static int LanguageNumb(string Language)
        {

            for (int i = 0; i < m_Languages.Length; i++)
            {
                if (m_Languages[i].ToLower() == Language.ToLower())
                    return i;
            }

            return -1;
        }

        public static string[] SupportedLanguages()
        {
            if (m_Languages == null)
            {
                //// Get system language
                //string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
                //int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
                //string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);
                //bool bExactLanguageFound = false;
                LoadLanguages();
            }

            return m_Languages;
        }

        private static void LoadLanguages()
        {
            if (Directory.Exists(LanguageDirectory))
            {
                string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");

                ArrayList tempList = new ArrayList();

                foreach (string folder in folders)
                {
                    string fileName = folder.Substring(@"language\".Length);

                    //
                    // Exclude cvs folder
                    //
                    if (fileName.ToLower() != "cvs")
                    {
                        if (fileName.Length > 0)
                        {
                            fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
                            tempList.Add(fileName);
                        }
                    }
                }
                
                m_Languages = new string[tempList.Count];

                for(int i=0; i < tempList.Count; i++)
                    m_Languages[i] = (string) tempList[i];
            }
        }
	}
}
