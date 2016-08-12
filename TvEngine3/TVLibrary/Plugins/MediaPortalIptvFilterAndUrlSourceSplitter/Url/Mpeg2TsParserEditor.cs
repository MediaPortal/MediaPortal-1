using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections;
using System.Globalization;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    internal partial class Mpeg2TsParserEditor : Form
    {
        public Mpeg2TsParserEditor()
        {
            InitializeComponent();

            this.checkBoxAlignToMpeg2TsPacket_Enter(null, null);
            this.Sections = new StreamSectionCollection();
            this.FilterProgramNumbers = new FilterProgramNumberCollection();
        }

        public Boolean AlignToMpeg2TSPacket { get; set; }
        public Boolean DetectDiscontinuity { get; set; }
        public Boolean SetNotScrambled { get; set; }
        public int TransportStreamID { get; set; }
        public int ProgramNumber { get; set; }
        public int ProgramMapPID { get; set; }
        public FilterProgramNumberCollection FilterProgramNumbers { get; private set; }

        internal StreamSectionCollection Sections { get; private set; }

        private void Mpeg2TsParserEditor_Shown(object sender, EventArgs e)
        {
            this.checkBoxAlignToMpeg2TsPacket.Checked = this.AlignToMpeg2TSPacket;
            this.checkBoxDetectDiscontinuity.Checked = this.DetectDiscontinuity;
            this.checkBoxSetNotScrambled.Checked = this.SetNotScrambled;

            this.textBoxTransportStreamId.Text = (this.TransportStreamID == Mpeg2TsParser.DefaultMpeg2TsTransportStreamID) ? String.Empty : this.TransportStreamID.ToString() ;
            this.textBoxProgramNumber.Text = (this.ProgramNumber == Mpeg2TsParser.DefaultMpeg2TsProgramNumber) ? String.Empty : this.ProgramNumber.ToString();
            this.textBoxProgramMapPID.Text = (this.ProgramMapPID == Mpeg2TsParser.DefaultMpeg2TsProgramMapPID) ? String.Empty : this.ProgramMapPID.ToString();

            foreach (var section in this.Sections)
            {
                TreeNode node = new TreeNode(section.Section.ShortName);
                node.Tag = section;

                ProgramAssociationSection pat = section.Section as ProgramAssociationSection;
                if (pat != null)
                {
                    if (pat.Programs.Count > 1)
                    {
                        labelForceStreamIdentification.Text = "It is not recommended to force stream identification. In program association section (PAT) are found at least two programs. If stream identification is forced, then filter fails to open stream.";
                        labelForceStreamIdentification.ForeColor = Color.Red;
                    }

                    for (int i = 0; i < pat.Programs.Count; i++)
                    {
                        TreeNode temp = new TreeNode(TransportStreamProgramMapSection.GetShortName(pat.Programs[i].ProgramMapPID, pat.Programs[i].ProgramNumber));
                        temp.Name = TransportStreamProgramMapSection.GetShortName(pat.Programs[i].ProgramMapPID, pat.Programs[i].ProgramNumber);

                        node.Nodes.Add(temp);

                        Boolean found = false;
                        foreach (var filterProgramNumber in this.FilterProgramNumbers)
                        {
                            if (filterProgramNumber.ProgramNumber == (int)pat.Programs[i].ProgramNumber)
                            {
                                found = true;

                                foreach (var patInternalSection in section.StreamSections)
                                {
                                    TransportStreamProgramMapSection pmt = patInternalSection.Section as TransportStreamProgramMapSection;
                                    if ((pmt != null) && (pmt.ProgramNumber == (uint)filterProgramNumber.ProgramNumber))
                                    {
                                        foreach (var programDefinition in pmt.Programs)
                                        {
                                            Boolean foundProgramElement = false;
                                            foreach (var programElement in filterProgramNumber.ProgramElements)
                                            {
                                                if (programElement.ProgramElementPID == (int)programDefinition.ElementaryPID)
                                                {
                                                    foundProgramElement = true;
                                                    break;
                                                }
                                            }

                                            if (!foundProgramElement)
                                            {
                                                filterProgramNumber.ProgramElements.Add(new ProgramElement() { ProgramElementPID = (int)programDefinition.ElementaryPID, LeaveProgramElement = false });
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }

                        if (!found)
                        {
                            FilterProgramNumber filterProgramNumber = new FilterProgramNumber((int)pat.Programs[i].ProgramNumber);

                            foreach (var patInternalSection in section.StreamSections)
                            {
                                TransportStreamProgramMapSection pmt = patInternalSection.Section as TransportStreamProgramMapSection;
                                if ((pmt != null) && (pmt.ProgramNumber == (uint)filterProgramNumber.ProgramNumber))
                                {
                                    foreach (var programDefinition in pmt.Programs)
                                    {
                                        filterProgramNumber.ProgramElements.Add(new ProgramElement() { ProgramElementPID = (int)programDefinition.ElementaryPID, LeaveProgramElement = false });
                                    }
                                }
                            }

                            this.FilterProgramNumbers.Add(filterProgramNumber);
                        }
                    }

                    foreach (var patInternalSection in section.StreamSections)
                    {
                        TransportStreamProgramMapSection pmt = patInternalSection.Section as TransportStreamProgramMapSection;
                        if (pmt != null)
                        {
                            // try to find tree node with same short name

                            TreeNode pmtNode = node.Nodes[patInternalSection.Section.ShortName];
                            pmtNode.Tag = patInternalSection;

                            foreach (var program in pmt.Programs)
                            {
                                TreeNode pmtProgram = new TreeNode(program.ShortName);

                                pmtNode.Nodes.Add(pmtProgram);
                            }
                        }
                        else
                        {
                            TreeNode patInternalSectionNode = new TreeNode(patInternalSection.Section.ShortName);
                            patInternalSectionNode.Tag = patInternalSection;

                            node.Nodes.Add(patInternalSectionNode);
                        }
                    }
                }

                this.treeViewSections.Nodes.Add(node);
            }

            this.RedrawFilterProgramMapPIDs(this.tabControlFilterProgramElements.SelectedIndex, true, false);
        }

        private void checkBoxAlignToMpeg2TsPacket_Enter(object sender, EventArgs e)
        {
            Control control = sender as Control;

            if (control != null)
            {
                String description = control.Tag as String;

                if (String.IsNullOrEmpty(description))
                {
                    this.labelDescription.Text = String.Empty;
                }
                else
                {
                    this.labelDescription.Text = description;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(this.textBoxTransportStreamId.Text))
                {
                    this.TransportStreamID = int.Parse(this.textBoxTransportStreamId.Text);

                    if (((this.TransportStreamID < 0) || (this.TransportStreamID > 65535)))
                    {
                        throw new ArgumentOutOfRangeException("TransportStreamID", this.TransportStreamID, "Must be greater than or equal to zero and lower than 65536.");
                    }
                }
                else
                {
                    this.TransportStreamID = Mpeg2TsParser.DefaultMpeg2TsTransportStreamID;
                    this.errorProvider.SetError(this.textBoxTransportStreamId, String.Empty);
                }
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxTransportStreamId, ex.ToString());
            }

            try
            {
                if (!String.IsNullOrEmpty(this.textBoxProgramNumber.Text))
                {
                    this.ProgramNumber = int.Parse(this.textBoxProgramNumber.Text);

                    if (((this.ProgramNumber < 0) || (this.ProgramNumber > 65535)))
                    {
                        throw new ArgumentOutOfRangeException("ProgramNumber", this.ProgramNumber, "Must be greater than or equal to zero and lower than 65536.");
                    }
                }
                else
                {
                    this.ProgramNumber = Mpeg2TsParser.DefaultMpeg2TsProgramNumber;
                    this.errorProvider.SetError(this.textBoxProgramNumber, String.Empty);
                }
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxProgramNumber, ex.ToString());
            }

            try
            {
                if (!String.IsNullOrEmpty(this.textBoxProgramMapPID.Text))
                {
                    this.ProgramMapPID = int.Parse(this.textBoxProgramMapPID.Text);

                    if (((this.ProgramMapPID < 0) || (this.ProgramMapPID > 0x1FFF)))
                    {
                        throw new ArgumentOutOfRangeException("ProgramMapPID", this.ProgramMapPID, "Must be greater than or equal to zero and lower than 8192.");
                    }
                }
                else
                {
                    this.ProgramMapPID = Mpeg2TsParser.DefaultMpeg2TsProgramMapPID;
                    this.errorProvider.SetError(this.textBoxProgramMapPID, String.Empty);
                }
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxProgramMapPID, ex.ToString());
            }

            this.AlignToMpeg2TSPacket = this.checkBoxAlignToMpeg2TsPacket.Checked;
            this.DetectDiscontinuity = this.checkBoxDetectDiscontinuity.Checked;
            this.SetNotScrambled = this.checkBoxSetNotScrambled.Checked;
        }

        private void Mpeg2TsParserEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                bool isError = false;

                isError |= (!String.IsNullOrEmpty(errorProvider.GetError(this.textBoxTransportStreamId)));
                isError |= (!String.IsNullOrEmpty(errorProvider.GetError(this.textBoxProgramNumber)));
                isError |= (!String.IsNullOrEmpty(errorProvider.GetError(this.textBoxProgramMapPID)));

                e.Cancel = isError;
            }
        }

        private String[] DumpBinaryData(Byte[] data)
        {
            // every byte is in HEX encoding plus space
            // every 16 bytes is new line

            List<String> lines = new List<String>();

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
			{
                builder.AppendFormat("{0:X2} ", data[i]);

                if (((i % 16) == 0x0F) && (i!= (data.Length - 1)))
                {
                    lines.Add(builder.ToString());
                    builder.Length = 0;
                }
			}

            if (builder.Length != 0)
            {
                lines.Add(builder.ToString());
            }

            return lines.ToArray();
        }

        private void treeViewSections_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                StreamSection streamSection = e.Node.Tag as StreamSection;

                if (streamSection != null)
                {
                    this.textBoxSectionData.Lines = streamSection.Section.ToHumanReadable(String.Empty).ToArray();
                    this.textBoxRawSectionData.Lines = DumpBinaryData(streamSection.Section.Payload);
                }
                else
                {
                    this.textBoxSectionData.Lines = new String[0];
                    this.textBoxRawSectionData.Lines = new String[0];
                }
            }
            else
            {
                this.textBoxSectionData.Lines = new String[0];
                this.textBoxRawSectionData.Lines = new String[0];
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxTransportStreamProgramMapSectionProgramNumber.Text))
            {
                try
                {
                    int programNumber = int.Parse(textBoxTransportStreamProgramMapSectionProgramNumber.Text, CultureInfo.CurrentUICulture);
                    bool found = false;

                    foreach (var filterProgramNumber in this.FilterProgramNumbers)
                    {
                        found |= (filterProgramNumber.ProgramNumber == programNumber);
                    }

                    if (!found)
                    {
                        this.FilterProgramNumbers.Add(new FilterProgramNumber(programNumber));

                        this.RedrawFilterProgramMapPIDs(this.tabControlFilterProgramElements.SelectedIndex, true, false);
                    }
                }
                catch
                {
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (this.tabControlFilterProgramElements.SelectedIndex != (-1))
            {
                this.FilterProgramNumbers.RemoveAt(this.tabControlFilterProgramElements.SelectedIndex);

                this.RedrawFilterProgramMapPIDs(this.tabControlFilterProgramElements.SelectedIndex, false, true);
            }
        }

        private void RedrawFilterProgramMapPIDs(int activeTab, bool adding, bool removing)
        {
            if (adding)
            {
                for (int i = 0; i < this.FilterProgramNumbers.Count; i++)
                {
                    FilterProgramNumber filterProgramNumber = this.FilterProgramNumbers[i];
                    if (i >= this.tabControlFilterProgramElements.TabPages.Count)
                    {
                        String name = String.Format("Program number {0} (0x{0:X4})", filterProgramNumber.ProgramNumber);
                        TabPage addedPage = new TabPage(name);

                        addedPage.Name = name;
                        addedPage.Padding = new System.Windows.Forms.Padding(3);
                        addedPage.Size = new System.Drawing.Size(571, 262);
                        addedPage.UseVisualStyleBackColor = true;

                        FilterProgramNumberControl control = new FilterProgramNumberControl();

                        control.Name = "ProgramNumber";
                        control.Dock = System.Windows.Forms.DockStyle.Fill;
                        control.Location = new System.Drawing.Point(3, 3);

                        control.FilterProgramNumber = filterProgramNumber;
                        addedPage.Controls.Add(control);

                        this.tabControlFilterProgramElements.TabPages.Add(addedPage);
                    }
                }
            }

            if (removing)
            {
                this.tabControlFilterProgramElements.TabPages.Remove(this.tabControlFilterProgramElements.TabPages[activeTab]);
            }
        }
    }
}
