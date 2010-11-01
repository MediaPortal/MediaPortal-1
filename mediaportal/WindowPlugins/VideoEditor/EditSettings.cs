#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace WindowPlugins.VideoEditor
{
  internal enum EditType
  {
    Join,
    Cut,
    Convert,
    Compress,
  }

  internal class EditSettings
  {
    private string fileName;
    private object settings;
    private bool deleteAfter;
    private EditType type;

    public EditSettings(object setting)
    {
      this.settings = setting;
    }

    public object Settings
    {
      get { return settings; }
    }

    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }

    public bool DeleteAfter
    {
      get { return deleteAfter; }
      set { deleteAfter = value; }
    }

    public EditType Type
    {
      get { return type; }
      set { type = value; }
    }
  }
}