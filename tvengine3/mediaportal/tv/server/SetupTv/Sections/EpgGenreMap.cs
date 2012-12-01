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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using TvControl;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Epg;
using MediaPortal.UserInterface.Controls;

namespace SetupTv.Sections
{
  public partial class EpgGenreMap : SectionSettings
  {

    #region variables

    protected List<MpGenre> _mpGenres;
    protected List<string> _allProgramGenres = new List<string>();

    #endregion

    public EpgGenreMap()
      : this("EPG Genre Map") { }

    public EpgGenreMap(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      base.LoadSettings();
      TvBusinessLayer layer = new TvBusinessLayer();

      // Load the list of all EPG provided program genres.
      _allProgramGenres = (List<string>)layer.GetProgramGenres();

      // Load the list of MP genres.
      _mpGenres = layer.GetMpGenres();

      // Populate the guide genre and program genre lists.
      PopulateGuideGenreList();
      PopulateProgramGenreList();
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.SaveMpGenres(_mpGenres);
      base.SaveSettings();
    }

    private void PopulateGuideGenreList()
    {
      // Populate the guide genre list with names.
      listViewGuideGenres.BeginUpdate();
      listViewGuideGenres.Items.Clear();

      for (int i = 0; i < _mpGenres.Count; i++)
      {
        string valueMovie = "";
        if (_mpGenres[i].IsMovie)
        {
          valueMovie = "x";
        }

        string valueEnabled = "";
        if (_mpGenres[i].Enabled)
        {
          valueEnabled = "x";
        }

        ListViewItem item = new ListViewItem(new string[] { _mpGenres[i].Name });
        item.Name = _mpGenres[i].Name;
        item.SubItems.Add(valueMovie);    // Movie subitem(1)
        item.SubItems.Add(valueEnabled);  // Enabled subitem(2)
        listViewGuideGenres.Items.Add(item);
      }

      listViewGuideGenres.EndUpdate();

      // Select the first guide genre in the list.
      // This forces (generates an event for) the mapped genre list to be populated.
      listViewGuideGenres.Items[0].Selected = true;
    }

    private void PopulateMappedGenreList()
    {
      listViewMappedGenres.BeginUpdate();
      listViewMappedGenres.Items.Clear();

      // Find the selected mp genre.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre selectedMpGenre = _mpGenres.Find(x => x.Id == selectedGenreId);

      // Populate the list of mapped genres.
      foreach (var mappedProgramGenre in selectedMpGenre.MappedProgramGenres)
      {
        // Provide an indication that the mapped program genre may be obsolete.  There is no way to know for sure.
        if (!_allProgramGenres.Contains(mappedProgramGenre))
        {
          listViewMappedGenres.Items.Add(mappedProgramGenre, 0);
        }
        else
        {
          listViewMappedGenres.Items.Add(mappedProgramGenre);
        }
      }

      listViewMappedGenres.EndUpdate();
    }

    private void PopulateProgramGenreList()
    {
      // Remove program genres already mapped to mp genres.
      List<string> unmappedProgramGenres = new List<string>(_allProgramGenres);
      foreach (var mpGenre in _mpGenres)
      {
        foreach (var mappedProgramGenre in mpGenre.MappedProgramGenres)
        {
          unmappedProgramGenres.Remove(mappedProgramGenre);
        }
      }

      // Add all unmapped genres to the list view.
      listViewProgramGenres.BeginUpdate();
      foreach (string programGenre in unmappedProgramGenres)
      {
        listViewProgramGenres.Items.Add(programGenre);
      }
      listViewProgramGenres.EndUpdate();
    }

    protected void MapProgramGenres()
    {
      // Find the selected mp genre.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre selectedMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Id == selectedGenreId);

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem programGenre in listViewProgramGenres.SelectedItems)
      {
        listViewProgramGenres.Items.Remove(programGenre);

        // Provide an indication that the mapped program genre may be obsolete.  There is no way to know for sure.
        if (!_allProgramGenres.Contains(programGenre.Text))
        {
          listViewMappedGenres.Items.Add(programGenre.Text, 0);
        }
        else
        {
          listViewMappedGenres.Items.Add(programGenre.Text);
        }

        // Update the genre map.
        selectedMpGenre.MapToProgramGenre(programGenre.Text);
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    protected void UnmapProgramGenres()
    {
      // Find the selected mp genre.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre selectedMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Id == selectedGenreId);

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem mappedGenre in listViewMappedGenres.SelectedItems)
      {
        listViewMappedGenres.Items.Remove(mappedGenre);
        listViewProgramGenres.Items.Add(mappedGenre);

        // Update the genre map.
        selectedMpGenre.UnmapProgramGenre(mappedGenre.Text);
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    private void listViewGuideGenres_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      // Determine if label is changed by checking for null. 
      if (e.Label == null)
      {
        return;
      }

      // Limit the number of genres that a user may input.
      if (e.Item > _mpGenres.Count)
      {
        return;
      }

      // Check for and don't allow empty genre names.
      if ("".Equals(e.Label))
      {
        e.CancelEdit = true;
        MessageBox.Show("Empty genre name not allowed.", "EPG Genre Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      // Find the edited mp genre.
      // Find the selected mp genre.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre editedMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Id == selectedGenreId);

      // Check for and disallow duplicate genre names.
      MpGenre testMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Name == e.Label);

      if (testMpGenre != null && testMpGenre.Id != editedMpGenre.Id)
      {
        if (e.Label.Equals(testMpGenre.Name))
        {
        e.CancelEdit = true;
        MessageBox.Show("Duplicate genre name not allowed.", "EPG Genre Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
        }
      }

      // Change the name of the MP genre.
      editedMpGenre.Name = e.Label;
    }

    private void buttonMapGenres_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        MapProgramGenres();
      }
    }

    private void buttonUnmapGenres_Click(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        UnmapProgramGenres();
      }
    }

    private void listViewGuideGenres_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewGuideGenres.SelectedItems.Count > 0)
      {
        PopulateMappedGenreList();
      }
    }

    private void mpButtonGenreIsMovie_Click(object sender, EventArgs e)
    {
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre selectedMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Id == selectedGenreId);

      // Set the only selected mp genre as the movie genre.  Toggle current value.
      bool oldValue = selectedMpGenre.IsMovie;
      foreach (var genre in _mpGenres)
      {
        genre.IsMovie = false;
      }
      selectedMpGenre.IsMovie = !oldValue;

      for (int i = 0; i < listViewGuideGenres.Items.Count; i++)
      {
        if (listViewGuideGenres.Items[i] != null)
        {
          listViewGuideGenres.Items[i].SubItems[1].Text = "";
        }
      }

      // Set the selected genre as the movie genre if it's been selected.
      if (selectedMpGenre.IsMovie)
      {
        listViewGuideGenres.Items[selectedMpGenre.Id].SubItems[1].Text = "x";
      }
    }

    private void mpButtonEnableGenre_Click(object sender, EventArgs e)
    {
      // Toggle the state.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      MpGenre selectedMpGenre = ((List<MpGenre>)_mpGenres).Find(x => x.Id == selectedGenreId);
      selectedMpGenre.Enabled = !selectedMpGenre.Enabled;

      if ("".Equals(listViewGuideGenres.Items[selectedGenreId].SubItems[2].Text))
      {
        listViewGuideGenres.Items[selectedGenreId].SubItems[2].Text = "x";
      }
      else
      {
        listViewGuideGenres.Items[selectedGenreId].SubItems[2].Text = "";
      }
    }

    private void listViewGuideGenres_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
      switch (e.ColumnIndex)
      {
        // "Movie" column
        case 1:
          if ("x".Equals(e.SubItem.Text))
          {
            /*
            if (e.Item.Selected)
            {
              e.SubItem.BackColor = System.Drawing.Color.FromArgb(51, 153, 255);
              e.SubItem.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            }
             */
            e.Graphics.DrawImage(imageList1.Images[3],
              new System.Drawing.Point(e.SubItem.Bounds.X + (e.SubItem.Bounds.Width / 2) - (imageList1.Images[3].Width / 2), e.SubItem.Bounds.Top));
          }
          else
          {
            e.DrawDefault = true;
          }
          break;

        // "Enabled" column
        case 2:
            if ("x".Equals(e.SubItem.Text))
            {
              /*
              if (e.Item.Selected)
              {
                e.SubItem.BackColor = System.Drawing.Color.FromArgb(51, 153, 255);
                e.SubItem.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
              }
               */
              e.Graphics.DrawImage(imageList1.Images[1],
                new System.Drawing.Point(e.SubItem.Bounds.X + (e.SubItem.Bounds.Width / 2) - (imageList1.Images[1].Width / 2), e.SubItem.Bounds.Top));
            }
            else
            {
              e.Graphics.DrawImage(imageList1.Images[2],
                new System.Drawing.Point(e.SubItem.Bounds.X + (e.SubItem.Bounds.Width / 2) - (imageList1.Images[2].Width / 2), e.SubItem.Bounds.Top));
            }
          break;
        default:
              e.DrawDefault = true;
          break;
      }
    }

    private void listViewGuideGenres_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
      e.DrawDefault = true;
    }

  }
}