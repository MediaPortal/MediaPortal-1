using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;

namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// todo: 
	///   -directory history
	///   -nicer 'up' icon
	///   -search by title
	///   -filtering
	///   -
	/// </summary>
	public class GUITVSearch:GUIWindow, IComparer
	{
    enum Controls:int
    {
      SortBy=2,
      SortAscending=3,
      SearchByGenre=4,
			SearchByTitle=5,
			SearchByDescription=6,
      btnLetter=7,
			btnShow=8,
			btnEpisode=9,
      ListControl=10,
			TitleControl=11,
			labelNumberOfItems=12,
			LabelProgramTitle=13,
			LabelProgramTime=14,
			LabelProgramDescription=15,
			LabelProgramGenre=17,
    }

    enum SearchMode
    {
      Genre, 
      Title,
			Description
    }
    enum SortMethod
    {
      Name,
      Date,
      Channel
    }
    SearchMode _SearchMode=SearchMode.Genre;
    SortMethod _SortMethod=SortMethod.Name;
    bool       _Ascending=true;
    int        _Level=0;
    string     _CurrentGenre=String.Empty;
    ArrayList   _recordings=new ArrayList();
    int        _SearchKind=-1;
    string     _SearchCriteria=String.Empty;
		string     _FilterLetter="#";
		string     _FilterShow="";
		string     _FilterEpsiode="";

		public GUITVSearch()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SEARCHTV;
		}

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvsearch.xml");
      return bResult;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.PreviousWindow();
          return;
        }
        case Action.ActionType.ACTION_SHOW_GUI:
          if (Recorder.IsViewing() || (g_Player.Playing && g_Player.IsTVRecording))
          {
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
          break;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          _recordings.Clear();
					
					if ( !GUITVHome.IsTVWindow(message.Param1) )
					{
						if (! g_Player.Playing)
						{
							if (GUIGraphicsContext.ShowBackground)
							{
								// stop timeshifting & viewing... 
	              
								Recorder.StopViewing();
							}
						}
					}
        }
        break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          _recordings.Clear();
          _SearchKind=-1;
          _SearchCriteria=String.Empty;
          TVDatabase.GetRecordings(ref _recordings);
          Update();
					
					GUIControl.AddItemLabelControl(GetID,(int)Controls.btnLetter,"#");
					for (char k='A'; k <='Z'; k++)
					{
						GUIControl.AddItemLabelControl(GetID,(int)Controls.btnLetter,k.ToString());
					}
          return true;
        }
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.SearchByGenre)
          {
            _SearchMode=SearchMode.Genre;
            _Level=0;
						_FilterEpsiode="";
						_FilterLetter="#";
						_FilterShow="";
            _SearchKind=-1;
            _SearchCriteria=String.Empty;
            Update();
          }
          if (iControl==(int)Controls.SearchByTitle)
					{
						_FilterEpsiode="";
						_FilterLetter="#";
						_FilterShow="";
            _SearchMode=SearchMode.Title;
            _Level=0;
            _SearchKind=-1;
            _SearchCriteria=String.Empty;
            Update();
					}
					if (iControl==(int)Controls.SearchByDescription)
					{
						_FilterEpsiode="";
						_FilterLetter="#";
						_FilterShow="";
						_SearchMode=SearchMode.Description;
						_Level=0;
						_SearchKind=-1;
						_SearchCriteria=String.Empty;
						Update();
					}
          if (iControl==(int)Controls.SortAscending)
          {
            _Ascending=!_Ascending;
            Update();
            GUIControl.FocusControl(GetID, iControl);
          }
          
          if (iControl==(int)Controls.SortBy)
          {
            switch ( _SortMethod)
            {
              case SortMethod.Name: 
                _SortMethod = SortMethod.Channel;
                break;
              case SortMethod.Channel: 
                _SortMethod = SortMethod.Date;
                break;
              case SortMethod.Date: 
                _SortMethod = SortMethod.Name;
                break;
            }
            Update();
            GUIControl.FocusControl(GetID, iControl);
          }

          if (iControl==(int)Controls.ListControl||iControl==(int)Controls.TitleControl)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
          }
					if (iControl==(int)Controls.btnLetter)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						_FilterLetter=msg.Label;
						_FilterShow="";
						_FilterEpsiode="";
						Update();
					}
					if (iControl==(int)Controls.btnShow)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						_FilterShow=msg.Label;
						_FilterEpsiode="";
						Update();
					}
					if (iControl==(int)Controls.btnEpisode)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						_FilterEpsiode=msg.Label;
						Update();
					}
					break;

				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					UpdateDescription();
					break;
      }
      return base.OnMessage(message);
    }

    void Update()
    {
			GUIListControl cntlList;
			GUIControl.ClearControl(GetID,(int)Controls.ListControl);
      GUIControl.ClearControl(GetID,(int)Controls.TitleControl);
      if (_Level==0 && _SearchMode==SearchMode.Genre)
      {
        GUIControl.ShowControl(GetID,(int)Controls.ListControl);
        GUIControl.HideControl(GetID,(int)Controls.TitleControl);
        GUIControl.FocusControl(GetID,(int)Controls.ListControl);
				GUIControl.DisableControl(GetID,(int)Controls.btnEpisode);
				GUIControl.DisableControl(GetID,(int)Controls.btnLetter);
				GUIControl.DisableControl(GetID,(int)Controls.btnShow);

				GUIControl.HideControl(GetID,(int)Controls.LabelProgramDescription);
				GUIControl.HideControl(GetID,(int)Controls.LabelProgramGenre);
				GUIControl.HideControl(GetID,(int)Controls.LabelProgramTime);
				GUIControl.HideControl(GetID,(int)Controls.LabelProgramTitle);
				cntlList = (GUIListControl)GetControl((int)Controls.ListControl);
      }
      else
      {
				if ( _FilterShow!="")
				{
					GUIControl.HideControl(GetID,(int)Controls.ListControl);
					GUIControl.ShowControl(GetID,(int)Controls.TitleControl);
					GUIControl.FocusControl(GetID,(int)Controls.TitleControl);
					GUIControl.ShowControl(GetID,(int)Controls.LabelProgramDescription);
					GUIControl.ShowControl(GetID,(int)Controls.LabelProgramGenre);
					GUIControl.ShowControl(GetID,(int)Controls.LabelProgramTime);
					GUIControl.ShowControl(GetID,(int)Controls.LabelProgramTitle);
					cntlList = (GUIListControl)GetControl((int)Controls.TitleControl);
				}
				else
				{
					GUIControl.ShowControl(GetID,(int)Controls.ListControl);
					GUIControl.HideControl(GetID,(int)Controls.TitleControl);
					GUIControl.FocusControl(GetID,(int)Controls.ListControl);
					
					GUIControl.HideControl(GetID,(int)Controls.LabelProgramDescription);
					GUIControl.HideControl(GetID,(int)Controls.LabelProgramGenre);
					GUIControl.HideControl(GetID,(int)Controls.LabelProgramTime);
					GUIControl.HideControl(GetID,(int)Controls.LabelProgramTitle);
					cntlList = (GUIListControl)GetControl((int)Controls.ListControl);
				}
				GUIControl.EnableControl(GetID,(int)Controls.btnEpisode);
				GUIControl.EnableControl(GetID,(int)Controls.btnLetter);
				GUIControl.EnableControl(GetID,(int)Controls.btnShow);
				
      }
			GUIControl cntlLabel = GetControl((int)Controls.labelNumberOfItems);
			cntlLabel.YPosition = cntlList.SpinY;

			ArrayList programs = new ArrayList();
			ArrayList episodes = new ArrayList();
			int itemCount=0;
      switch (_SearchMode)
      {
        case SearchMode.Genre:
          if (_Level==0)
          {
            ArrayList genres = new ArrayList();
            TVDatabase.GetGenres(ref genres);
            foreach(string genre in genres)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder=true;
              item.Label=genre;
              item.Path=genre;
              Utils.SetDefaultIcons(item);
              GUIControl.AddListItemControl(GetID,(int)Controls.ListControl,item);
              itemCount++;
            }
          }
          else
          {
            GUIListItem item = new GUIListItem();
            item.IsFolder=true;
            item.Label="..";
            item.Label2="";
            item.Path="";
            item.IconImage="defaultFolderBackBig.png";
            item.IconImageBig="defaultFolderBackBig.png";

            GUIControl.AddListItemControl(GetID,(int)Controls.ListControl,item);
            GUIControl.AddListItemControl(GetID,(int)Controls.TitleControl,item);

            ArrayList titles = new ArrayList();
            TVDatabase.SearchProgramsPerGenre(_CurrentGenre,titles, _SearchKind, _SearchCriteria);
            foreach(TVProgram program in titles)
            {
							//dont show programs which have ended
							if (program.EndTime < DateTime.Now) continue;
							if (_FilterLetter!="#")
							{
								if (_SearchMode==SearchMode.Description)
								{
									if (!program.Description.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
								}
								else if (_SearchMode==SearchMode.Title)
								{
									if (!program.Title.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
								}
								bool add=true;
								foreach (TVProgram prog in programs)
								{
									if (prog.Title == program.Title)
									{
										add=false;
									}
								}
								if (add)
								{
									programs.Add(program);
								}

								if (_FilterShow!="")
								{
									if (program.Title==_FilterShow)
									{
										episodes.Add(program);
									}
								}
							}
							if (_FilterShow!="" && program.Title!=_FilterShow) continue;

							string strTime=String.Format("{0} {1}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
							if (_FilterEpsiode!="" && strTime != _FilterEpsiode) continue;
		
							strTime=String.Format("{0} {1} - {2}", 
								Utils.GetShortDayString(program.StartTime) , 
                program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

              item = new GUIListItem();
              item.IsFolder=false;
              item.Label=program.Title;
              item.Label2=strTime;
              item.Path=program.Title;
              item.MusicTag=program;
							bool isSerie;
              if (IsRecording(program, out isSerie))
              {
								if (isSerie)
									item.PinImage="tvguide_recordserie_button.png";
								else
									item.PinImage="tvguide_record_button.png";
              }
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program.Channel,ref item);
              GUIControl.AddListItemControl(GetID,(int)Controls.ListControl,item);
              GUIControl.AddListItemControl(GetID,(int)Controls.TitleControl,item);
              itemCount++;
            }
          }
          break;
          case SearchMode.Title:
          {
            ArrayList titles = new ArrayList();
            long start=Utils.datetolong( DateTime.Now );
            long end  =Utils.datetolong( DateTime.Now.AddMonths(1) );
            TVDatabase.SearchPrograms(start,end, ref titles, _SearchKind, _SearchCriteria);
            foreach(TVProgram program in titles)
						{
							if (_FilterLetter!="#")
							{
								if (_SearchMode==SearchMode.Description)
								{
									if (!program.Description.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
								}
								else if (_SearchMode==SearchMode.Title)
								{
									if (!program.Title.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
								}
								bool add=true;
								foreach (TVProgram prog in programs)
								{
									if (prog.Title == program.Title)
									{
										add=false;
									}
								}
								if (add)
								{
									programs.Add(program);
								}

								if (_FilterShow!="")
								{
									if (program.Title==_FilterShow)
									{
										episodes.Add(program);
									}
								}
							}

							if (_FilterShow!="" && program.Title!=_FilterShow) continue;

							string strTime=String.Format("{0} {1}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
							if (_FilterEpsiode!="" && strTime != _FilterEpsiode) continue;
		
							strTime=String.Format("{0} {1} - {2}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
								program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder=false;
              item.Label=program.Title;
              item.Label2=strTime;
              item.Path=program.Title;
              item.MusicTag=program;
							bool isSerie;
              if (IsRecording(program, out isSerie))
              {
                if (isSerie)
									item.PinImage="tvguide_recordserie_button.png";
								else
									item.PinImage="tvguide_record_button.png";
              }
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program.Channel,ref item);
              GUIControl.AddListItemControl(GetID,(int)Controls.ListControl,item);
              GUIControl.AddListItemControl(GetID,(int)Controls.TitleControl,item);
              itemCount++;
            }
          }
          break;


				case SearchMode.Description:
				{
					ArrayList titles = new ArrayList();
					long start=Utils.datetolong( DateTime.Now );
					long end  =Utils.datetolong( DateTime.Now.AddMonths(1) );
					TVDatabase.SearchProgramsByDescription(start,end, ref titles, _SearchKind, _SearchCriteria);
					foreach(TVProgram program in titles)
					{
						if (_FilterLetter!="#")
						{
							if (_SearchMode==SearchMode.Description)
							{
								if (!program.Description.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
							}
							else if (_SearchMode==SearchMode.Title)
							{
								if (!program.Title.ToLower().StartsWith(_FilterLetter.ToLower())) continue;
							}
							bool add=true;
							foreach (TVProgram prog in programs)
							{
								if (prog.Title == program.Title)
								{
									add=false;
								}
							}
							if (add)
							{
								programs.Add(program);
							}

							if (_FilterShow!="")
							{
								if (program.Title==_FilterShow)
								{
									episodes.Add(program);
								}
							}
						}

						if (_FilterShow!="" && program.Title!=_FilterShow) continue;

						string strTime=String.Format("{0} {1}", 
							Utils.GetShortDayString(program.StartTime) , 
							program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						if (_FilterEpsiode!="" && strTime != _FilterEpsiode) continue;
		
						strTime=String.Format("{0} {1} - {2}", 
							Utils.GetShortDayString(program.StartTime) , 
							program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

						GUIListItem item = new GUIListItem();
						item.IsFolder=false;
						item.Label=program.Description;
						item.Label2=strTime;
						item.Path=program.Title;
						item.MusicTag=program;
						bool isSerie;
						if (IsRecording(program,out isSerie))
						{
							if (isSerie)
								item.PinImage="tvguide_recordserie_button.png";
							else
								item.PinImage="tvguide_record_button.png";
						}
						Utils.SetDefaultIcons(item);
						SetChannelLogo(program.Channel,ref item);
						GUIControl.AddListItemControl(GetID,(int)Controls.ListControl,item);
						GUIControl.AddListItemControl(GetID,(int)Controls.TitleControl,item);
						itemCount++;
					}
				}
					break;
      }
      string strObjects=String.Format("{0} {1}", itemCount, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);

			GUIControl.ClearControl(GetID, (int)Controls.btnShow);
			programs.Sort();
			int selItem=0;
			int count=0;
			foreach (TVProgram prog in programs)
			{
				GUIControl.AddItemLabelControl(GetID,(int)Controls.btnShow,prog.Title.ToString());			
				if (_FilterShow==prog.Title)
					selItem=count;
				count++;
			}
			GUIControl.SelectItemControl(GetID,(int)Controls.btnShow,selItem);

			selItem=0;
			count=0;
			GUIControl.ClearControl(GetID, (int)Controls.btnEpisode);
			foreach (TVProgram prog in episodes)
			{
				string strTime=String.Format("{0} {1}", 
					Utils.GetShortDayString(prog.StartTime) , 
					prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
				  GUIControl.AddItemLabelControl(GetID,(int)Controls.btnEpisode,strTime.ToString());			
				if (_FilterEpsiode==strTime)
					selItem=count;
				count++;
			}
			GUIControl.SelectItemControl(GetID,(int)Controls.btnEpisode,selItem);
      OnSort();

      string strLine=String.Empty;
      switch (_SortMethod)
      {
        case SortMethod.Name: 
          strLine = GUILocalizeStrings.Get(622);
          break;
        case SortMethod.Channel: 
          strLine = GUILocalizeStrings.Get(620);
          break;
        case SortMethod.Date: 
          strLine = GUILocalizeStrings.Get(621);
          break;
      }
      GUIControl.SetControlLabel(GetID, (int)Controls.SortBy, strLine);

      if (_Ascending)
        GUIControl.DeSelectControl(GetID, (int)Controls.SortAscending);
      else
        GUIControl.SelectControl(GetID, (int)Controls.SortAscending);

			UpdateStateButtons();
    }
    #region Sort Members
    void OnSort()
    {
      GUIListControl list=(GUIListControl)GetControl((int)Controls.ListControl);
      list.Sort(this);
      list=(GUIListControl)GetControl((int)Controls.TitleControl);
      list.Sort(this);
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;

      TVProgram prog1=item1.MusicTag as TVProgram;
      TVProgram prog2=item2.MusicTag as TVProgram;

      int iComp=0;
      switch (_SortMethod)
      {
        case SortMethod.Name:
          if (_Ascending)
          { 
            iComp=String.Compare(item1.Label,item2.Label,true);
          }
          else
          {
              
            iComp=String.Compare(item2.Label,item1.Label,true);
          }
          return iComp;

        case SortMethod.Channel:
          if (prog1!=null && prog2!=null)
          {
           if (_Ascending)
           { 
             iComp=String.Compare(prog1.Channel,prog2.Channel,true);
           }
           else
           {
              
             iComp=String.Compare(prog2.Channel,prog1.Channel,true);
           }
           return iComp;
          }
        return 0;

        case SortMethod.Date:
          if (prog1!=null && prog2!=null)
          {
            if (_Ascending)
            { 
              iComp=(int)(prog1.Start-prog2.Start);
            }
            else
            {
              
              iComp=(int)(prog2.Start-prog1.Start);
            }
            return iComp;
          }
          return 0;
      }
      return iComp;
    }
    #endregion


    GUIListItem GetItem(int iItem)
    {
      int iControl;
      iControl=(int)Controls.ListControl;
      if (_Level!=0)
        iControl=(int)Controls.TitleControl;
      
      GUIListItem  item = GUIControl.GetListItem(GetID,iControl,iItem);
      return item;
    }

    void OnClick(int iItem)
    {
      GUIListItem item = GetItem(iItem);
      if (item==null) return;
      switch (_SearchMode)
      {
        case SearchMode.Genre:
          if (_Level==0)
          {
						_FilterLetter="#";
						_FilterShow="";
						_FilterEpsiode="";
            _CurrentGenre=item.Label;
            _Level++;
            Update();
          }
          else
          {
            if (item.IsFolder) 
						{
							_FilterLetter="#";
							_FilterShow="";
							_FilterEpsiode="";
              _CurrentGenre=String.Empty;
              _Level=0;
              Update();
            }
            else
            {
              TVProgram program = item.MusicTag as TVProgram;
              OnRecord(program);
            }
          }
        break;
				case SearchMode.Title:
				{
					TVProgram prog = item.MusicTag as TVProgram;
					OnRecord(prog);
				}
					break;
				case SearchMode.Description:
				{
					TVProgram prog = item.MusicTag as TVProgram;
					OnRecord(prog);
				}
					break;
      }
    }

    void SetChannelLogo(string channel, ref GUIListItem item)
    {
      string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,channel);
      if (!System.IO.File.Exists(strLogo))
      {
        strLogo="defaultVideoBig.png";
      }
      item.ThumbnailImage=strLogo;
      item.IconImageBig=strLogo;
      item.IconImage=strLogo;
    }

    void OnRecord(TVProgram program)
    {
      if (program==null) return;
      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg!=null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
			
        //610=None
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i=610; i <= 615; ++i)
        {
          dlg.Add( GUILocalizeStrings.Get(i));
        }
				dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri

        dlg.DoModal( GetID);
        if (dlg.SelectedLabel==-1) return;
        TVRecording rec=new TVRecording();
        rec.Title=program.Title;
        rec.Channel=program.Channel;
        rec.Start=program.Start;
        rec.End=program.End;

        switch (dlg.SelectedLabel)
        {
          case 0://none
						foreach (TVRecording rec1 in _recordings)
						{
							if (rec1.IsRecordingProgram(program,true))
							{
								if (rec1.RecType != TVRecording.RecordingType.Once)
								{
									//delete specific series
									rec1.CanceledSeries.Add(program.Start);
									TVDatabase.AddCanceledSerie(rec1,program.Start);
								}
								else
								{
									//cancel recording
									rec1.Canceled=Utils.datetolong(DateTime.Now);
									TVDatabase.UpdateRecording( rec1);
								}
								Recorder.StopRecording(rec1);
							}
						}
            _recordings.Clear();
            TVDatabase.GetRecordings(ref _recordings);
            Update();
            return;
          case 1://once
            rec.RecType=TVRecording.RecordingType.Once;
            break;
          case 2://everytime, this channel
            rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
            break;
          case 3://everytime, all channels
            rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
            break;
          case 4://weekly
            rec.RecType=TVRecording.RecordingType.Weekly;
            break;
          case 5://daily
            rec.RecType=TVRecording.RecordingType.Daily;
            break;
          case 6://Mo-Fi
            rec.RecType=TVRecording.RecordingType.WeekDays;
            break;
        }
        TVDatabase.AddRecording(ref rec);
        _recordings.Clear();
        TVDatabase.GetRecordings(ref _recordings);
        Update();
      }
    }

    bool IsRecording(TVProgram program, out bool isSerie)
    {
      bool bRecording=false;
			isSerie=false;
      foreach (TVRecording record in _recordings)
      {
        if (record.IsRecordingProgram(program,true) ) 
        {
					if (record.RecType !=TVRecording.RecordingType.Once)
						isSerie=true;
          bRecording=true;
          break;
        }
      
      }
      return bRecording;
    }

    

		GUIListItem GetSelectedItem()
		{
			int iControl;
			iControl=(int)Controls.TitleControl;
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
			return item;
		}
		void UpdateDescription()
		{
			if (_Level==0 && _SearchMode==SearchMode.Genre) return;
			GUIListItem item = GetSelectedItem();
			TVProgram prog = item.MusicTag as TVProgram;
			if (prog==null)
			{
				GUIPropertyManager.SetProperty("#TV.Search.Title","");
				GUIPropertyManager.SetProperty("#TV.Search.Genre","");
				GUIPropertyManager.SetProperty("#TV.Search.Time","");
				GUIPropertyManager.SetProperty("#TV.Search.Description","");
				GUIPropertyManager.SetProperty("#TV.Search.thumb","");
				return;
			}

			string strTime=String.Format("{0} {1} - {2}", 
				Utils.GetShortDayString(prog.StartTime) , 
				prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
				prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

			GUIPropertyManager.SetProperty("#TV.Search.Title",prog.Title);
			GUIPropertyManager.SetProperty("#TV.Search.Time",strTime);
			if (prog!=null)
			{
				GUIPropertyManager.SetProperty("#TV.Search.Description",prog.Description);
				GUIPropertyManager.SetProperty("#TV.Search.Genre",prog.Genre);
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.Search.Description","");
				GUIPropertyManager.SetProperty("#TV.Search.Genre","");
			}

    
			string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,prog.Channel);
			if (System.IO.File.Exists(strLogo))
			{
				GUIPropertyManager.SetProperty("#TV.Search.thumb",strLogo);
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.Search.thumb","defaultVideoBig.png");
			}
		}
		void UpdateStateButtons()
		{
				GUIControl.DeSelectControl(GetID,(int)Controls.SearchByDescription);
			GUIControl.DeSelectControl(GetID,(int)Controls.SearchByTitle);
			GUIControl.DeSelectControl(GetID,(int)Controls.SearchByGenre);
			if (_SearchMode==SearchMode.Title)
				GUIControl.SelectControl(GetID,(int)Controls.SearchByTitle);
			if (_SearchMode==SearchMode.Description)
				GUIControl.SelectControl(GetID,(int)Controls.SearchByDescription);
			if (_SearchMode==SearchMode.Genre)
				GUIControl.SelectControl(GetID,(int)Controls.SearchByGenre);
					
		}
	}
}
