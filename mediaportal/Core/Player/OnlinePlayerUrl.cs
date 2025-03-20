using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Text;

namespace MediaPortal.Player
{
  public class OnlinePlayerUrl
  {
    public const string URL_SCHEME = "onlinevideos";

    public struct Track
    {
      public string Url;
      public string Language;
      public string Description;
      public bool IsDefault;
      public object Tag;
    }

    public string VideoUrl { get; private set; }
    public Track[] AudioTracks { get; private set; }
    public Track[] SubtitleTracks { get; private set; }
    public bool Valid { get; private set; }
    public int DefaultAudio { get; private set; }
    public int DefaultSubtitle { get; private set; }
    public string SubtitleFile { get; private set; }

    /// <summary>
    /// Create (deserialize) new instance from 'onlinevideos' url scheme.
    /// </summary>
    /// <param name="strUrl">Formated 'onlinevideos' url scheme.</param>
    public OnlinePlayerUrl(string strUrl)
    {
      Uri uri = new Uri(strUrl);
      if (uri.Scheme == URL_SCHEME)
      {
        NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
        this.VideoUrl = args.Get("videoUrl");

        if (!int.TryParse(args.Get("audioDefault"), out int i))
          i = -1;
        this.DefaultAudio = i;

        if (!int.TryParse(args.Get("subtitleDefault"), out i))
          i = -1;
        this.DefaultSubtitle = i;

        this.AudioTracks = parseTracks("audio", args, this.DefaultAudio).ToArray();
        this.SubtitleTracks = parseTracks("subtitle", args, this.DefaultSubtitle).ToArray();
        this.SubtitleFile = args.Get("subtitleFile");
        this.Valid = true;
      }
    }

    /// <summary>
    /// Create new instance.
    /// </summary>
    /// <param name="strVideoUrl">Url of the video.</param>
    /// <param name="audiolinks">Optional external audio links.</param>
    /// <param name="subtitleLinks">Optional external subtitle links.</param>
    /// <param name="strSubtitleFile">Path to downloaded subtitle(s). It will be used to load subtitles by subtitle engine.</param>
    public OnlinePlayerUrl(string strVideoUrl, Track[] audiolinks, Track[] subtitleLinks, string strSubtitleFile)
    {
      if (Uri.IsWellFormedUriString(strVideoUrl, UriKind.Absolute))
      {
        this.VideoUrl = strVideoUrl;
        this.AudioTracks = audiolinks;
        this.SubtitleTracks = subtitleLinks;
        this.SubtitleFile = strSubtitleFile;
        this.Valid = true;
      }
    }

    /// <summary>
    /// Serialize this instance to url.
    /// </summary>
    /// <returns>Serialized url.</returns>
    public override string ToString()
    {
      if (this.Valid)
      {
        StringBuilder sb = new StringBuilder(1024);

        sb.Append(URL_SCHEME);
        sb.Append("://127.0.0.1/VideoLink?videoUrl=");
        sb.Append(HttpUtility.UrlEncode(this.VideoUrl));

        appendTracks(this.AudioTracks, "audio", sb);
        appendTracks(this.SubtitleTracks, "subtitle", sb);
        
        if (!string.IsNullOrWhiteSpace(this.SubtitleFile))
        {
          sb.Append("&subtitleFile=");
          sb.Append(HttpUtility.UrlEncode(this.SubtitleFile));
        }

        return sb.ToString();
      }

      return null;
    }

    private static void appendTracks(Track[] tracks, string strPrefix, StringBuilder sb)
    {
      if (tracks == null)
        return;

      int iDefaultTrack = -1;
      int iCnt = 0;

      for (int i = 0; i < tracks.Length; i++)
      {
        Track track = tracks[i];

        if (!string.IsNullOrWhiteSpace(track.Url))
        {
          sb.Append('&');
          sb.Append(strPrefix);
          sb.Append("Url");
          if (iCnt > 0)
            sb.Append(iCnt);
          sb.Append('=');
          sb.Append(HttpUtility.UrlEncode(track.Url));
          //if (tracks.Length == 1)
          //  return;

          sb.Append('&');
          sb.Append(strPrefix);
          sb.Append("Lang");
          if (iCnt > 0)
            sb.Append(iCnt);
          sb.Append('=');
          sb.Append(HttpUtility.UrlEncode(track.Language));

          sb.Append('&');
          sb.Append(strPrefix);
          sb.Append("Descr");
          if (iCnt > 0)
            sb.Append(iCnt);
          sb.Append('=');
          sb.Append(HttpUtility.UrlEncode(track.Description));

          if (track.IsDefault && iDefaultTrack < 0)
            iDefaultTrack = iCnt;

          iCnt++;
        }
      }

      if (iDefaultTrack >= 0)
      {
        sb.Append('&');
        sb.Append(strPrefix);
        sb.Append("Default=");
        sb.Append(iDefaultTrack);
      }
    }
    private static List<Track> parseTracks(string strPrefix, NameValueCollection args, int iDefault)
    {
      List<Track> result = new List<Track>();

      string strArgUrl = strPrefix + "Url";
      string strArgLang = strPrefix + "Lang";
      string strArgDescr = strPrefix + "Descr";

      int iCnt = 0;
      while (true)
      {
        string strSuffix = iCnt > 0 ? iCnt.ToString() : null;
        Track audio = new Track()
        {
          Url = args.Get(strArgUrl + strSuffix),
          Language = args.Get(strArgLang + strSuffix),
          Description = args.Get(strArgDescr + strSuffix)
        };

        if (!string.IsNullOrWhiteSpace(audio.Url))
        {
          if (iDefault == result.Count)
            audio.IsDefault = true;

          result.Add(audio);
          iCnt++;
        }
        else
          break;
      }

      return result;
    }
  }
}
