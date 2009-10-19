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
    }
}
