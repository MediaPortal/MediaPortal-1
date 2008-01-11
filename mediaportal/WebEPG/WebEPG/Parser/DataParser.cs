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
using System.Text;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  ///  Parser class for delimited EPG data
  /// </summary>
  public class DataParser : IParser
  {
    #region Variables
    DataRows _rows;
    DataRowParser _parser;
    Type _dataType;
    #endregion

    #region Constructors/Destructors
    public DataParser(DataParserTemplate template)
    {
      _rows = new DataRows(template.rowDelimiter);
      _parser = new DataRowParser(template.Template, template.dataDelimiter);
      _dataType = typeof(ProgramData);
    }
    #endregion

    #region Private Methods
    private void ProfilerStart()
    {
      //if (_strSource.Length == 0)
      //  return;

      //int index = 0;
      //int profileIndex = 0;
      //int tagCount = 0;
      //int subProfileStart = 0;
      //int subProfileIndex = 0;

      //int[,] arrayProfilePos = new int[_strSource.Length, 2];
      //int[,] subProfilePos = new int[_strSource.Length, 2];

      //for (index = 0; index < _strSource.Length; index++)
      //{
      //  if (_strSource[index] == _cTag)
      //  {
      //    arrayProfilePos[profileIndex, 0] = index;
      //    arrayProfilePos[profileIndex, 1] = index;
      //    profileIndex++;
      //    tagCount++;
      //  }

      //  if (_strSource[index] == _cDelim)
      //  {
      //    arrayProfilePos[profileIndex, 0] = index;
      //    arrayProfilePos[profileIndex, 1] = index;
      //    profileIndex++;
      //    tagCount++;
      //    subProfilePos[subProfileIndex, 0] = subProfileStart;
      //    subProfilePos[subProfileIndex, 1] = tagCount;
      //    subProfileStart = profileIndex;
      //    tagCount = 0;
      //    subProfileIndex++;
      //  }
      //}

      //_arrayTagPos = new int[profileIndex, 2];
      //for (index = 0; index < profileIndex; index++)
      //{
      //  _arrayTagPos[index, 0] = arrayProfilePos[index, 0];
      //  _arrayTagPos[index, 1] = arrayProfilePos[index, 1];
      //}

      //_profileCount = subProfileIndex;
      //_subProfile = new int[subProfileIndex, 2];
      //for (index = 0; index < subProfileIndex; index++)
      //{
      //  _subProfile[index, 0] = subProfilePos[index, 0];
      //  _subProfile[index, 1] = subProfilePos[index, 1];
      //}
    }
    #endregion

    #region IParser Implementations
    public int ParseUrl(HTTPRequest site)
    {
      HTMLPage webPage = new HTMLPage(site);
      return _rows.RowCount(webPage.GetPage());
    }

    public IParserData GetData(int index)
    {
      string rowSource = _rows.GetSource(index);

      IParserData rowData = (IParserData)Activator.CreateInstance(_dataType);

      _parser.ParseRow(rowSource, ref rowData);

      return rowData;
    }
    #endregion
  }
}
