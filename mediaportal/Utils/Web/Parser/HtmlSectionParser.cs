#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text.RegularExpressions;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Parses a section of HTML source for elements from a given template
  /// </summary>
  public class HtmlSectionParser
  {
    #region Private Structs

    private struct DataField
    {
      public MatchTag htmlTag;
      public bool hasData;
      public bool optional;
      public string source;
      public ArrayList dataElements;

      public override string ToString()
      {
        if (htmlTag == null)
        {
          return source;
        }
        else
        {
          return htmlTag.ToString();
        }
      }
    }

    private struct Sections
    {
      public ArrayList dataFields;
      public int minFields;
      public int dataTags;
      public bool optionalData;
    }

    private struct ElementData
    {
      public string name;
      public string start;
      public string end;
    }

    #endregion

    #region Variables

    private Sections _templateData;
    private HtmlSectionTemplate _template;
    private string _matchField;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlSectionParser"/> class.
    /// </summary>
    /// <param name="template">The template.</param>
    public HtmlSectionParser(HtmlSectionTemplate template)
    {
      _template = template;
      _template.Tags = template.Tags + "Z";
      _templateData = GetSections(template.Template);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Parses the section.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="data">The data.</param>
    /// <returns>bool - success/fail</returns>
    public bool ParseSection(string source, ref IParserData data)
    {
      Sections sourceData = GetSections(source);
      bool hasOptional = false;
      bool hasMatched = false;
      int dataFound = 0;

      if (sourceData.dataFields.Count == 0)
      {
        return false;
      }

      if (sourceData.dataFields.Count > _templateData.minFields)
      {
        hasOptional = true;
      }

      int s = 0;
      for (int t = 0; t < _templateData.dataFields.Count; t++)
      {
        DataField templateField = (DataField) _templateData.dataFields[t];

        if (templateField.optional)
        {
          if (!hasOptional)
          {
            continue;
          }
        }

        if (s < sourceData.dataFields.Count)
        {
          DataField sourceField = (DataField) sourceData.dataFields[s];

          if (!templateField.hasData &&
              templateField.htmlTag != null &&
              sourceField.htmlTag != null)
          {
            if (templateField.htmlTag.SameType(sourceField.htmlTag))
            {
              s++;
            }
          }
          else
          {
            if (templateField.source != string.Empty &&
                templateField.hasData)
            {
              int index = 0;
              for (int i = 0; i < templateField.dataElements.Count; i++)
              {
                ElementData element = (ElementData) templateField.dataElements[i];

                int startPos;
                if (index < sourceField.source.Length)
                {
                  if (element.start == string.Empty ||
                      (startPos = sourceField.source.IndexOf(element.start, index, StringComparison.OrdinalIgnoreCase)) ==
                      -1)
                  {
                    startPos = index;
                  }
                  else
                  {
                    startPos = startPos + element.start.Length;
                  }

                  int endPos;
                  if (element.end == string.Empty ||
                      (endPos = sourceField.source.IndexOf(element.end, startPos, StringComparison.OrdinalIgnoreCase)) ==
                      -1)
                  {
                    endPos = sourceField.source.Length;
                  }

                  string elementSource = sourceField.source.Substring(startPos, endPos - startPos);

                  if (elementSource != string.Empty)
                  {
                    if (element.name[0] == '*')
                    {
                      if (hasMatched && element.name == "*VALUE")
                      {
                        data.SetElement(_matchField, elementSource);
                        hasMatched = false;
                      }
                      else
                      {
                        if (IsMatch(element.name, elementSource))
                        {
                          hasMatched = true;
                        }
                      }
                    }
                    else
                    {
                      data.SetElement(element.name, elementSource);
                    }
                    dataFound++;
                    if (dataFound == _templateData.dataTags)
                    {
                      break;
                    }
                  }
                  index = endPos + element.end.Length;
                }
              }
            }
            s++;
          }
        }
      }

      if (dataFound == 0 && _templateData.dataTags > 0)
      {
        return false;
      }

      return true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Strips the unwanted HTML tags.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>stripped source</returns>
    private string StripTags(string source)
    {
      string stripped = string.Empty;
      source = HtmlString.NewLines(source);

      MatchTagCollection tags = HtmlString.TagList(source);

      for (int i = 0; i < tags.Count; i++)
      {
        MatchTag tag = tags[i];

        if (_template.Tags.IndexOf(char.ToUpper(tag.TagName[0])) != -1)
        {
          stripped += source.Substring(tag.Index, tag.Length);
        }

        int start = tag.Index + tag.Length;
        if (start < source.Length)
        {
          int end;
          if (i + 1 < tags.Count)
          {
            tag = tags[i + 1];
            end = tag.Index;
          }
          else
          {
            end = source.Length;
          }
          stripped += source.Substring(start, end - start);
        }
      }

      return stripped;
    }

    /// <summary>
    /// Gets the sections.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    private Sections GetSections(string source)
    {
      source = StripTags(source);
      Sections data = new Sections();
      data.dataFields = new ArrayList();
      data.minFields = 0;
      data.optionalData = false;
      data.dataTags = 0;

      MatchTagCollection tags = HtmlString.TagList(source);

      bool isOptionalTag = false;
      bool zTag = false;


      for (int i = 0; i < tags.Count; i++)
      {
        MatchTag tag = tags[i];
        // check if tag + data is optional / regex
        zTag = false;
        if (char.ToUpper(tag.TagName[0]) == 'Z')
        {
          zTag = true;
          isOptionalTag = true;
          if (tag.IsClose)
          {
            isOptionalTag = false;
          }

          //i++;
          //if (i < tags.Count)
          //  tag = tags[i];
          //else
          //  break;
        }

        // Check if tag is one of interest
        if (_template.Tags.IndexOf(char.ToUpper(tag.TagName[0])) != -1)
        {
          DataField section;
          if (!zTag)
          {
            // Add tag to array of fields
            section = new DataField();
            section.optional = isOptionalTag;
            section.htmlTag = tag;
            section.hasData = false;
            section.source = tag.FullTag;
            if (section.source.IndexOf("<#") != -1 || section.source.IndexOf("<*") != -1)
            {
              section.hasData = true;
              if (isOptionalTag)
              {
                data.optionalData = true;
              }

              section.dataElements = GetElements(section.source);
              data.dataTags += section.dataElements.Count;
            }

            data.dataFields.Add(section);
            if (!isOptionalTag)
            {
              data.minFields++;
            }
          }

          // Add data between this tag and the next to field array
          int start = tag.Index + tag.Length;
          int end;
          if (i + 1 < tags.Count)
          {
            tag = tags[i + 1];
            zTag = false;
            if (char.ToUpper(tag.TagName[0]) == 'Z')
            {
              zTag = true;
              isOptionalTag = true;
              if (tag.IsClose)
              {
                isOptionalTag = false;
              }
            }

            //  start = tag.Index + tag.Length;
            //  i++;
            //  if (i + 1 < tags.Count)
            //    tag = tags[i + 1];
            //  else
            //    break;
            //}
            end = tag.Index;
          }
          else
          {
            end = source.Length;
          }

          if (!zTag)
          {
            section = new DataField();
            section.optional = isOptionalTag;
            section.htmlTag = null;
            section.source = HtmlString.Decode(source.Substring(start, end - start));
            section.hasData = false;
            if (section.source.IndexOf("<#") != -1 || section.source.IndexOf("<*") != -1)
            {
              section.hasData = true;
              if (isOptionalTag)
              {
                data.optionalData = true;
              }

              section.dataElements = GetElements(section.source);
              data.dataTags += section.dataElements.Count;
            }
            data.dataFields.Add(section);
            if (!isOptionalTag)
            {
              data.minFields++;
            }
          }
        }
      }

      return data;
    }

    /// <summary>
    /// Gets the elements.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>array of elements</returns>
    private ArrayList GetElements(string source)
    {
      ArrayList elements = new ArrayList();

      //source = HtmlString.ToAscii(source);

      Regex elementTag = new Regex(@"<[#*][A-Z][^>]+>");
      MatchCollection elementTags = elementTag.Matches(source);
      for (int i = 0; i < elementTags.Count; i++)
      {
        Match tag = elementTags[i];

        ElementData element = new ElementData();
        element.name = source.Substring(tag.Index, tag.Length);
        element.start = string.Empty;
        element.end = string.Empty;

        int pos;
        if ((pos = element.name.IndexOf(":")) != -1)
        {
          int sepPos;
          if ((sepPos = element.name.IndexOf(",")) != -1)
          {
            element.start = element.name.Substring(pos + 1, sepPos - pos - 1);
            element.end = element.name.Substring(sepPos + 1, element.name.Length - sepPos - 2);
          }
          element.name = element.name.Substring(1, pos - 1);
        }
        else
        {
          if (i == 0 && tag.Index > 0)
          {
            element.start = source.Substring(0, tag.Index);
          }

          if (i + 1 == elementTags.Count && tag.Index + tag.Length != source.Length)
          {
            element.end = source.Substring(tag.Index + tag.Length, source.Length - tag.Index - tag.Length);
          }

          if (i + 1 < elementTags.Count)
          {
            Match nextTag = elementTags[i + 1];
            element.end = source.Substring(tag.Index + tag.Length, nextTag.Index - tag.Index - tag.Length);
          }

          element.name = element.name.Substring(1, element.name.Length - 2);
        }
        elements.Add(element);
      }

      return elements;
    }

    /// <summary>
    /// Determines whether the specified tag is the *MATCH tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="match">The match.</param>
    /// <returns>
    /// 	<c>true</c> if the specified tag is *MATCH; otherwise, <c>false</c>.
    /// </returns>
    private bool IsMatch(string tag, string match)
    {
      if (tag == "*MATCH")
      {
        for (int i = 0; i < _template.MatchValues.Count; i++)
        {
          if (_template.MatchValues[i].match == match)
          {
            _matchField = _template.MatchValues[i].field;
            return true;
          }
        }
      }

      return false;
    }

    #endregion
  }
}