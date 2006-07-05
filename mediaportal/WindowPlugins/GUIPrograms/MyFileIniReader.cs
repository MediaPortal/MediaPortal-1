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

using System.IO;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for MyFileIniReader.
  /// </summary>
  public class MyFileIniReader
  {
    private string m_FileName = "";
    private ILog _log;

    // event: read new section line
    public delegate void IniEventHandler(string strLine);
    public event IniEventHandler OnReadNewSection = null;
    // event: read new entry line
    public event IniEventHandler OnReadNewEntry = null;
    // event: read a no-section, no-entry line
    public event IniEventHandler OnReadAdditionalLine = null;
    // event: read the end of a section
    public event IniEventHandler OnReadEndSection = null;

    public MyFileIniReader()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    private bool IsSectionLine(string strVal)
    {
      return (strVal.StartsWith("[")) && (strVal.EndsWith("]"));
    }

    private bool IsEntryValueLine(string strLine)
    {
      int nFirstSpacePos = strLine.IndexOf(" ");
      int nFirstEqualPos = strLine.IndexOf("=");
      bool bRes = false;
      if (nFirstEqualPos ==  - 1)
      {
        bRes = false;
      }
      else if (nFirstSpacePos ==  - 1)
      {
        bRes = true;
      }
      else
      {
        bRes = (nFirstEqualPos < nFirstSpacePos);
      }
      return bRes;
    }

    public void Start()
    {
      string strNextLine;
      bool HasReadOneSection = false;
      if (m_FileName == "")
        return ;
      if (!File.Exists(m_FileName))
      {
        _log.Info("MyFileIniReader: INI-File not found ({0})", m_FileName);
        return ;
      }
      StreamReader reader = new StreamReader(m_FileName, Encoding.GetEncoding(1252));
      try
      {
        do
        {
          strNextLine = reader.ReadLine().Trim();

          // check if line is a section
          if (IsSectionLine(strNextLine))
          {
            if (HasReadOneSection)
            {
              this.OnReadEndSection("");
            }
            this.OnReadNewSection(strNextLine);
            HasReadOneSection = true;
          }
          else if (IsEntryValueLine(strNextLine))
          {
            this.OnReadNewEntry(strNextLine);
          }
          else
          {
            // otherwise send the line as is and decide elsewhere if this is useful!
            this.OnReadAdditionalLine(strNextLine);
          }
        }
        while (reader.Peek() !=  - 1);
        if (HasReadOneSection)
        {
          this.OnReadEndSection("");
        }
      }
      finally
      {
        reader.Close();
      }

    }



    public MyFileIniReader(string strFileName)
    {
      m_FileName = strFileName;
    }
  }
}
