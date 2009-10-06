using System;

using System.Collections.Specialized;

using System.IO;

using System.Text;

namespace MpeCore.Classes
{
    public class PathUtil
    {

        /// <summary>

        /// Creates a relative path from one file or folder to another.

        /// </summary>

        /// <param name="fromDirectory">Contains the directory that defines the start of the relative path.</param>

        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>

        /// <returns>The relative path from the start directory to the end path.</returns>

        /// <exception cref="ArgumentNullException"></exception>

        /// <exception cref="ArgumentException"></exception>

        public static string RelativePathTo(string fromDirectory, string toPath)
        {

            if (fromDirectory == null)

                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)

                throw new ArgumentNullException("fromDirectory");

            if (System.IO.Path.IsPathRooted(fromDirectory) && System.IO.Path.IsPathRooted(toPath))
            {

                if (string.Compare(System.IO.Path.GetPathRoot(fromDirectory),

                System.IO.Path.GetPathRoot(toPath), true) != 0)
                {
                    return toPath;
                }

            }

            StringCollection relativePath = new StringCollection();

            string[] fromDirectories = fromDirectory.Split(System.IO.Path.DirectorySeparatorChar);

            string[] toDirectories = toPath.Split(System.IO.Path.DirectorySeparatorChar);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);

            int lastCommonRoot = -1;

            // find common root

            for (int x = 0; x < length; x++)
            {

                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)

                    break;

                lastCommonRoot = x;

            }

            if (lastCommonRoot == -1)
            {

                return toPath;

            }

            // add relative folders in from path

            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)

                if (fromDirectories[x].Length > 0)

                    relativePath.Add("..");

            // add to folders to path

            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)

                relativePath.Add(toDirectories[x]);

            // create relative path

            string[] relativeParts = new string[relativePath.Count];

            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;

        }


    }
}