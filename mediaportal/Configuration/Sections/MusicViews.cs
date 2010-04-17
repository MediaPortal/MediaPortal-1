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


#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicViews : BaseViews
  {
    private string[] selections = new string[]
                                    {
                                      "album",
                                      "artist",
                                      "albumartist",
                                      "title",
                                      "genre",
                                      "year",
                                      "track",
                                      "timesplayed",
                                      "rating",
                                      "favorites",
                                      "recently added",
                                      "composer",
                                      "conductor"
                                    };

    private string[] sqloperators = new string[]
                                      {
                                        "",
                                        "=",
                                        ">",
                                        "<",
                                        ">=",
                                        "<=",
                                        "<>",
                                        "like",
                                        "not like",
                                        "group",
                                      };

    private string[] viewsAs = new string[]
                                 {
                                   "List",
                                   "Icons",
                                   "Big Icons",
                                   "Filmstrip",
                                   "Albums",
                                 };

    private string[] sortBy = new string[]
                                {
                                  "Name",
                                  "Date",
                                  "Year",
                                  "Size",
                                  "Track",
                                  "Duration",
                                  "Title",
                                  "Artist",
                                  "Album",
                                  "Filename",
                                  "Rating"
                                };

    public MusicViews()
      : this("Music Views") {}

    public MusicViews(string name)
      : base(name) {}

    public override void LoadSettings()
    {
      base.LoadSettings("Music", selections, sqloperators, viewsAs, sortBy);
    }

    public override void SaveSettings()
    {
      base.SaveSettings("Music");
    }
  }
}