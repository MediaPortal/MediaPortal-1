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

using System.IO;

namespace Mediaportal.Util
{
  public class FileUtils
  {
    // function copied 1:1 from \trunk\mediaportal\Core\Util\Util.cs
    public static string MakeFileName(string strText)
    {
      if (strText == null) return string.Empty;
      if (strText.Length == 0) return string.Empty;

      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('\\', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_');
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');

      bool unclean = true;
      char[] invalids = Path.GetInvalidFileNameChars();
      while (unclean)
      {
        unclean = false;

        char[] filechars = strFName.ToCharArray();

        foreach (char c in filechars)
        {
          if (!unclean)
            foreach (char i in invalids)
            {
              if (c == i)
              {
                unclean = true;
                //Log.Warn("Utils: *** File name {1} still contains invalid chars - {0}", Convert.ToString(c), strFName);
                strFName = strFName.Replace(c, '_');
                break;
              }
            }
        }
      }
      return strFName;
    }
  }
}