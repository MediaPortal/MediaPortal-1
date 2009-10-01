using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Interfaces;
using MpeCore.Classes;

namespace MpeCore.Classes.SectionPanel
{
    public partial class InstallSection : Form, ISectionPanel
    {
        private ShowModeEnum Mode = ShowModeEnum.Preview;
        private SectionItem Section = new SectionItem();
        private PackageClass Package;
        private SectionResponseEnum _resp = SectionResponseEnum.Cancel;

        public InstallSection()
        {
            InitializeComponent();
        }

        #region ISectionPanel Members


        public SectionParamCollection Params
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
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Preview;
            Section = sectionItem;
            Package = packageClass;
            SetValues();
            timer1.Enabled = true;
            ShowDialog();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Mode = ShowModeEnum.Real;
            Package = packageClass;
            Section = sectionItem;
            progressBar1.Maximum = Package.GetInstallableFileCount();
            packageClass.FileInstalled += packageClass_FileInstalled;
            SetValues();
            ShowDialog();
            return _resp;
        }

        void packageClass_FileInstalled(object sender, Events.InstallEventArgs e)
        {
            progressBar1.Value++;
        }

        private void SetValues()
        {
            
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value++;
            if (progressBar1.Value > progressBar1.Maximum - 2)
                progressBar1.Value = 0;
        }

        private void InstallSection_Load(object sender, EventArgs e)
        {
            if(Mode==ShowModeEnum.Real)
            {
                Package.Install();
                button_next.Enabled = true;
            }
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            _resp = SectionResponseEnum.Next;
            Close();
        }
    }
}
