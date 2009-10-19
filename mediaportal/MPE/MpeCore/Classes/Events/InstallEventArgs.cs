using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes;

namespace MpeCore.Classes.Events
{
    public class InstallEventArgs
    {
        public InstallEventArgs(GroupItem groupItem, FileItem fileItem)
        {
            Group = groupItem;
            Item = fileItem;
            Description = string.Empty;
        }

        public InstallEventArgs(string description)
        {
            Group = new GroupItem();
            Item = new FileItem();
            Description = description;
        }
        /// <summary>
        /// Gets or sets the currently  intalled file item
        /// </summary>
        /// <value>The item.</value>
        public FileItem Item { get; set; }
        /// <summary>
        /// Gets or sets the currently  intalling group
        /// </summary>
        /// <value>The group.</value>
        public GroupItem Group { get; set; }

        public string Description { get; set; }
    }
}
