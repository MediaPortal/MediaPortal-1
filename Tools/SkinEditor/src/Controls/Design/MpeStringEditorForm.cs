#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mpe.Controls.Design
{

  #region MpeStringEditorForm

  /// <summary>
  /// Summary description for StringSelector.
  /// </summary>
  public class MpeStringEditorForm : UserControl
  {
    #region Variables

    private ColumnHeader colId;
    private ColumnHeader colString;
    private ListView stringList;
    private IWindowsFormsEditorService editorService;
    private Container components = null;
    private Button okButton;
    private Button cancelButton;
    private ListBox propertyList;
    private GroupBox groupBox1;
    private RadioButton simpleRadio;
    private Label label1;
    private CheckBox checkBox1;
    private TextBox simpleTextbox;
    private RadioButton propRadio;
    private RadioButton stringRadio;
    private Panel simplePanel;
    private Panel propPanel;
    private Panel stringPanel;
    private static ArrayList properties;

    #endregion

    #region Constructors

    public MpeStringEditorForm(string currentValue, MpeParser parser, IWindowsFormsEditorService editorService)
    {
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      InitializeComponent();
      Height = 184;
      propPanel.Location = simplePanel.Location;
      propPanel.Size = simplePanel.Size;
      stringPanel.Location = simplePanel.Location;
      stringPanel.Size = simplePanel.Size;
      propPanel.Visible = false;
      stringPanel.Visible = false;
      simpleTextbox.Text = currentValue;
      if (properties == null)
      {
        properties = new ArrayList();
        properties.Add("itemcount");
        properties.Add("selecteditem");
        properties.Add("selecteditem2");
        properties.Add("selectedthumb");
        properties.Add("title");
        properties.Add("artist");
        properties.Add("album");
        properties.Add("track");
        properties.Add("year");
        properties.Add("comment");
        properties.Add("director");
        properties.Add("genre");
        properties.Add("cast");
        properties.Add("dvdlabel");
        properties.Add("imdbnumber");
        properties.Add("file");
        properties.Add("plot");
        properties.Add("plotoutline");
        properties.Add("rating");
        properties.Add("tagline");
        properties.Add("votes");
        properties.Add("credits");
        properties.Add("thumb");
        properties.Add("currentplaytime");
        properties.Add("shortcurrentplaytime");
        properties.Add("duration");
        properties.Add("shortduration");
        properties.Add("playlogo");
        properties.Add("playspeed");
        properties.Add("percentage");
        properties.Add("currentmodule");
        properties.Add("channel");
        properties.Add("TV.start");
        properties.Add("TV.stop");
        properties.Add("TV.current");
        properties.Add("TV.Record.channel");
        properties.Add("TV.Record.start");
        properties.Add("TV.Record.stop");
        properties.Add("TV.Record.genre");
        properties.Add("TV.Record.title");
        properties.Add("TV.Record.description");
        properties.Add("TV.Record.thumb");
        properties.Add("TV.View.channel");
        properties.Add("TV.View.thumb");
        properties.Add("TV.View.start");
        properties.Add("TV.View.stop");
        properties.Add("TV.View.genre");
        properties.Add("TV.View.title");
        properties.Add("TV.View.description");
        properties.Add("TV.View.Percentage");
        properties.Add("TV.Guide.Day");
        properties.Add("TV.Guide.thumb");
        properties.Add("TV.Guide.Title");
        properties.Add("TV.Guide.Time");
        properties.Add("TV.Guide.Duration");
        properties.Add("TV.Guide.TimeFromNow");
        properties.Add("TV.Guide.Description");
        properties.Add("TV.Guide.Genre");
        properties.Add("TV.Guide.EpisodeName");
        properties.Add("TV.Guide.SeriesNumber");
        properties.Add("TV.Guide.EpisodeNumber");
        properties.Add("TV.Guide.EpisodePart");
        properties.Add("TV.Guide.EpisodeDetail");
        properties.Add("TV.Guide.Date");
        properties.Add("TV.Guide.StarRating");
        properties.Add("TV.Guide.Classification");
        properties.Add("TV.RecordedTV.Title");
        properties.Add("TV.RecordedTV.Time");
        properties.Add("TV.RecordedTV.Description");
        properties.Add("TV.RecordedTV.thumb");
        properties.Add("TV.RecordedTV.Genre");
        properties.Add("TV.Scheduled.Title");
        properties.Add("TV.Scheduled.Time");
        properties.Add("TV.Scheduled.Description");
        properties.Add("TV.Scheduled.thumb");
        properties.Add("TV.Scheduled.Genre");
        properties.Add("TV.Search.Title");
        properties.Add("TV.Search.Time");
        properties.Add("TV.Search.Description");
        properties.Add("TV.Search.thumb");
        properties.Add("TV.Search.Genre");
      }
      this.editorService = editorService;
      MpeStringTable table = parser.GetStringTable("English");
      int[] keys = table.Keys;
      ListViewItem sel = null;
      for (int i = 0; i < keys.Length; i++)
      {
        string s = table[keys[i]];
        //ListViewItem item = stringList.Items.Add(s);
        ListViewItem item = stringList.Items.Add(keys[i].ToString("D6"));
        item.Tag = keys[i];
        //item.SubItems.Add(keys[i].ToString("D6"));
        item.SubItems.Add(s);
        if (currentValue.Equals(keys[i].ToString()))
        {
          item.Selected = true;
          sel = item;
        }
      }
      if (sel != null)
      {
        MpeLog.Debug("Is the selected string visible?");
        sel.EnsureVisible();
      }
      propertyList.DataSource = properties;
    }

    #endregion

    #region Properties

    public string SelectedValue
    {
      get { return ((int) stringList.SelectedItems[0].Tag).ToString(); }
    }

    #endregion

    #region Methods

    public void Close()
    {
      if (editorService != null)
      {
        editorService.CloseDropDown();
      }
    }

    #endregion

    #region Event Handlers

    private void OnMouseWheel(object sender, MouseEventArgs e)
    {
      int i = 0;
      if (e.Delta > 0)
      {
        i = stringList.SelectedIndices[0] - 1;
      }
      else
      {
        i = stringList.SelectedIndices[0] + 1;
      }
      if (i < 0)
      {
        i = 0;
      }
      else if (i >= stringList.Items.Count)
      {
        i = stringList.Items.Count - 1;
      }
      stringList.Items[i].Selected = true;
      stringList.Items[i].EnsureVisible();
    }

    private void OnDoubleClick(object sender, EventArgs e)
    {
      Close();
    }

    #endregion

    #region Component Designer Generated Code

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      stringList = new ListView();
      colId = new ColumnHeader();
      colString = new ColumnHeader();
      okButton = new Button();
      cancelButton = new Button();
      propertyList = new ListBox();
      groupBox1 = new GroupBox();
      propRadio = new RadioButton();
      stringRadio = new RadioButton();
      simpleRadio = new RadioButton();
      simplePanel = new Panel();
      checkBox1 = new CheckBox();
      label1 = new Label();
      simpleTextbox = new TextBox();
      propPanel = new Panel();
      stringPanel = new Panel();
      groupBox1.SuspendLayout();
      simplePanel.SuspendLayout();
      propPanel.SuspendLayout();
      stringPanel.SuspendLayout();
      SuspendLayout();
      // 
      // stringList
      // 
      stringList.Columns.AddRange(new ColumnHeader[]
                                    {
                                      colId,
                                      colString
                                    });
      stringList.Dock = DockStyle.Fill;
      stringList.FullRowSelect = true;
      stringList.Location = new Point(0, 0);
      stringList.MultiSelect = false;
      stringList.Name = "stringList";
      stringList.Size = new Size(272, 100);
      stringList.Sorting = SortOrder.Ascending;
      stringList.TabIndex = 0;
      stringList.View = View.Details;
      stringList.DoubleClick += new EventHandler(OnDoubleClick);
      stringList.MouseWheel += new MouseEventHandler(OnMouseWheel);
      // 
      // colId
      // 
      colId.Text = "Id";
      colId.Width = 46;
      // 
      // colString
      // 
      colString.Text = "Value";
      colString.Width = 170;
      // 
      // okButton
      // 
      okButton.Location = new Point(120, 156);
      okButton.Name = "okButton";
      okButton.TabIndex = 2;
      okButton.Text = "OK";
      // 
      // cancelButton
      // 
      cancelButton.Location = new Point(200, 156);
      cancelButton.Name = "cancelButton";
      cancelButton.TabIndex = 3;
      cancelButton.Text = "Cancel";
      // 
      // propertyList
      // 
      propertyList.Dock = DockStyle.Fill;
      propertyList.Location = new Point(0, 0);
      propertyList.Name = "propertyList";
      propertyList.Size = new Size(272, 69);
      propertyList.TabIndex = 4;
      // 
      // groupBox1
      // 
      groupBox1.Controls.Add(propRadio);
      groupBox1.Controls.Add(stringRadio);
      groupBox1.Controls.Add(simpleRadio);
      groupBox1.Location = new Point(8, 0);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(272, 48);
      groupBox1.TabIndex = 5;
      groupBox1.TabStop = false;
      groupBox1.Text = "Selection";
      // 
      // propRadio
      // 
      propRadio.FlatStyle = FlatStyle.System;
      propRadio.Location = new Point(184, 16);
      propRadio.Name = "propRadio";
      propRadio.Size = new Size(80, 24);
      propRadio.TabIndex = 2;
      propRadio.Text = "Properties";
      propRadio.Click += new EventHandler(OnPropRadioClicked);
      // 
      // stringRadio
      // 
      stringRadio.FlatStyle = FlatStyle.System;
      stringRadio.Location = new Point(96, 16);
      stringRadio.Name = "stringRadio";
      stringRadio.Size = new Size(64, 24);
      stringRadio.TabIndex = 1;
      stringRadio.Text = "Strings";
      stringRadio.Click += new EventHandler(OnStringRadioClicked);
      // 
      // simpleRadio
      // 
      simpleRadio.Checked = true;
      simpleRadio.FlatStyle = FlatStyle.System;
      simpleRadio.Location = new Point(8, 16);
      simpleRadio.Name = "simpleRadio";
      simpleRadio.Size = new Size(64, 24);
      simpleRadio.TabIndex = 0;
      simpleRadio.TabStop = true;
      simpleRadio.Text = "Simple";
      simpleRadio.Click += new EventHandler(OnSimpleRadioClicked);
      // 
      // simplePanel
      // 
      simplePanel.Controls.Add(checkBox1);
      simplePanel.Controls.Add(label1);
      simplePanel.Controls.Add(simpleTextbox);
      simplePanel.Location = new Point(8, 48);
      simplePanel.Name = "simplePanel";
      simplePanel.Size = new Size(272, 104);
      simplePanel.TabIndex = 6;
      // 
      // checkBox1
      // 
      checkBox1.Enabled = false;
      checkBox1.Location = new Point(48, 32);
      checkBox1.Name = "checkBox1";
      checkBox1.Size = new Size(216, 16);
      checkBox1.TabIndex = 2;
      checkBox1.Text = "Add to String Table";
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new Point(8, 8);
      label1.Name = "label1";
      label1.Size = new Size(26, 16);
      label1.TabIndex = 1;
      label1.Text = "Text";
      // 
      // simpleTextbox
      // 
      simpleTextbox.AutoSize = false;
      simpleTextbox.Location = new Point(48, 5);
      simpleTextbox.Name = "simpleTextbox";
      simpleTextbox.Size = new Size(216, 20);
      simpleTextbox.TabIndex = 0;
      simpleTextbox.Text = "";
      // 
      // propPanel
      // 
      propPanel.Controls.Add(propertyList);
      propPanel.Location = new Point(8, 232);
      propPanel.Name = "propPanel";
      propPanel.Size = new Size(272, 72);
      propPanel.TabIndex = 7;
      // 
      // stringPanel
      // 
      stringPanel.Controls.Add(stringList);
      stringPanel.Location = new Point(8, 312);
      stringPanel.Name = "stringPanel";
      stringPanel.Size = new Size(272, 100);
      stringPanel.TabIndex = 8;
      // 
      // MpeStringEditorForm
      // 
      BackColor = SystemColors.Control;
      Controls.Add(cancelButton);
      Controls.Add(okButton);
      Controls.Add(stringPanel);
      Controls.Add(simplePanel);
      Controls.Add(groupBox1);
      Controls.Add(propPanel);
      Name = "MpeStringEditorForm";
      Size = new Size(288, 440);
      groupBox1.ResumeLayout(false);
      simplePanel.ResumeLayout(false);
      propPanel.ResumeLayout(false);
      stringPanel.ResumeLayout(false);
      ResumeLayout(false);
    }

    #endregion

    private void OnSimpleRadioClicked(object sender, EventArgs e)
    {
      if (simpleRadio.Checked)
      {
        simplePanel.Visible = true;
        propPanel.Visible = false;
        stringPanel.Visible = false;
      }
    }

    private void OnPropRadioClicked(object sender, EventArgs e)
    {
      if (propRadio.Checked)
      {
        propPanel.Visible = true;
        simplePanel.Visible = false;
        stringPanel.Visible = false;
      }
    }

    private void OnStringRadioClicked(object sender, EventArgs e)
    {
      if (stringRadio.Checked)
      {
        stringPanel.Visible = true;
        simplePanel.Visible = false;
        propPanel.Visible = false;
      }
    }
  }

  #endregion

  #region MpeStringEditor

  public class MpeStringEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (context.Instance is MpeControl)
      {
        try
        {
          MpeControl mpc = (MpeControl) context.Instance;
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
          MpeStringEditorForm selector = new MpeStringEditorForm((string) value, mpc.Parser, editorService);
          editorService.DropDownControl(selector);
          return selector.SelectedValue;
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      else if (context.Instance is MpeItem)
      {
        try
        {
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
          MpeStringEditorForm selector =
            new MpeStringEditorForm((string) value, MediaPortalEditor.Global.Parser, editorService);
          editorService.DropDownControl(selector);
          return selector.SelectedValue;
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      return base.EditValue(context, provider, value);
    }
  }

  #endregion
}