namespace CybrDisplayPlugin
{
    using CybrDisplayPlugin.Drivers;
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.InputDevices;
    using System;
    using System.IO;

    public class GUI_SettingsKeyPad : GUIWindow
    {
        [SkinControl(0x3d)]
        protected GUIToggleButtonControl btnEnableCustom = new GUIToggleButtonControl(0x4dab);
        [SkinControl(60)]
        protected GUIToggleButtonControl btnEnableKeyPad = new GUIToggleButtonControl(0x4dab);
        [SkinControl(0x3e)]
        protected GUIButtonControl btnKeyPadMapping = new GUIButtonControl(0x4dab);
        private MatrixMX.KeyPadControl KPSettings = new MatrixMX.KeyPadControl();
        private bool selectedEnableCustom;
        private bool selectedEnableKeyPad;

        public GUI_SettingsKeyPad()
        {
            this.GetID = 0x4dab;
        }

        private void BackupButtons()
        {
            this.selectedEnableKeyPad = this.btnEnableKeyPad.Selected;
            this.selectedEnableCustom = this.btnEnableCustom.Selected;
        }

        public override bool Init()
        {
            this.Restore();
            return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.KeyPad));
        }

        private void LoadSettings()
        {
            this.KPSettings = XMLUTILS.LoadKeyPadSettings();
        }

        public override void OnAdded()
        {
            Log.Info("CybrDisplay.GUI_SettingsKeyPad.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
            base.OnAdded();
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (control == this.btnEnableKeyPad)
            {
                Log.Info("CybrDisplay.GUI_SettingsKeyPad.OnClicked(): called - btnEnableKeyPad clicked", new object[0]);
                this.OnEnableKeyPadChanged();
            }
            if (control == this.btnEnableCustom)
            {
                Log.Info("CybrDisplay.GUI_SettingsKeyPad.OnClicked(): called - btnEnableCustom clicked", new object[0]);
                this.OnEnableCustomChanged();
            }
            if (control == this.btnKeyPadMapping)
            {
                Log.Info("CybrDisplay.GUI_SettingsKeyPad.OnClicked(): called - btnRemoteMapping clicked", new object[0]);
                try
                {
                    if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml")))
                    {
                        MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping();
                    }
                    new InputMappingForm("MatrixMX_Keypad").ShowDialog();
                }
                catch (Exception exception)
                {
                    Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
                }
            }
            base.OnClicked(controlId, control, actionType);
        }

        private void OnEnableCustomChanged()
        {
            this.BackupButtons();
            this.SaveSettings();
            XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.KeyPad);
            this.Restore();
            GUIControl.FocusControl(this.GetID, this.btnEnableCustom.GetID);
            this.RestoreButtons();
        }

        private void OnEnableKeyPadChanged()
        {
            this.BackupButtons();
            this.SaveSettings();
            XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.KeyPad);
            this.Restore();
            GUIControl.FocusControl(this.GetID, this.btnEnableKeyPad.GetID);
            this.RestoreButtons();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            this.SaveSettings();
            base.OnPageDestroy(newWindowId);
            XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.KeyPad);
        }

        protected override void OnPageLoad()
        {
            this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.KeyPad));
            this.Restore();
            base.LoadSkin();
            base.OnPageLoad();
            this.LoadSettings();
            this.SetEnableKeyPad();
            this.SetEnableCustom();
            GUIControl.FocusControl(this.GetID, this.btnEnableKeyPad.GetID);
            GUIPropertyManager.SetProperty("#currentmodule", "CybrDisplay KeyPad Control Setup");
        }

        private void RestoreButtons()
        {
            if (this.selectedEnableKeyPad)
            {
                GUIControl.SelectControl(this.GetID, this.btnEnableKeyPad.GetID);
            }
            if (this.selectedEnableCustom)
            {
                GUIControl.SelectControl(this.GetID, this.btnEnableCustom.GetID);
            }
        }

        private void SaveSettings()
        {
            this.KPSettings.EnableKeyPad = this.btnEnableKeyPad.Selected;
            this.KPSettings.EnableCustom = this.btnEnableCustom.Selected;
            XMLUTILS.SaveKeyPadSettings(this.KPSettings);
        }

        private void SetEnableCustom()
        {
            if (this.KPSettings.EnableKeyPad)
            {
                this.btnEnableCustom.Selected = this.KPSettings.EnableCustom;
            }
        }

        private void SetEnableKeyPad()
        {
            this.btnEnableKeyPad.Selected = this.KPSettings.EnableKeyPad;
        }
    }
}

