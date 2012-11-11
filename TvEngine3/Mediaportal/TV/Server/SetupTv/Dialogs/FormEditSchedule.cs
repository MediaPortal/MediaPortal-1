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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mediaportal.TV.Server.RuleBasedScheduler;
using Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEditSchedule : Form
  {

    private Schedule _schedule;
    private Program _program;
    private ScheduleRulesTemplate _scheduleRulesTemplate;

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

    public ScheduleRulesTemplate ScheduleRulesTemplate
    {
      get { return _scheduleRulesTemplate; }
      set { _scheduleRulesTemplate = value; }
    }

    private void FormEditSchedule_Load(object sender, System.EventArgs e)
    {
      Init();
      PreFillInProgramData();

    }

    private void PreFillInProgramData()
    {
      PreFillInProgramDataForPrograms();
      PreFillInProgramDataForChannels();
      PreFillInProgramDataForCategories();
      PreFillInProgramDataForCredits();
      PreFillInProgramDataForDates();
      SetupEventHandlersForDates();
    }

    private void SetupEventHandlersForDates()
    {
      dateTimePickerStartingBetweenTo.ValueChanged += new System.EventHandler(dateTimePickerStartingBetweenTo_ValueChanged);
      dateTimePickerStartingAround.ValueChanged += new System.EventHandler(dateTimePickerStartingAround_ValueChanged);
      dateTimePickerStartingBetweenFrom.ValueChanged += new System.EventHandler(dateTimePickerStartingBetweenFrom_ValueChanged);
    }

    private void PreFillInProgramDataForDates()
    {
      dateTimePickerOnDate.Value = Program.StartTime;      
      dateTimePickerStartingBetweenFrom.Value = Program.StartTime;
      dateTimePickerStartingBetweenTo.Value = Program.EndTime;
      dateTimePickerStartingBetweenFrom.Checked = false;
      dateTimePickerStartingBetweenTo.Checked = false;      
      dateTimePickerStartingAround.Value = Program.StartTime;      
      mpNumericTextBoxStartingAroundDeviation.Value = 0;
      SetStartingBetweenState();
    }

    private void PreFillInProgramDataForChannels()
    {
      var channel = Program.Channel;
      listBoxChannels.Items.Add(channel);
    }

    private void PreFillInProgramDataForPrograms()
    {
      var props = GetProgramPropertyInfos();
      foreach (PropertyInfo prop in props)
      {
        if (prop.Name.ToUpperInvariant().Equals("TITLE"))
        {
          var prgField = new ProgramField(prop.Name, prop.PropertyType);
          ConditionOperator op = ConditionOperator.Equals;          
          var prgCond = new SelectedProgramCondition<string>(prgField.Name, Program.Title, op);
          AddToListBox(prgCond, prgField.Name, listBoxPrograms);
          break;
        }
      }
                                    
    }

    private void PreFillInProgramDataForCredits()
    {
      IList<ProgramCredit> programCredits = Program.ProgramCredits;

      if (programCredits.Count == 0)
      {
        programCredits = ServiceAgents.Instance.ProgramServiceAgent.ListAllCredits();
      }

      if (programCredits != null)
      {        
        foreach (var programCreditDto in programCredits)
        {
          AddToCheckedListBox(programCreditDto, programCreditDto.ToString(), listBoxCredits, false);  
        }
      }
    }

    private void PreFillInProgramDataForCategories()
    {
      ProgramCategory category = Program.ProgramCategory;
      if (category == null)
      {
        category = ServiceAgents.Instance.ProgramServiceAgent.ListAllCategories().FirstOrDefault();
      }
      if (category != null)
      {
        TVDatabase.Entities.ProgramCategory programCategoryDto = category;
        AddToCheckedListBox(programCategoryDto, programCategoryDto.ToString(), listBoxCategories, false);
      }
    }

    private void AddTvGroups()
    {
      mpComboBoxChannelsGroup.Items.Clear();
      ChannelGroupIncludeRelationEnum include = ChannelGroupIncludeRelationEnum.GroupMaps;      
      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(MediaTypeEnum.TV, include);      
      foreach (ChannelGroup group in groups)
        mpComboBoxChannelsGroup.Items.Add(new ComboBoxExItem(group.GroupName, -1, group.IdGroup));
      if (mpComboBoxChannelsGroup.Items.Count == 0)
        mpComboBoxChannelsGroup.Items.Add(new ComboBoxExItem("(no groups defined)", -1, -1));
      mpComboBoxChannelsGroup.SelectedIndex = 0;
    }

    private void Init()
    {
      SetRecordingInterval();
      AddTvGroups();
      SetScheduleName();
      PopulateProgramFieldsComboBox();              
      PopulateCategoriesComboBox();
      PopulateOperatorsComboBox();
      PopulateRolesComboBox();
      PopulateKeepMethodsComboBox();      
    }

    private void PopulateKeepMethodsComboBox()
    {
      foreach (string name in Enum.GetNames(typeof (KeepMethodType)))
      {
        mpComboBoxKeepMethods.Items.Add(name);
      }
    }

    private void PopulateRolesComboBox()
    {
      IList<string> roles = ServiceAgents.Instance.ProgramServiceAgent.ListAllDistinctCreditRoles();      
      foreach (var programCredit in roles)
      {
        mpComboBoxRoles.Items.Add(programCredit);
      }
    }

    private void PopulateOperatorsComboBox()
    {
      foreach (object enumValue in Enum.GetValues(typeof (ConditionOperator)))
      {
        mpComboBoxOperators.Items.Add(enumValue);
      }
    }

    private void PopulateCategoriesComboBox()
    {
      IList<ProgramCategory> categories = ServiceAgents.Instance.ProgramServiceAgent.ListAllCategories();
      IList<TVDatabase.Entities.ProgramCategory> categoriesDtos = categories;
      foreach (var programCategory in categoriesDtos)
      {
        mpComboBoxCategories.Items.Add(programCategory);
      }
    }

    private void SetScheduleName()
    {
      if (_program != null)
      {
        mpTextBoxScheduleName.Text = _program.Title;
      }
      else if (_schedule != null)
      {
        mpTextBoxScheduleName.Text = _schedule.ProgramName;
      }
    }

    private void SetRecordingInterval()
    {
      mpNumericTextBoxPreRec.Value = int.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "7").Value);
      mpNumericTextBoxPostRec.Value = int.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", "10").Value);
    }

    private void PopulateProgramFieldsComboBox()
    {
      IEnumerable<PropertyInfo> props = GetProgramPropertyInfos();
      foreach (ProgramField pField in from prop in props
                                      where prop.PropertyType == typeof (string) ||
                                            prop.PropertyType == typeof (DateTime) ||
                                            prop.PropertyType == typeof (int)
                                      select new ProgramField(prop.Name, prop.PropertyType))
      {
        mpComboBoxProgramFields.Items.Add(pField);
      }
    }

    private static IEnumerable<PropertyInfo> GetProgramPropertyInfos()
    {      
      /*var programDto = new TVDatabaseEntities.Program();
      Type t = programDto.GetType();
      var props = t.GetProperties().Where(
        prop => Attribute.IsDefined(prop, typeof(MetadataTypeAttribute)));
      return props;*/

      MetadataTypeAttribute[] metadataTypes = typeof(TVDatabase.Entities.Program).GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().ToArray();
      MetadataTypeAttribute metadata = metadataTypes.FirstOrDefault();
      IList<PropertyInfo> props = new List<PropertyInfo>();
      if (metadata != null)
      {
        PropertyInfo[] properties = metadata.MetadataClassType.GetProperties();
        foreach (PropertyInfo propertyInfo in properties.Where(propertyInfo => Attribute.IsDefined(propertyInfo, typeof(ProgramAttribute)))) 
        {
          props.Add(propertyInfo);
        }
      }
      return props;
    }


    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      KeepMethodType enumKeepMethodType;
      Enum.TryParse((string)mpComboBoxKeepMethods.SelectedItem, out enumKeepMethodType);
      var schedule = new RuleBasedSchedule
                       {
                         ScheduleName = mpTextBoxScheduleName.Text,
                         MaxAirings = int.MaxValue,
                         Priority = mpNumericTextBoxPriority.Value,
                         KeepMethod = (int) enumKeepMethodType,
                         KeepDate = dateTimePickerOnDate.MinDate,
                         PreRecordInterval = mpNumericTextBoxPreRec.Value,
                         PostRecordInterval = mpNumericTextBoxPostRec.Value
                       };

      var rules = new ScheduleConditionList();

      foreach (object obj in listBoxPrograms.Items)
      {
        if (obj is SelectedProgramCondition<string>)
        {
          var selectedProgramCondition = obj as SelectedProgramCondition<string>;
          var programCondition = new ProgramCondition<string>(selectedProgramCondition.Name, selectedProgramCondition.Value, selectedProgramCondition.ConditionOperator);
          rules.Add(programCondition);
        }
        else if (obj is SelectedProgramCondition<int>)
        {
          var selectedProgramCondition = obj as SelectedProgramCondition<int>;
          var programCondition = new ProgramCondition<int>(selectedProgramCondition.Name, selectedProgramCondition.Value, selectedProgramCondition.ConditionOperator);
          rules.Add(programCondition);
        }
        else if (obj is SelectedProgramCondition<DateTime>)
        {
          var selectedProgramCondition = obj as SelectedProgramCondition<DateTime>;
          var programCondition = new ProgramCondition<DateTime>(selectedProgramCondition.Name, selectedProgramCondition.Value, selectedProgramCondition.ConditionOperator);
          rules.Add(programCondition);
        }
      }

      if (radioOnChannels.Checked)
      {
        var channelList = new ObservableCollection<TVDatabase.Entities.Channel>();
        foreach (TVDatabase.Entities.Channel channel in listBoxChannels.Items)
        {
          channelList.Add(channel);          
        }
        if (channelList.Count > 0)
        {
          var onChannelsCondition = new OnChannelsCondition(channelList);
          rules.Add(onChannelsCondition);
        }
      }
      else if (radioNotOnChannels.Checked)
      {
        var channelList = new ObservableCollection<TVDatabase.Entities.Channel>();
        foreach (TVDatabase.Entities.Channel channel in listBoxChannels.Items)
        {
          channelList.Add(channel);
        }
        if (channelList.Count > 0)
        {
          var notOnChannelsCondition = new NotOnChannelsCondition(channelList);
          rules.Add(notOnChannelsCondition);
        }
      }



      if (mpRadioButtonInCategory.Checked)
      {
        IList<TVDatabase.Entities.ProgramCategory> categoryList = new ObservableCollection<TVDatabase.Entities.ProgramCategory>();
        foreach (TVDatabase.Entities.ProgramCategory categoryDto in listBoxCategories.CheckedItems)
        {
          categoryList.Add(categoryDto);
        }
        if (categoryList.Count > 0)
        {
          var onCategoryCondition = new OnCategoryCondition(categoryList);
          rules.Add(onCategoryCondition);
        }
      }
      else if (mpRadioButtonNotInCategory.Checked)
      {
        IList<TVDatabase.Entities.ProgramCategory> categoryList = new ObservableCollection<TVDatabase.Entities.ProgramCategory>();
        foreach (TVDatabase.Entities.ProgramCategory categoryDto in listBoxCategories.CheckedItems)
        {
          categoryList.Add(categoryDto);
        }
        if (categoryList.Count > 0)
        {
          var notOnCategoryCondition = new NotOnCategoryCondition(categoryList);
          rules.Add(notOnCategoryCondition);
        }
      }


      IList<TVDatabase.Entities.ProgramCredit> creditsList = new ObservableCollection<TVDatabase.Entities.ProgramCredit>();
      foreach (TVDatabase.Entities.ProgramCredit creditDto in listBoxCredits.CheckedItems)
      {
        creditsList.Add(creditDto);
      }
      if (creditsList.Count > 0)
      {
        var creditCondition = new CreditCondition(creditsList);
        rules.Add(creditCondition);
      }

      if (dateTimePickerOnDate.Checked)
      {
        var onDateCondition = new OnDateCondition(dateTimePickerOnDate.Value);
        rules.Add(onDateCondition);
      }

    if (dateTimePickerStartingAround.Checked)
      {
        var startingAroundCondition = new StartingAroundCondition(dateTimePickerStartingAround.Value, mpNumericTextBoxStartingAroundDeviation.Value);
        rules.Add(startingAroundCondition);
      }

      if (dateTimePickerStartingBetweenFrom.Checked && dateTimePickerStartingBetweenTo.Checked)
      {
        var startingBetweenCondition = new StartingBetweenCondition(dateTimePickerStartingBetweenFrom.Value, dateTimePickerStartingBetweenTo.Value);
        rules.Add(startingBetweenCondition);
      }

      IList<DayOfWeek> ondays = new ObservableCollection<DayOfWeek>();
      if (checkBoxMonday.Checked)
      {        
        ondays.Add(DayOfWeek.Monday);
      }            
      if (checkBoxTuesday.Checked)
      {
        ondays.Add(DayOfWeek.Tuesday);
      }            
      if (checkBoxWednesday.Checked)
      {
        ondays.Add(DayOfWeek.Wednesday);
      }            
      if (checkBoxThursday.Checked)
      {
        ondays.Add(DayOfWeek.Thursday);
      }            
      if (checkBoxFriday.Checked)
      {
        ondays.Add(DayOfWeek.Friday);
      }            
      if (checkBoxSaturday.Checked)
      {
        ondays.Add(DayOfWeek.Saturday);
      }            
      if (checkBoxSunday.Checked)
      {
        ondays.Add(DayOfWeek.Sunday);
      }            
      
      if (ondays.Count > 0)
      {       
        var onDayCondition = new OnDayCondition(ondays);
        rules.Add(onDayCondition);
      }

      if (checkBoxOnlyNewEpisodes.Checked)
      {
        var onlyRecordNewEpisodesCondition = new OnlyRecordNewEpisodesCondition();  //todo populate episodes list.
        rules.Add(onlyRecordNewEpisodesCondition);
      }

      if (checkBoxSkipRepeats.Checked)
      {        
        var skipRepeatsCondition = new SkipRepeatsCondition();
        rules.Add(skipRepeatsCondition);
      }

      if (checkBoxNewTitles.Checked)
      {
        var onlyRecordNewTitlesCondition = new OnlyRecordNewTitlesCondition(); //todo populate titles list.
        rules.Add(onlyRecordNewTitlesCondition);
      }

      schedule.Rules = ScheduleConditionHelper.Serialize<ScheduleConditionList>(rules);      
      ServiceAgents.Instance.ScheduleServiceAgent.SaveRuleBasedSchedule(schedule);
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
        IList<Channel> channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllChannels();
        foreach (Channel ch in channels)
        {
          if (ch.MediaType != (decimal) MediaTypeEnum.TV) continue;
          bool hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
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

          if (hasFta && hasScrambled)
          {
          }
          else if (hasScrambled)
          {
          }
          else
          {
          }          
          //ComboBoxExItem item = new ComboBoxExItem(ch.displayName, imageIndex, ch.idChannel);         
          mpComboBoxChannels.Items.Add(ch);
        }
      }
      else
      {
        ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroup(idItem.Id);
        IList<GroupMap> maps = group.GroupMaps;
        bool hasFta = false;
        foreach (GroupMap map in maps)
        {
          Channel ch = map.Channel;
          if (ch.MediaType != (decimal) MediaTypeEnum.TV)
          hasFta = false;
          bool hasScrambled = false;
          IList<TuningDetail> tuningDetails = ch.TuningDetails;
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
          //ComboBoxExItem item = new ComboBoxExItem(ch.displayName, imageIndex, ch.idChannel);
          mpComboBoxChannels.Items.Add(ch);
          //mpComboBoxChannels.Items.Add(item);
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
      mpComboBoxOperators.SelectedIndex = 0;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      AddProgramCondition();
    }

    private void AddProgramCondition()
    {
      var prgField = mpComboBoxProgramFields.SelectedItem as ProgramField;
      if (prgField != null)
      {
        ConditionOperator op = (ConditionOperator)mpComboBoxOperators.SelectedItem;
        object prgCond = null;
        int index = groupBox1.Controls.IndexOfKey("mpTextBoxProgramValue");
        if (index > -1)
        {
          prgCond = CreateSelectedProgramCondition(groupBox1.Controls[index], op, prgField);
          AddToListBox(prgCond, prgField.Name, listBoxPrograms);
        }
      }
    }

    private object CreateSelectedProgramCondition(Control ctrl, ConditionOperator op, ProgramField prgField)
    {
      object prgCond;      
      if (prgField.Type == typeof (int))
      {
        prgCond = new SelectedProgramCondition<int>(prgField.Name,
                                                    ((MPNumericTextBox)ctrl).Value, op);
      }
      else if (prgField.Type == typeof (DateTime))
      {
        prgCond = new SelectedProgramCondition<DateTime>(prgField.Name,
                                                         ((DateTimePicker)ctrl).Value, op);
      }
      else
      {
        prgCond = new SelectedProgramCondition<string>(prgField.Name, ctrl.Text, op);
      }
      return prgCond;
    }

    private void AddToCheckedListBox(object prgCond, string name, CheckedListBox checkedListBox, bool isChecked)
    {
      if (DoesListBoxItemAlreadyExist(prgCond, name, checkedListBox)) return;      
      checkedListBox.Items.Add(prgCond, isChecked);            
    }

    private void AddToListBox(object prgCond, string name, ListBox listBox)
    {      
      if (DoesListBoxItemAlreadyExist(prgCond, name, listBox)) return;      
      listBox.Items.Add(prgCond); 
    }

    private static bool DoesListBoxItemAlreadyExist(object prgCond, string name, ListBox listBox)
    {
      if (listBox.Items.Cast<object>().Any(it => it.ToString().IndexOf(name) >= 0))
      {
        //MessageBox.Show("item already exists '" + prgCond.ToString() + "'");
        return true;
      }
      return false;
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      ClearListBoxSelection(listBoxPrograms);
    }

    private void ClearListBoxSelection(ListBox listBox)
    {
      while (listBox.SelectedItems.Count > 0)
      {
        listBox.Items.Remove(listBox.SelectedItems[0]);
      }
    }

    private void mpButtonAddChannelCondition_Click(object sender, EventArgs e)
    {
      var channel = mpComboBoxChannels.SelectedItem as TVDatabase.Entities.Channel;
      if (channel != null)
      {
        AddToListBox(channel, channel.DisplayName, listBoxChannels);
      }
    }

    private void mpButtonRemoveChannelCondition_Click(object sender, EventArgs e)
    {
      ClearListBoxSelection(listBoxChannels);
    }

    private void mpButtonRemoveCategoryCondition_Click(object sender, EventArgs e)
    {
      ClearListBoxSelection(listBoxCategories);
    }

    private void mpButtonAddCategoryCondition_Click(object sender, EventArgs e)
    {
      var categoryDto = mpComboBoxCategories.SelectedItem as TVDatabase.Entities.ProgramCategory;
      if (categoryDto != null)
      {
        AddToCheckedListBox(categoryDto, categoryDto.Category, listBoxCategories, true);        
      }      
    }

    private void radioOnAllChannels_CheckedChanged(object sender, EventArgs e)
    {
      listBoxChannels.Items.Clear();
      SetEnabledStateForChannelsControls();
    }

    private void SetEnabledStateForChannelsControls()
    {
      listBoxChannels.Enabled = !radioOnAllChannels.Checked;
      mpComboBoxChannelsGroup.Enabled = !radioOnAllChannels.Checked;
      mpComboBoxChannels.Enabled = !radioOnAllChannels.Checked;
      mpButtonAddChannelCondition.Enabled = !radioOnAllChannels.Checked;
      mpButtonAddAllChannelCondition.Enabled = !radioOnAllChannels.Checked;
      mpButtonRemoveChannelCondition.Enabled = !radioOnAllChannels.Checked;
    }

    private void radioOnChannels_CheckedChanged(object sender, EventArgs e)
    {
      listBoxChannels.Items.Clear();
      SetEnabledStateForChannelsControls();
    }

    private void radioNotOnChannels_CheckedChanged(object sender, EventArgs e)
    {
      listBoxChannels.Items.Clear();
      SetEnabledStateForChannelsControls();
    }

    private void mpButtonCreditAdd_Click(object sender, EventArgs e)
    {
      TVDatabase.Entities.ProgramCredit credit = new TVDatabase.Entities.ProgramCredit
                                  {Role = mpComboBoxRoles.SelectedItem as string, Person = mpTextBoxPerson.Text};

      AddToCheckedListBox(credit, credit.ToString(), listBoxCredits, true);
    }

    private void mpButtonCreditRemove_Click(object sender, EventArgs e)
    {
      ClearListBoxSelection(listBoxCredits);
    }

    private void mpRadioButtonInAllCategories_CheckedChanged(object sender, EventArgs e)
    {

      listBoxCategories.Items.Clear();
      SetEnabledStateForCategoriesControls();
    }

    private void SetEnabledStateForCategoriesControls()
    {
      listBoxCategories.Enabled = !mpRadioButtonInAllCategories.Checked;
      mpComboBoxCategories.Enabled = !mpRadioButtonInAllCategories.Checked;
      mpButtonAddCategoryCondition.Enabled = !mpRadioButtonInAllCategories.Checked;
      mpButtonAddAllCategoryCondition.Enabled = !mpRadioButtonInAllCategories.Checked;
      mpButtonRemoveCategoryCondition.Enabled = !mpRadioButtonInAllCategories.Checked;
    }

    private void mpRadioButtonInCategory_CheckedChanged(object sender, EventArgs e)
    {
      listBoxCategories.Items.Clear();
      SetEnabledStateForCategoriesControls();
    }

    private void mpRadioButtonNotInCategory_CheckedChanged(object sender, EventArgs e)
    {
      listBoxCategories.Items.Clear();
      SetEnabledStateForCategoriesControls();
    }

    private void dateTimePickerStartingAround_ValueChanged(object sender, EventArgs e)
    {
      mpNumericTextBoxStartingAroundDeviation.Enabled = dateTimePickerStartingAround.Checked;
      SetStartingBetweenState();
    }    

    private void dateTimePickerOnDate_ValueChanged(object sender, EventArgs e)
    {
      checkBoxMonday.Checked = false;
      checkBoxMonday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxTuesday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxWednesday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxThursday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxFriday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxSaturday.Enabled = !dateTimePickerOnDate.Checked;
      checkBoxSunday.Enabled = !dateTimePickerOnDate.Checked;
    }

    private void checkBoxMonday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void SetStartingBetweenState()
    {
      
      dateTimePickerStartingBetweenFrom.Enabled = !dateTimePickerStartingAround.Checked;
      if (!dateTimePickerStartingBetweenFrom.Enabled)
      {
        dateTimePickerStartingBetweenFrom.Checked = false;
      }
      
      dateTimePickerStartingBetweenTo.Enabled = !dateTimePickerStartingAround.Checked;
      if (!dateTimePickerStartingBetweenTo.Enabled)
      {
        dateTimePickerStartingBetweenTo.Checked = false;  
      }
    }

    private void SetStartingAroundState()
    {      
      dateTimePickerStartingAround.Enabled = !dateTimePickerStartingBetweenFrom.Checked &&
                                             !dateTimePickerStartingBetweenTo.Checked;

      mpNumericTextBoxStartingAroundDeviation.Enabled = !dateTimePickerStartingBetweenFrom.Checked &&
                                             !dateTimePickerStartingBetweenTo.Checked;

      if (!dateTimePickerStartingAround.Enabled)
      {
        dateTimePickerStartingAround.Checked = false;
      }
    }


    private void SetOnDateState()
    {      
      dateTimePickerOnDate.Enabled = !checkBoxMonday.Checked && !checkBoxTuesday.Checked &&
                                     !checkBoxWednesday.Checked && !checkBoxThursday.Checked && !checkBoxFriday.Checked &&
                                     !checkBoxSaturday.Checked && !checkBoxSunday.Checked;
      if (!dateTimePickerOnDate.Enabled)
      {
        dateTimePickerOnDate.Checked = false;   
      }
    }
   
    private void checkBoxTuesday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void checkBoxWednesday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void checkBoxThursday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void checkBoxFriday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void checkBoxSaturday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void checkBoxSunday_CheckedChanged(object sender, EventArgs e)
    {
      SetOnDateState();
    }

    private void dateTimePickerStartingBetweenFrom_ValueChanged(object sender, EventArgs e)
    {
      SetStartingAroundState();
    }

    private void dateTimePickerStartingBetweenTo_ValueChanged(object sender, EventArgs e)
    {
      SetStartingAroundState();
    }    
  }

  internal class SelectedProgramCondition<T>
  {
    public SelectedProgramCondition(string name, T value, ConditionOperator op)
    {
      Name = name;
      Value = value;
      ConditionOperator = op;
    }

    public override string ToString()
    {
      return ("[" + Name + "] " + ConditionOperator + " [" + Value + "]");
    }

    public T Value { get; private set; }
    public ConditionOperator ConditionOperator { get; private set; }
    public string Name { get; set; }
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