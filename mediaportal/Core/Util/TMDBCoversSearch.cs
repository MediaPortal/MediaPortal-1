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
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMDB.com for movie-posters
  /// </summary>
  public class TMDBCoverSearch
  {
    private ArrayList _imageList = new ArrayList();

    public int Count
    {
      get { return _imageList.Count; }
    }

    public string this[int index]
    {
      get
      {
        if (index < 0 || index >= _imageList.Count) return string.Empty;
        return (string) _imageList[index];
      }
    }

    /// <summary>
    /// Cover search using TMDB API by IMDBmovieID for accuracy.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros).
    /// Or if no IMDBid movie title can be used with lesser accuracy .
    /// </summary>
    /// <param name="imdbMovieID"></param>
    /// <param name="movieTitle"></param>
    public void SearchCovers(string movieTitle, string imdbMovieID)

    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      _imageList.Clear();

      try
      {
        _imageList =
          InternalCSScriptGrabbersLoader.Movies.ImagesGrabber.MovieImagesGrabber.GetTmdbCoverImages(movieTitle,
            imdbMovieID);
      }
      catch (Exception ex)
      {
        Log.Error("Util TMDBCoverSearch error: {0}", ex.Message);
      }
    }

    public void SearchActorImage(string actorName, ref ArrayList actorThumbs)
    {
      if (!Win32API.IsConnectedToInternet() || string.IsNullOrEmpty(actorName))
      {
        return;
      }

      actorThumbs.Clear();
      
      try
      {
        actorThumbs = InternalCSScriptGrabbersLoader.Movies.ImagesGrabber.MovieImagesGrabber.GetTmdbActorImage(actorName);
      }
      catch (Exception ex)
      {
        Log.Error("Util TMDBSearchActorImage error: {0}", ex.Message);
      }
    }
  }
}