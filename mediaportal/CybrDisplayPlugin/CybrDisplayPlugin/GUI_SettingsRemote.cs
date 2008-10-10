namespace CybrDisplayPlugin
{
    using CybrDisplayPlugin.Drivers;
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.InputDevices;
    using System;
    using System.IO;

    public class GUI_SettingsRemote : GUIWindow
    {
        [SkinControl(50)]
        protected GUIToggleButtonControl btnDisableRemote = new GUIToggleButtonControl(0x4daa);
        [SkinControl(0x33)]
        protected GUIToggleButtonControl btnDisableRepeat = new GUIToggleButtonControl(0x4daa);
        [SkinControl(0x35)]
        protected GUIButtonControl btnRemoteMapping = new GUIButtonControl(0x4daa);
        [SkinControl(0x34)]
        protected GUISelectButtonControl btnRepeatDelay;
        private VLSYS_Mplay.RemoteControl RCSettings = new VLSYS_Mplay.RemoteControl();
        private bool selectedDisableRemote;
        private bool selectedDisableRepeat;
        private int selectedRepeatDelayIndex;

        public GUI_SettingsRemote()
        {
            this.GetID = 0x4daa;
        }

        private void BackupButtons()
        {
            this.selectedDisableRemote = this.btnDisableRemote.Selected;
            this.selectedDisableRepeat = this.btnDisableRepeat.Selected;
            this.selectedRepeatDelayIndex = this.btnRepeatDelay.SelectedItem;
        }

        public override bool Init()
        {
            this.Restore();
            return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Remote));
        }

        private void LoadSettings()
        {
            this.RCSettings = XMLUTILS.LoadRemoteSettings();
        }

        public override void OnAdded()
        {
            Log.Info("CybrDisplay.GUI_SettingsRemote.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
            base.OnAdded();
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (control == this.btnDisableRemote)
            {
                this.OnDisableRemoteChanged();
            }
            if (control == this.btnDisableRepeat)
            {
                this.OnDisableRepeatChanged();
            }
            if (control == this.btnRepeatDelay)
            {
                this.OnRepeatDelayChanged();
                GUIControl.FocusControl(this.GetID, controlId);
            }
            if (control == this.btnRemoteMapping)
            {
                try
                {
                    if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml")))
                    {
                        VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping();
                    }
                    new InputMappingForm("VLSYS_Mplay").ShowDialog();
                }
                catch (Exception exception)
                {
                    Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
                }
            }
            base.OnClicked(controlId, control, actionType);
        }

        private void OnDisableRemoteChanged()
        {
            this.BackupButtons();
            this.SaveSettings();
            XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Remote);
            this.Restore();
            GUIControl.FocusControl(this.GetID, this.btnDisableRemote.GetID);
            this.RestoreButtons();
        }

        private void OnDisableRepeatChanged()
        {
            this.BackupButtons();
            this.SaveSettings();
            XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Remote);
            this.Restore();
            GUIControl.FocusControl(this.GetID, this.btnDisableRepeat.GetID);
            this.RestoreButtons();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            this.SaveSettings();
            base.OnPageDestroy(newWindowId);
            XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.Remote);
        }

        protected override void OnPageLoad()
        {
            this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Remote));
            this.Restore();
            base.LoadSkin();
            base.OnPageLoad();
            this.LoadSettings();
            this.SetDisableRemote();
            this.SetDisableRepeat();
            this.SetRepeatDelay();
            GUIControl.FocusControl(this.GetID, this.btnDisableRemote.GetID);
            GUIPropertyManager.SetProperty("#currentmodule", "CybrDisplay Remote Control Setup");
        }

        private void OnRepeatDelayChanged()
        {
            this.BackupButtons();
            this.SaveSettings();
            this.RestoreButtons();
        }

        private void RestoreButtons()
        {
            if (this.selectedDisableRemote)
            {
                GUIControl.SelectControl(this.GetID, this.btnDisableRemote.GetID);
            }
            if (this.selectedDisableRepeat)
            {
                GUIControl.SelectControl(this.GetID, this.btnDisableRepeat.GetID);
            }
            if (this.btnRepeatDelay != null)
            {
                this.SetRepeatDelay();
            }
        }

        private void SaveSettings()
        {
            this.RCSettings.DisableRemote = this.btnDisableRemote.Selected;
            this.RCSettings.DisableRepeat = this.btnDisableRepeat.Selected;
            this.RCSettings.RepeatDelay = this.btnRepeatDelay.SelectedItem;
            XMLUTILS.SaveRemoteSettings(this.RCSettings);
        }

        private void SetDisableRemote()
        {
            this.btnDisableRemote.Selected = this.RCSettings.DisableRemote;
        }

        private void SetDisableRepeat()
        {
            if (this.RCSettings.DisableRemote)
            {
                this.btnDisableRepeat.Selected = this.RCSettings.DisableRepeat;
            }
        }

        private void SetRepeatDelay()
        {
            if (!this.RCSettings.DisableRemote & !this.RCSettings.DisableRepeat)
            {
                GUIControl.ClearControl(this.GetID, this.btnRepeatDelay.GetID);
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "0");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "25");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "50");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "75");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "100");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "125");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "150");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "175");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "200");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "225");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "250");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "275");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "300");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "325");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "350");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "375");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "400");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "425");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "450");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "475");
                GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "500");
                GUIControl.SelectItemControl(this.GetID, this.btnRepeatDelay.GetID, this.RCSettings.RepeatDelay);
            }
        }
    }
}

