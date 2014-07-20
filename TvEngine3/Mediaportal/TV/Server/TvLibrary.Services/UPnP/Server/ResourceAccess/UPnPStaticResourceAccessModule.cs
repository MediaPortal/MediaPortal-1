#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Exceptions;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using MediaPortal.Common;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.ResourceAccess
{
  public class UPnPStaticResourceAccessModule : HttpModule, IDisposable
  {
    protected readonly object _syncObj = new object();

    public static TimeSpan RESOURCE_CACHE_TIME = TimeSpan.FromMinutes(5);
    public static TimeSpan CACHE_CLEANUP_INTERVAL = TimeSpan.FromMinutes(1);

    public UPnPStaticResourceAccessModule()
    {
    }


    protected class Range
    {
      protected long _from;
      protected long _to;

      public Range(long from, long to)
      {
        _from = from;
        _to = to;
      }

      public long From
      {
        get { return _from; }
      }

      public long To
      {
        get { return _to; }
      }

      public long Length
      {
        get { return _to - _from + 1; }
      }
    }


    protected IList<Range> ParseRanges(string byteRangesSpecifier, long size)
    {
      if (string.IsNullOrEmpty(byteRangesSpecifier) || size == 0)
        return null;
      IList<Range> result = new List<Range>();
      try
      {
        string[] tokens = byteRangesSpecifier.Split(new char[] {'='});
        if (tokens.Length == 2 && tokens[0].Trim() == "bytes")
          foreach (string rangeSpec in tokens[1].Split(new char[] {','}))
          {
            tokens = rangeSpec.Split(new char[] {'-'});
            if (tokens.Length != 2)
              return new Range[] {};
            if (!string.IsNullOrEmpty(tokens[0]))
              if (!string.IsNullOrEmpty(tokens[1]))
                result.Add(new Range(long.Parse(tokens[0]), long.Parse(tokens[1])));
              else
                result.Add(new Range(long.Parse(tokens[0]), size - 1));
            else
              result.Add(new Range(Math.Max(0, size - long.Parse(tokens[1])), size - 1));
          }
      }
      catch (Exception e)
      {
        this.LogDebug("UPnPStaticResourceAccessModule: Received illegal Range header", e);
        // As specified in RFC2616, section 14.35.1, ignore invalid range header
      }
      return result;
    }

    public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
    {
      var uri = request.Uri;
      List<string> uriPath = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Split('/').ToList<string>();

      // Check the request path to see if it's for us.
      if (!uri.AbsolutePath.StartsWith("/"+UPnPResourceAccessUtils.RESOURCE_STATIC_ACCESS_PATH))
        return false;

      try
      {
        this.LogDebug("UPnPStaticResourceAccessModule: Attempting to load resource");
        foreach (string tmp in uriPath)
        {
          Log.Debug(tmp);
        }


        // create the file path: %ProgramData\Team MediaPortal\MediaPortal TV Server\ResourceServer\[requested folder structure]\[file]
        String filePath = String.Format(@"{0}\{1}\{2}", PathManager.GetDataPath, UPnPResourceAccessUtils.RESOURCE_STATIC_DIRECTORY, string.Join("\\", uriPath.GetRange(1, uriPath.Count - 1)));
        if (!File.Exists(filePath))
          throw new BadRequestException(string.Format("Media item with path '{0}' not found.", filePath));
        this.LogDebug("Media item with path '{0}'.", filePath);
        response.ContentType = UPnPResourceAccessUtils.GetMimeFromRegistry(filePath);
        FileStream fs = File.OpenRead(filePath);
        SendWholeFile(response, fs, false);

        // Attempt to grab the media item from the database.
        /*var item = MediaLibraryHelper.GetMediaItem(mediaItemGuid);
        if (item == null)
          throw new BadRequestException(string.Format("Media item '{0}' not found.", mediaItemGuid));

        if (request.QueryString.Contains("aspect") && request.QueryString["aspect"].Value == "THUMBNAILSMALL")
        {
          var thumb = item.Aspects[ThumbnailSmallAspect.ASPECT_ID].GetAttributeValue(ThumbnailSmallAspect.ATTR_THUMBNAIL);
          response.ContentType = "image/jpeg";
          MemoryStream ms = new MemoryStream((byte[])thumb);
          SendWholeFile(response, ms, false);
        }
        else
        {
          // Grab the mimetype from the media item and set the Content Type header.
          var mimeType = item.Aspects[DlnaItemAspect.ASPECT_ID].GetAttributeValue(DlnaItemAspect.ATTR_MIME_TYPE);
          if (mimeType == null)
            throw new InternalServerException("Media item has bad mime type, re-import media item");
          response.ContentType = mimeType.ToString();

          // Grab the resource path for the media item.
          var resourcePathStr =
            item.Aspects[ProviderResourceAspect.ASPECT_ID].GetAttributeValue(
              ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH);
          var resourcePath = ResourcePath.Deserialize(resourcePathStr.ToString());

          var ra = GetResourceAccessor(resourcePath);
          IFileSystemResourceAccessor fsra = ra as IFileSystemResourceAccessor;
          using (var resourceStream = fsra.OpenRead())
          {
            // HTTP/1.1 RFC2616 section 14.25 'If-Modified-Since'
            if (!string.IsNullOrEmpty(request.Headers["If-Modified-Since"]))
            {
              DateTime lastRequest = DateTime.Parse(request.Headers["If-Modified-Since"]);
              if (lastRequest.CompareTo(fsra.LastChanged) <= 0)
                response.Status = HttpStatusCode.NotModified;
            }

            // HTTP/1.1 RFC2616 section 14.29 'Last-Modified'
            response.AddHeader("Last-Modified", fsra.LastChanged.ToUniversalTime().ToString("r"));

            // DLNA Requirement: [7.4.26.1-6]
            // Since the DLNA spec allows contentFeatures.dlna.org with any request, we'll put it in.
            if (!string.IsNullOrEmpty(request.Headers["getcontentFeatures.dlna.org"]))
            {
              if (request.Headers["getcontentFeatures.dlna.org"] != "1")
              {
                // DLNA Requirement [7.4.26.5]
                throw new BadRequestException("Illegal value for getcontentFeatures.dlna.org");
              }
            }
            var dlnaString = DlnaProtocolInfoFactory.GetProfileInfo(item).ToString();
            response.AddHeader("contentFeatures.dlna.org", dlnaString);

            Log.Debug("DlnaResourceAccessModule: returning contentFeatures {0}", dlnaString);

            // DLNA Requirement: [7.4.55-57]
            // TODO: Bad implementation of requirement
            if (!string.IsNullOrEmpty(request.Headers["transferMode.dlna.org"]))
            {
              string transferMode = request.Headers["transferMode.dlna.org"];
              Log.Debug("Requested transfer of type " + transferMode);
              if (transferMode == "Streaming")
              {
                response.AddHeader("transferMode.dlna.org", "Streaming");
              }
              if (transferMode == "Interactive")
              {
                response.AddHeader("transferMode.dlna.org", "Interactive");
              }
              if (transferMode == "Background")
              {
                response.AddHeader("transferMode.dlna.org", "Background");
              }
            }

            string byteRangesSpecifier = request.Headers["Range"];
            IList<Range> ranges = ParseRanges(byteRangesSpecifier, resourceStream.Length);
            bool onlyHeaders = request.Method == Method.Header || response.Status == HttpStatusCode.NotModified;
            if (ranges != null && ranges.Count == 1)
              // We only support one range
              SendRange(response, resourceStream, ranges[0], onlyHeaders);
            else
              SendWholeFile(response, resourceStream, onlyHeaders);
          }
        }*/
      }
      catch (FileNotFoundException ex)
      {
        throw new InternalServerException(string.Format("Failed to proccess media item"), ex);
      }

      return true;
    }

    protected void SendRange(IHttpResponse response, Stream resourceStream, Range range, bool onlyHeaders)
    {
      if (range.From > resourceStream.Length)
      {
        response.Status = HttpStatusCode.RequestedRangeNotSatisfiable;
        response.SendHeaders();
        return;
      }
      response.Status = HttpStatusCode.PartialContent;
      response.ContentLength = range.Length;
      response.AddHeader("Content-Range",
                         string.Format("bytes {0}-{1}/{2}", range.From, range.To, resourceStream.Length));
      response.SendHeaders();

      if (onlyHeaders)
        return;

      resourceStream.Seek(range.From, SeekOrigin.Begin);
      Send(response, resourceStream, range.Length);
    }

    protected void SendWholeFile(IHttpResponse response, Stream resourceStream, bool onlyHeaders)
    {
      response.Status = HttpStatusCode.OK;
      response.ContentLength = resourceStream.Length;
      response.SendHeaders();

      if (onlyHeaders)
        return;

      Send(response, resourceStream, resourceStream.Length);
    }

    protected void Send(IHttpResponse response, Stream resourceStream, long length)
    {
      const int BUF_LEN = 8192;
      byte[] buffer = new byte[BUF_LEN];
      int bytesRead;
      while ((bytesRead = resourceStream.Read(buffer, 0, length > BUF_LEN ? BUF_LEN : (int) length)) > 0)
        // Don't use Math.Min since (int) length is negative for length > Int32.MaxValue
      {
        length -= bytesRead;
        response.SendBody(buffer, 0, bytesRead);
      }
    }

    public void Dispose()
    {

    }
  }
}