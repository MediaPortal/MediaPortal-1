using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeMaker.Dialogs;

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
            Package = null;            
            txt_name.Text = pak.GeneralInfo.Name;
            txt_guid.Text = pak.GeneralInfo.Id;
            txt_version1.Text = pak.GeneralInfo.Version.Major;
            txt_version2.Text = pak.GeneralInfo.Version.Minor;
            txt_version3.Text = pak.GeneralInfo.Version.Build;
            txt_version4.Text = pak.GeneralInfo.Version.Revision;
            txt_author.Text = pak.GeneralInfo.Author;
            cmb_status.Text = pak.GeneralInfo.DevelopmentStatus;
            txt_homepage.Text = pak.GeneralInfo.HomePage;
            txt_forum.Text = pak.GeneralInfo.ForumPage;
            txt_update.Text = pak.GeneralInfo.UpdateUrl;
            txt_description.Text = pak.GeneralInfo.ExtensionDescription;
            txt_versiondesc.Text = pak.GeneralInfo.VersionDescription;
            Package = pak;
            RefreshIcon();
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
                Package.GeneralInfo.Author = txt_author.Text;
                Package.GeneralInfo.DevelopmentStatus = cmb_status.Text;
                Package.GeneralInfo.HomePage = txt_homepage.Text;
                Package.GeneralInfo.ForumPage = txt_forum.Text;
                Package.GeneralInfo.UpdateUrl = txt_update.Text;
                Package.GeneralInfo.ExtensionDescription = txt_description.Text;
                Package.GeneralInfo.VersionDescription = txt_versiondesc.Text;
                Package.GeneralInfo.OnlineLocation = txt_online.Text;
            }
        }

        private void RefreshIcon()
        {
            if (File.Exists(Package.GeneralInfo.Params[ParamNamesConst.ICON].Value))
                img_logo.LoadAsync(Package.GeneralInfo.Params[ParamNamesConst.ICON].Value);
        }

        private void btn_gen_guid_Click(object sender, EventArgs e)
        {
            txt_guid.Text = Guid.NewGuid().ToString();
        }


        private void btn_params_Click(object sender, EventArgs e)
        {
            ParamEdit dlg = new ParamEdit();
            dlg.Set(Package.GeneralInfo.Params);
            dlg.ShowDialog();
            RefreshIcon();
        }


    }
}
