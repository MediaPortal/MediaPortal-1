using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace TsPacketChecker
{

  public partial class Form1 : Form
  {
    private bool stopThread;
    private BufferedTsFileReader reader;

    public Form1()
    {
      InitializeComponent();
      CheckForIllegalCrossThreadCalls = false;
    }

    #region helper functions
    protected delegate void MethodAddSectionNode(TreeNode sectionNode);
    void AddSectionNode(TreeNode sectionNode)
    {
      TrSections.Nodes.Add(sectionNode);
    }
    private void AddThreadSafeSectionNode(TreeNode node)
    {
      Invoke(new MethodAddSectionNode(AddSectionNode), new Object[] { node });
    }
    private void WriteLog(string msg)
    {
      edLog.Text += msg + Environment.NewLine;
    }
    #endregion


    #region Form events
    private void btnSelectFile_Click(object sender, EventArgs e)
    {
      if (openDlg.ShowDialog() == DialogResult.OK)
        edTsFile.Text = openDlg.FileName;
    }
    private void btnAnalyze_Click(object sender, EventArgs e)
    {
      stopThread = false;
      Thread worker = new Thread(new ThreadStart(AnalyzeTs));
      worker.Start();
    }
    private void btnStop_Click(object sender, EventArgs e)
    {
      stopThread = true;
    }
    #endregion

    private void AnalyzeTs()
    {
      PrBar.Value = 0;
      WriteLog("Analyzer running...");
      byte[] tsPacket;
      
      reader = new BufferedTsFileReader();
      reader.Open(edTsFile.Text, 50000);
      reader.SeekToFirstPacket();
      TsHeader header;

      TreeNode patNode= new TreeNode("PAT");
      PatParser patParser = new PatParser(patNode);
      TreeNode catNode = new TreeNode("CAT");
      CatParser catParser = new CatParser(catNode);
      TreeNode linkageNode = new TreeNode("ChannelLinkage");
      ChannelLinkageParser linkageParser = new ChannelLinkageParser(linkageNode);
      PacketChecker checker = new PacketChecker(double.Parse(edPcrDiff.Text));
      while (reader.GetNextPacket(out tsPacket, out header))
      {
        checker.ProcessPacket(tsPacket, header);
        if (header.TransportError) continue;
        if (header.Pid >= 0x1FFF) continue;

        if (!patParser.IsReady)
          patParser.OnTsPacket(tsPacket);
        else
        {
          if (!(bool)patNode.Tag)
          {
            WriteLog("- PAT and PMT parsers finished.");
            AddThreadSafeSectionNode(patNode);
            patNode.Tag = true;
          }
        }
        if (!catParser.IsReady)
          catParser.OnTsPacket(tsPacket);
        else
        {
          if (!(bool)catNode.Tag)
          {
            WriteLog("- CAT parser finished.");
            AddThreadSafeSectionNode(catNode);
            catNode.Tag = true;
          }
        }
        linkageParser.OnTsPacket(tsPacket);

        PrBar.Value = reader.GetPositionInPercent();
        if (stopThread) break;
      }
      reader.Close();
      PrBar.Value = 100;
      WriteLog("Finished.");
      AddThreadSafeSectionNode(linkageNode);
      WriteLog(checker.GetStatistics());
      WriteLog(checker.GetErrorDetails());
    }
  }
}