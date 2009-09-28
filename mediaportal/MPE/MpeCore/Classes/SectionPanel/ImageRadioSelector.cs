using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.SectionPanel
{
    public partial class ImageRadioSelector : Form, ISectionPanel
    {
        private ShowModeEnum Mode = ShowModeEnum.Preview;
        private SectionItem Section = new SectionItem();
        private PackageClass Package;
        private SectionResponseEnum _resp = SectionResponseEnum.Cancel;

        private const string CONST_IMAGE_1 = "First option Image file";
        private const string CONST_IMAGE_2 = "Second option Image file";
        private const string CONST_TEXT = "Description ";

        public ImageRadioSelector()
        {
            InitializeComponent();
        }

        #region ISectionPanel Members


        public SectionParamCollection Params { get; set; }

        public bool Unique
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private void SetValues()
        {
            if (File.Exists(Section.Params[CONST_IMAGE_1].Value))
                pictureBox1.LoadAsync(Section.Params[CONST_IMAGE_1].Value);
            if (File.Exists(Section.Params[CONST_IMAGE_1].Value))
                pictureBox3.LoadAsync(Section.Params[CONST_IMAGE_2].Value);
            label1.Text = Section.Params[CONST_TEXT].Value;
            if (Section.IncludedGroups.Count > 1)
            {
                radioButton1.Checked = Package.Groups[Section.IncludedGroups[0]].Checked;
                radioButton1.Tag = Package.Groups[Section.IncludedGroups[0]];
                radioButton1.Text = Package.Groups[Section.IncludedGroups[0]].DisplayName;
                radioButton2.Checked = Package.Groups[Section.IncludedGroups[1]].Checked;
                radioButton2.Tag = Package.Groups[Section.IncludedGroups[1]];
                radioButton2.Text = Package.Groups[Section.IncludedGroups[1]].DisplayName;
            }
        }

        public SectionParamCollection Init()
        {
            throw new NotImplementedException();
        }

        public SectionParamCollection GetDefaultParams()
        {
            SectionParamCollection param = new SectionParamCollection();
            param.Add(new SectionParam(CONST_IMAGE_1, "", ValueTypeEnum.File,
                                       "The file of first option. Idicated size (225,127)"));
            param.Add(new SectionParam(CONST_IMAGE_2, "", ValueTypeEnum.File,
                                       "The file of first option. Idicated size (225,127)"));
            param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String,
                                       "Description of this operation"));
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Preview;
            Section = sectionItem;
            Package = packageClass;
            SetValues();
            ShowDialog();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Real;
            Package = packageClass;
            Section = sectionItem;
            SetValues();
            ShowDialog();
            return _resp;
        }

        #endregion

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (Mode == ShowModeEnum.Preview)
                return;

            if (Section.IncludedGroups.Count > 1)
            {
                Package.Groups[Section.IncludedGroups[0]].Checked = radioButton1.Checked;
                Package.Groups[Section.IncludedGroups[1]].Checked = radioButton2.Checked;
            }
        }

        private void button_back_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Back;
            this.Close();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Next;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Cancel;
            this.Close();
        }
    
    }
}
