using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
    public partial class Welcome : Base, ISectionPanel
    {

        private const string CONST_TEXT1 = "Header text";
        private const string CONST_TEXT2 = "Description";

        private SectionItem Section = new SectionItem();

        private SectionResponseEnum resp = SectionResponseEnum.Cancel;
        public Welcome()
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
            param.Add(new SectionParam(CONST_TEXT1, "", ValueTypeEnum.String, ""));
            param.Add(new SectionParam(CONST_TEXT2, "", ValueTypeEnum.String, ""));
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            Section = sectionItem;
            SetValues();
            ShowDialog();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Section = sectionItem;
            SetValues();
            ShowDialog();
            return resp;
        }

        #endregion

        private void SetValues()
        {
            lbl_desc1.Text = Section.Params[CONST_TEXT1].Value;
            lbl_desc2.Text = Section.Params[CONST_TEXT2].Value;
        }

        private void Welcome_Load(object sender, EventArgs e)
        {

        }

        private void button_back_Click(object sender, EventArgs e)
        {
            resp = SectionResponseEnum.Back;
            this.Close();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            resp = SectionResponseEnum.Next;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            resp = SectionResponseEnum.Cancel;
            this.Close();
        }
    }
}
