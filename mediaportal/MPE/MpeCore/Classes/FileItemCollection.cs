using System;
using System.Collections.Generic;
using System.IO;

namespace MpeCore.Classes
{
    public class FileItemCollection
    {
        public FileItemCollection()
        {
            Items = new List<FileItem>();
        }

        /// <summary>
        /// Gets or sets list of included FileItems.
        /// </summary>
        /// <value>The items.</value>
        public List<FileItem> Items { get; set; }


        /// <summary>
        /// Adds the specified file item.
        /// </summary>
        /// <param name="item">The file item.</param>
        public void Add(FileItem item)
        {
            Items.Add(item);
           
        }

        /// <summary>
        /// Determine if exists the name of the local file.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool ExistLocalFileName(FileItem item)
        {
            return ExistLocalFileName(item.LocalFileName);
        }

        /// <summary>
        /// Determine if exists the name of the local file.
        /// </summary>
        /// <param name="fileName">File name with path.</param>
        /// <returns></returns>
        public bool ExistLocalFileName(string fileName)
        {
            foreach (FileItem item in Items)
            {
                if(item.LocalFileName.CompareTo(fileName)==0)
                    return true;
            }
            return false;
        }
    }
}
