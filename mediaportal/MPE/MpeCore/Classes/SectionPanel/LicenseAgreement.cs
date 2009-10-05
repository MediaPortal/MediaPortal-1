using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
    public partial class LicenseAgreement :Form,  ISectionPanel
    {
        private PackageClass Package;
        private SectionResponseEnum _resp = SectionResponseEnum.Cancel;


        private const string CONST_TEXT = "License text";
        private const string CONST_TEXT_FILE = "License text file";

        public LicenseAgreement()
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

        public SectionParamCollection Init()
        {
            throw new NotImplementedException();
        }

        public SectionParamCollection GetDefaultParams()
        {
            SectionParamCollection param = new SectionParamCollection();
            param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String, "The text of license agreement"));
            param.Add(new SectionParam(CONST_TEXT_FILE, "", ValueTypeEnum.File, "The file of license agreement can be RTF file"));
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            //Mode = ShowModeEnum.Preview;
            Package = packageClass;
            Params = sectionItem.Params;
            SetValues();
            ShowDialog();
        }

        private void SetValues()
        {
            if(File.Exists(Params[CONST_TEXT_FILE].Value) )
            {
                richTextBox1.LoadFile(Params[CONST_TEXT_FILE].Value);
            }
            else
            {
                richTextBox1.Text = Params[CONST_TEXT].Value;
            }

        }


        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Package = packageClass;
            Params = sectionItem.Params;
            SetValues();
            ShowDialog();
            return _resp;
        }

        #endregion

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
