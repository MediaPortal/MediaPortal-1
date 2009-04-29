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

using MediaPortal.Playlists;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListTest
  {
    [SetUp]
    public void Init()
    {
    }

    [Test]
    public void NewlyAddedSongsAreNotMarkedPlayed()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      Assert.IsFalse(pl.AllPlayed());
    }

    [Test]
    public void RemoveReallyRemovesASong()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      pl.Remove("myfile.mp3");

      Assert.AreEqual(0, pl.Count);
    }

    [Test]
    public void AllPlayedReturnsTrueWhenAllArePlayed()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      item.Played = true;
      pl.Add(item);

      item = new PlayListItem("my 2:d song", "myfile2.mp3");
      item.Played = true;
      pl.Add(item);

      Assert.IsTrue(pl.AllPlayed());
    }

    [Test]
    public void ResetSetsAllItemsToFalse()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      PlayListItem item2 = new PlayListItem("my 2:d song", "myfile2.mp3");
      pl.Add(item2);

      pl[0].Played = true;
      pl[1].Played = true;

      pl.ResetStatus();

      Assert.IsFalse(pl[0].Played);
      Assert.IsFalse(pl[1].Played);
    }
  }
}