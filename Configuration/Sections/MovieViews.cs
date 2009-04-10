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


#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MovieViews : BaseViews
  {
    private string defaultVideoViews = Config.GetFile(Config.Dir.Base, "defaultVideoViews.xml");
    private string customVideoViews = Config.GetFile(Config.Dir.Config, "Views.xml");

    private string[] selections = new string[]
                                    {
                                      "watched",
                                      "actor",
                                      "title",
                                      "genre",
                                      "year",
                                      "rating",
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
                                      };

    private string[] viewsAs = new string[]
                                 {
                                   "List",
                                   "Icons",
                                   "Big Icons",
                                   "Filmstrip",
                                 };

    private string[] sortBy = new string[]
                                {
                                  "Name",
                                  "Date",
                                  "Size",
                                  "Year",
                                  "Rating",
                                  "Label",
                                };

    public MovieViews()
      : this("Video Views")
    {
    }

    public MovieViews(string name)
      : base(name)
    {
    }

    public override void LoadSettings()
    {
      base.LoadSettings("Video", selections, sqloperators, viewsAs, sortBy);
    }

    public override void SaveSettings()
    {
      base.SaveSettings("Video");
    }
  }
}