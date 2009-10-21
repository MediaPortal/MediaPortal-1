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
    public partial class InstallSection : BaseHorizontalLayout, ISectionPanel
    {

        public InstallSection()
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

        public SectionParamCollection Init()
        {
            throw new NotImplementedException();
        }

        public SectionParamCollection GetDefaultParams()
        {
            return new SectionParamCollection(base.Params);
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
            SetValues();
            Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.BeforPanelShow);
            ShowDialog();
            Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelHide);
            return base.Resp;
        }

        void packageClass_FileInstalled(object sender, Events.InstallEventArgs e)
        {
            progressBar1.Value++;
            if (!string.IsNullOrEmpty(e.Description))
                lbl_curr_file.Text = e.Description;
            else
            {
                lbl_curr_file.Text = e.Item.DestinationFilename;
            }

            Refresh();
            lbl_curr_file.Refresh();
            progressBar1.Refresh();
        }

        private void SetValues()
        {
            button_back.Visible = false;
            button_cancel.Visible = false;
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

        }



        private void InstallSection_Shown(object sender, EventArgs e)
        {
            this.BringToFront();
            if (Mode == ShowModeEnum.Real)
            {
                base.button_next.Enabled = false;

                foreach (var actionItem in Section.Actions.Items)
                {
                    if (actionItem.ExecuteLocation != ActionExecuteLocationEnum.AfterPanelShow)
                        continue;
                    if (!string.IsNullOrEmpty(actionItem.ConditionGroup) && !Package.Groups[actionItem.ConditionGroup].Checked)
                        continue;

                    progressBar1.Value = progressBar1.Minimum;
                    progressBar1.Maximum = MpeInstaller.ActionProviders[actionItem.ActionType].ItemsCount(Package,
                                                                                                          actionItem);
                    MpeInstaller.ActionProviders[actionItem.ActionType].ItemProcessed += packageClass_FileInstalled;
                    MpeInstaller.ActionProviders[actionItem.ActionType].Execute(Package, actionItem);
                    MpeInstaller.ActionProviders[actionItem.ActionType].ItemProcessed -= packageClass_FileInstalled;
                }
                base.button_next.Enabled = true;
                //Package.Install();
                ////this.Close();
            }
        }

        #region ISectionPanel Members

        public string DisplayName
        {
            get { return "Install Section"; }
        }

        public string Guid
        {
            get { return "{839F908C-05A5-47ac-8AD4-BE8A7BC44DAE}"; }
        }

        #endregion
    }
}
