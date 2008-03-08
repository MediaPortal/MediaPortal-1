#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections;
using System.Data.SqlTypes;
using Gentle.Framework;
using TvDatabase;

#endregion

namespace MatroskaImporter
{
  public partial class MatroskaImporter : Form
  {
    #region Event delegates

    public delegate void TagLookupSuccessful(Dictionary<string, MatroskaTagInfo> FoundTags);
    public event TagLookupSuccessful OnTagLookupCompleted;

    #endregion

    #region Constructor

    public MatroskaImporter()
    {
      InitializeComponent();

      try
      {
        LoadSettings();        
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Loading of settings failed. Make sure you're running this tool from your TVE3 installation dir and gentle.NET is able to contact to your TvLibrary! \n\n{0}", ex.Message));
        this.Close();
      }
      this.OnTagLookupCompleted += new TagLookupSuccessful(OnLookupCompleted);
    }

    #endregion

    #region Settings

    private void LoadSettings()
    {
      try
      {
        IList allCards = Card.ListAll();
        foreach (Card tvCard in allCards)
        {
          if (!cbRecPaths.Items.Contains(tvCard.RecordingFolder))
            cbRecPaths.Items.Add(tvCard.RecordingFolder);
        }
        if (cbRecPaths.Items.Count > 0)
          cbRecPaths.SelectedIndex = 0;
      }
      catch (Exception) {}
    }

    #endregion
    
    #region User input events

    private void btnLookup_Click(object sender, EventArgs e)
    {
      btnLookup.Enabled = false;
      try
      {
        GetRecordings();

        GetTagFiles();

        btnImport.Enabled = (tvTagRecs.Nodes.Count > 0);
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error gathering recording informations: \n{0}", ex.Message));
      }
      btnLookup.Enabled = true;
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
      foreach (TreeNode tagRec in tvTagRecs.Nodes)
      {
        Recording currentTagRec = tagRec.Tag as Recording;
        if (currentTagRec != null)
        {
          bool RecFileFound = false;
          foreach (TreeNode dbRec in tvDbRecs.Nodes)
          {
            Recording currentDbRec = dbRec.Tag as Recording;
            if (currentDbRec != null)
            {
              if (Path.GetFileNameWithoutExtension(currentDbRec.FileName) == Path.GetFileNameWithoutExtension(currentTagRec.FileName))
                RecFileFound = true;
            }
          }
          if (!RecFileFound)
          {
            if (MessageBox.Show(this, string.Format("Import {0} now? \n{1}", currentTagRec.Title, currentTagRec.FileName), "Recording not found in DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
              try
              {
                currentTagRec.Persist();
              }
              catch (Exception ex)
              {
                MessageBox.Show(string.Format("Importing failed: {0}", ex.Message), "Could not import", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
          }
        }
      }
      GetRecordings();
    }

    private void btnExit_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void cbRecPaths_TextUpdate(object sender, EventArgs e)
    {
      btnLookup.Enabled = Directory.Exists(cbRecPaths.Text);
    }

    #endregion
    
    #region Tag retrieval

    private void GetTagFiles()
    {
      Dictionary<string, MatroskaTagInfo> importTags = new Dictionary<string, MatroskaTagInfo>();
      try
      {
        importTags = MatroskaTagHandler.GetAllMatroskaTags(cbRecPaths.Text);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }

      if (OnTagLookupCompleted != null)
        OnTagLookupCompleted(importTags);
    }

    private void OnLookupCompleted(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      tvTagRecs.Nodes.Clear();
      tvTagRecs.BeginUpdate();
      foreach (KeyValuePair<string, MatroskaTagInfo> kvp in FoundTags)
      {
        tvTagRecs.Nodes.Add(BuildNodeFromRecording(BuildRecordingFromTag(kvp.Key, kvp.Value)));
      }
      tvTagRecs.EndUpdate();
    }

    #endregion
    
    #region Recording retrieval

    private void GetRecordings()
    {
      tvDbRecs.Nodes.Clear();
      tvDbRecs.BeginUpdate();
      IList recordings = Recording.ListAll();
      foreach (Recording rec in recordings)
      {
        tvDbRecs.Nodes.Add(BuildNodeFromRecording(rec));
      }
      tvDbRecs.EndUpdate();
    }

    #endregion

    #region Visualisation

    private TreeNode BuildNodeFromRecording(Recording aRec)
    {
      TreeNode[] subitems = new TreeNode[] { new TreeNode(((Channel)aRec.ReferencedChannel()).DisplayName), new TreeNode("ChannelID: " + aRec.IdChannel.ToString()), new TreeNode(aRec.Genre), new TreeNode(aRec.Description), new TreeNode("ServerID: " + aRec.IdServer) };
      TreeNode recItem = new TreeNode(aRec.Title, subitems);
      recItem.Tag = aRec;

      return recItem;
    }

    #endregion

    #region Tag to recording conversion

    private Recording BuildRecordingFromTag(string aFileName, MatroskaTagInfo aTag)
    {
      string physicalFile = GetRecordingFilename(aFileName);
      Recording tagRec = new Recording(GetChannelIdByDisplayName(aTag.channelName),
                                       GetRecordingStartTime(physicalFile),
                                       GetRecordingStartTime(physicalFile),
                                       aTag.title,
                                       aTag.description,
                                       aTag.genre,
                                       physicalFile,
                                       0,
                                       SqlDateTime.MaxValue.Value,
                                       0,
                                       GetServerId()
                                       );

      return tagRec;
    }

    private DateTime GetRecordingStartTime(string aFileName)
    {
      DateTime startTime = SqlDateTime.MinValue.Value;
      if (File.Exists(aFileName))
      {
        FileInfo fi = new FileInfo(aFileName);
        startTime = fi.CreationTime;
      }
      return startTime;
    }

    private string GetRecordingFilename(string aTagFilename)
    {
      string recordingFile = Path.ChangeExtension(aTagFilename, ".ts");
      try
      {
        string[] validExtensions = new string[] { ".ts", ".mpg" };
        foreach (string ext in validExtensions)
        {
          string[] lookupFiles = Directory.GetFiles(Path.GetDirectoryName(aTagFilename), string.Format("{0}{1}", Path.GetFileNameWithoutExtension(aTagFilename), ext), SearchOption.TopDirectoryOnly);
          if (lookupFiles.Length == 1)
          {
            recordingFile = lookupFiles[0];
            return recordingFile;
          }
        }
      }
      catch (Exception)
      {
      }
      return recordingFile;
    }

    private int GetServerId()
    {
      int serverId = 1;
      try
      {
        string localHost = System.Net.Dns.GetHostName();
        IList dbsServers = Server.ListAll();
        foreach (Server computer in dbsServers)
        {
          if (computer.HostName.ToLower() == localHost.ToLower())
            serverId = computer.IdServer;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      return serverId;
    }

    private int GetChannelIdByDisplayName(string aChannelName)
    {
      int channelId = -1;
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
      sb.AddConstraint(Operator.Equals, "displayName", aChannelName);
      sb.SetRowLimit(1);
      SqlStatement stmt = sb.GetStatement(true);
      IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
      if (channels.Count == 1)
        channelId = ((Channel)channels[0]).IdChannel;

      return channelId;
    }

    #endregion

  }
}