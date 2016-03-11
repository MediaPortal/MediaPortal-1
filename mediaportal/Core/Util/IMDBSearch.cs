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
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using MediaPortal.GUI.Library;
using System.Web;
using MediaPortal.Profile;

namespace MediaPortal.Util
{
  /// <summary>
  /// Search IMDB.com for movie-posters and actors
  /// </summary>
  public class IMDBSearch
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
        return (string)_imageList[index];
      }
    }

    /// <summary>
    /// Cover search on IMDB movie page.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros).
    /// Parameter defaultOnly = TRUE will download only IMDB movie default cover.
    /// </summary>
    /// <param name="imdbID"></param>
    /// <param name="defaultOnly"></param>
    public void SearchCovers(string imdbID, bool defaultOnly)

    {
      if (!Win32API.IsConnectedToInternet() || string.IsNullOrEmpty(imdbID))
      {
        return;
      }

      if (!imdbID.StartsWith("tt")) return;
      
      _imageList.Clear();
      
      try
      {
        _imageList = InternalCSScriptGrabbersLoader.Movies.ImagesGrabber.MovieImagesGrabber.GetIMDBImages(imdbID, defaultOnly);
      }
      catch (Exception ex)
      {
        Log.Error("Util IMDBSearchCovers error: {0}", ex.Message);
      }
    }
  }
}