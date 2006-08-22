/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Xml;
using MediaPortal.GUI.Library;
using ProgramsDatabase;
using Programs.Utils;
using MediaPortal.Utils.Services;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramContentManager.
  /// </summary>
  public class ProgramContentManager
  {
    static XmlNodeList NodeList = null;
    static XmlElement rootElement = null; 
    static ILog _log;
    static IConfig _config;

    static public int NodeCount
    {
      get
      {
        if (NodeList != null)
        {
          return NodeList.Count;
        }
        else
        {
          return - 1;
        }
      }
    }

    static public int NodeID(int Index)
    {
      int result = - 1;
      if (NodeList == null)
      {
        return - 1;
      }
      if ((Index >= 0) && (Index <= NodeList.Count - 1))
      {
        XmlNode node = NodeList.Item(Index);
        if (node != null)
        {
          //				string strVal = node.Attributes["id"].Value;
          //				result = Convert.ToInt32(strVal.Length > 0 ? strVal : "-1");
          result = ExtractNodeID(node);
        }
      }
      return result;
    }

    static int ExtractNodeID(XmlNode node)
    {
      string strVal = node.Attributes["id"].Value;
      return Convert.ToInt32(strVal.Length > 0 ? strVal : "-1");
    }

    static public string NodeTitle(int Index)
    {
      string result = "";
      if (NodeList == null)
      {
        return "";
      }
      if ((Index >= 0) && (Index <= NodeList.Count - 1))
      {
        XmlNode node = NodeList.Item(Index);
        if (node != null)
        {
          XmlNode titleNode = node.SelectSingleNode("title");
          if (titleNode != null)
          {
            result = titleNode.InnerText;
          }
        }
      }
      return result;
    }

    static public int GetIndexOfID(int ContentID)
    {
      int result = - 1;
      XmlNode node = null;
      for (int i = 0; i < NodeCount; i++)
      {
        node = NodeList.Item(i);
        if (ExtractNodeID(node) == ContentID)
        {
          result = i;
          break;
        }
      }
      return result;
    }

    private ProgramContentManager()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    static ProgramContentManager()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _config = services.Get<IConfig>();

      if (System.IO.File.Exists(_config.Get(Config.Options.ConfigPath) + "FileDetailContents.xml"))
      {
        try
        {
          XmlDocument document = new XmlDocument();
          document.Load(_config.Get(Config.Options.ConfigPath) + "FileDetailContents.xml");
          rootElement = document.DocumentElement;
          if ((rootElement != null) && (rootElement.Name.Equals("contentprofiles")))
          {
            NodeList = rootElement.SelectNodes("/contentprofiles/profile");
          }
        }
        catch (Exception ex)
        {
          _log.Info("exception in ProgramContentManager err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
      else
      {
        _log.Info("Warning: myPrograms did not find the expected 'FileDetailContents.xml' in your MP root directory!");
      }
    }

    static public string GetFieldValue(AppItem curApp, FileItem curFile, string strFieldName, string strValueIfEmpty)
    {
      string result = "";
      if (rootElement == null)
      {
        return "";
      }
      XmlNode node = rootElement.SelectSingleNode(String.Format("/contentprofiles/profile[@id={0}]", curApp.ContentID));
      if (node != null)
      {
        XmlNode fieldnode = node.SelectSingleNode(String.Format("fields/field[@fieldid=\"{0}\"]", strFieldName));
        if (fieldnode != null)
        {
          result = ParseExpressions(fieldnode.InnerText, curApp, curFile);
        }
        else
        {
          _log.Info("ProgramContentManager Warning, no data found for \n{0}\n{1}\n{2}", curApp.Title, curFile.Title, node.InnerXml);
        }
      }
      else
      {
        _log.Info("ProgramContentManager Warning, no data found for \n{0}\n{1}", curApp.Title, curFile.Title);
      }
      if (result == "")
      {
        result = strValueIfEmpty;
      }
      return result;
    }

    static string ParseExpressions(string strExpression, AppItem curApp, FileItem curFile)
    {
      string result = strExpression;
      if (curApp == null)
        return result;
      if (curFile == null)
        return result;
      if (result.Length == 0)
        return result;

      int iNextValueTagStart = result.IndexOf("[");
      int iNextValueTagEnd = - 1;
      string Head = "";
      string Expression = "";
      string Tail = "";
      while (iNextValueTagStart >= 0)
      {
        iNextValueTagEnd = result.IndexOf("]", iNextValueTagStart);
        if (iNextValueTagEnd > iNextValueTagStart)
        {
          iNextValueTagEnd = iNextValueTagEnd + 1;
          if (iNextValueTagStart > 0)
          {
            Head = result.Substring(0, iNextValueTagStart);
          }
          else
          {
            Head = "";
          }
          Expression = result.Substring(iNextValueTagStart, iNextValueTagEnd - iNextValueTagStart);
          if (result.Length - iNextValueTagEnd > 0)
          {
            Tail = result.Substring(iNextValueTagEnd, result.Length - iNextValueTagEnd);
          }
          else
          {
            Tail = "";
          }
          result = Head + ParseOneExpression(Expression, curFile) + Tail;
        }
        iNextValueTagStart = result.IndexOf("[");
      }

      return result;
    }

    static string ParseOneExpression(string strTagExpression, FileItem curFile)
    {
      string result = "";
      if (strTagExpression.StartsWith("[VALUEOFTAG("))
      {
        result = ParseVALUEOFTAG(strTagExpression, curFile);
      }
      else if (strTagExpression.StartsWith("[NAMEOFCATEGORY("))
      {
        result = ParseNAMEOFCATEGORY(strTagExpression, curFile);
      }
      else if (strTagExpression.StartsWith("[VALUEOFCATEGORY("))
      {
        result = ParseVALUEOFCATEGORY(strTagExpression, curFile);
      }
      return result;
    }

    static string ParseVALUEOFTAG(string strTagExpression, FileItem curFile)
    {
      string result = "";
      string TagName = "";
      int Start = strTagExpression.IndexOf("\"");
      int End = strTagExpression.IndexOf("\"", Start + 1);
      if ((Start >= 0) && (End > Start))
      {
        TagName = strTagExpression.Substring(Start, End - Start + 1);
        TagName = TagName.TrimStart('"');
        TagName = TagName.TrimEnd('"');
        TagName = TagName.ToLower();

        switch (TagName)
        {
          case "system":
          {
            result = curFile.System_;
            break;
          }
          case "yearmanu":
          {
            result = curFile.YearManu;
            break;
          }
          case "rating":
          {
            if (curFile.Rating >= 0)
            {
              result = String.Format("{0}/10", curFile.Rating);
            }
            break;
          }
          case "genre":
          {
            string sep = "";
            if (curFile.Genre != "")
            {
              result = curFile.Genre;
              sep = " / ";
            }
            if (curFile.Genre2 != "")
            {
              result = result + sep + curFile.Genre2;
              sep = " / ";
            }
            if (curFile.Genre3 != "")
            {
              result = result + sep + curFile.Genre3;
              sep = " / ";
            }
            if (curFile.Genre4 != "")
            {
              result = result + sep + curFile.Genre4;
              sep = " / ";
            }
            if (curFile.Genre5 != "")
            {
              result = result + sep + curFile.Genre5;
              sep = " / ";
            }
            break;
          }
          case "overview":
          {
            result = curFile.Overview;
            break;
          }
          case "year":
          {
            if (curFile.Year >= 1900)
            {
              result = String.Format("{0}", curFile.Year);
            }
            break;
          }
          case "manufacturer":
          {
            result = curFile.Manufacturer;
            break;
          }
          default:
          {
            result = curFile.GetValueOfTag(TagName);
            break;
          }
        }
      }
      return result;
    }

    static string ParseNAMEOFCATEGORY(string strTagExpression, FileItem curFile)
    {
      string result = "";
      string TagName = "";
      int TagNumber = - 1;
      int Start = strTagExpression.IndexOf("(");
      int End = strTagExpression.IndexOf(")", Start + 1);
      if ((Start >= 0) && (End > Start))
      {
        TagName = strTagExpression.Substring(Start, End - Start + 1);
        TagName = TagName.TrimStart('(');
        TagName = TagName.TrimEnd(')');
        TagName = TagName.ToLower();
        TagNumber = ProgramUtils.StrToIntDef(TagName, - 1);
        if (TagNumber >= 0)
        {
          result = curFile.GetNameOfCategory(TagNumber);
        }
        else
        {
          _log.Info("Warning: ProgramContentManager: Invalid number {0}", TagName);
        }
      }
      return result;
    }

    static string ParseVALUEOFCATEGORY(string strTagExpression, FileItem curFile)
    {
      string result = "";
      string TagName = "";
      int TagNumber = - 1;
      int Start = strTagExpression.IndexOf("(");
      int End = strTagExpression.IndexOf(")", Start + 1);
      if ((Start >= 0) && (End > Start))
      {
        TagName = strTagExpression.Substring(Start, End - Start + 1);
        TagName = TagName.TrimStart('(');
        TagName = TagName.TrimEnd(')');
        TagName = TagName.ToLower();
        TagNumber = ProgramUtils.StrToIntDef(TagName, - 1);
        if (TagNumber >= 0)
        {
          result = curFile.GetValueOfCategory(TagNumber);
        }
        else
        {
          _log.Info("Warning: ProgramContentManager: Invalid number {0}", TagName);
        }

      }
      return result;
    }

  }
}