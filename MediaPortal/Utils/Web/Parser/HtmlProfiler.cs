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
using System.Text.RegularExpressions;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Builds a profiles of HTML source for paring
  /// </summary>
  public class HtmlProfiler
  {
    #region Private Struct

    private struct Profile
    {
      public MatchTagCollection tags;
      public string tagMap;
    }

    #endregion

    #region Variables

    private Profile _template;
    private Profile _page;
    private HtmlSectionTemplate _sectionTemplate;
    private string _pageSource;
    private MatchCollection _matches;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlProfiler"/> class.
    /// </summary>
    /// <param name="template">The template.</param>
    public HtmlProfiler(HtmlSectionTemplate template)
    {
      _sectionTemplate = template;
      _template = BuildProfile(_sectionTemplate.Template);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the number of matches.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    public int MatchCount(string source)
    {
      _pageSource = source;
      _page = BuildProfile(_pageSource);

      try
      {
        Regex templateMap = new Regex(_template.tagMap);
        _matches = templateMap.Matches(_page.tagMap);
      }
      catch (ArgumentException) // ex)
      {
        return 0;
      }

      return _matches.Count;
    }

    /// <summary>
    /// Get the source by index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>source section</returns>
    public string GetSource(int index)
    {
      string source = string.Empty;

      if (_matches != null && index < _matches.Count)
      {
        Match sub = _matches[index];
        if (sub.Length != 0)
        {
          MatchTag start = (MatchTag) _page.tags[sub.Index];

          int tagIndex = sub.Index + sub.Length;
          int sourceLength;

          if (_page.tags.Count > tagIndex)
          {
            // get source data until the start of the next tag
            MatchTag end = (MatchTag) _page.tags[tagIndex];
            sourceLength = end.Index - start.Index;
          }
          else
          {
            // If last tag, get source data until the end of the current tag
            MatchTag end = (MatchTag) _page.tags[tagIndex - 1];
            sourceLength = end.Index + end.Length - start.Index;
          }

          source = _pageSource.Substring(start.Index, sourceLength);
        }
      }

      return source;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the profile.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>the profile</returns>
    private Profile BuildProfile(string source)
    {
      Profile build = new Profile();
      if (source == null || source.Length == 0)
      {
        return build;
      }

      MatchTagCollection tags = HtmlString.TagList(source);
      build.tags = new MatchTagCollection();
      build.tagMap = string.Empty;

      for (int i = 0; i < tags.Count; i++)
      {
        MatchTag tag = tags[i];
        char tagStart = char.ToUpper(tag.TagName[0]);
        // Test if special tag -- used for regex searchs
        // these tags are only copied into the tagMap no position index is stored
        if (tagStart == 'Z')
        {
          build.tagMap += tag.TagName.Substring(1);
        }

        if (_sectionTemplate.Tags.IndexOf(tagStart) != -1 &&
            tag.TagName != "br")
        {
          if (tagStart == 'T')
          {
            if (char.ToUpper(tag.TagName[1]) != 'A')
            {
              tagStart = char.ToUpper(tag.TagName[1]);
            }
          }

          if (tag.IsClose)
          {
            build.tagMap += tagStart;
          }
          else
          {
            build.tagMap += char.ToLower(tagStart);
          }

          build.tags.Add(tag);
        }
      }

      return build;
    }

    #endregion
  }
}