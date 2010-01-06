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

using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class GeneralInfoItem
  {
    public GeneralInfoItem()
    {
      Version = new VersionInfo();
      Id = Guid.NewGuid().ToString();
      ReleaseDate = DateTime.Now;
      Tags = string.Empty;
      Params = new SectionParamCollection();
      Params.Add(new SectionParam(ParamNamesConst.ICON, "", ValueTypeEnum.File,
                                  "The icon file of the package (jpg,png,bmp)"));
      Params.Add(new SectionParam(ParamNamesConst.ONLINE_ICON, "", ValueTypeEnum.String,
                                  "The icon file of the package stored online (jpg,png,bmp)"));
      Params.Add(new SectionParam(ParamNamesConst.CONFIG, "", ValueTypeEnum.Template,
                                  "The file used to configure the extension.\n If have .exe extension the will be executed\n If have .dll extension used like MP plugin configuration"));
    }

    public string Name { get; set; }
    public string Id { get; set; }
    public string Author { get; set; }
    public string HomePage { get; set; }
    public string ForumPage { get; set; }
    public string UpdateUrl { get; set; }
    public VersionInfo Version { get; set; }
    public string ExtensionDescription { get; set; }
    public string VersionDescription { get; set; }
    public string DevelopmentStatus { get; set; }
    public string OnlineLocation { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Tags { get; set; }

    /// <summary>
    /// Gets or sets the location of packed file.
    /// </summary>
    /// <value>The location.</value>
    public string Location { get; set; }

    public SectionParamCollection Params { get; set; }

    public TagCollection TagList
    {
      get { return new TagCollection(Tags); }
    }
  }
}