#region Copyright (C) 2005-2007 Team MediaPortal
/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
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
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using SetupTv;
using System.Reflection;

namespace ProcessPlugins.EpgGrabber
{
  public partial class Zap2itPluginConfig : SetupTv.SectionSettings
  {
    Zap2it.LineupManager lineupManager;
    TvLibrary.CountryCollection countryList;

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="T:PluginConfig"/> class.
    /// </summary>
    public Zap2itPluginConfig()
      : this( "Zap2It" )
    {
      //InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:PluginConfig"/> class.
    /// </summary>
    public Zap2itPluginConfig( string name )
      : base( name )
    {
      InitializeComponent();
    }
    #endregion

    #region Section Activated/Deactiviated Overrides
    public override void OnSectionDeActivated()
    {

      PluginSettings.UseDvbEpgGrabber = false;
      PluginSettings.Username = textBoxUsername.Text;
      PluginSettings.Password = textBoxPassword.Text;
      PluginSettings.GuideDays = (int)numericUpDownDays.Value;
      PluginSettings.AddNewChannels = checkBoxAddChannels.Checked;
      PluginSettings.RenameExistingChannels = checkBoxRenameChannels.Checked;
      PluginSettings.ChannelNameTemplate = comboBoxNameFormat.Text;
      PluginSettings.NotifyOnCompletion = checkBoxNotification.Checked;
      PluginSettings.AppendMetaInformationToDescription = checkBoxAppendMeta.Checked;
      PluginSettings.ExternalInput = (TvLibrary.Implementations.AnalogChannel.VideoInputType)Enum.Parse( typeof( TvLibrary.Implementations.AnalogChannel.VideoInputType ), comboBoxExternalInput.SelectedItem.ToString() );
      PluginSettings.AllowChannelNumberOnlyMapping = checkBoxAllowChannelNumberOnlyMapping.Checked;
      PluginSettings.ExternalInputCountry = countryList.GetTunerCountry( comboBoxExternalInputCountry.SelectedItem.ToString() );
      PluginSettings.SortChannelsByNumber = checkBoxSortChannelsByChannelNumber.Checked;
      PluginSettings.DeleteChannelsWithNoEPGMapping = checkBoxDeleteChannelsWithNoEPGMapping.Checked;

      if( checkboxForceUpdate.Checked )
      {
        PluginSettings.NextPoll = DateTime.Now;
      }

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      countryList = new TvLibrary.CountryCollection();

      this.Text += " v" + Assembly.GetExecutingAssembly().GetName().Version.ToString( 2 );
      this.textBoxUsername.Text = PluginSettings.Username;
      this.textBoxPassword.Text = PluginSettings.Password;
      this.numericUpDownDays.Value = PluginSettings.GuideDays;
      this.checkBoxAddChannels.Checked = PluginSettings.AddNewChannels;
      this.checkBoxRenameChannels.Checked = PluginSettings.RenameExistingChannels;
      this.comboBoxNameFormat.Text = PluginSettings.ChannelNameTemplate;
      this.checkBoxNotification.Checked = PluginSettings.NotifyOnCompletion;
      this.checkBoxAppendMeta.Checked = PluginSettings.AppendMetaInformationToDescription;

      this.comboBoxExternalInput.Items.Clear();
      this.comboBoxExternalInput.Items.AddRange( Enum.GetNames( typeof( TvLibrary.Implementations.AnalogChannel.VideoInputType ) ) );
      this.comboBoxExternalInput.SelectedIndex = this.comboBoxExternalInput.FindStringExact( PluginSettings.ExternalInput.ToString() );
      this.comboBoxExternalInput.Enabled = this.checkBoxAddChannels.Checked;

      this.comboBoxExternalInputCountry.Items.Clear();
      this.comboBoxExternalInputCountry.Items.AddRange( countryList.Countries );
      this.comboBoxExternalInputCountry.SelectedIndex = this.comboBoxExternalInputCountry.FindStringExact( PluginSettings.ExternalInputCountry.ToString() );
      this.comboBoxExternalInputCountry.Enabled = this.checkBoxAddChannels.Checked;

      this.checkBoxAllowChannelNumberOnlyMapping.Checked = PluginSettings.AllowChannelNumberOnlyMapping;
      this.checkBoxSortChannelsByChannelNumber.Checked = PluginSettings.SortChannelsByNumber;
      this.checkBoxDeleteChannelsWithNoEPGMapping.Checked = PluginSettings.DeleteChannelsWithNoEPGMapping;
    }
    #endregion


    /// <summary>
    /// Handles the Click event of the pbZap2itLogo control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void pbZap2itLogo_Click( object sender, EventArgs e )
    {
      System.Diagnostics.Process.Start( "http://labs.zap2it.com" );
    }

    #region Main Tab
    /// <summary>
    /// Handles the LinkClicked event of the linkZap2it control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
    private void linkZap2it_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
    {
      System.Diagnostics.Process.Start( "http://labs.zap2it.com" );
    }

    /// <summary>
    /// Handles the Validating event of the textBoxUsername control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
    private void textBoxUsername_Validating( object sender, CancelEventArgs e )
    {
      if( textBoxUsername.Text.Length < 6 )
      {
        errorProvider.SetError( textBoxUsername, "Username must be at least 6 characters" );
        e.Cancel = true;
      }
      else
      {
        errorProvider.SetError( textBoxUsername, string.Empty );
      }
    }

    /// <summary>
    /// Handles the Validating event of the textBoxPassword control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
    private void textBoxPassword_Validating( object sender, CancelEventArgs e )
    {
      if( textBoxPassword.Text.Length < 6 )
      {
        errorProvider.SetError( textBoxPassword, "Password must be at least 6 characters" );
        e.Cancel = true;
      }
      else
      {
        errorProvider.SetError( textBoxPassword, string.Empty );
      }
    }

    /// <summary>
    /// Handles the CheckedChanged event of the checkBoxAddChannels control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void checkBoxAddChannels_CheckedChanged( object sender, EventArgs e )
    {
      comboBoxExternalInput.Enabled = checkBoxAddChannels.Checked;
    }
    #endregion

    #region Advanced Tab
    /// <summary>
    /// Handles the Validating event of the comboBoxNameFormat control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
    private void comboBoxNameFormat_Validating( object sender, CancelEventArgs e )
    {
      if( comboBoxNameFormat.Text.IndexOf( "{" ) > -1 && comboBoxNameFormat.Text.IndexOf( "}" ) > -1 )
      {
        errorProvider.SetError( comboBoxNameFormat, string.Empty );
      }
      else
      {
        errorProvider.SetError( comboBoxNameFormat, "Invalid channel name format" );
        e.Cancel = true;
      }

    }
    #endregion

    #region Lineup Manager Tab
    /// <summary>
    /// Handles the Enter event of the tabPageLineupManager control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void tabPageLineupManager_Enter( object sender, EventArgs e )
    {
      // Make sure we are using the most current username and password
      lineupManager = new Zap2it.LineupManager( textBoxUsername.Text, textBoxPassword.Text );
    }

    /// <summary>
    /// Handles the ItemChecked event of the channelListView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.Windows.Forms.ItemCheckedEventArgs"/> instance containing the event data.</param>
    private void channelListView_ItemChecked( object sender, ItemCheckedEventArgs e )
    {
      ListViewItem item = e.Item;
      item.ImageIndex = ( item.Checked ? 1 : 0 );
      toolStripBtnSave.Enabled = true;
    }

    #region Tool Strip Control
    /// <summary>
    /// Handles the SelectedIndexChanged event of the toolStripLineups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripLineups_SelectedIndexChanged( object sender, EventArgs e )
    {
      toolStripBtnLoad.Enabled = ( toolStripLineups.SelectedIndex > -1 );
    }

    /// <summary>
    /// Handles the Click event of the toolStripBtnRefresh control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripBtnRefresh_Click( object sender, EventArgs e )
    {
      toolStripBtnRefresh.Enabled = false;
      tabPageLineupManager.UseWaitCursor = true;
      progressBar.Visible = true;
      progressBar.Enabled = true;
      bgLineupGrabber.RunWorkerAsync();
    }

    /// <summary>
    /// Handles the Click event of the toolStripBtnLoad control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripBtnLoad_Click( object sender, EventArgs e )
    {
      if( toolStripLineups.SelectedItem is Zap2it.WebEntities.WebLineup )
      {
        toolStripBtnLoad.Enabled = false;
        tabPageLineupManager.UseWaitCursor = true;
        progressBar.Visible = true;
        progressBar.Enabled = true;
        bgChannelGrabber.RunWorkerAsync();
      }
    }

    /// <summary>
    /// Handles the Click event of the toolStripBtnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripBtnSave_Click( object sender, EventArgs e )
    {
      if( toolStripLineups.SelectedItem is Zap2it.WebEntities.WebLineup )
      {

        Zap2it.WebEntities.WebLineup lineup = (Zap2it.WebEntities.WebLineup)toolStripLineups.SelectedItem;
        Zap2it.WebEntities.WebChannelCollection webChannels = new Zap2it.WebEntities.WebChannelCollection();
        foreach( ListViewItem item in channelListView.Items )
        {
          Zap2it.WebEntities.WebChannel webChannel = (Zap2it.WebEntities.WebChannel)item.Tag;
          webChannel.Enabled = item.Checked;
          webChannels.Add( webChannel );
        }
        if( webChannels.Count > 0 )
        {
          toolStripBtnSave.Enabled = false;
          toolStripLineupManager.Enabled = false;
          tabPageLineupManager.UseWaitCursor = true;
          progressBar.Visible = true;
          progressBar.Enabled = true;
          bgLineupSaver.RunWorkerAsync( webChannels );
        }
      }
    }
    #endregion

    #region Context Strip Menu
    /// <summary>
    /// Handles the Opening event of the contextMenuStrip control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
    private void contextMenuStrip_Opening( object sender, CancelEventArgs e )
    {
      if( channelListView.FocusedItem != null )
      {
        toolStripMenuItemChannelName.Text = channelListView.FocusedItem.Text;
        toolStripMenuItemEnabled.Checked = channelListView.FocusedItem.Checked;
      }
    }

    /// <summary>
    /// Handles the Click event of the toolStripMenuItemIconView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripMenuItemIconView_Click( object sender, EventArgs e )
    {
      channelListView.View = View.LargeIcon;
    }

    /// <summary>
    /// Handles the Click event of the toolStripMenuItemListView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripMenuItemListView_Click( object sender, EventArgs e )
    {
      channelListView.View = View.Details;
    }

    /// <summary>
    /// Handles the Click event of the toolStripMenuItemEnabled control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
    private void toolStripMenuItemEnabled_Click( object sender, EventArgs e )
    {
      if( channelListView.FocusedItem != null )
      {
        channelListView.FocusedItem.Checked = !channelListView.FocusedItem.Checked;
      }
    }
    #endregion

    #region Background Lineup List Grabber
    /// <summary>
    /// Handles the DoWork event of the bgLineupGrabber control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
    private void bgLineupGrabber_DoWork( object sender, DoWorkEventArgs e )
    {
      if( lineupManager != null )
        e.Result = lineupManager.RetrieveLineups();
    }

    /// <summary>
    /// Handles the RunWorkerCompleted event of the bgLineupGrabber control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
    private void bgLineupGrabber_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      try
      {
        if( e.Error != null )
        {
          MessageBox.Show( e.Error.Message, "Error accessing lineups", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
        else
        {
          Zap2it.WebEntities.WebLineupCollection lineups = e.Result as Zap2it.WebEntities.WebLineupCollection;
          if( lineups.Count > 0 )
          {
            toolStripLineups.BeginUpdate();
            toolStripLineups.Items.Clear();
            foreach( Zap2it.WebEntities.WebLineup lineup in lineups )
            {
              toolStripLineups.Items.Add( lineup );
            }
            toolStripLineups.SelectedIndex = 0;
            toolStripLineups.EndUpdate();
            channelListView.Items.Clear();
          }
        }
        toolStripBtnRefresh.Enabled = true;
        tabPageLineupManager.UseWaitCursor = false;
        progressBar.Enabled = false;
        progressBar.Visible = false;
        tabPageLineupManager.Cursor = DefaultCursor;
      }
      catch( NullReferenceException )
      {
      }
    }
    #endregion

    #region Background Channel Grabber
    /// <summary>
    /// Handles the DoWork event of the bgChannelGrabber control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
    private void bgChannelGrabber_DoWork( object sender, DoWorkEventArgs e )
    {
      Zap2it.WebEntities.WebLineup lineup = (Zap2it.WebEntities.WebLineup)toolStripLineups.SelectedItem;
      if( lineupManager != null )
        e.Result = lineupManager.RetrieveChannelsForLineup( lineup );
    }

    /// <summary>
    /// Handles the RunWorkerCompleted event of the bgChannelGrabber control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
    private void bgChannelGrabber_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      try
      {
        if( e.Error != null )
        {
          MessageBox.Show( e.Error.Message, "Error accessing channel list", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
        else
        {
          Zap2it.WebEntities.WebChannelCollection webChannels = e.Result as Zap2it.WebEntities.WebChannelCollection;
          channelListView.BeginUpdate();
          channelListView.Items.Clear();
          foreach( Zap2it.WebEntities.WebChannel webChannel in webChannels )
          {
            ListViewItem item = new ListViewItem( webChannel.Station, webChannel.Enabled ? 1 : 0 );
            item.Checked = webChannel.Enabled;
            item.Tag = webChannel;
            item.SubItems.Add( webChannel.ChannelNum );
            channelListView.Items.Add( item );
          }
        }
        channelListView.EndUpdate();
        toolStripBtnLoad.Enabled = true;
        tabPageLineupManager.UseWaitCursor = false;
        progressBar.Enabled = false;
        progressBar.Visible = false;
        tabPageLineupManager.Cursor = DefaultCursor;
      }
      catch( NullReferenceException )
      {
      }
    }
    #endregion

    #region Background Lineup Saver
    /// <summary>
    /// Handles the DoWork event of the bgLineupSaver control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
    private void bgLineupSaver_DoWork( object sender, DoWorkEventArgs e )
    {
      if( lineupManager != null && e.Argument is Zap2it.WebEntities.WebChannelCollection )
      {
        Zap2it.WebEntities.WebChannelCollection webChannels = (Zap2it.WebEntities.WebChannelCollection)e.Argument;
        Zap2it.WebEntities.WebLineup lineup = (Zap2it.WebEntities.WebLineup)toolStripLineups.SelectedItem;
        e.Result = lineupManager.SetChannelsForLineup( lineup, webChannels );
      }
    }

    /// <summary>
    /// Handles the RunWorkerCompleted event of the bgLineupSaver control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
    private void bgLineupSaver_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
      try
      {
        if( e.Error != null )
        {
          MessageBox.Show( e.Error.Message, "Error accessing lineups", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
        tabPageLineupManager.UseWaitCursor = false;
        progressBar.Enabled = false;
        progressBar.Visible = false;
        toolStripLineupManager.Enabled = true;
        tabPageLineupManager.Cursor = DefaultCursor;
      }
      catch( NullReferenceException )
      {
      }
    }
    #endregion

    private void okButton_Click( object sender, EventArgs e )
    {
      PluginSettings.UseDvbEpgGrabber = false;
      PluginSettings.Username = textBoxUsername.Text;
      PluginSettings.Password = textBoxPassword.Text;
      PluginSettings.GuideDays = (int)numericUpDownDays.Value;
      PluginSettings.AddNewChannels = checkBoxAddChannels.Checked;
      PluginSettings.RenameExistingChannels = checkBoxRenameChannels.Checked;
      PluginSettings.ChannelNameTemplate = comboBoxNameFormat.Text;
      PluginSettings.NotifyOnCompletion = checkBoxNotification.Checked;
      PluginSettings.ExternalInput = (TvLibrary.Implementations.AnalogChannel.VideoInputType)Enum.Parse( typeof( TvLibrary.Implementations.AnalogChannel.VideoInputType ), comboBoxExternalInput.SelectedItem.ToString() );
      if( checkboxForceUpdate.Checked )
      {
        PluginSettings.NextPoll = DateTime.Now;
      }
      PluginSettings.AllowChannelNumberOnlyMapping = checkBoxAllowChannelNumberOnlyMapping.Checked;
    }

    #endregion
  }
}