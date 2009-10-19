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
    public partial class ImageRadioSelector : BaseHorizontalLayout, ISectionPanel
    {

        //private PackageClass Package;
        //private SectionResponseEnum _resp = SectionResponseEnum.Cancel;

        private const string CONST_IMAGE_1 = "First option Image file";
        private const string CONST_IMAGE_2 = "Second option Image file";
        private const string CONST_TEXT = "Description ";

        public ImageRadioSelector()
        {
            InitializeComponent();
        }

        #region ISectionPanel Members


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
            SectionParamCollection _param = new SectionParamCollection(Params);

            _param.Add(new SectionParam(CONST_IMAGE_1, "", ValueTypeEnum.File,
                                             "The file of first option. Idicated size (225,127)"));
            _param.Add(new SectionParam(CONST_IMAGE_2, "", ValueTypeEnum.File,
                                             "The file of first option. Idicated size (225,127)"));
            _param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String,
                                             "Description of this operation"));
            return _param;
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
            Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.BeforPanelShow);
            Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelShow);
            ShowDialog();
            Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelHide);
            return base.Resp;
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



        #region ISectionPanel Members

        public string DisplayName
        {
            get { return "Image Radio button Section"; }
        }

        public string Guid
        {
            get { return "{3BD8934A-FEFD-41a7-9F12-B30DABEF556B}"; }
        }

        #endregion
    }
}
