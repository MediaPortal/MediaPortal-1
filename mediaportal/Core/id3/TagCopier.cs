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
  /// <summary>
  /// Summary description for TagCopier.
  /// </summary>
  public class TagCopier
  {
    public static void Copy(string source, string destination)
    {
      // TODO: Exception handler, or finally block, to clean up on failures?

      // We need to copy the tags from the source file to a temporary file.
      FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
      sourceStream.Seek(0, SeekOrigin.Begin);

      // Create a temporary file:
      TemporaryFile tempFile = new TemporaryFile(destination);

      // Open it.
      FileStream tempStream = new FileStream(tempFile.Path, FileMode.Create, FileAccess.Write, FileShare.None);

      // Copy the tags.
      CopyTags(sourceStream, tempStream);

      // We don't need the source file any more.
      sourceStream.Close();

      // Get the destination file open:
      FileStream destinationStream = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.None);

      // Now we need to skip the tags in the destination.
      TagUtil.SkipTags(destinationStream);

      // Copy the remainder of the output file into the temporary file.
      StreamCopier.Copy(destinationStream, tempStream);

      // We don't need the destination file any more.
      destinationStream.Close();

      // We don't need the temp file any more.
      tempStream.Close();

      // Do the rename shuffle.
      tempFile.Swap();
    }

    /// <summary>
    /// Copy ID3v2 tags from one stream to another.
    /// </summary>
    /// <param name="source">The source stream. Assumed positioned at the 'ID3' header, i.e. the start of the file.</param>
    /// <param name="dest">The destination stream.</param>
    private static void CopyTags(Stream source, Stream dest)
    {
      // Figure out the length of the ID3 header in the source stream:
      long returnPos = source.Position;
      long tagEndOffset = TagUtil.GetTagEndOffset(source);
      source.Seek(returnPos, SeekOrigin.Begin);

      // Now copy that many bytes into the destination:
      StreamCopier.Copy(source, dest, tagEndOffset);
    }
  }
}