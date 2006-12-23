#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Vst;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Wa;
using MediaPortal.GUI.Library;
using MediaPortal.Player.DSP;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
  public partial class MusicDSP : SectionSettings
  {
    #region Variables
    // Private Variables
    private int _stream;
    // BASS DSP / FX variables
    private DSP_Stacker _stacker;
    private DSP_Gain _gain = null;
    private BASS_FX_DSPDAMP _damp = null;
    private BASS_FX_DSPCOMPRESSOR _comp = null;
    private int _dampPrio = 3;
    private int _compPrio = 2;
    // VST Related variables
    private int _vstHandle;
    private VSTPROC _vstProc;
    private Dictionary<string, int> _vstHandles = new Dictionary<string, int>();
    // Winamp related variables
    private WINAMP_DSP[] _dsps;
    private int _waDspPlugin;
    private Dictionary<string, int> _waDspPlugins = new Dictionary<string, int>();

    // Protected Variables

    // Public Variables

    #endregion

    #region Constructors/Destructors
    public MusicDSP()
      : this("Music DSP")
    {
    }

    public MusicDSP(string name)
      : base(name)
    {
      InitializeComponent();

      // Init DSP specific vars
      BassWa.BASS_WADSP_Init(this.Handle);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// Set the Tooltip and strings for the about page, when loading the control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MusicDSP_Load(object sender, EventArgs e)
    {
      toolTip.SetToolTip(textBoxMusicFile, "Select a Music file to test the effects.");
      // DSP Page
      toolTip.SetToolTip(DSPTabPg, "Allows setting of BASS DSP Effects.");
      toolTip.SetToolTip(groupBoxGain, "Amplifies the channel signal by a given factor.");
      toolTip.SetToolTip(checkBoxDAmp, "Applies dynamic Amplification with the selected Preset.");
      toolTip.SetToolTip(trackBarGain, "Changes the db value for the Amplification.");
      toolTip.SetToolTip(textBoxGainDBValue, "Enter the Gain Value in db.");
      toolTip.SetToolTip(groupBoxCompressor, "Compressors are commonly used to control the level, by making loud passages quieter, and quiet passages louder.");
      toolTip.SetToolTip(checkBoxCompressor, "Turn on the Compressor.\r\nCompressors are commonly used to control the level, by making loud passages quieter, and quiet passages louder.");
      toolTip.SetToolTip(trackBarCompressor, "Changes the threshold for the Compressor.");
      // VST Page
      toolTip.SetToolTip(listBoxFoundVSTPlugins, "Lists all VST compatible plugins found in the Plugin directory.");
      toolTip.SetToolTip(listBoxSelectedVSTPlugins, "Lists all enabled VST plugins.\r\nDouble click to open the VST editor.\r\n(If the plugin offers one)");
      // WinAmp Page
      toolTip.SetToolTip(listBoxFoundWAPlugins, "Lists all Winamp DSP plugins found in the Plugin directory.");
      toolTip.SetToolTip(listBoxSelectedWAPlugins, "Lists all enabled Winamp DSP plugins.\r\nDouble click to open the plugin editor.\r\n(If the plugin offers one)");
    }

    /// <summary>
    /// Select the Music file to Play for testing the DSP Settings
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btFileselect_Click(object sender, EventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      if (ofd.ShowDialog() == DialogResult.OK)
      {
        textBoxMusicFile.Text = ofd.FileName;
      }
    }

    /// <summary>
    /// Play the selected Music File
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btPlay_Click(object sender, EventArgs e)
    {
      if (File.Exists(textBoxMusicFile.Text))
      {
        // Init BASS
        MediaPortal.Player.BassAudioEngine bassEngine = MediaPortal.Player.BassMusicPlayer.Player;
        
        _stream = Bass.BASS_StreamCreateFile(textBoxMusicFile.Text, 0, 0, BASSStream.BASS_SAMPLE_FLOAT | BASSStream.BASS_STREAM_AUTOFREE | BASSStream.BASS_SAMPLE_SOFTWARE);
        if (_stream != 0)
        {
          // Attach the BASS DSP Effects to the stream
          if (_stacker != null)
          {
            _stacker.ChannelHandle = _stream;
            _stacker.Start();
          }

          if (checkBoxDAmp.Checked)
          {
            BassFx.BASS_FX_DSP_Set(_stream, BASSFXDsp.BASS_FX_DSPFX_DAMP, _dampPrio);
            BassFx.BASS_FX_DSP_SetParameters(_stream, _damp);
          }

          if (checkBoxCompressor.Checked)
          {
            BassFx.BASS_FX_DSP_Set(_stream, BASSFXDsp.BASS_FX_DSPFX_COMPRESSOR, _compPrio);
            BassFx.BASS_FX_DSP_SetParameters(_stream, _comp);
          }


          // Attach VST plugins to Stream
          foreach (string item in listBoxSelectedVSTPlugins.Items)
          {
            string plugin = String.Format(@"{0}\{1}", textBoxVSTPluginDir.Text, item);
            _vstHandle = BassVst.BASS_VST_ChannelSetDSP(_stream, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
            // Copy the parameters of the old handle
            int vstold = _vstHandles[item];
            BassVst.BASS_VST_SetParamCopyParams(vstold, _vstHandle); 
            // Now find out to which stream the old handle was assigned and free it
            BASS_VST_INFO bassvstinfo = new BASS_VST_INFO();
            BassVst.BASS_VST_GetInfo(vstold, bassvstinfo);
            BassVst.BASS_VST_ChannelRemoveDSP(bassvstinfo.channelHandle, vstold);
            _vstHandles[item] = _vstHandle;
          }

          // Attach WinAmp handles to Stream
          foreach (WINAMP_DSP dsp in listBoxSelectedWAPlugins.Items)
          {
            _waDspPlugin = _waDspPlugins[dsp.file];
            BassWa.BASS_WADSP_Start(_waDspPlugin, 0, 0);
            BassWa.BASS_WADSP_ChannelSetDSP(_waDspPlugin, _stream, 1);
          }
          btPlay.Enabled = false;
          btStop.Enabled = true;
          Bass.BASS_ChannelPlay(_stream, false);
        }
        else
        {
          MessageBox.Show("Can't play file. Probably not a valid music file");
        }
      }
      else
      {
        MessageBox.Show("File specified does not exist");
      }
    }

    /// <summary>
    /// Stop Playing the active Music File
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btStop_Click(object sender, EventArgs e)
    {
      btPlay.Enabled = true;
      btStop.Enabled = false;
      // Stop the DSP Stacker
      if (_stacker != null)
        _stacker.Stop();
      // Save the VST plugin parameters before freeing the stream
      foreach (string item in listBoxSelectedVSTPlugins.Items)
      {
        string plugin = String.Format(@"{0}\{1}", textBoxVSTPluginDir.Text, item);
        _vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
        // Copy the parameters of the old handle
        int vstold = _vstHandles[item];
        BassVst.BASS_VST_SetParamCopyParams(vstold, _vstHandle);
        _vstHandles[item] = _vstHandle;
      }
      // Stop the WinAmp DSP
      foreach (WINAMP_DSP dsp in listBoxSelectedWAPlugins.Items)
      {
        _waDspPlugin = _waDspPlugins[dsp.file];
        BassWa.BASS_WADSP_Stop(_waDspPlugin);
      }
      Bass.BASS_ChannelStop(_stream);
    }

    /// <summary>
    /// Sets the parameter for a given Bass effect
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    private void setBassDSP(string id, string name, string value)
    {
      switch (id)
      {
        case "Gain":
          if (name == "Gain_dbV")
          {
            double gainDB = double.Parse(value);
            SetDSPGain(gainDB);
            textBoxGainDBValue.Text = value;
            trackBarGain.Value = (int)(gainDB * 1000d);
          }
          break;
        
        case "DynAmp":
          if (name == "Preset")
          {
            checkBoxDAmp.Checked = true;
            comboBoxDynamicAmplification.SelectedIndex = Convert.ToInt32(value);
            SetDAmpPreset(Convert.ToInt32(value));
          }
          break;

        case "Compressor":
          if (name == "Threshold")
          {
            checkBoxCompressor.Checked = true;
            trackBarCompressor.Value = Convert.ToInt32(value);
          }
          break;
      }
    }

    #region VSTPlugins
    /// <summary>
    /// Select the Directory, where VST plugins are located
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonSelectVSTDir_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      fbd.Description = "Select VST Plugin folder";
      fbd.SelectedPath = Application.StartupPath;
      if (fbd.ShowDialog() == DialogResult.OK)
      {
        textBoxVSTPluginDir.Text = fbd.SelectedPath;
      }
    }

    /// <summary>
    /// Scan the selected directory for VST Plugins and add them to the list box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonVSTSearch_Click(object sender, EventArgs e)
    {
      listBoxFoundVSTPlugins.Items.Clear();
      if (Directory.Exists(textBoxVSTPluginDir.Text))
      {
        DirectoryInfo di = new DirectoryInfo(textBoxVSTPluginDir.Text);
        FileInfo[] fi = di.GetFiles("*.dll", SearchOption.AllDirectories);
        foreach (FileInfo vstplugin in fi)
        {
          try
          {
            BASS_VST_INFO vstInfo = new BASS_VST_INFO();
            _vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, vstplugin.FullName, BASSVSTDsp.BASS_VST_DEFAULT, 1);
            // When Handle > 0 this Vst Plugin is a DSP Plugin
            if (_vstHandle > 0)
            {
              listBoxFoundVSTPlugins.Items.Add(vstplugin.Name);
            }
            BassVst.BASS_VST_ChannelRemoveDSP(0, _vstHandle);
          }
          catch (Exception ex)
          {
            Log.Error("Error reading VST Plugin Information: {0}", ex.Message);
          }
        }
        // And now remove the plugins already selected
        foreach (string selitem in listBoxSelectedVSTPlugins.Items)
        {
          for (int i = 0; i < listBoxFoundVSTPlugins.Items.Count; i++)
          {
            if (selitem == (string)listBoxFoundVSTPlugins.Items[i])
            {
              listBoxFoundVSTPlugins.Items.RemoveAt(i);
            }
          }
        }
      }
    }

    /// <summary>
    /// Add the selected VST plugin(s) to the Selected Plugin Listbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonVSTAdd_Click(object sender, EventArgs e)
    {
      // Get the vst handle and enable it
      string plugin = String.Format(@"{0}\{1}", textBoxVSTPluginDir.Text, listBoxFoundVSTPlugins.SelectedItem);
      _vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
      if (_vstHandle > 0)
      {
        _vstHandles[listBoxFoundVSTPlugins.Text] = _vstHandle;
        listBoxSelectedVSTPlugins.Items.Add(listBoxFoundVSTPlugins.SelectedItem);
        listBoxFoundVSTPlugins.Items.RemoveAt(listBoxFoundVSTPlugins.SelectedIndex);
      }
      else
      {
        MessageBox.Show("Error loading VST Plugin. Probably not valid", "VST Plugin", MessageBoxButtons.OK);
        Log.Debug("Couldn't load VST Plugin {0}. Error code: {1}", plugin, Bass.BASS_ErrorGetCode());
      }
    }

    /// <summary>
    /// Don't use the selected VST Plugin anymore
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonVSTRemove_Click(object sender, EventArgs e)
    {
      // Remove VST Handle
      BassVst.BASS_VST_ChannelRemoveDSP(0, _vstHandles[listBoxSelectedVSTPlugins.Text]);
      _vstHandles.Remove(listBoxSelectedVSTPlugins.Text);
      listBoxFoundVSTPlugins.Items.Add(listBoxSelectedVSTPlugins.SelectedItem);
      listBoxSelectedVSTPlugins.Items.RemoveAt(listBoxSelectedVSTPlugins.SelectedIndex);
    }

    /// <summary>
    /// Open VST Plugin Configuration window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void listBoxSelectedVSTPlugins_DoubleClick(object sender, EventArgs e)
    {
      _vstHandle = _vstHandles[listBoxSelectedVSTPlugins.Text];
      BASS_VST_INFO vstInfo = new BASS_VST_INFO();
      if (BassVst.BASS_VST_GetInfo(_vstHandle, vstInfo) && vstInfo.hasEditor)
      {
        // Set a handle to the callback procedure
        _vstProc = new VSTPROC(vstEditorCallBack);
        BassVst.BASS_VST_SetCallback(_vstHandle, _vstProc, 0);
        // create a new System.Windows.Forms.Form
        Form f = new Form();
        f.Width = vstInfo.editorWidth + 4;
        f.Height = vstInfo.editorHeight + 34;
        f.Closing += new CancelEventHandler(f_Closing);
        f.Text = vstInfo.effectName;
        f.Show();
        BassVst.BASS_VST_EmbedEditor(_vstHandle, f.Handle);
      }
      else
      {
        MessageBox.Show("Plugin has no Editor");
      }
    }

    /// <summary>
    /// The VST Editor window has been closed. Free the resources
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void f_Closing(object sender, CancelEventArgs e)
    {
      // unembed the VST editor
      BassVst.BASS_VST_EmbedEditor(_vstHandle, IntPtr.Zero);
    }

    /// <summary>
    /// This routine is called, whenever a change is done in the VST Editor
    /// </summary>
    /// <param name="vstEditor"></param>
    /// <param name="action"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    private int vstEditorCallBack(int vstEditor, int action, int param1, int param2, int user)
    {
      switch (action)
      {
        case (int)BASSVSTAction.BASS_VST_PARAM_CHANGED:
          // Some slider has been changed in the editor
          BASS_VST_PARAM_INFO paramInfo = new BASS_VST_PARAM_INFO();
          for (int i = BassVst.BASS_VST_GetParamCount(vstEditor) - 1; i >= 0; i--)
          {
            BassVst.BASS_VST_SetParam(_vstHandle, i, BassVst.BASS_VST_GetParam(vstEditor, i));
            BassVst.BASS_VST_GetParamInfo(_vstHandle, i, paramInfo);
          }
          break;
        case (int)BASSVSTAction.BASS_VST_EDITOR_RESIZED:
          // the editor window requests a new size,
          break;
        case (int)BASSVSTAction.BASS_VST_AUDIO_MASTER:
          break;
      }
      return 0;
    }
    #endregion VSTPlugins

    #region WinAmpPlugins
    private void buttonSelectWADir_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      fbd.Description = "Select Winamp Plugin folder";
      fbd.SelectedPath = Application.StartupPath;
      if (fbd.ShowDialog() == DialogResult.OK)
      {
        textBoxWAPluginDir.Text = fbd.SelectedPath;
      }
    }

    private void buttonWASearch_Click(object sender, EventArgs e)
    {
      listBoxFoundWAPlugins.Items.Clear();
      if (Directory.Exists(textBoxWAPluginDir.Text))
      {
        _dsps = BassWa.BASS_WADSP_FindPlugins(textBoxWAPluginDir.Text);
        listBoxFoundWAPlugins.Items.AddRange(_dsps);
        // If plugins are already selected, remove them from the found plugin list
        foreach (WINAMP_DSP dspSelected in listBoxSelectedWAPlugins.Items)
        {
          for (int i = 0; i < listBoxFoundWAPlugins.Items.Count; i++)
          {
            WINAMP_DSP dspFound = (WINAMP_DSP)listBoxFoundWAPlugins.Items[i];
            if (dspSelected.file == dspFound.file)
            {
              listBoxFoundWAPlugins.Items.RemoveAt(i);
              break;
            }
          }
        }
      }
    }

    private void buttonWAAdd_Click(object sender, EventArgs e)
    {
      // Get the winamp handle and enable it
      WINAMP_DSP dsp = (WINAMP_DSP)listBoxFoundWAPlugins.SelectedItem;
      _waDspPlugin = BassWa.BASS_WADSP_Load(dsp.file, 5, 5, 100, 100, null);
      if (_waDspPlugin > 0)
      {
        _waDspPlugins[dsp.file] = _waDspPlugin;
        listBoxSelectedWAPlugins.Items.Add(listBoxFoundWAPlugins.SelectedItem);
        listBoxFoundWAPlugins.Items.RemoveAt(listBoxFoundWAPlugins.SelectedIndex);
      }
      else
      {
        MessageBox.Show("Error loading WinAmp Plugin. Probably not valid", "WinAmp Plugin", MessageBoxButtons.OK);
        Log.Debug("Couldn't load WinAmp Plugin {0}. Error code: {1}", listBoxFoundWAPlugins.SelectedItem, Bass.BASS_ErrorGetCode());
      }
    }

    private void buttonWARemove_Click(object sender, EventArgs e)
    {
      // Remove Winamp Handle
      WINAMP_DSP dsp = (WINAMP_DSP)listBoxSelectedWAPlugins.SelectedItem;
      BassWa.BASS_WADSP_FreeDSP(_waDspPlugins[dsp.file]);
      _waDspPlugins.Remove(listBoxSelectedWAPlugins.Text);
      listBoxFoundWAPlugins.Items.Add(listBoxSelectedWAPlugins.SelectedItem);
      listBoxSelectedWAPlugins.Items.RemoveAt(listBoxSelectedWAPlugins.SelectedIndex);
    }

    private void listBoxSelectedWAPlugins_DoubleClick(object sender, EventArgs e)
    {
      WINAMP_DSP dsp = (WINAMP_DSP)listBoxSelectedWAPlugins.SelectedItem;
      _waDspPlugin = _waDspPlugins[dsp.file];
      BassWa.BASS_WADSP_Config(_waDspPlugin, 0);
    }
    #endregion WinAmpPlugins

    #region DSP Gain
    private void buttonSetGain_Click(object sender, System.EventArgs e)
    {
      if (_gain == null)
        _gain = new DSP_Gain();
      
      try
      {
        double gainDB = double.Parse(this.textBoxGainDBValue.Text);
        trackBarGain.Value = (int)(gainDB * 1000d);
        SetDSPGain(gainDB);
      }
      catch { }
    }

    private void trackBarGain_ValueChanged(object sender, System.EventArgs e)
    {
      if (_gain == null)
        _gain = new DSP_Gain();

      this.textBoxGainDBValue.Text = Convert.ToString(trackBarGain.Value / 1000d);
      buttonSetGain_Click(this, EventArgs.Empty);
    }

    private void SetDSPGain(double gainDB)
    {
      if (_stacker == null)
        _stacker = new DSP_Stacker();

      if (_gain == null)
        _gain = new DSP_Gain();

      if (gainDB == 0.0)
        _gain.SetBypass(true);
      else
      {
        _gain.SetBypass(false);
        _gain.Gain_dBV = gainDB;
      }

      // Do we have the gain already in the stacker?
      if (_stacker.IndexOf(_gain) == -1)
        _stacker.Add(_gain);
    }
    #endregion DSP Gain

    #region Dynamic Amplification
    private void checkBoxDAmp_CheckedChanged(object sender, System.EventArgs e)
    {
      comboBoxDynamicAmplification.Enabled = checkBoxDAmp.Checked;
      if (comboBoxDynamicAmplification.SelectedIndex == -1)
        comboBoxDynamicAmplification.SelectedIndex = 0;
      
      if (_stream == 0)
        return;

      if (checkBoxDAmp.Checked)
      {
        SetDAmpPreset(comboBoxDynamicAmplification.SelectedIndex);
        BassFx.BASS_FX_DSP_Set(_stream, BASSFXDsp.BASS_FX_DSPFX_DAMP, _dampPrio);
        BassFx.BASS_FX_DSP_SetParameters(_stream, _damp);
      }
      else
      {
        BassFx.BASS_FX_DSP_Remove(_stream, BASSFXDsp.BASS_FX_DSPFX_DAMP);
      }
    }

    private void SetDAmpPreset(int preset)
    {
      if (_damp == null)
        _damp = new BASS_FX_DSPDAMP();

      switch (preset)
      {
        case 0:
          _damp.Preset_Soft();
          break;
        case 1:
          _damp.Preset_Medium();
          break;
        case 2:
          _damp.Preset_Hard();
          break;
      }
    }
    #endregion Dynamic Amplification

    #region Compressor

    private void checkBoxCompressor_CheckedChanged(object sender, System.EventArgs e)
    {
      if (_comp == null)
        _comp = new BASS_FX_DSPCOMPRESSOR();

      if (_stream == 0)
        return;

      if (checkBoxCompressor.Checked)
      {
        BassFx.BASS_FX_DSP_Set(_stream, BASSFXDsp.BASS_FX_DSPFX_COMPRESSOR, _compPrio);
        BassFx.BASS_FX_DSP_SetParameters(_stream, _comp);

      }
      else
      {
        BassFx.BASS_FX_DSP_Remove(_stream, BASSFXDsp.BASS_FX_DSPFX_COMPRESSOR);
      }
    }

    private void trackBarCompressor_ValueChanged(object sender, System.EventArgs e)
    {
      labelCompThreshold.Text = String.Format("Threshold: {0:#0.0} dB", trackBarCompressor.Value / 10d);

      if (_stream == 0)
        return;

      _comp.fThreshold = (float)Un4seen.Bass.Utils.DBToLevel(trackBarCompressor.Value / 10d, 1.0);
      BassFx.BASS_FX_DSP_SetParameters(_stream, _comp);
    }

    #endregion Compressor
    #endregion

    #region SectionSettings Overloads
    public override void OnSectionActivated()
    {
      //
      // Disable the complete Tab Page, when the Music Player is not the BASS engine
      //
      SectionSettings section = SectionSettings.GetSection("Music");

      if (section != null)
      {
        string player = (string)section.GetSetting("audioPlayer");
        if (player.IndexOf("BASS") == -1)
        {
          MusicDSPTabCtl.Enabled = false;
          MessageBox.Show(this, "DSP effects are only available with the BASS music player selected.",
              "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        else
        {
          MusicDSPTabCtl.Enabled = true;
        }
      }
    }

    /// <summary>
    /// Load Effects Settings
    /// </summary>
    public override void LoadSettings()
    {
      // BASS DSP/FX
      foreach (BassEffect basseffect in Settings.Instance.BassEffects)
      {
        foreach (BassEffectParm parameter in basseffect.Parameter)
        {
          setBassDSP(basseffect.EffectName, parameter.Name, parameter.Value);
        }
      }

      // VST Plugins
      textBoxVSTPluginDir.Text = Settings.Instance.VSTPluginDirectory;
      foreach (VSTPlugin plugins in Settings.Instance.VSTPlugins)
      {
        // Get the vst handle and enable it
        string plugin = String.Format(@"{0}\{1}", textBoxVSTPluginDir.Text, plugins.PluginDll);
        _vstHandle = BassVst.BASS_VST_ChannelSetDSP(0, plugin, BASSVSTDsp.BASS_VST_DEFAULT, 1);
        if (_vstHandle > 0)
        {
          listBoxSelectedVSTPlugins.Items.Add(plugins.PluginDll);
          _vstHandles[plugins.PluginDll] = _vstHandle;
          // Set all parameters for the plugin
          foreach (VSTPluginParm paramter in plugins.Parameter)
          {
            System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
            try
            {
              BassVst.BASS_VST_SetParam(_vstHandle, paramter.Index, float.Parse(paramter.Value));
            }
            catch (Exception)
            { }
          }
        }
        else
        {
          Log.Debug("Couldn't load VST Plugin {0}. Error code: {1}", plugin, Bass.BASS_ErrorGetCode());
        }
      }

      // WinAmp Plugins
      textBoxWAPluginDir.Text = Settings.Instance.WinAmpPluginDirectory;

      // Get the available plugins in the directory and fill the found listbox
      if (Directory.Exists(textBoxWAPluginDir.Text))
      {       
        WINAMP_DSP[] dsps = BassWa.BASS_WADSP_FindPlugins(Settings.Instance.WinAmpPluginDirectory);
        listBoxFoundWAPlugins.Items.AddRange(dsps);
        // Now remove those already selected from the found listbox
        foreach (WinAmpPlugin plugins in Settings.Instance.WinAmpPlugins)
        {
          for (int i = 0; i < listBoxFoundWAPlugins.Items.Count; i++)
          {
            WINAMP_DSP dsp = (WINAMP_DSP)listBoxFoundWAPlugins.Items[i];
            if (dsp.file == plugins.PluginDll)
            {
              listBoxFoundWAPlugins.Items.RemoveAt(i);
              _waDspPlugin = BassWa.BASS_WADSP_Load(plugins.PluginDll, 5, 5, 100, 100, null);
              if (_waDspPlugin > 0)
              {
                listBoxSelectedWAPlugins.Items.Add(dsp);
                _waDspPlugins[plugins.PluginDll] = _waDspPlugin;
                break;
              }
              else
              {
                Log.Debug("Couldn't load WinAmp Plugin {0}. Error code: {1}", plugins.PluginDll, Bass.BASS_ErrorGetCode());
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Save Effects Settings
    /// </summary>
    public override void SaveSettings()
    {
      // Settings for BASS DSP/FX
      Settings.Instance.BassEffects.Clear();
      BassEffect basseffect;

      // Gain
      if (textBoxGainDBValue.Text != "0")
      {
        basseffect = new BassEffect();
        basseffect.EffectName = "Gain";
        basseffect.Parameter.Add(new BassEffectParm("Gain_dbV", textBoxGainDBValue.Text));
        Settings.Instance.BassEffects.Add(basseffect);
      }

      // Dynamic Amplification
      if (checkBoxDAmp.Checked)
      {
        basseffect = new BassEffect();
        basseffect.EffectName = "DynAmp";
        basseffect.Parameter.Add(new BassEffectParm("Preset", comboBoxDynamicAmplification.SelectedIndex.ToString()));
        Settings.Instance.BassEffects.Add(basseffect);
      }
 
      // Compressor
      if (checkBoxCompressor.Checked)
      {
        basseffect = new BassEffect();
        basseffect.EffectName = "Compressor";
        basseffect.Parameter.Add(new BassEffectParm("Threshold", trackBarCompressor.Value.ToString()));
        Settings.Instance.BassEffects.Add(basseffect);
      }

      // Settings for VST Plugings
      Settings.Instance.VSTPluginDirectory = textBoxVSTPluginDir.Text;

      // Clear all Settings first
      Settings.Instance.VSTPlugins.Clear();
      VSTPlugin vstplugin;
      foreach (string plugindll in _vstHandles.Keys)
      {
        vstplugin = new VSTPlugin();
        vstplugin.PluginDll = plugindll;
        _vstHandle = _vstHandles[plugindll];
        BASS_VST_PARAM_INFO paramInfo = new BASS_VST_PARAM_INFO();
        for (int i = BassVst.BASS_VST_GetParamCount(_vstHandle) - 1; i >= 0; i--)
        {
          BassVst.BASS_VST_GetParamInfo(_vstHandle, i, paramInfo);
          float value = BassVst.BASS_VST_GetParam(_vstHandle, i);
          vstplugin.Parameter.Add(new VSTPluginParm(paramInfo.name, i, value.ToString()));
        }
        Settings.Instance.VSTPlugins.Add(vstplugin);
      }

      // Settings for WinAmpPlugins
      Settings.Instance.WinAmpPluginDirectory = textBoxWAPluginDir.Text;
      WinAmpPlugin winampplugin;

      // Clear all settings first
      Settings.Instance.WinAmpPlugins.Clear();
      foreach (WINAMP_DSP wadsp in listBoxSelectedWAPlugins.Items)
      {
        winampplugin = new WinAmpPlugin();
        winampplugin.PluginDll = wadsp.file;
        Settings.Instance.WinAmpPlugins.Add(winampplugin);
      }
      Settings.SaveSettings();
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }

      // Clean up BASS Resources
      try
      {
        // Some Winamp dsps might raise an exception when closing
        BassWa.BASS_WADSP_Free();
      }
      catch
      { }
      Bass.BASS_Stop();
      Bass.BASS_Free();

      base.Dispose(disposing);
    }
    #endregion

  }
}
