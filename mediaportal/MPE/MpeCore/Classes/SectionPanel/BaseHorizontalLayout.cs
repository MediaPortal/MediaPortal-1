using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MpeCore.Classes.SectionPanel
{
    public partial class BaseHorizontalLayout : Form
    {

        private const string Const_LABEL_BIG = "Header Title";
        private const string Const_LABEL_SMALL = "Header description" ;
        private const string Const_IMAGE = "Header image";

        public SectionResponseEnum Resp = SectionResponseEnum.Cancel;
        public PackageClass Package=new PackageClass();
        public ShowModeEnum Mode = ShowModeEnum.Preview;
        public SectionItem Section = new SectionItem();
        public SectionParamCollection Params { get; set; }

        public BaseHorizontalLayout()
        {
            InitializeComponent();
            Params = new SectionParamCollection();
            Params.Add(new SectionParam(Const_LABEL_BIG, "", ValueTypeEnum.String,
                                       "Header title"));
            Params.Add(new SectionParam(Const_LABEL_SMALL, "", ValueTypeEnum.String,
                                       "Description of section, shown in under section title"));
            Params.Add(new SectionParam(Const_IMAGE, "", ValueTypeEnum.File,
                           "Image in upper right part"));


        }

        private void button_back_Click(object sender, EventArgs e)
        {
            Resp = SectionResponseEnum.Back;
            this.Close();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            Resp = SectionResponseEnum.Next;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            Resp = SectionResponseEnum.Cancel;
            this.Close();
        }

        private void BaseVerticalLayout_Load(object sender, EventArgs e)
        {

        }

        private void BaseHorizontalLayout_Shown(object sender, EventArgs e)
        {
            lbl_large.Text = Section.Params[Const_LABEL_BIG].Value;
            lbl_small.Text = Section.Params[Const_LABEL_SMALL].Value;
            if (File.Exists(Section.Params[Const_IMAGE].Value))
                pictureBox1.LoadAsync(Section.Params[Const_IMAGE].Value);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            Text = string.Format("Extension Installer for  {0} - {1}", Package.GeneralInfo.Name,
                                 Package.GeneralInfo.Version);
        }

    }
}
