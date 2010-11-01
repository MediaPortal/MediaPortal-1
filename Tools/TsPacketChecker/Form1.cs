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
using System.Xml;

namespace TsPacketChecker
{

  public partial class Form1 : Form
  {
    private bool stopThread;
    private BufferedTsFileReader reader;
    private string caption = "TsPacketChecker (by gemx)";
    private string tsFile="";

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
    protected delegate void MethodSortTreeView();
    void SortTreeView()
    {
      TrSections.TreeViewNodeSorter = new NodeSorter();
      TrSections.Sort();
    }


    private void AddThreadSafeSectionNode(TreeNode node)
    {
      Invoke(new MethodAddSectionNode(AddSectionNode), new Object[] { node });
    }
    private void ThreadSafeSort()
    {
      Invoke(new MethodSortTreeView(SortTreeView));
    }
    public void WriteLog(string msg)
    {
      edLog.Text += msg + Environment.NewLine;
    }
    #endregion

    #region Import/Export functions
    public void TreeViewToXml(String path)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild(xmlDocument.CreateElement("ROOT"));
      XmlRekursivExport(xmlDocument,xmlDocument.DocumentElement, TrSections.Nodes);
      xmlDocument.Save(path);
    }

    public void XmlToTreeView(String path)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(path);
      TrSections.Nodes.Clear();
      XmlRekursivImport(TrSections.Nodes, xmlDocument.DocumentElement.ChildNodes);
    }

    private XmlNode XmlRekursivExport(XmlDocument xmlDocument,XmlNode nodeElement, TreeNodeCollection treeNodeCollection)
    {
      XmlNode xmlNode = null;
      foreach (TreeNode treeNode in treeNodeCollection)
      {
        xmlNode = xmlDocument.CreateElement("TreeViewNode");

        xmlNode.Attributes.Append(xmlDocument.CreateAttribute("value"));
        xmlNode.Attributes["value"].Value = treeNode.Text;


        if (nodeElement != null)
          nodeElement.AppendChild(xmlNode);

        if (treeNode.Nodes.Count > 0)
        {
          XmlRekursivExport(xmlDocument,xmlNode, treeNode.Nodes);
        }
      }
      return xmlNode;
    }

    private void XmlRekursivImport(TreeNodeCollection elem, XmlNodeList xmlNodeList)
    {
      TreeNode treeNode;
      foreach (XmlNode myXmlNode in xmlNodeList)
      {
        treeNode = new TreeNode(myXmlNode.Attributes["value"].Value);

        if (myXmlNode.ChildNodes.Count > 0)
        {
          XmlRekursivImport(treeNode.Nodes, myXmlNode.ChildNodes);
        }
        elem.Add(treeNode);
      }
    }
    #endregion

    #region Form events
    private void opentsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openDlg.Filter = "Ts Files (*.ts)|*.ts";
      if (openDlg.ShowDialog() == DialogResult.OK)
      {
        tsFile = openDlg.FileName;
        this.Text = caption + " - " + tsFile;
      }
    }
    private void importFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openDlg.Filter = "XML Files (*.xml)|*.xml";
      if (openDlg.ShowDialog() == DialogResult.OK)
      {
        tsFile = "";
        this.Text = caption + " - " + openDlg.FileName;
        XmlToTreeView(openDlg.FileName);
      }
    }
    private void exportToXMLToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (saveDlg.ShowDialog() == DialogResult.OK)
      {
        TreeViewToXml(saveDlg.FileName);
        MessageBox.Show("The data has been successfully exported.", "Information");
      }
    }
    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Close();
    }
    private void btnAnalyze_Click(object sender, EventArgs e)
    {
      if (tsFile == "")
        opentsToolStripMenuItem_Click(null, new EventArgs());
      edLog.Text = "";
      TrSections.Nodes.Clear();
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
      if (!System.IO.File.Exists(tsFile))
      {
        MessageBox.Show("The ts file doesn't exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      PrBar.Value = 0;
      WriteLog("Analyzer running...");
      byte[] tsPacket;
      
      reader = new BufferedTsFileReader();
      if (!reader.Open(tsFile, 50000))
      {
        MessageBox.Show("Error opening the ts file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      if (!reader.SeekToFirstPacket())
      {
        MessageBox.Show("No snyc byte found in whole file. Doesn't seem to be a valid ts file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      TsHeader header;

      TreeNode patNode= new TreeNode("PAT");
      PatParser patParser = new PatParser(patNode);
      TreeNode catNode = new TreeNode("CAT");
      CatParser catParser = new CatParser(catNode);
      TreeNode linkageNode = new TreeNode("ChannelLinkage");
      ChannelLinkageParser linkageParser = new ChannelLinkageParser(linkageNode);
      TreeNode sdtNode = new TreeNode("SDT");
      SdtParser sdtParser = new SdtParser(sdtNode);
      TreeNode nitNode = new TreeNode("NIT");
      NITParser nitParser = new NITParser(nitNode);
      PacketChecker checker = new PacketChecker(double.Parse(edPcrDiff.Text));
      EitParser eitParser = new EitParser(this);

      int maxPATPidsCount = 0;
       while (reader.GetNextPacket(out tsPacket, out header))
      {
        checker.ProcessPacket(tsPacket, header);
        if (header.TransportError) continue;
        if (header.Pid >= 0x1FFF) continue;

        List<ushort> streamPids = patParser.GetPmtStreamPids();
        checker.AddPidsToCheck(streamPids);
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
          patParser.Reset();
          if (maxPATPidsCount != streamPids.Count)
          {
            if (maxPATPidsCount > 0)
              WriteLog("- [Warning] Got different number of pmts and pid than in prev. run. prev. max=" + maxPATPidsCount.ToString() + " current=" + streamPids.Count.ToString());
            if (maxPATPidsCount<streamPids.Count)
              maxPATPidsCount = streamPids.Count;
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
        nitParser.OnTsPacket(tsPacket);
        linkageParser.OnTsPacket(tsPacket);
        sdtParser.OnTsPacket(tsPacket);
        eitParser.OnTsPacket(tsPacket);
        PrBar.Value = reader.GetPositionInPercent();
        if (stopThread) break;
      }
      reader.Close();
      PrBar.Value = 100;
      WriteLog("Finished.");
      ThreadSafeSort();
      WriteLog("Incomplete sections=" + PatParser.incompleteSections.ToString());
      WriteLog("max PAT/PMT pid count: " + maxPATPidsCount.ToString());
      if (!(bool)patNode.Tag)
        AddThreadSafeSectionNode(patNode);
      nitNode.Text += " (" + nitParser.GetChannelCount().ToString() + " channels)";
      AddThreadSafeSectionNode(nitNode);
      sdtNode.Text = "SDT (" + sdtParser.GetServiceCount().ToString() + " services)";
      AddThreadSafeSectionNode(sdtNode);
      AddThreadSafeSectionNode(linkageNode);
      WriteLog(checker.GetStatistics());
      WriteLog(checker.GetErrorDetails());
    }
  }
  public class NodeSorter : System.Collections.IComparer
  {
    public int Compare(object x, object y)
    {
      TreeNode tx = x as TreeNode;
      TreeNode ty = y as TreeNode;

      string s1 = tx.Text;
      string s2 = ty.Text;
      if (s1[0] == '#')
      {
        s1 = s1.Remove(0, 1);
        s2 = s2.Remove(0, 1);
        s1 = s1.Trim();
        s2 = s2.Trim();
        int n1 = Int32.Parse(s1.Substring(0, s1.IndexOf(" ")));
        int n2 = Int32.Parse(s2.Substring(0, s2.IndexOf(" ")));
        if (n1 > n2)
          return 1;
        else if (n1 == n2)
          return 0;
        else
          return -1;
      }
      else
        return String.Compare(s1, s2);
    }
  }

}