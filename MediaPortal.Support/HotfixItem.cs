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

// Structure that holds Windows hotfix information.
// Copyright (C) 2005-2006  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 25-9-2005
 * Time: 16:21
 * 
 */
namespace MediaPortal.Support
{
  public class HotfixItem
  {
    private string name = string.Empty;
    private string displayName = string.Empty;
    private string category = "unknown";
    private string url = string.Empty;
    private string releaseType = string.Empty;
    private string installDate = string.Empty;
    private string uninstallString = string.Empty;

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public string DisplayName
    {
      get
      {
        if (!displayName.Equals(string.Empty))
        {
          return displayName;
        }
        else
        {
          return name;
        }
      }
      set { displayName = value; }
    }

    public string Category
    {
      get { return category; }
      set { category = value; }
    }

    public string URL
    {
      get { return url; }
      set { url = value; }
    }

    public string ReleaseType
    {
      get { return releaseType; }
      set { releaseType = value; }
    }

    public string InstallDate
    {
      get { return installDate; }
      set { installDate = value; }
    }

    public string UninstallString
    {
      get { return uninstallString; }
      set { uninstallString = value.Replace("\"", ""); }
    }

    public HotfixItem()
    {
    }

    public HotfixItem(
      string Name,
      string DisplayName,
      string Category,
      string URL,
      string ReleaseType,
      string InstallDate,
      string UninstallString
      )
    {
      name = Name;
      displayName = DisplayName;
      category = Category;
      url = URL;
      releaseType = ReleaseType;
      installDate = InstallDate;
      uninstallString = UninstallString.Replace("\"", "");
    }
  }
}