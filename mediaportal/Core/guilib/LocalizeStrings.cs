
using System;
using System.Collections;
using System.Xml;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class GUILocalizeStrings
	{

		static System.Collections.Hashtable m_mapStrings = new Hashtable();

    // singleton. Dont allow any instance of this class
    private GUILocalizeStrings()
    {
    }


    static public void Dispose()
    {
      m_mapStrings.Clear();
    }
    static bool LoadMap(string strFileName, ref System.Collections.Hashtable map, bool bDetermineNumberOfChars)
    {
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
          string strLine=node.SelectSingleNode("value").InnerText;
          int    iCode       =(int)System.Int32.Parse(node.SelectSingleNode("id").InnerText);
          map[iCode]=strLine;
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("exception loading language {0} err:{1} stack:{2}", strFileName, ex.Message,ex.StackTrace);
        return false;
      }
    }

		static public bool	Load(string strFileName)
		{
			System.Collections.Hashtable mapEnglish = new Hashtable();
			LoadMap(strFileName,ref m_mapStrings,true);
			LoadMap(@"language\English\strings.xml",ref mapEnglish,false);
			foreach (DictionaryEntry d in mapEnglish)
			{
				if (!m_mapStrings.ContainsKey((int)d.Key))
				{
					m_mapStrings[d.Key] = (string)d.Value;
					Log.Write("language file:{0} is missing entry for id:{1} text:{2}", strFileName,(int)d.Key,(string)d.Value);
				}
			}
			mapEnglish=null;
			return true;
		}

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
	}
}
