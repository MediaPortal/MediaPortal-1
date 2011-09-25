#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Gentle.Framework;
using MediaPortal.UserInterface.Controls;
using SetupControls;
using TvDatabase;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;

namespace SetupTv.Sections
{
  public partial class FormEditSchedule : Form
  {

    private Schedule _schedule;
    private Program _program;

    public FormEditSchedule()
    {
      InitializeComponent();
    }

    public Schedule Schedule
    {
      get { return _schedule; }
      set { _schedule = value; }
    }

    public Program Program
    {
      get { return _program; }
      set { _program = value; }
    }

    private void FormEditSchedule_Load(object sender, System.EventArgs e)
    {
      Init();
    }

    private void AddGroups()
    {
      mpComboBoxChannelsGroup.Items.Clear();
      IList<ChannelGroup> groups = ChannelGroup.ListAll();
      foreach (ChannelGroup group in groups)
        mpComboBoxChannelsGroup.Items.Add(new ComboBoxExItem(group.GroupName, -1, group.IdGroup));
      if (mpComboBoxChannelsGroup.Items.Count == 0)
        mpComboBoxChannelsGroup.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      mpComboBoxChannelsGroup.SelectedIndex = 0;
    }

    private void Init()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      mpNumericTextBoxPreRec.Value = int.Parse(layer.GetSetting("preRecordInterval", "7").Value);
      mpNumericTextBoxPostRec.Value = int.Parse(layer.GetSetting("postRecordInterval", "10").Value);

      ProgramDTO programDto = new ProgramDTO();
      Type t = programDto.GetType();
      PropertyInfo[] pi = t.GetProperties();
      foreach (PropertyInfo prop in pi)
      {
        if (prop.PropertyType == typeof(string) ||
            prop.PropertyType == typeof(DateTime) ||
            prop.PropertyType == typeof(int)
          )
        {
          ProgramField pField = new ProgramField(prop.Name, prop.PropertyType);          
          mpComboBoxProgramFields.Items.Add(pField);
        }        
      }
        

      AddGroups();
      if (_program != null)
      {
        mpTextBoxScheduleName.Text = _program.Title;
      }
      else if (_schedule != null)
      {
        mpTextBoxScheduleName.Text = _schedule.ProgramName;
      }

      IList<ProgramCategory> categories = ProgramCategory.ListAll();
      foreach (var programCategory in categories)
      {
        mpComboBoxCategories.Items.Add(programCategory);
      }


      foreach (object enumValue in Enum.GetValues(typeof(ConditionOperator)))
      {
        mpComboBoxOperators.Items.Add(enumValue);
      }
      


      IList<ProgramCredit> roles = ProgramCredit.ListAllDistinctRoles();
      foreach (var programCredit in roles)
      {
        mpComboBoxRoles.Items.Add(programCredit.Role);
      }
      
      

      foreach(string name in Enum.GetNames(typeof(KeepMethodType)))
      {
        mpComboBoxKeepMethods.Items.Add(name);
      }
    }


    private void mpButtonSave_Click(object sender, EventArgs e)
    {
     Close();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void mpComboBoxChannelsGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBoxExItem idItem = (ComboBoxExItem)mpComboBoxChannelsGroup.Items[mpComboBoxChannelsGroup.SelectedIndex];
      mpComboBoxChannels.Items.Clear();
      if (idItem.Id == -1)
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());
        foreach (Channel ch in channels)
        {
          if (ch.IsTv == false) continue;
          bool hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.FreeToAir)
            {
              hasFta = true;
            }
            if (!detail.FreeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.DisplayName, imageIndex, ch.IdChannel);

          mpComboBoxChannels.Items.Add(item);
        }
      }
      else
      {
        ChannelGroup group = ChannelGroup.Retrieve(idItem.Id);
        IList<GroupMap> maps = group.ReferringGroupMap();
        foreach (GroupMap map in maps)
        {
          Channel ch = map.ReferencedChannel();
          if (ch.IsTv == false) continue;
          bool hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();
          foreach (TuningDetail detail in tuningDetails)
          {
            if (detail.FreeToAir)
            {
              hasFta = true;
            }
            if (!detail.FreeToAir)
            {
              hasScrambled = true;
            }
          }

          int imageIndex;
          if (hasFta && hasScrambled)
          {
            imageIndex = 5;
          }
          else if (hasScrambled)
          {
            imageIndex = 4;
          }
          else
          {
            imageIndex = 3;
          }
          ComboBoxExItem item = new ComboBoxExItem(ch.DisplayName, imageIndex, ch.IdChannel);
          mpComboBoxChannels.Items.Add(item);
        }
      }
      if (mpComboBoxChannels.Items.Count > 0)
        mpComboBoxChannels.SelectedIndex = 0;
    }

    private void mpComboBoxProgramFields_SelectedIndexChanged(object sender, EventArgs e)
    {
      var prgField = mpComboBoxProgramFields.SelectedItem as ProgramField;
      if (prgField != null)
      {
        Point location = mpTextBoxProgramValue.Location;
        Size size = mpTextBoxProgramValue.Size;
        string name = mpTextBoxProgramValue.Name;
        
        //groupBox1.Controls.Remove(mpTextBoxProgramValue);
        groupBox1.Controls.RemoveByKey("mpTextBoxProgramValue");
        mpTextBoxProgramValue.Dispose();

        Control ctrl = prgField.Control;
        ctrl.Size = size;
        ctrl.Location = location;
        ctrl.Name = name;
        groupBox1.Controls.Add(ctrl);

        mpComboBoxOperators.Items.Clear();
        foreach (ConditionOperator cond in prgField.ConditionOperators)
        {
          mpComboBoxOperators.Items.Add(cond);          
        }
      }
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      ProgramField prgField = mpComboBoxProgramFields.SelectedItem as ProgramField;
      if(prgField != null)
      {
        ConditionOperator op = (ConditionOperator) mpComboBoxOperators.SelectedItem;
        if (prgField.Type == typeof (string))
        {
          int index = groupBox1.Controls.IndexOfKey("mpTextBoxProgramValue");
          if (index > -1)
          {            
            var prgCond = new ProgramCondition<string>(prgField.Name, groupBox1.Controls[index].Text, op);
            var item = new ListViewItem
                                  {Tag = prgCond, Text = prgCond.ToString(), ToolTipText = prgCond.ToString()};
            mpListViewPrograms.Items.Add(item);
          }
        }
        else if (prgField.Type == typeof(int))
        {          
          int index = groupBox1.Controls.IndexOfKey("mpTextBoxProgramValue");
          if (index > -1)
          {
            var prgCond = new ProgramCondition<int>(prgField.Name, ((MPNumericTextBox)groupBox1.Controls[index]).Value, op);
            var item = new ListViewItem
                                  {Tag = prgCond, Text = prgCond.ToString(), ToolTipText = prgCond.ToString()};
            mpListViewPrograms.Items.Add(item);
          }
        }

        else if (prgField.Type == typeof(DateTime))
        {
          int index = groupBox1.Controls.IndexOfKey("mpTextBoxProgramValue");
          if (index > -1)
          {
            var prgCond = new ProgramCondition<DateTime>(prgField.Name, ((DateTimePicker)groupBox1.Controls[index]).Value, op);
            var item = new ListViewItem { Tag = prgCond, Text = prgCond.ToString(), ToolTipText = prgCond.ToString() };
            mpListViewPrograms.Items.Add(item);
          }
        }
        
      }
      
    }
  }

  internal class ProgramField
  {
    public ProgramField(string name, Type type)
    {
      Name = name;
      Type = type;
      ConditionOperators = GetConditionOperator(type);
      Control = GetControl(type);
      
    }

    private Control GetControl(Type type)
    {
      Control c;
      if (type == typeof(int))
      {
        c = new MPNumericTextBox();
      }
      else if (type == typeof(DateTime))
      {
        DateTimePicker dateTimePicker = new DateTimePicker();
        dateTimePicker.Format = DateTimePickerFormat.Custom;
        dateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
        c = dateTimePicker;
      }
      else
      {
        c = new MPTextBox();
      }
      return c;
    }

    private static IList<ConditionOperator> GetConditionOperator(Type type)
    {
      IList<ConditionOperator> conditionOperators = new List<ConditionOperator>();
      conditionOperators.Add(ConditionOperator.Equals);
      if (type == typeof(string))
      {
        conditionOperators.Add(ConditionOperator.Contains);
        conditionOperators.Add(ConditionOperator.NotContains);
        conditionOperators.Add(ConditionOperator.StartsWith);
      }
      return conditionOperators;
    }

    public Control Control { get; private set; }
    public string Name { get; private set; }
    public Type Type { get; private set; }
    public IList<ConditionOperator> ConditionOperators { get; private set; }

    public override string ToString()
    {
      return Name;
    }
  }
}