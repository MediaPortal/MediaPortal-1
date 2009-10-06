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
    }
}
