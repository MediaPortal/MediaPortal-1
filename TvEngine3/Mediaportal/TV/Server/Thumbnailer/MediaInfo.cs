#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Thumbnailer
{
  internal class MediaInfo
  {
    #region enum

    private enum StreamKind
    {
      General,
      Video,
      Audio,
      Text,
      Chapters,
      Image
    }

    private enum InfoKind
    {
      Name,
      Text,
      Measure,
      Options,
      NameText,
      MeasureText,
      Info,
      HowTo
    }

    #endregion

    #region imports

    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_New();

    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfo_Delete(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfo_Close(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Get(IntPtr handle, IntPtr streamKind, UIntPtr streamNumber,
                                                [MarshalAs(UnmanagedType.LPWStr)] string parameter,
                                                IntPtr kindOfInfo, IntPtr kindOfSearch);

    #endregion

    public static TimeSpan GetVideoDuration(string fileName)
    {
      IntPtr handle = MediaInfo_New();
      if (handle == null || handle == IntPtr.Zero)
      {
        Log.Error("media info: failed to initialise");
        return TimeSpan.Zero;
      }
      try
      {
        if ((int)MediaInfo_Open(handle, fileName) == 0)
        {
          Log.Error("media info: failed to open file, file name = {0}", fileName);
          return TimeSpan.Zero;
        }

        try
        {
          IntPtr durationBuffer = MediaInfo_Get(handle, (IntPtr)StreamKind.Video, (UIntPtr)0, "Duration", (IntPtr)InfoKind.Text, (IntPtr)InfoKind.Name);
          if (durationBuffer == null || durationBuffer == IntPtr.Zero)
          {
            Log.Error("media info: failed to get duration for file, file name = {0}", fileName);
            return TimeSpan.Zero;
          }

          string durationString = Marshal.PtrToStringUni(durationBuffer);
          int durationMilliSeconds;
          if (!int.TryParse(durationString, out durationMilliSeconds))
          {
            Log.Error("media info: failed to interpret duration, duration = {0}, file name = {1}", durationString, fileName);
            return TimeSpan.Zero;
          }
          return new TimeSpan(0, 0, 0, 0, durationMilliSeconds);
        }
        finally
        {
          MediaInfo_Close(handle);
        }
      }
      finally
      {
        MediaInfo_Delete(handle);
      }
    }
  }
}