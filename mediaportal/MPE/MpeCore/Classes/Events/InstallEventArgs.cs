using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes;

namespace MpeCore.Classes.Events
{
    public class InstallEventArgs
    {
        public FileItem Item { get; set; }
        public GroupItem Group { get; set; }
    }
}
