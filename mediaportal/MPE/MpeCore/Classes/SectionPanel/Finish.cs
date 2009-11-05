using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MpeCore.Classes.SectionPanel;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
    public partial class Finish : BaseVerticalLayout, ISectionPanel
    {

        private const string CONST_TEXT1 = "Header text";
        private const string CONST_IMAGE = "Left part image";

        private SectionItem Section = new SectionItem();
        private PackageClass _packageClass = new PackageClass();
        private List<CheckBox> CheckBoxs = new List<CheckBox>();
        public ShowModeEnum Mode = ShowModeEnum.Preview;


        private SectionResponseEnum resp = SectionResponseEnum.Cancel;
        public Finish()
        {
            InitializeComponent();
            CheckBoxs.Add(checkBox1);
            CheckBoxs.Add(checkBox2);
            CheckBoxs.Add(checkBox3);
            CheckBoxs.Add(checkBox4);
            CheckBoxs.Add(checkBox5);
            CheckBoxs.Add(checkBox6);
            CheckBoxs.Add(checkBox7);
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

        public string Guid
        {
            get { return "{BB49DFA5-04AB-45d1-8CEB-92C4544615E0}"; }
        }

        public string DisplayName
        {
            get { return "Setup Complete"; }
        }

        public SectionParamCollection Init()
        {
            throw new NotImplementedException();
        }

        public SectionParamCollection GetDefaultParams()
        {
            SectionParamCollection param = new SectionParamCollection();
            param.Add(new SectionParam(CONST_TEXT1, "The Extension Installer Wizard has successfully installed [Name].", ValueTypeEnum.String, ""));
            param.Add(new SectionParam(CONST_IMAGE, "", ValueTypeEnum.File, ""));
            return param;
        }

        public void Preview(PackageClass packageClass, SectionItem sectionItem)
        {
            Section = sectionItem;
            _packageClass = packageClass;
            Mode = ShowModeEnum.Preview;
            SetValues();
            ShowDialog();
        }

        public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
        {
            Section = sectionItem;
            _packageClass = packageClass;
            SetValues();
            Base.ActionExecute(_packageClass, Section, ActionExecuteLocationEnum.BeforPanelShow);
            Base.ActionExecute(_packageClass, Section, ActionExecuteLocationEnum.AfterPanelShow);
            Mode = ShowModeEnum.Real;
            if (!packageClass.Silent)
                ShowDialog();
            else
                resp = SectionResponseEnum.Next;
            Base.ActionExecute(_packageClass, Section, ActionExecuteLocationEnum.AfterPanelHide);
            return resp;
        }

        #endregion

        private void SetValues()
        {
            lbl_desc1.Text = _packageClass.ReplaceInfo(Section.Params[CONST_TEXT1].Value);
            if (File.Exists(Section.Params[CONST_IMAGE].Value))
            {
                base.pictureBox1.Load(Section.Params[CONST_IMAGE].Value);
            }
            foreach (CheckBox checkBox in CheckBoxs)
            {
                checkBox.Visible = false;

            }
            int i = 0;
            foreach (var includedGroup in Section.IncludedGroups)
            {
                CheckBoxs[i].Visible = true;
                CheckBoxs[i].Text = _packageClass.Groups[includedGroup].DisplayName;
                CheckBoxs[i].Checked = _packageClass.Groups[includedGroup].Checked;
                CheckBoxs[i].Tag = _packageClass.Groups[includedGroup];
                i++;
                if (i > 6)
                    break;
            }
            button_next.Text = "Next>";
            switch (Section.WizardButtonsEnum)
            {
                case WizardButtonsEnum.BackNextCancel:
                    button_next.Visible = true;
                    button_cancel.Visible = true;
                    button_back.Visible = true;
                    break;
                case WizardButtonsEnum.NextCancel:
                    button_next.Visible = true;
                    button_cancel.Visible = true;
                    button_back.Visible = false;
                    break;
                case WizardButtonsEnum.BackFinish:
                    button_next.Visible = true;
                    button_cancel.Visible = false;
                    button_back.Visible = true;
                    button_next.Text = "Finish";
                    break;
                case WizardButtonsEnum.Cancel:
                    button_next.Visible = false;
                    button_cancel.Visible = true;
                    button_back.Visible = false;
                    break;
                case WizardButtonsEnum.Next:
                    button_next.Visible = true;
                    button_cancel.Visible = false;
                    button_back.Visible = false;
                    break;
                case WizardButtonsEnum.Finish:
                    button_next.Visible = true;
                    button_cancel.Visible = false;
                    button_back.Visible = false;
                    button_next.Text = "Finish";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Mode == ShowModeEnum.Preview)
                return;
            CheckBox box = (CheckBox) sender;
            GroupItem item = box.Tag as GroupItem;
            item.Checked = box.Checked;
        }
    }
}
