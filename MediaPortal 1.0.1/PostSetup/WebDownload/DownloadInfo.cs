#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Collections;
using System.IO;
using System.Net;

namespace PostSetup
{
  /// <summary>
  /// Summary description for DownloadInfo.
  /// </summary>
  // The RequestState class passes data across async calls.
  public class DownloadInfo
  {
    private const int BufferSize = 1024;
    public byte[] BufferRead;

    public bool useFastBuffers;
    public byte[] dataBufferFast;
    public ArrayList dataBufferSlow;

    public int dataLength;
    public int bytesProcessed;

    public WebRequest Request;
    public Stream ResponseStream;

    public DownloadProgressHandler ProgressCallback;

    public DownloadInfo()
    {
      BufferRead = new byte[BufferSize];
      Request = null;
      dataLength = -1;
      bytesProcessed = 0;
      useFastBuffers = true;
    }
  }
}