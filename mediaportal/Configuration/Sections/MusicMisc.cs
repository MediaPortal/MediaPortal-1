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

using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class MusicMisc : SectionSettings
  {
    private string[] JumpToOptions = new string[] {"Now Playing", "Playlist", "Don't jump (stay at current window)"};

    public MusicMisc()
      : this("Music Miscellaneous") {}

    public MusicMisc(string name) : base(name)
    {
      InitializeComponent();

      PlayNowJumpToCmbBox.Items.Clear();
      PlayNowJumpToCmbBox.Items.AddRange(JumpToOptions);
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        string playNowJumpTo = xmlreader.GetValueAsString("musicmisc", "playnowjumpto", "nowplaying");

        switch (playNowJumpTo)
        {
          case "nowplaying":
            PlayNowJumpToCmbBox.Text = JumpToOptions[0];
            break;

          case "playlist":
            PlayNowJumpToCmbBox.Text = JumpToOptions[1];
            break;

          case "none":
            PlayNowJumpToCmbBox.Text = JumpToOptions[2];
            break;
        }
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        string playNowJumpTo = "nowplaying";

        if (PlayNowJumpToCmbBox.Text == JumpToOptions[0]) // Now playing
        {
          playNowJumpTo = "nowplaying";
        }

        else if (PlayNowJumpToCmbBox.Text == JumpToOptions[1])
        {
          playNowJumpTo = "playlist";
        }

        else
        {
          playNowJumpTo = "none";
        }

        xmlwriter.SetValue("musicmisc", "playnowjumpto", playNowJumpTo);
      }
    }
  }
}