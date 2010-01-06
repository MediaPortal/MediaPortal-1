#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.IO;

namespace Roger.ID3
{
  internal class TagRemover
  {
    public static void Remove(string path)
    {
      // TODO: Exception handler to clean up on failures.

      // Open the source file, and then skip over any tags.
      FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

      // Go back to the beginning of the stream.
      stream.Seek(0, SeekOrigin.Begin);

      TagUtil.SkipTags(stream);

      // Create a temporary file:
      TemporaryFile tempFile = new TemporaryFile(path);

      FileStream dest = new FileStream(tempFile.Path, FileMode.Create, FileAccess.Write, FileShare.None);

      StreamCopier.Copy(stream, dest);

      stream.Close();
      dest.Close();

      tempFile.Swap();
    }
  }
}