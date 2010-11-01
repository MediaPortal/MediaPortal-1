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

using System;
using System.Collections.Generic;

namespace MediaPortal.Music.Database
{
  public class AlbumSort : IComparer<MusicAlbumInfo>
  {
    private string _album;
    private string _artistName;
    private int _year;

    public AlbumSort(string album, string artistName, int year)
    {
      _album = album.ToLower();
      _artistName = artistName.ToLower();
      _year = year;
    }

    public int Compare(MusicAlbumInfo info1, MusicAlbumInfo info2)
    {
      int fitness1 = GetFitness(info1.Title, info1.Artist, info1.Title2);
      int fitness2 = GetFitness(info2.Title, info2.Artist, info2.Title2);
      if (fitness1 > fitness2)
      {
        return -1;
      }
      if (fitness1 < fitness2)
      {
        return 1;
      }
      return String.Compare(info1.Title2, info2.Title2);
    }

    private int GetFitness(string albumName, string artistName, string year)
    {
      int fitness = 0;
      if (_year > 0 && year.IndexOf(_year.ToString()) >= 0)
      {
        fitness += 4;
      }

      if (_artistName != string.Empty)
      {
        string[] parts = _artistName.Split(new char[] {' '});
        int[] offsets = new int[parts.Length];
        for (int i = 0; i < parts.Length; ++i)
        {
          offsets[i] = -1;
          int pos = artistName.ToLower().IndexOf(parts[i].ToLower());
          if (pos >= 0)
          {
            if (i > 0)
            {
              if (pos > offsets[i - 1])
              {
                fitness += 2;
              }
              else
              {
                fitness++;
              }
            }
            else
            {
              fitness += 2;
            }
            offsets[i] = pos;
          }
        }
      }

      if (_album != string.Empty)
      {
        string[] parts = _album.Split(new char[] {' '});
        int[] offsets = new int[parts.Length];
        for (int i = 0; i < parts.Length; ++i)
        {
          offsets[i] = -1;
          int pos = albumName.ToLower().IndexOf(parts[i].ToLower());
          if (pos >= 0)
          {
            if (i > 0)
            {
              if (pos > offsets[i - 1])
              {
                fitness += 2;
              }
              else
              {
                fitness++;
              }
            }
            else
            {
              fitness += 2;
            }
            offsets[i] = pos;
          }
        }
      }
      return fitness;
    }
  }
}