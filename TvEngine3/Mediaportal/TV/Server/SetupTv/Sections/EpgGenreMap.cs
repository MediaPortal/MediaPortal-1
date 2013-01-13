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
using System.Linq;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class EpgGenreMap : SectionSettings
  {

    #region variables

    protected List<TvGuideCategory> _mpGenres;
    protected List<ProgramCategory> _allProgramGenres = new List<ProgramCategory>();

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

      // Load the list of all EPG provided program genres.
      //_allProgramGenres = (List<string>)layer.GetProgramGenres();

      /*_allProgramGenres = new List<TvGuideCategory>();
      _allProgramGenres.Add("genre_static0");
      _allProgramGenres.Add("genre_static1");
      */

      // Load the list of MP genres.
      //_mpGenres = layer.GetMpGenres();
      /*_mpGenres = new List<MpGenre>();
      _mpGenres.Add(new MpGenre("mpgenre_static0", 0));
      _mpGenres.Add(new MpGenre("mpgenre_static1", 1));
      */

      _allProgramGenres = ServiceAgents.Instance.ProgramCategoryServiceAgent.ListAllProgramCategories().ToList();
      _mpGenres = ServiceAgents.Instance.ProgramCategoryServiceAgent.ListAllTvGuideCategories().ToList();

      // Populate the guide genre and program genre lists.
      PopulateGuideGenreList();
      PopulateProgramGenreList();
    }

    public override void SaveSettings()
    {      
      //layer.SaveMpGenres(_mpGenres);
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
        if (_mpGenres[i].IsEnabled)
        {
          valueEnabled = "x";
        }

        ListViewItem item = new ListViewItem(new string[] { _mpGenres[i].Name });
        item.Name = _mpGenres[i].Name;
        item.SubItems.Add(valueMovie);    // Movie subitem(1)
        item.SubItems.Add(valueEnabled);  // Enabled subitem(2)
        listViewGuideGenres.Items.Add(item);
        item.Tag = _mpGenres[i];
      }

      listViewGuideGenres.EndUpdate();

      // Select the first guide genre in the list.
      // This forces (generates an event for) the mapped genre list to be populated.
      if (listViewGuideGenres.Items.Count > 0)
      {
        listViewGuideGenres.Items[0].Selected = true;
      }
    }

    private void PopulateMappedGenreList()
    {
      listViewMappedGenres.BeginUpdate();
      listViewMappedGenres.Items.Clear();

      // Find the selected mp genre.
      int selectedGenreId = (listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory).IdTvGuideCategory;
      TvGuideCategory selectedMpGenre = _mpGenres.Find(x => x.IdTvGuideCategory == selectedGenreId);

      if (selectedMpGenre != null)
      {
        
      }

      // Populate the list of mapped genres.
      foreach (ProgramCategory mappedProgramGenre in selectedMpGenre.ProgramCategories)
      {
        // Provide an indication that the mapped program genre may be obsolete.  There is no way to know for sure.
        if (!_allProgramGenres.Contains(mappedProgramGenre))
        {
          ListViewItem item = listViewMappedGenres.Items.Add(mappedProgramGenre.Category, 0);
          item.Tag = mappedProgramGenre;
        }
        else
        {
          ListViewItem item = listViewMappedGenres.Items.Add(mappedProgramGenre.Category);
          item.Tag = mappedProgramGenre;
        }
      }

      listViewMappedGenres.EndUpdate();
    }

    private void PopulateProgramGenreList()
    {
      // Remove program genres already mapped to mp genres.
      var unmappedProgramGenres = new List<ProgramCategory>(_allProgramGenres);
      foreach (var mpGenre in _mpGenres)
      {
        foreach (ProgramCategory mappedProgramGenre in mpGenre.ProgramCategories)
        {
          unmappedProgramGenres.Remove(mappedProgramGenre);
        }
      }

      // Add all unmapped genres to the list view.
      listViewProgramGenres.BeginUpdate();
      foreach (ProgramCategory programGenre in unmappedProgramGenres)
      {
        ListViewItem item = listViewProgramGenres.Items.Add(programGenre.Category);
        item.Tag = programGenre;
      }
      listViewProgramGenres.EndUpdate();
    }

    protected void MapProgramGenres()
    {
      // Find the selected mp genre.
      TvGuideCategory programCategory = listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory;
      TvGuideCategory selectedMpGenre = ((List<TvGuideCategory>)_mpGenres).Find(x => x.IdTvGuideCategory == programCategory.IdTvGuideCategory);

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem programGenre in listViewProgramGenres.SelectedItems)
      {
        listViewProgramGenres.Items.Remove(programGenre);

        // Provide an indication that the mapped program genre may be obsolete.  There is no way to know for sure.
        var programCategoryItem = programGenre.Tag as ProgramCategory;
        if (!_allProgramGenres.Contains(programCategoryItem))
        {
          listViewMappedGenres.Items.Add(programGenre.Text, 0);
        }
        else
        {
          listViewMappedGenres.Items.Add(programGenre.Text);
        }

        // Update the genre map.
        bool hasProgramCategoryAlready = false;
        foreach (ProgramCategory category in selectedMpGenre.ProgramCategories)
        {
          if (category.IdProgramCategory == programCategoryItem.IdProgramCategory)
          {
            hasProgramCategoryAlready = true;
            break;
          }
        }
        if (!hasProgramCategoryAlready)
        {
          selectedMpGenre.ProgramCategories.Add(programCategoryItem);          
        }        
      }

      listViewMappedGenres.EndUpdate();
      listViewProgramGenres.EndUpdate();
    }

    protected void UnmapProgramGenres()
    {
      // Find the selected mp genre.
      TvGuideCategory programCategory = listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory;
      TvGuideCategory selectedMpGenre = ((List<TvGuideCategory>)_mpGenres).Find(x => x.IdTvGuideCategory == programCategory.IdTvGuideCategory);

      listViewMappedGenres.BeginUpdate();
      listViewProgramGenres.BeginUpdate();

      foreach (ListViewItem mappedGenre in listViewMappedGenres.SelectedItems)
      {
        listViewMappedGenres.Items.Remove(mappedGenre);
        listViewProgramGenres.Items.Add(mappedGenre);

        // Update the genre map.

        ProgramCategory categoryFoundToDelete = null;

        foreach (ProgramCategory category in selectedMpGenre.ProgramCategories)
        {
          if (category.IdTvGuideCategory == programCategory.IdTvGuideCategory)
          {
            categoryFoundToDelete = category;
            break;
          }
        }

        if (categoryFoundToDelete != null)
        {
          selectedMpGenre.ProgramCategories.Remove(categoryFoundToDelete);
        }

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
      TvGuideCategory programCategory = listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory;      
      TvGuideCategory editedMpGenre = ((List<TvGuideCategory>)_mpGenres).Find(x => x.IdTvGuideCategory == programCategory.IdTvGuideCategory);

      // Check for and disallow duplicate genre names.
      TvGuideCategory testMpGenre = _mpGenres.Find(x => x.Name == e.Label);

      if (testMpGenre != null && testMpGenre.IdTvGuideCategory != editedMpGenre.IdTvGuideCategory)
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
      TvGuideCategory programCategory = listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory;
      TvGuideCategory selectedMpGenre = _mpGenres.Find(x => x.IdTvGuideCategory == programCategory.IdTvGuideCategory);

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
        listViewGuideGenres.Items[selectedGenreId].SubItems[1].Text = "x";
      }
    }

    private void mpButtonEnableGenre_Click(object sender, EventArgs e)
    {
      // Toggle the state.
      int selectedGenreId = listViewGuideGenres.SelectedItems[0].Index;
      TvGuideCategory programCategory = listViewGuideGenres.SelectedItems[0].Tag as TvGuideCategory;
      TvGuideCategory selectedMpGenre = _mpGenres.Find(x => x.IdTvGuideCategory == programCategory.IdTvGuideCategory);
      selectedMpGenre.IsEnabled = !selectedMpGenre.IsEnabled;

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