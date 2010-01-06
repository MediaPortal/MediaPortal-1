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