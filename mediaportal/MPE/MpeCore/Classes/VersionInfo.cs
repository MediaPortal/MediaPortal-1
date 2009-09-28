using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class VersionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// Version number components are stored in string format
        /// </summary>
        public VersionInfo()
        {
            Major = "0";
            Minor = "0";
            Build = "0";
            Revision = "0";
        }

        public string Major { get; set; }
        public string Minor { get; set; }
        public string Build { get; set; }
        public string Revision { get; set; }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Build + "." + Revision;
        }
    }
}
