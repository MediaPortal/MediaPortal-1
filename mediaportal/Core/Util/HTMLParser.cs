#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Collections;
using System.Web;

namespace MediaPortal.Util
{
  /// Developed by Ravi Bhavnani and published in CodeProject.com
  /// <summary>
  /// A class that helps you to extract HTML information from a string.
  /// </summary>
  public class HTMLParser
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public HTMLParser() {}

    /// <summary>
    /// Constructs a HTMLParser with specific content.
    /// </summary>
    /// <param name="strContent">The parser's content.</param>
    public HTMLParser
      (string strContent)
    {
      Content = strContent;
    }

    /////////////
    // Properties

    /// <summary>Gets and sets the content to be parsed.</summary>
    public string Content
    {
      get { return m_strContent; }
      set
      {
        m_strContent = value;
        m_strContentLC = m_strContent.ToLower();
        resetPosition();
      }
    }

    /// <summary>Gets the parser's current position.</summary>
    public int Position
    {
      get { return m_nIndex; }
    }

    /////////////////
    // Static methods

    /// <summary>
    /// Retrieves the collection of HTML links in a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="strRootUrl">Root url (may be null).</param>
    /// <param name="documents">Collection of document link strings.</param>
    /// <param name="images">Collection of image link strings.</param>
    public static void getLinks
      (string strString,
       string strRootUrl,
       ref ArrayList documents,
       ref ArrayList images)
    {
      // Remove comments and JavaScript and fix links
      strString = HTMLParser.removeComments(strString);
      strString = HTMLParser.removeScripts(strString);
      HTMLParser parser = new HTMLParser(strString);
      parser.replaceEvery("\'", "\"");

      // Set root url
      string rootUrl = "";
      if (strRootUrl != null)
        rootUrl = strRootUrl.Trim();
      if ((rootUrl.Length > 0) && !rootUrl.EndsWith("/"))
        rootUrl += "/";

      // Extract HREF targets
      string strUrl = "";
      parser.resetPosition();
      while (parser.skipToEndOfNoCase("href=\""))
      {
        if (parser.extractTo("\"", ref strUrl))
        {
          strUrl = strUrl.Trim();
          if (strUrl.Length > 0)
          {
            if (strUrl.IndexOf("mailto:") == -1)
            {
              // Get fully qualified url (best guess)
              if (!strUrl.StartsWith("http://") && !strUrl.StartsWith("ftp://"))
              {
                try
                {
                  UriBuilder uriBuilder = new UriBuilder(rootUrl);
                  uriBuilder.Path = strUrl;
                  strUrl = uriBuilder.Uri.ToString();
                }
                catch (Exception)
                {
                  strUrl = "http://" + strUrl;
                }
              }

              // Add url to document list if not already present
              if (!documents.Contains(strUrl))
                documents.Add(strUrl);
            }
          }
        }
      }

      // Extract SRC targets
      parser.resetPosition();
      while (parser.skipToEndOfNoCase("src=\""))
      {
        if (parser.extractTo("\"", ref strUrl))
        {
          strUrl = strUrl.Trim();
          if (strUrl.Length > 0)
          {
            // Get fully qualified url (best guess)
            if (!strUrl.StartsWith("http://") && !strUrl.StartsWith("ftp://"))
            {
              try
              {
                UriBuilder uriBuilder = new UriBuilder(rootUrl);
                uriBuilder.Path = strUrl;
                strUrl = uriBuilder.Uri.ToString();
              }
              catch (Exception)
              {
                strUrl = "http://" + strUrl;
              }
            }

            // Add url to images list if not already present
            if (!images.Contains(strUrl))
              images.Add(strUrl);
          }
        }
      }
    }

    /// <summary>
    /// Removes all HTML comments from a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>Comment-free version of string.</returns>
    public static string removeComments
      (string strString)
    {
      // Return comment-free version of string
      string strCommentFreeString = "";
      string strSegment = "";
      HTMLParser parser = new HTMLParser(strString);

      while (parser.extractTo("<!--", ref strSegment))
      {
        strCommentFreeString += strSegment;
        if (!parser.skipToEndOf("-->"))
          return strString;
      }

      parser.extractToEnd(ref strSegment);
      strCommentFreeString += strSegment;
      return strCommentFreeString;
    }

    /// <summary>
    /// Returns an unanchored version of a string, i.e. without the enclosing
    /// leftmost &lt;a...&gt; and rightmost &lt;/a&gt; tags.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>Unanchored version of string.</returns>
    public static string removeEnclosingAnchorTag
      (string strString)
    {
      string strStringLC = strString.ToLower();
      int nStart = strStringLC.IndexOf("<a");
      if (nStart != -1)
      {
        nStart++;
        nStart = strStringLC.IndexOf(">", nStart);
        if (nStart != -1)
        {
          nStart++;
          int nEnd = strStringLC.LastIndexOf("</a>");
          if (nEnd != -1)
          {
            string strRet = strString.Substring(nStart, nEnd - nStart);
            return strRet;
          }
        }
      }
      return strString;
    }

    /// <summary>
    /// Returns an unquoted version of a string, i.e. without the enclosing
    /// leftmost and rightmost double " characters.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>Unquoted version of string.</returns>
    public static string removeEnclosingQuotes
      (string strString)
    {
      int nStart = strString.IndexOf("\"");
      if (nStart != -1)
      {
        int nEnd = strString.LastIndexOf("\"");
        if (nEnd > nStart)
          return strString.Substring(nStart, nEnd - nStart - 1);
      }
      return strString;
    }

    /// <summary>
    /// Returns a version of a string without any HTML tags.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>Version of string without HTML tags.</returns>
    public static string removeHtml
      (string strString)
    {
      // Do some common case-sensitive replacements
      Hashtable replacements = new Hashtable();
      replacements.Add("&nbsp;", " ");
      replacements.Add("&amp;", "&");
      replacements.Add("&aring;", "");
      replacements.Add("&auml;", "");
      replacements.Add("&eacute;", "");
      replacements.Add("&iacute;", "");
      replacements.Add("&igrave;", "");
      replacements.Add("&ograve;", "");
      replacements.Add("&ouml;", "");
      replacements.Add("&quot;", "\"");
      replacements.Add("&szlig;", "");
      HTMLParser parser = new HTMLParser(strString);
      foreach (string key in replacements.Keys)
      {
        string val = replacements[key] as string;
        if (strString.IndexOf(key) != -1)
          parser.replaceEveryExact(key, val);
      }

      // Do some sequential replacements
      parser.replaceEveryExact("&#0", "&#");
      parser.replaceEveryExact("&#39;", "'");
      parser.replaceEveryExact("</", " <~/");
      parser.replaceEveryExact("<~/", "</");

      // Case-insensitive replacements
      replacements.Clear();
      replacements.Add("<br>", " ");
      replacements.Add("<br />", " ");
      replacements.Add("<br/>", " ");
      replacements.Add("<p>", " ");
      replacements.Add("<p />", " ");
      replacements.Add("<p/>", " ");
      foreach (string key in replacements.Keys)
      {
        string val = replacements[key] as string;
        if (strString.IndexOf(key) != -1)
          parser.replaceEvery(key, val);
      }
      strString = parser.Content;

      // Remove all tags
      string strClean = "";
      int nIndex = 0;
      int nStartTag = 0;
      while ((nStartTag = strString.IndexOf("<", nIndex)) != -1)
      {
        // Extract to start of tag
        string strSubstring = strString.Substring(nIndex, (nStartTag - nIndex));
        strClean += strSubstring;
        nIndex = nStartTag + 1;

        // Skip over tag
        int nEndTag = strString.IndexOf(">", nIndex);
        if (nEndTag == (-1))
          break;
        nIndex = nEndTag + 1;
      }

      // Gather remaining text
      if (nIndex < strString.Length)
        strClean += strString.Substring(nIndex, strString.Length - nIndex);
      strString = strClean;
      strClean = "";

      // Finally, reduce spaces
      parser.Content = strString;
      parser.replaceEveryExact("  ", " ");
      strString = parser.Content.Trim();

      // Return the de-HTMLized string
      return strString;
    }

    /// <summary>
    /// Removes all scripts from a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>Version of string without any scripts.</returns>
    public static string removeScripts
      (string strString)
    {
      // Get script-free version of content
      string strStringSansScripts = "";
      string strSegment = "";
      HTMLParser parser = new HTMLParser(strString);

      while (parser.extractToNoCase("<script", ref strSegment))
      {
        strStringSansScripts += strSegment;
        if (!parser.skipToEndOfNoCase("</script>"))
        {
          parser.Content = strStringSansScripts;
          return strString;
        }
      }

      parser.extractToEnd(ref strSegment);
      strStringSansScripts += strSegment;
      return (strStringSansScripts);
    }

    /////////////
    // Operations

    /// <summary>
    /// Checks if the parser is case-sensitively positioned at the start
    /// of a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>
    /// true if the parser is positioned at the start of the string, false
    /// otherwise.
    /// </returns>
    public bool at
      (string strString)
    {
      if (m_strContent.IndexOf(strString, Position) == Position)
        return (true);
      return (false);
    }

    /// <summary>
    /// Checks if the parser is case-insensitively positioned at the start
    /// of a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>
    /// true if the parser is positioned at the start of the string, false
    /// otherwise.
    /// </returns>
    public bool atNoCase
      (string strString)
    {
      strString = strString.ToLower();
      if (m_strContentLC.IndexOf(strString, Position) == Position)
        return (true);
      return (false);
    }

    /// <summary>
    /// Extracts the text from the parser's current position to the case-
    /// sensitive start of a string and advances the parser just after the
    /// string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="strExtract">The extracted text.</param>
    /// <returns>true if the parser was advanced, false otherwise.</returns>
    public bool extractTo
      (string strString,
       ref string strExtract)
    {
      int nPos = m_strContent.IndexOf(strString, Position);
      if (nPos != -1)
      {
        strExtract = m_strContent.Substring(m_nIndex, nPos - m_nIndex);
        m_nIndex = nPos + strString.Length;
        return (true);
      }
      return (false);
    }

    /// <summary>
    /// Extracts the text from the parser's current position to the case-
    /// insensitive start of a string and advances the parser just after the
    /// string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="strExtract">The extracted text.</param>
    /// <returns>true if the parser was advanced, false otherwise.</returns>
    public bool extractToNoCase
      (string strString,
       ref string strExtract)
    {
      strString = strString.ToLower();
      int nPos = m_strContentLC.IndexOf(strString, Position);
      if (nPos != -1)
      {
        strExtract = m_strContent.Substring(m_nIndex, nPos - m_nIndex);
        m_nIndex = nPos + strString.Length;
        return (true);
      }
      return (false);
    }

    /// <summary>
    /// Extracts the text from the parser's current position to the case-
    /// sensitive start of a string and position's the parser at the start
    /// of the string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="strExtract">The extracted text.</param>
    /// <returns>true if the parser was advanced, false otherwise.</returns>
    public bool extractUntil
      (string strString,
       ref string strExtract)
    {
      int nPos = m_strContent.IndexOf(strString, Position);
      if (nPos != -1)
      {
        strExtract = m_strContent.Substring(m_nIndex, nPos - m_nIndex);
        m_nIndex = nPos;
        return (true);
      }
      return (false);
    }

    /// <summary>
    /// Extracts the text from the parser's current position to the case-
    /// insensitive start of a string and position's the parser at the start
    /// of the string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="strExtract">The extracted text.</param>
    /// <returns>true if the parser was advanced, false otherwise.</returns>
    public bool extractUntilNoCase
      (string strString,
       ref string strExtract)
    {
      strString = strString.ToLower();
      int nPos = m_strContentLC.IndexOf(strString, Position);
      if (nPos != -1)
      {
        strExtract = m_strContent.Substring(m_nIndex, nPos - m_nIndex);
        m_nIndex = nPos;
        return (true);
      }
      return (false);
    }

    /// <summary>
    /// Extracts the text from the parser's current position to the end
    /// of its content and does not advance the parser's position.
    /// </summary>
    /// <param name="strExtract">The extracted text.</param>
    public void extractToEnd
      (ref string strExtract)
    {
      strExtract = "";
      if (Position < m_strContent.Length)
      {
        int nRemainLen = m_strContent.Length - Position;
        strExtract = m_strContent.Substring(Position, nRemainLen);
      }
    }

    /// <summary>
    /// Case-insensitively replaces every occurence of a string in the
    /// parser's content with another.
    /// </summary>
    /// <param name="strOccurrence">The occurrence.</param>
    /// <param name="strReplacement">The replacement string.</param>
    /// <returns>The number of occurences replaced.</returns>
    public int replaceEvery
      (string strOccurrence,
       string strReplacement)
    {
      // Initialize replacement process
      int nReplacements = 0;
      strOccurrence = strOccurrence.ToLower();

      // For every occurence...
      int nOccurrence = m_strContentLC.IndexOf(strOccurrence);
      while (nOccurrence != -1)
      {
        // Create replaced substring
        string strReplacedString = m_strContent.Substring(0, nOccurrence) + strReplacement;

        // Add remaining substring (if any)
        int nStartOfRemainingSubstring = nOccurrence + strOccurrence.Length;
        if (nStartOfRemainingSubstring < m_strContent.Length)
        {
          string strSecondPart = m_strContent.Substring(nStartOfRemainingSubstring,
                                                        m_strContent.Length - nStartOfRemainingSubstring);
          strReplacedString += strSecondPart;
        }

        // Update the original string
        m_strContent = strReplacedString;
        m_strContentLC = m_strContent.ToLower();
        nReplacements++;

        // Find the next occurence
        nOccurrence = m_strContentLC.IndexOf(strOccurrence);
      }
      return (nReplacements);
    }

    /// <summary>
    /// Case sensitive version of replaceEvery()
    /// </summary>
    /// <param name="strOccurrence">The occurrence.</param>
    /// <param name="strReplacement">The replacement string.</param>
    /// <returns>The number of occurences replaced.</returns>
    public int replaceEveryExact
      (string strOccurrence,
       string strReplacement)
    {
      int nReplacements = 0;
      while (m_strContent.IndexOf(strOccurrence) != -1)
      {
        m_strContent = m_strContent.Replace(strOccurrence, strReplacement);
        nReplacements++;
      }
      m_strContentLC = m_strContent.ToLower();
      return (nReplacements);
    }

    /// <summary>
    /// Resets the parser's position to the start of the content.
    /// </summary>
    public void resetPosition()
    {
      m_nIndex = 0;
    }

    /// <summary>
    /// Advances the parser's position to the start of the next case-sensitive
    /// occurence of a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>true if the parser's position was advanced, false otherwise.</returns>
    public bool skipToStartOf
      (string strString)
    {
      bool bStatus = seekTo(strString, false, false);
      return (bStatus);
    }

    /// <summary>
    /// Advances the parser's position to the start of the next case-insensitive
    /// occurence of a string.
    /// </summary>
    /// <param name="strText">The string.</param>
    /// <returns>true if the parser's position was advanced, false otherwise.</returns>
    public bool skipToStartOfNoCase
      (string strText)
    {
      bool bStatus = seekTo(strText, true, false);
      return (bStatus);
    }

    /// <summary>
    /// Advances the parser's position to the end of the next case-sensitive
    /// occurence of a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <returns>true if the parser's position was advanced, false otherwise.</returns>
    public bool skipToEndOf
      (string strString)
    {
      bool bStatus = seekTo(strString, false, true);
      return (bStatus);
    }

    /// <summary>
    /// Advances the parser's position to the end of the next case-insensitive
    /// occurence of a string.
    /// </summary>
    /// <param name="strText">The string.</param>
    /// <returns>true if the parser's position was advanced, false otherwise.</returns>
    public bool skipToEndOfNoCase
      (string strText)
    {
      bool bStatus = seekTo(strText, true, true);
      return (bStatus);
    }

    ///////////////////////////
    // Implementation (members)

    /// <summary>Content to be parsed.</summary>
    private string m_strContent = "";

    /// <summary>Lower-cased version of content to be parsed.</summary>
    private string m_strContentLC = "";

    /// <summary>Current position in content.</summary>
    private int m_nIndex = 0;

    ///////////////////////////
    // Implementation (methods)

    /// <summary>
    /// Advances the parser's position to the next occurence of a string.
    /// </summary>
    /// <param name="strString">The string.</param>
    /// <param name="bNoCase">Flag: perform a case-insensitive search.</param>
    /// <param name="bPositionAfter">Flag: position parser just after string.</param>
    /// <returns></returns>
    private bool seekTo
      (string strString,
       bool bNoCase,
       bool bPositionAfter)
    {
      if (Position < m_strContent.Length)
      {
        // Find the start of the string - return if not found
        int nNewIndex = 0;
        if (bNoCase)
        {
          strString = strString.ToLower();
          nNewIndex = m_strContentLC.IndexOf(strString, Position);
        }
        else
        {
          nNewIndex = m_strContent.IndexOf(strString, Position);
        }
        if (nNewIndex == -1)
          return (false);

        // Position the parser
        m_nIndex = nNewIndex;
        if (bPositionAfter)
          m_nIndex += strString.Length;
        return (true);
      }
      return (false);
    }
  }
}