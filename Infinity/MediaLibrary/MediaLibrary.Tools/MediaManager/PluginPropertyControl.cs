using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;
using MediaLibrary.Settings;
using MediaLibrary.Configuration;

namespace MediaManager
{
    /// <summary>
    /// This control is a panel with up to three elements.
    /// The Caption represents the properties name
    /// A generic control sets/gets the properties value
    /// A Button leads to a DialogBox which sets/gets the properties value
    /// </summary>
    class MLPluginPropertyControl : Panel
    {
        MLPluginProperty MyPropery;              
        Control MyControl = null;

        private IMLPlugin _Plugin;
        public IMLPlugin Plugin
        {
            get { return _Plugin; }
            set { _Plugin = value; }
        }

        object _value;
        public string Key
        {
            get { return MyPropery.Name; }
        }
        public object Value
        {
            // retrieve the value of the control
            get
            {
                object value;


                switch (MyPropery.DataType.ToLower())
                {
                    case "folder":
                    case "file":
                    case "custom":
                        value = MyControl.Text;
                        break;
                    case "folderlist":
                    case "stringlist":
                        value = _value;
                        break;
                    case "string":
                        value = MyControl.Text;
                        if (MyPropery.Choices2.Count > 0)
                            value = ((ComboBox)MyControl).SelectedValue as string;
                        break;
                    case "int":
                        value = Convert.ToInt32(MyControl.Text);
                        MyControl.Text = value.ToString();

                        if (MyPropery.Choices2.Count > 0)
                            value = ((ComboBox)MyControl).SelectedValue as string;
                        break;
                    case "float":
                        value = Convert.ToDouble(MyControl.Text);
                        MyControl.Text = value.ToString();
                        break;
                    case "date":
                        DateTime d = Convert.ToDateTime(MyControl.Text);
                        value = d.ToString("d");
                        MyControl.Text = value.ToString();
                        break;
                    case "time":
                        DateTime t = Convert.ToDateTime(MyControl.Text);
                        value = t.ToString("T");
                        MyControl.Text = value.ToString();
                        break;
                    case "bool":
                        value = ((CheckBox)MyControl).Checked;
                        break;
                    default:
                        value = MyPropery.DefaultValue;
                        break;
                }
                _value = value;
                return value;
            }

            set
            {
                _value = value;
                MyControl.Text = Convert.ToString(_value);

                switch (MyPropery.DataType.ToLower())
                {
                    case "label": // This is for "This plugin has no properties"                        
                    case "folder":
                    case "folderlist":
                    case "stringlist":
                    case "file":
                        break;
                    case "string":
                    case "int":
                    case "float":
                    case "date":
                    case "time":
                        if (MyPropery.Choices != null)
                        {
                            ((ComboBox)MyControl).SelectedIndex = ((ComboBox)MyControl).Items.IndexOf(_value);
                        }

                        if (MyPropery.Choices2.Count != 0)
                        {
                            ((ComboBox)MyControl).SelectedIndex = ((ComboBox)MyControl).Items.IndexOf(_value);
                            ((ComboBox)MyControl).SelectedValue = _value;
                        }
                        break;
                    case "bool":
                        ((CheckBox)MyControl).Checked = (bool)value;
                        MyControl.Text = "";
                        break;
                    default:
                        MyControl.Text = "Unknown IMLPluginProperty.DataType";
                        break;
                }
            }
        }    

        public MLPluginPropertyControl(MLPluginProperty MLPluginProperty)
            : base()
        {
            Label MyCaption;            
            ToolTip MyToolTip = new ToolTip();
            Button MyButton = null;

            MyPropery = MLPluginProperty;
            
            _value = MyPropery.DefaultValue;
            if (MyPropery.Value != null)
            {
                _value = MyPropery.Value;
            }

            
            // The Caption of the control
            MyCaption = new Label();
            MyCaption.Text = MyPropery.Caption;
            MyCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            MyCaption.Width = (int)this.Font.Size * 30;
            if (MyPropery.IsMandatory)
            {
                MyCaption.Font = new System.Drawing.Font(MyCaption.Font, System.Drawing.FontStyle.Bold);
            }

            // the get/set property control
            switch (MyPropery.DataType.ToLower())
            {
                case "label": // This is for "This plugin has no properties"
                    MyControl = new Label();
                    ((Label)MyControl).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    MyControl.Left = (int)this.Font.Size * 30;
                    MyControl.Width = this.ClientRectangle.Width - MyControl.Left;
                    break;
                case "string":
                case "password":
                case "int":
                case "float":
                case "date":
                case "time":
                    if (MyPropery.Choices == null && MyPropery.Choices2.Count == 0)
                    {
                        MyControl = new TextBox();
                        if (MyPropery.DataType.ToLower() == "password")
                        {
                            ((TextBox)MyControl).PasswordChar = '*';
                        }
                    }
                    else
                    {
                        MyControl = new ComboBox();
                        if (MyPropery.CanTypeChoices == true)
                        {
                            ((ComboBox)MyControl).DropDownStyle = ComboBoxStyle.DropDown;
                        }
                        else
                        {
                            ((ComboBox)MyControl).DropDownStyle = ComboBoxStyle.DropDownList;
                        }

                        if (MyPropery.Choices != null)
                        {
                            ((ComboBox)MyControl).Items.AddRange(MyPropery.Choices);
                            ((ComboBox)MyControl).SelectedIndex = ((ComboBox)MyControl).Items.IndexOf(_value);
                        }

                        if (MyPropery.Choices2.Count != 0)
                        {
                            ArrayList MyChoices2 = new ArrayList();
                            MediaItemEntryEx mix;
                            MediaItemEntryEx selectedmix=null;
                            object selectedobject=null;
                            
                            foreach (string key in MyPropery.Choices2.Keys)
                            {
                                mix = new MediaItemEntryEx();
                                mix.Key = key;
                                mix.Value = MyPropery.Choices2[key];
                                MyChoices2.Add(mix);

                            }
                            ((ComboBox)MyControl).DataSource = MyChoices2;
                            ((ComboBox)MyControl).DisplayMember = "Values";
                            ((ComboBox)MyControl).ValueMember = "Keys";
                            // TODO: this doesn't work
                            //((ComboBox)MyControl).SelectedIndex = MyChoices2.IndexOf(selectedmix);
                            //((ComboBox)MyControl).SelectedValue = selectedobject;                            
                        }                                           
                    }
                    MyControl.Left = (int)this.Font.Size * 30;
                    MyControl.Width = this.ClientRectangle.Width - MyControl.Left;
                    MyControl.Text = Convert.ToString(_value);
                    break;
                case "bool":
                    MyControl = new CheckBox();
                    ((CheckBox)MyControl).Checked = Convert.ToBoolean(_value);
                    ((CheckBox)MyControl).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    MyControl.Left = (int)this.Font.Size * 30;
                    break;
                case "file":
                case "folder":
                case "custom":
                    MyControl = new TextBox();
                    ((TextBox)MyControl).Text = Convert.ToString(_value);

                    MyButton = new Button();
                    MyButton.Text = "...";
                    MyButton.Width = (int)(MyButton.Height * 1.5);
                    MyButton.UseVisualStyleBackColor = true;
                    MyButton.Click += new System.EventHandler(MyButton_Click);
                    MyControl.Left = (int)this.Font.Size * 30;
                    MyControl.Width = this.ClientRectangle.Width - MyButton.Width - MyControl.Left;
                    MyControl.Text = Convert.ToString(_value);
                    break;
                case "folderlist":
                case "stringlist":
                    MyControl = new TextBox();
                    ((TextBox)MyControl).Text = Convert.ToString(_value);
                    ((TextBox)MyControl).ReadOnly = true;

                    MyButton = new Button();
                    MyButton.Text = "...";
                    MyButton.Width = (int)(MyButton.Height * 1.5);
                    MyButton.UseVisualStyleBackColor = true;
                    MyButton.Click += new System.EventHandler(MyButton_Click);
                    MyControl.Left = (int)this.Font.Size * 30;
                    MyControl.Width = this.ClientRectangle.Width - MyButton.Width - MyControl.Left;
                    MyControl.Text = Convert.ToString(_value);
                    break;
                default:
                    MyControl = new Label();
                    ((Label)MyControl).Text = "Unknown IMLPluginProperty.DataType";
                    ((Label)MyControl).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    MyControl.Left = (int)this.Font.Size * 30;
                    MyControl.Text = Convert.ToString(_value);

                    break;
            }
            MyControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            if( MyButton != null )
            {
                MyButton.Left = this.ClientRectangle.Width - MyButton.Width;
                MyButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.Controls.Add(MyButton);
                MyToolTip.SetToolTip(MyButton, MyPropery.HelpText);
            }

            this.Controls.Add(MyCaption);
            this.Controls.Add(MyControl);
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            MyToolTip.SetToolTip(this, MyPropery.HelpText);
            MyToolTip.SetToolTip(MyCaption, MyPropery.HelpText);
            MyToolTip.SetToolTip(MyControl, MyPropery.HelpText);
        }
    
        private void MyButton_Click(object sender, EventArgs e)
        {
            object MyButtonDialog = null;   
            
            
            switch (MyPropery.DataType.ToLower())
            {
                case "folder":
                    MyButtonDialog = new FolderBrowserDialog();
                    ((FolderBrowserDialog)MyButtonDialog).SelectedPath = _value as string;
                    if (((FolderBrowserDialog)MyButtonDialog).ShowDialog() == DialogResult.OK)
                        _value = Convert.ToString(((FolderBrowserDialog)MyButtonDialog).SelectedPath);
                    break;
                case "folderlist":
                    MyButtonDialog = new FolderlistDialog();
                    ((FolderlistDialog)MyButtonDialog).Text = MyPropery.Caption;
                    ((FolderlistDialog)MyButtonDialog).Folderlist = MLExtentsions.Split(Convert.ToString(_value));
                    if (((FolderlistDialog)MyButtonDialog).ShowDialog() == DialogResult.OK)
                        _value = MLExtentsions.Join(((FolderlistDialog)MyButtonDialog).Folderlist);
                    break;
                case "stringlist":
                    MyButtonDialog = new StringlistDialog();
                    ((StringlistDialog)MyButtonDialog).Text = MyPropery.Caption;
                    ((StringlistDialog)MyButtonDialog).Stringlist = MLExtentsions.Split(Convert.ToString(_value));
                    if (((StringlistDialog)MyButtonDialog).ShowDialog() == DialogResult.OK)
                        _value = MLExtentsions.Join(((StringlistDialog)MyButtonDialog).Stringlist);
                    break;
                case "file":
                    MyButtonDialog = new OpenFileDialog();
                    ((OpenFileDialog)MyButtonDialog).Title = MyPropery.Caption;
                    ((OpenFileDialog)MyButtonDialog).RestoreDirectory = true;
                    ((OpenFileDialog)MyButtonDialog).FileName = _value as string;
                    if (((OpenFileDialog)MyButtonDialog).ShowDialog() == DialogResult.OK)
                        _value = Convert.ToString(((OpenFileDialog)MyButtonDialog).FileName);
                    break;
                case "custom":
                    // we call plugin.EditCustomProperty
                    string s = Convert.ToString(_value);
                    Plugin.EditCustomProperty(this.TopLevelControl.Handle, MyPropery.Name, ref s);
                    _value = s;
                    break;
                default:
                    break;
            }            
            MyControl.Text = Convert.ToString(_value);
        }
    }



    // this is needed for ComboBox.DataSource
    class MediaItemEntryEx : MLHashItemEntry
    {
        public object Keys
        {
            get
            {
                return Key;
            }
        }

        public object Values
        {
            get
            {
                return Value;
            }
        }
    }
}
