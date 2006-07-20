#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.MusicVideos.Database;

namespace MediaPortal.GUI.MusicVideos
{
  public partial class SetupForm : Form
  {
    public SetupForm()
    {
      InitializeComponent();
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      YahooSettings loSettings = YahooSettings.getInstance();
      //int liItemIndex = 1;
      foreach (string lskey in loSettings.moYahooSiteTable.Keys)
      {
        countryList.Items.Add(lskey);
        if (lskey == loSettings.msDefaultCountryName)
        {
          countryList.SelectedIndex = countryList.Items.Count - 1;
        }
      }

      //}
      switch (loSettings.msDefaultBitRate)
      {
        case "56":
          {
            bitrate56.Checked = true;
            break;
          }
        case "128":
          {
            bitrate128.Checked = true;
            break;
          }
        case "300":
          {
            bitrate300.Checked = true;
            break;
          }
        case "768":
          {
            bitrate768.Checked = true;
            break;
          }
      }
      //liItemIndex++;
      //}
      YahooFavorites loFavorites = new YahooFavorites();
      ArrayList loFavList = loFavorites.getFavoriteNames();
      foreach (string lsFav in loFavList)
      {
        FavoriteList.Items.Add(lsFav);
      }

    }

    private void DoneBtn_Click(object sender, EventArgs e)
    {
      YahooSettings loSettings = YahooSettings.getInstance();
      if (bitrate56.Checked)
      {
        loSettings.msDefaultBitRate = "56";
      }
      else if (bitrate128.Checked)
      {
        loSettings.msDefaultBitRate = "128";
      }
      else if (bitrate300.Checked)
      {
        loSettings.msDefaultBitRate = "300";
      }
      else if (bitrate768.Checked)
      {
        loSettings.msDefaultBitRate = "768";
      }
      String lsSelCountry = Convert.ToString(countryList.SelectedItem);

      loSettings.msDefaultCountryName = lsSelCountry;
      loSettings.saveSettings();
      this.Close();
    }

    private void countryList_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void EditBtn_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection loSelectedList = FavoriteList.SelectedItems;
      if (loSelectedList.Count > 0)
      {
        string lsFavName = loSelectedList[0].Text;
        SetupFavoriteForm loFavForm = new SetupFavoriteForm();
        loFavForm.textBoxFavoriteName.Text = lsFavName;
        DialogResult dialogResult = loFavForm.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          String lsNewFavName = loFavForm.textBoxFavoriteName.Text;
          YahooFavorites loFavManager = new YahooFavorites();
          MusicVideoDatabase.getInstance().updateFavorite(lsFavName, lsNewFavName);
          updateFavoriteList();
          //loFavManager.addFavorite();
        }

      }
    }

    private void AddBtn_Click(object sender, EventArgs e)
    {
      Log.Write("in");
      //string lsFavName = loSelectedList[0].Text;
      SetupFavoriteForm loFavForm = new SetupFavoriteForm();
      Log.Write("0");
      //loFavForm.msFavoriteName = "tes";
      Log.Write("1");
      DialogResult dialogResult = loFavForm.ShowDialog(this);
      Log.Write("2");
      if (dialogResult == DialogResult.OK)
      {
        Log.Write("3");
        //YahooFavorites loFavManager = new YahooFavorites();
        String lsFavName = loFavForm.textBoxFavoriteName.Text;
        if (!String.IsNullOrEmpty(lsFavName))
        {

          MusicVideoDatabase.getInstance().createFavorite(lsFavName);
          updateFavoriteList();
        }
      }
    }
    private void updateFavoriteList()
    {
      Log.Write("in Update Favorites");
      YahooFavorites loFavorites = new YahooFavorites();
      ArrayList loFavList = loFavorites.getFavoriteNames();
      FavoriteList.Clear();
      foreach (string lsFav in loFavList)
      {
        FavoriteList.Items.Add(lsFav);
      }
    }

    private void DeleteBtn_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection loSelectedList = FavoriteList.SelectedItems;
      if (loSelectedList.Count > 0)
      {
        string lsFavName = loSelectedList[0].Text;
        MusicVideoDatabase.getInstance().DeleteFavorite(lsFavName);
        updateFavoriteList();
      }
    }

  }
}