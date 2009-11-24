using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes.Project
{
    public class FolderGroup : FileItem
    {
        public FolderGroup()
        {
            Folder = string.Empty;
            Group = string.Empty;
        }

        public string Folder { get; set; }
        public string Group { get; set; }
        public bool Recursive { get; set; }

        public override string ToString()
        {
            return Folder;
        }
    }
}
