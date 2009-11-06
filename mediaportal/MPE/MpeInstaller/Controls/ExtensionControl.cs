using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeInstaller.Dialogs;

namespace MpeInstaller.Controls
{
    public partial class ExtensionControl : UserControl
    {
        public PackageClass Package;
        public PackageClass UpdatePackage = null;
        public ExtensionControl(PackageClass packageClass)
        {
            InitializeComponent();
            lbl_name.Text = packageClass.GeneralInfo.Name + " "  ;
            lbl_version.Text = packageClass.GeneralInfo.Version.ToString();
            lbl_description.Text = packageClass.GeneralInfo.ExtensionDescription;
            if (Directory.Exists(packageClass.LocationFolder))
            {
                DirectoryInfo di = new DirectoryInfo(packageClass.LocationFolder);
                FileInfo[] fileInfos = di.GetFiles("icon.*");
                if (fileInfos.Length > 0)
                    img_logo.LoadAsync(fileInfos[0].FullName);
            }
            Package = packageClass;
            UpdatePackage = MpeCore.MpeInstaller.KnownExtensions.GetUpdate(Package);
            if (UpdatePackage != null)
            {
                btn_update.Visible = true;
                img_update.Visible = true;
            }
            else
            {
                btn_update.Visible = false;
                img_update.Visible = false;
            }
            Selected = false;
            SelectControl();
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    SelectControl();
                }
            }
        }

        private void SelectControl()
        {
            BackColor = _selected ? SystemColors.GradientInactiveCaption : Color.White; 
            BorderStyle = _selected ? BorderStyle.FixedSingle : BorderStyle.FixedSingle;
            lbl_description.ForeColor = _selected ? SystemColors.ButtonFace : Color.Black;
            lbl_name.ForeColor = _selected ? SystemColors.ButtonFace : Color.Black;
            lbl_version.ForeColor = _selected ? SystemColors.ButtonFace : Color.Black;
            Height = _selected ? 123 : 90;

            if (Parent == null)
                return;
            ExtensionListControl parent = Parent.Parent as ExtensionListControl;
            if (Selected)
            {
                if (parent != null && Selected)
                {
                    if (parent.SelectedItem != null)
                    {
                        parent.SelectedItem.Selected = false;
                    }
                }
                if (parent != null) parent.SelectedItem = this;
            }
        }

        private void ExtensionControl_Click(object sender, EventArgs e)
        {
            this.Selected = true;
        }

        private void lbl_description_Click(object sender, EventArgs e)
        {
            ExtensionControl_Click(null, null);
        }

        private void lbl_name_Click(object sender, EventArgs e)
        {
            ExtensionControl_Click(null, null);
        }

        private void img_logo_Click(object sender, EventArgs e)
        {
            ExtensionControl_Click(null, null);
        }

        private void btn_uninstall_Click(object sender, EventArgs e)
        {
            ExtensionListControl parent = Parent.Parent as ExtensionListControl;
            if (parent == null)
                return;
            parent.OnUninstallExtension(this);
        }

        private void lbl_version_Click(object sender, EventArgs e)
        {
            ExtensionControl_Click(null, null);
        }

        private void img_update_Click(object sender, EventArgs e)
        {
            ExtensionControl_Click(null, null);
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            ExtensionListControl parent = Parent.Parent as ExtensionListControl;
            if (parent == null)
                return;
            parent.OnUpdateExtension(this);
        }
    }
}
