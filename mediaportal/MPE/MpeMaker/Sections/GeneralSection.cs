using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Sections
{
    public partial class GeneralSection : UserControl,ISectionControl
    {
        public PackageClass Package { get; set; }

        public GeneralSection()
        {
            InitializeComponent();
            Package = null;
        }

        private void GeneralSection_Load(object sender, EventArgs e)
        {

        }

        #region ISectionControl Members

        public void Set(PackageClass pak)
        {
            txt_name.Text = pak.GeneralInfo.Name;
            txt_guid.Text = pak.GeneralInfo.Id;
            txt_version1.Text = pak.GeneralInfo.Version.Major;
            txt_version2.Text = pak.GeneralInfo.Version.Minor;
            txt_version3.Text = pak.GeneralInfo.Version.Build;
            txt_version4.Text = pak.GeneralInfo.Version.Revision;
            Package = pak;
        }

        public PackageClass Get()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void txt_name_TextChanged(object sender, EventArgs e)
        {
            if (Package != null)
            {
                Package.GeneralInfo.Name = txt_name.Text;
                Package.GeneralInfo.Id = txt_guid.Text;
                Package.GeneralInfo.Version.Major = txt_version1.Text;
                Package.GeneralInfo.Version.Minor = txt_version2.Text;
                Package.GeneralInfo.Version.Build = txt_version3.Text;
                Package.GeneralInfo.Version.Revision = txt_version4.Text;
            }
        }
    }
}
