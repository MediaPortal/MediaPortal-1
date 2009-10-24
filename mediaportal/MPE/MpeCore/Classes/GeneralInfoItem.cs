using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{

    public class GeneralInfoItem
    {
        public GeneralInfoItem()
        {
            Version=new VersionInfo();
            Id = Guid.NewGuid().ToString();
            Params=new SectionParamCollection();
            Params.Add(new SectionParam(ParamNamesConst.ICON,"",ValueTypeEnum.File,"The icon file of the package"));
            Params.Add(new SectionParam(ParamNamesConst.ONLINE_ICON, "", ValueTypeEnum.String, "The icon file of the package stored online "));
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

        /// <summary>
        /// Gets or sets the location of packed file.
        /// </summary>
        /// <value>The location.</value>
        public string Location { get; set; }

        public  SectionParamCollection Params { get; set; }
    }
}
