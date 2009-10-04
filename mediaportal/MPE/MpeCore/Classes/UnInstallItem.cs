using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class UnInstallItem
    {
        public string BackUpFile { get; set; }
        public string OriginalFile { get; set; }
        public DateTime FileDate { get; set; }
        public long FileSize { get; set; }
        public string InstallType { get; set; }
    }
}
