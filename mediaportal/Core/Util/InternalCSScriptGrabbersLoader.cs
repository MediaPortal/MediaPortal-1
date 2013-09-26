using System;
using System.Collections;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using CSScriptLibrary;

namespace MediaPortal.Util
{
  public class InternalCSScriptGrabbersLoader
  {
    #region Movies

    public class Movies
    {
      public static string InternalMovieScriptDirectory = Config.GetSubFolder(Config.Dir.Config, "scripts");

      public class ImagesGrabber
      {
        private IInternalMovieImagesGrabber _movieImagesGrabber;
        private bool _movieImagesGrabberLoaded;

        public IInternalMovieImagesGrabber MovieImagesGrabber
        {
          get
          {
            if (!_movieImagesGrabberLoaded)
            {
              if (!LoadScript())
              {
                MovieImagesGrabber = null;
              }

              _movieImagesGrabberLoaded = true; // only try to load it once
            }
            return _movieImagesGrabber;
          }

          set { _movieImagesGrabber = value; }
        }

        public bool LoadScript()
        {
          string scriptFileName = InternalMovieScriptDirectory + @"\InternalMovieImagesGrabber.csscript";

          // Script support script.csscript
          if (!File.Exists(scriptFileName))
          {
            Log.Error("InternalMovieImagesGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
            return false;
          }

          try
          {
            Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
            AsmHelper script = new AsmHelper(CSScript.Load(scriptFileName, null, false));
            MovieImagesGrabber = (IInternalMovieImagesGrabber) script.CreateObject("MovieImagesGrabber");
          }
          catch (Exception ex)
          {
            Log.Error("InternalMovieImagesGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
            return false;
          }

          return true;
        }
      }

      public interface IInternalMovieImagesGrabber
      {
        // IMDB Covers
        ArrayList GetIMDBImages(string imdbID, bool defaultOnly);

        // IMPAwards covers
        ArrayList GetIMPAwardsImages(string movieName, string imdbMovieID);

        // TMDB Fanart
        ArrayList GetTmdbFanartByApi(int movieId, string imdbTT, string title, bool random, int countFA,
          string strSearch, out string fileFanArtDefault, out string fanartUrl);

        // TMDB Covers
        ArrayList GetTmdbCoverImages(string movieTitle, string imdbMovieID);

        // TMDB ActorImage
        ArrayList GetTmdbActorImage(string actorName);
      }

    }

    #endregion
  }
}
