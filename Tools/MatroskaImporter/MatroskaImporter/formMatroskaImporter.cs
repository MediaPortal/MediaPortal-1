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
    #region Delegates

    protected delegate void MethodTreeViewTags(Dictionary<string, MatroskaTagInfo> FoundTags);

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
      MatroskaTagHandler.OnTagLookupCompleted += new MatroskaTagHandler.TagLookupSuccessful(OnLookupCompleted);
    }

    #endregion

    #region Fields

    private string fCurrentImportPath;
    public string CurrentImportPath
    {
      get { return fCurrentImportPath; }
      set
      {
        fCurrentImportPath = value;
      }
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
          if (!string.IsNullOrEmpty(tvCard.RecordingFolder) && !cbRecPaths.Items.Contains(tvCard.RecordingFolder))
            cbRecPaths.Items.Add(tvCard.RecordingFolder);
        }
        if (cbRecPaths.Items.Count > 0)
        {
          cbRecPaths.SelectedIndex = 0;
          cbRecPaths_TextUpdate(this, null);
        }
      }
      catch (Exception ex) 
      {
        MessageBox.Show(string.Format("Error gathering recording folders of all tv cards: \n{0}", ex.Message));
      }
    }

    #endregion

    #region User input events

    private void btnLookup_Click(object sender, EventArgs e)
    {
      btnLookup.Enabled = false;
      btnImport.Enabled = false;
      if (cbTagRecordings.Checked)
      {
        try
        {
          GetTagFiles();
        }
        catch (Exception ex2)
        {
          MessageBox.Show(string.Format("Error gathering matroska tags: \n{0}", ex2.Message));
        }
      }
      else      
        OnLookupCompleted(new Dictionary<string, MatroskaTagInfo>());      

      if (cbDbRecordings.Checked)
      {
        try
        {
          GetRecordings();
        }
        catch (Exception ex)
        {
          MessageBox.Show(string.Format("Error gathering recording informations: \n{0}", ex.Message));
        }
      }
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
      foreach (TreeNode tagRec in tvTagRecs.Nodes)
      {
        Recording currentTagRec = tagRec.Tag as Recording;
        if (currentTagRec != null)
        {
          bool RecFileFound = false;
          bool AskForImport = cbConfirmImport.Checked;
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
            if (!AskForImport || MessageBox.Show(this, string.Format("Import {0} now? \n{1}", currentTagRec.Title, currentTagRec.FileName), "Recording not found in DB", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
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
      CurrentImportPath = cbRecPaths.Text;
      if (fCurrentImportPath[fCurrentImportPath.Length - 1] != '\\' && fCurrentImportPath[fCurrentImportPath.Length - 1] != '/')
        CurrentImportPath += @"\";

      btnLookup.Enabled = Directory.Exists(CurrentImportPath);
    }

    private void MatroskaImporter_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        cbUseThread.Visible = !cbUseThread.Visible;
        cBSortCulture.Visible = !cBSortCulture.Visible;
      }
    }

    #endregion

    #region Tag retrieval

    private void GetTagFiles()
    {
      Dictionary<string, MatroskaTagInfo> importTags = new Dictionary<string, MatroskaTagInfo>();
      try
      {
        if (cbUseThread.Checked)
        {
          Thread lookupThread = new Thread(new ParameterizedThreadStart(MatroskaTagHandler.GetAllMatroskaTags));
          lookupThread.Start((object)CurrentImportPath);
          lookupThread.IsBackground = true;
        }
        else
        {
          MatroskaTagHandler.GetAllMatroskaTags((object)CurrentImportPath);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void OnLookupCompleted(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      if (cbUseThread.Checked)
        Invoke(new MethodTreeViewTags(AddTagFiles), new object[] { FoundTags });
      else
        AddTagFiles(FoundTags);
    }
    
    /// <summary>
    /// Invoke method from MethodTreeViewTags delegate!!!
    /// </summary>
    /// <param name="FoundTags"></param>
    private void AddTagFiles(Dictionary<string, MatroskaTagInfo> FoundTags)
    {
      tvTagRecs.Nodes.Clear();
      tvTagRecs.BeginUpdate();
      foreach (KeyValuePair<string, MatroskaTagInfo> kvp in FoundTags)
      {
        Recording TagRec = BuildRecordingFromTag(kvp.Key, kvp.Value);
        if (TagRec != null)
        {
          TreeNode TagNode = BuildNodeFromRecording(TagRec);
          if (TagNode != null)
            tvTagRecs.Nodes.Add(TagNode);
        }
      }
      if (cBSortCulture.Checked)
        tvTagRecs.TreeViewNodeSorter = new RecordSorter();
      else
        tvTagRecs.TreeViewNodeSorter = new RecordSorterInvariant();
      try
      {
        tvTagRecs.Sort();
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error sorting tag recordings: \n{0}", ex.Message));
      }     
      tvTagRecs.EndUpdate();
      btnLookup.Enabled = true;
      btnImport.Enabled = (tvTagRecs.Nodes.Count > 0);
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
        TreeNode RecNode = BuildNodeFromRecording(rec);
        if (RecNode != null)
          tvDbRecs.Nodes.Add(RecNode);
      }
      if (cBSortCulture.Checked)
        tvDbRecs.TreeViewNodeSorter = new RecordSorter();
      else
        tvDbRecs.TreeViewNodeSorter = new RecordSorterInvariant();

      try
      {
        tvDbRecs.Sort();
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Error sorting db recordings: \n{0}", ex.Message));
      }      
      tvDbRecs.EndUpdate();
    }

    #endregion

    #region Visualisation

    private TreeNode BuildNodeFromRecording(Recording aRec)
    {
      try
      {
        Channel lookupChannel = null;
        string channelId = "unknown";
        string channelName = "unknown";
        string startTime = SqlDateTime.MinValue.Value == aRec.StartTime ? "unknown" : aRec.StartTime.ToString();
        string endTime = SqlDateTime.MinValue.Value == aRec.EndTime ? "unknown" : aRec.EndTime.ToString();
        try
        {
          lookupChannel = (Channel)aRec.ReferencedChannel();
          if (lookupChannel != null)
          {
            channelName = lookupChannel.DisplayName;
            channelId = lookupChannel.IdChannel.ToString();
          }
        }
        catch (Exception)
        {
        }

        TreeNode[] subitems = new TreeNode[] { 
                                               new TreeNode("Channel name: " + channelName), 
                                               new TreeNode("Channel ID: " + channelId), 
                                               new TreeNode("Genre: " + aRec.Genre), 
                                               new TreeNode("Description: " + aRec.Description), 
                                               new TreeNode("Start time: " + startTime), 
                                               new TreeNode("End time: " + endTime), 
                                               new TreeNode("Server ID: " + aRec.IdServer)
                                             };

        TreeNode recItem = new TreeNode(aRec.Title, subitems);
        recItem.Tag = aRec;
        return recItem;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not build TreeNode from recording: {0}\n{1}", aRec.Title, ex.Message));
        return null;
      }      
    }

    #endregion

    #region Tag to recording conversion

    private Recording BuildRecordingFromTag(string aFileName, MatroskaTagInfo aTag)
    {
      Recording tagRec = null;
      try
      {
        string physicalFile = GetRecordingFilename(aFileName);
        tagRec = new Recording(GetChannelIdByDisplayName(aTag.channelName),
                                         GetRecordingStartTime(physicalFile),
                                         GetRecordingEndTime(physicalFile),
                                         aTag.title,
                                         aTag.description,
                                         aTag.genre,
                                         physicalFile,
                                         0,
                                         SqlDateTime.MaxValue.Value,
                                         0,
                                         GetServerId()
                                         );
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not build recording from tag: {0}\n{1}", aFileName, ex.Message));
      }
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

    private DateTime GetRecordingEndTime(string aFileName)
    {
      DateTime endTime = SqlDateTime.MinValue.Value;
      if (File.Exists(aFileName))
      {
        FileInfo fi = new FileInfo(aFileName);
        endTime = fi.LastWriteTime;
      }
      return endTime;
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
        MessageBox.Show(string.Format("Could not get ServerID for recording!\n{0}", ex.Message));
      }
      return serverId;
    }

    private int GetChannelIdByDisplayName(string aChannelName)
    {
      int channelId = -1;
      if (string.IsNullOrEmpty(aChannelName))
        return channelId;
      try
      {
        SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "displayName", aChannelName);
        sb.SetRowLimit(1);
        SqlStatement stmt = sb.GetStatement(true);
        IList channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        if (channels.Count == 1)
          channelId = ((Channel)channels[0]).IdChannel;
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not get ChannelID for DisplayName: {0}\n{1}", aChannelName, ex.Message));
      }
      return channelId;
    }

    #endregion

  }
}