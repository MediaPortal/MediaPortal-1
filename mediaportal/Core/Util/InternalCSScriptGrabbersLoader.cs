#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
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
        private static IInternalMovieImagesGrabber _movieImagesGrabber;
        private static bool _movieImagesGrabberLoaded;
        private static AsmHelper _asmHelper;

        public static void ResetGrabber()
        {
          if (_asmHelper != null)
          {
            _asmHelper.Dispose();
            _asmHelper = null;
          }

          if (_movieImagesGrabber != null)
          {
            _movieImagesGrabber.SafeDispose();
            _movieImagesGrabber = null;
          }

          _movieImagesGrabberLoaded = false;
        }

        public static IInternalMovieImagesGrabber MovieImagesGrabber
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

        private static bool LoadScript()
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
            _asmHelper = new AsmHelper(CSScript.Load(scriptFileName, null, false));
            MovieImagesGrabber = (IInternalMovieImagesGrabber) _asmHelper.CreateObject("MovieImagesGrabber");
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
