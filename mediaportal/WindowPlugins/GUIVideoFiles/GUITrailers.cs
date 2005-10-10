using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUITrailers : GUIWindow
	{
		[SkinControlAttribute(2)]			protected GUIButtonControl buttonOne=null;
		[SkinControlAttribute(3)]			protected GUISelectButtonControl btnletter=null;
		[SkinControlAttribute(4)]			protected GUIListControl listview=null;
		[SkinControlAttribute(5)]			protected GUIToggleButtonControl btntoggleplot = null;
		[SkinControlAttribute(6)]			protected GUIToggleButtonControl btntogglecast = null;
		[SkinControlAttribute(24)]			protected GUIImage poster =null;
		[SkinControlAttribute(50)]			protected GUILabelControl label0 = null;
		[SkinControlAttribute(51)]			protected GUIFadeLabel label1 = null;
		[SkinControlAttribute(52)]			protected GUILabelControl label2 = null;
		[SkinControlAttribute(53)]			protected GUILabelControl label3 = null;
		[SkinControlAttribute(54)]			protected GUILabelControl label4 = null;
		[SkinControlAttribute(55)]			protected GUILabelControl label5 = null;
		[SkinControlAttribute(56)]			protected GUILabelControl label6 = null;
		[SkinControlAttribute(57)]			protected GUITextScrollUpControl castarea=null;
		[SkinControlAttribute(58)]			protected GUILabelControl label8 =null;
		[SkinControlAttribute(59)]			protected GUITextScrollUpControl plotarea=null;

		#region Variables

		int Prev_SelectedItem = 0;	// remember listview selections

		string TempHTML;	// GetWeb string
		string MMSUrl;
		string currentletter ="";
		string casturl;

		DateTime RefreshDaily = DateTime.Now.Date; //for the daily update, if HTPC is on 24h it will refresh strings every day.
		
		string[] MovieURL = new string[2000];	// strings for all movies
		string[] MovieName = new string[2000];

		string[] LMovieUrl = new string[200]; // strings for letterbutton movies
		string[] LMovieName = new string[200];

		string[] JAMovieUrl = new string[50]; // strings for JustAdded movies
		string[] JAMovieName = new string[50];

		string[] MWMovieUrl = new string[50]; //  strings for MostWatched movies
		string[] MWMovieName = new string[50];

		string[] Trailers = new string[25];
		string[] TrailersUrl = new string[25];

		string[] Clips = new string[25];
		string[] ClipsUrl = new string[25];

		string[] More = new string[25];
		string[] MoreUrl = new string[25];

		bool foundt = false;		// bools for if trailer, clips or more is found
		bool foundc = false;
		bool foundm = false;

		bool allview = false; // bools for reminding which view the user is in
		bool tcmview = false;
		bool tview = false;
		bool cview = false;
		bool mview = false;
		bool jaview = false;
		bool mwview = false;
		bool letterview = false;
		bool plotview = true;
		bool castview = false;
		
		string ptitle;			// Used by SetGUIProperties, GetGUIProperties
		string pgenre;			// Before MP start playing a movie fullscreen
		string pruntime;		// it wil save the #tags info in these string
		string preleasedate;    // and will load them back after playing the 
		string pplot;			// movie.
		string pcast;

		string _downloadedText		= string.Empty;

		#endregion

		public override int GetID
		{
			get
			{
				return 5900;
			}
			set
			{
			}
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin+@"\mytrailers.xml");
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)  // For when a button is pressed
		{
			if (control==buttonOne) 
				OnButtonOne();
			if (control==btnletter)
				OnButtonTwo();
			if (control==listview)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
				OnMessage(msg);
				int itemIndex = (int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(itemIndex);
				}
			}
			if (control==btntoggleplot)
				ToggleButtonPlot();
			if(control==btntogglecast)
				ToggleButtonCast();
			base.OnClicked (controlId, control, actionType);
		}
		public override void OnAction(Action action)
		{
			if(action.wID == Action.ActionType.ACTION_STOP)
				SetGUIProperties();
			base.OnAction (action);
		}

		protected override void OnPageDestroy(int newWindowId)
		{
			GetGUIProperties();
			base.OnPageDestroy (newWindowId);
		}



		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			btnletter.RestoreSelection = false;
			btnletter.AddSubItem("#");
			for(char k = 'A'; k <= 'Z'; k++)
			{
				btnletter.AddSubItem(k.ToString());
			}
			LoadSettings();
			ShowLabelsFalse();

		}
			
				
		private void OnButtonOne()
		{
			GUIDialogMenu dlgm = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlgm.Reset();
			dlgm.SetHeading(5902);
			dlgm.AddLocalizedString(5903);	// Diaglog for which movies to show
			dlgm.AddLocalizedString(5904);
			dlgm.AddLocalizedString(5905);
			dlgm.DoModal(GUIWindowManager.ActiveWindow);

			if(dlgm.SelectedLabel==0)
			{
				if(RefreshDaily.Equals(DateTime.Now.Date)==false) // For the daily refresh
					Array.Clear(JAMovieName,0,50);
				if(JAMovieName[0]==null)
				{GetJustAdded();}
				buttonOne.Focus=false;
				listview.Focus=true;
				Prev_SelectedItem = 0;
				ShowJustAdded();
			}
			if(dlgm.SelectedLabel==1)
			{
				if(RefreshDaily.Equals(DateTime.Now.Date)==false)
					Array.Clear(MWMovieName,0,50);
				if(MWMovieName[0]==null)
				{GetMostWatched();}
				buttonOne.Focus=false;
				listview.Focus=true;
				Prev_SelectedItem = 0;
				ShowMostWatched();
			}
			if(dlgm.SelectedLabel==2)
			{
				if(RefreshDaily.Equals(DateTime.Now.Date)==false)
					Array.Clear(MovieName,0,2000);
				if(MovieName[0]==null)
				{GetTrailers();}
				buttonOne.Focus=false;
				listview.Focus=true;
				Prev_SelectedItem = 0;
				ShowMovies();
			}
		}
		
		private void OnButtonTwo()
		{
			if(RefreshDaily.Equals(DateTime.Now.Date)==false) // daily update
				Array.Clear(MovieName,0,2000);
			if(MovieName[0]==null)
				GetTrailers();

			int i = 0;
			int j = 0;
			string letter;
			listview.Clear();
			Array.Clear(LMovieName,0,200);
			Array.Clear(LMovieUrl,0,200);
			ResetViews();
			letterview=true;
			allview=false;
			jaview=false;
			mwview=false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\background.png");
			GUIPropertyManager.SetProperty("#title","");
			            
			letter = btnletter.SelectedLabel;
			
						
			while(MovieName[i]!=null)
			{
				if(MovieName[i].StartsWith("The "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("The ")==true) {}
				else if(MovieName[i].StartsWith("A "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("A ")==true) {}
				else if(MovieName[i].StartsWith("An "+letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					listview.Add(item);
					j++;
				}
				else if(MovieName[i].StartsWith("An ")==true) {}
				else if(MovieName[i].StartsWith(letter)==true)
				{
					LMovieName[j] = MovieName[i];
					LMovieUrl[j] = MovieURL[i];
					GUIListItem item = new GUIListItem();
					item.Label = LMovieName[j];
					listview.Add(item);
					item.IsFolder = true;
					Utils.SetDefaultIcons(item);
					j++;
				}
				else if(letter.Equals("#")==true)
				{
					for(int n = 0; n <= 9; n++)
						if(MovieName[i].StartsWith(n.ToString())==true)
						{
							LMovieName[j] = MovieName[i];
							LMovieUrl[j] = MovieURL[i];
							GUIListItem item = new GUIListItem();
							item.Label = LMovieName[j];
							item.IsFolder = true;
							Utils.SetDefaultIcons(item);
							listview.Add(item);
							j++;
						}
				}

				i++;
			}
			if(currentletter==letter)
				listview.SelectedListItemIndex = Prev_SelectedItem;
			else
			{
				listview.SelectedListItemIndex = 0;	
				currentletter = letter;
			}
		}
		private void ToggleButtonPlot()
		{
			btntoggleplot.Selected=true;
			btntogglecast.Selected=false;
			plotarea.Visible=true;
			castarea.Visible=false;
			label8.Visible=true;
			label6.Visible=false;
		}
		private void ToggleButtonCast()
		{
			GetCastInfo(casturl);
			btntoggleplot.Selected=false;
			btntogglecast.Selected=true;
			plotarea.Visible=false;
			castarea.Visible=true;
			label8.Visible=false;
			label6.Visible=true;
		}

		private void OnClick(int itemindex) // // When something is pressed in the listview
		{
			
			if(tcmview==true)
			{
				if(itemindex==0)
				{
					if(allview==true)
						ShowMovies();
					else if(jaview==true)
						ShowJustAdded();
					else if(mwview==true)
						ShowMostWatched();
					else if(letterview==true)
						OnButtonTwo();
				}
				else if(itemindex==1)
				{
					if(foundt==true)
						ShowTrailers();
					else if(foundt==false & foundc==true)
						ShowClips();
				}
				else if(itemindex==2)
				{
					if(foundt==false & foundc==true)
						ShowMore();
					else if(foundc==true)
						ShowClips();
				}
				else if(itemindex==3)
					ShowMore();
			}
			else if(tview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(TrailersUrl[itemindex-1]);
				}
			}
			else if(cview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(ClipsUrl[itemindex-1]);
				}

			}

			else if(mview==true)
			{
				if(itemindex==0)
					ShowTrailersClipsMore();
				else 
				{
					Prev_SelectedItem = listview.SelectedListItemIndex;
					Play(MoreUrl[itemindex-1]);
				}
			}
			else if(jaview==true)
			{
				GetMovieInfo(JAMovieUrl[itemindex], JAMovieName[itemindex], itemindex);
				ShowTrailersClipsMore();
				ShowMovieInfo(JAMovieUrl[itemindex], JAMovieName[itemindex], itemindex);
			}
			else if(mwview==true)
			{
				GetMovieInfo(MWMovieUrl[itemindex], MWMovieName[itemindex], itemindex);
				ShowTrailersClipsMore();
				ShowMovieInfo(MWMovieUrl[itemindex], MWMovieName[itemindex], itemindex);
			}
			else if(letterview==true)
			{
				GetMovieInfo(LMovieUrl[itemindex], LMovieName[itemindex], itemindex);
				ShowTrailersClipsMore();
				ShowMovieInfo(LMovieUrl[itemindex], LMovieName[itemindex], itemindex);
			}
			else if(allview==true)
			{
				GetMovieInfo(MovieURL[itemindex], MovieName[itemindex], itemindex);
				ShowTrailersClipsMore();
				ShowMovieInfo(MovieURL[itemindex], MovieName[itemindex], itemindex);
			}


		}

		void GetWebPage(string url, out string HTMLDownload) // Get url and put in string
		{
			if(_workerCompleted)
			{
				_workerCompleted = false;

				BackgroundWorker worker = new BackgroundWorker();

				worker.DoWork += new DoWorkEventHandler(DownloadWorker);
				worker.RunWorkerAsync(url);

				using(WaitCursor cursor = new WaitCursor())
				{
					while(_workerCompleted == false)
						GUIWindowManager.Process();
				}

				HTMLDownload = _downloadedText;

				_downloadedText = null;
			}
			else
			{
				HTMLDownload = string.Empty;
			}
		}

		void DownloadWorker(object sender, DoWorkEventArgs e)
		{
			WebClient wc = new WebClient();

			try
			{
				byte[] HTMLBuffer;

				HTMLBuffer = wc.DownloadData((string)e.Argument);
				
				_downloadedText = Encoding.ASCII.GetString(HTMLBuffer);
			}
			catch(Exception ex)
			{
				Log.Write("GUITrailers.DownloadWorker: {0}", ex.Message);
			}
			finally
			{
				wc.Dispose();
			}

			_workerCompleted = true;
		}

		bool _workerCompleted = true;

		void GetTrailers()
		{
			GUIPropertyManager.SetProperty("#title", "Getting movies...");
			GetWebPage(@"http://movies.yahoo.com/trailers/archive/", out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"<a\shref=.(?<trailerurl>/shop.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach (Match m in mc)
			{
				MovieURL[i] = m.Groups["trailerurl"].Value;
				MovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
			GUIPropertyManager.SetProperty("#title", "");
		}

		void ShowMovies()
		{
			ResetViews();
			allview = true;
			jaview = false;
			mwview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\background.png");
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5905));

			
			listview.Clear();
			int i = 0;
			while(MovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				Utils.SetDefaultIcons(item);
				item.Label = MovieName[i];
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
		}

		void GetMovieInfo(string url, string name, int i)
		{
			GUIPropertyManager.SetProperty("#title", "Getting moviesinfo...");
			Prev_SelectedItem = i;
			ResetViews();
			tcmview = true;

			TempHTML = "";
			foundt = false;
			foundc = false;
			foundm = false;
			Array.Clear(Trailers,0,25);
			Array.Clear(TrailersUrl,0,25);
			Array.Clear(Clips,0,25);
			Array.Clear(ClipsUrl,0,25);
			Array.Clear(More,0,25);
			Array.Clear(MoreUrl,0,25);

			GetWebPage(@"http://movies.yahoo.com/"+url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"(?sx-m).*?(?:</tr>)");

			int t = 0;
			int c = 0;
			int mo = 0;

			foreach (Match m in mc)
			{
				// get trailers & teasers can be more then 1
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/trailers/")!=-1)
				{
					// search for 700 kbit stream
					if(m.Value.IndexOf("700-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*).");
						TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";

						Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
						Trailers[t] = m2.Groups["trailername"].Value;
						t++;
					}
						// if there is no 700 kbit stream then get 300 kbit stream
					else
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/trailers/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*).");
						TrailersUrl[t] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"].Value + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Log.Write(m1.Groups["id"].Value);
						Log.Write(m1.Groups["segment"].Value);
						Log.Write(m1.Groups["movienumber"].Value);

						Match m2 = Regex.Match(m.Value, @".(?<trailername>.*)</a>");
						Trailers[t] = m2.Groups["trailername"].Value;
						Log.Write(m2.Groups["trailername"].Value);
						t++;
					}
					foundt = true;
				}
				// search for clips
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/clips/") !=-1)
				{
					// search for 700 kbit stream
					if(m.Value.IndexOf("700-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}
					else if(m.Value.IndexOf("700-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}

						// if there is no 700 kbit stream then get 300 kbit stream
					else if(m.Value.IndexOf("300-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}
					else if(m.Value.IndexOf("300-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/clips/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						ClipsUrl[c] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						Clips[c] = m1.Groups["clipsname"].Value;

						c++;
					}
					foundc = true;
				}
				// search for more "clips"
				if(m.Value.IndexOf("http://us.rd.yahoo.com/movies/more/")!=-1)
				{
					// search for 700 kbit stream
					if(m.Value.IndexOf("700-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}
						// if there is no 700 kbit stream then get 300 kbit stream
					else if(m.Value.IndexOf("700-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-700-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}
					else if(m.Value.IndexOf("300-p") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-p.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?id=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}
					else if(m.Value.IndexOf("300-s") !=-1)
					{
						Match m1 = Regex.Match(m.Value, @"<a\shref=.http://us.rd.yahoo.com/movies/more/(?<movienumber>\d*)/.*id=.*wmv-300-s.(?<id>\d*)-(?<segment>\d*)..*>.(?<clipsname>.*).</");
						MoreUrl[mo] = "http://playlist.yahoo.com/makeplaylist.dll?sid=" + m1.Groups["id"].Value + "&segment=" + m1.Groups["segment"] + "&s=" + m1.Groups["movienumber"].Value + "&ru=y&b=639r4gd1i7uth433192d0&type=t";
						More[mo] = m1.Groups["clipsname"].Value;

						mo++;
					}

					foundm = true;
				}
			}
			ptitle = name;
			GUIPropertyManager.SetProperty("#title", ptitle);
		}

		void ShowTrailersClipsMore()
		{
			ResetViews();
			tcmview=true;
			ShowLabelsTrue();
			SetGUIProperties();
			
			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);
			
			if(foundt==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5906);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			if(foundc==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5907);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			if(foundm==true)
			{
				GUIListItem item = new GUIListItem();
				item.Label = GUILocalizeStrings.Get(5908);
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listview.Add(item);
			}
			
		}

		void ShowTrailers()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			tview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(Trailers[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = Trailers[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
			buttonOne.Focus = false;
		}
		void ShowClips()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			cview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(Clips[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = Clips[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
			buttonOne.Focus = false;
		}
		void ShowMore()
		{
			ResetViews();
			GUIPropertyManager.SetProperty("#title", ptitle);
			mview = true;

			listview.Clear();
			GUIListItem item1 = new GUIListItem();
			item1.Label = "..";
			item1.IsFolder = true;
			Utils.SetDefaultIcons(item1);
			listview.Add(item1);

			int i = 0;
			while(More[i] != null)
			{
				GUIListItem item = new GUIListItem();
				item.Label = More[i];
				item.IconImage = "defaultVideo.png";
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus = true;
			buttonOne.Focus = false;
		}

		void ResetViews()
		{
			tcmview = false;
			tview = false;
			cview = false;
			mview = false;
			ShowLabelsFalse();
		}
	
		void GetMMSURL(string url)
		{
			TempHTML = "";
			GetWebPage(url, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			Match m = Regex.Match(TempHTML, @"<Ref\shref\s=\s.(?<mmsurl>mms.*).\s/>");
			MMSUrl = m.Groups["mmsurl"].Value;
			if(MMSUrl.Equals("")==true)
			{
				GUIDialogOK dlg = new GUIDialogOK();
				dlg.SetHeading(1025);
				dlg.SetLine(1, 5909);
				dlg.DoModal(GUIWindowManager.ActiveWindow);
			}
		}
		void Play(string sort)
		{
			GetGUIProperties();
			GetMMSURL(sort);
			GUIGraphicsContext.IsFullScreenVideo=true;
			GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
			g_Player.FullScreen=true;
			g_Player.Play(MMSUrl);

		}
		void ShowMovieInfo(string url, string name, int i)
		{
			TempHTML="";
			string coverurl;
			string PosterUrl="";
			string Plot="";
			string Genre="";
			string Runtime="";
			string ReleaseDate="";
			coverurl = "http://movies.yahoo.com/"+url;
			coverurl = coverurl.Replace("trailer","info");

			GetWebPage(coverurl, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			// Get PosterUrl
			Match m = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://us.movies1.yimg.com/movies.yahoo.com/images/.*.jpg)");
			PosterUrl = m.Groups["posterurl"].Value;
			// If Cover is TV-serie example Desperate Housewives
			if(PosterUrl.Equals("")==true)
			{
				Match p = Regex.Match(TempHTML,@"<img\ssrc=(?<posterurl>http://shopping.yahoo.com/video/images/muze/dvd.*.jpg)");
				PosterUrl = p.Groups["posterurl"].Value;
			}
			// Download Poster
			WebClient wc = new WebClient();
			name = name.Replace(":","-");
			wc.DownloadFile(PosterUrl, @"thumbs\MPTemp -"+name + ".jpg");
			while(System.IO.File.Exists(@"thumbs\MPTemp -"+name + ".jpg")!=true)
			{
			}
			// Display Poster
			poster.SetFileName(@"thumbs\MPTemp -"+name + ".jpg");

			// Get MoviePlot
			int EP = TempHTML.IndexOf("<br clear=\"all\" />");
			int BP = TempHTML.LastIndexOf("size=-1>",EP)+8;
			int TP = EP-BP;

			Plot = TempHTML.Substring(BP,TP);
			GUIPropertyManager.SetProperty("#plot", Plot);
            
			// Get Genre
			Match m1 = Regex.Match(TempHTML,@"<b>Genres:\s</b>(?<genre>.*)");
			Genre = m1.Groups["genre"].Value;
			GUIPropertyManager.SetProperty("#genre", Genre);

			// Get Runtime
			Match m2 = Regex.Match(TempHTML,@"<b>Running\sTime:\s</b>(?<runtime>.*?)</font>");
			Runtime = m2.Groups["runtime"].Value;
			GUIPropertyManager.SetProperty("#runtime", Runtime);

			// Get ReleaseDate
			Match m3 = Regex.Match(TempHTML,@"<b>Release\sDate:</b>.*;(?<releasedate>.*)\.");
			ReleaseDate = m3.Groups["releasedate"].Value;
			GUIPropertyManager.SetProperty("#year", ReleaseDate);
			
			casturl = url;
			ShowLabelsTrue();
		}
		void GetJustAdded()
		{
			GetWebPage("http://movies.yahoo.com/trailers/", out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;
		
			int BJA = TempHTML.IndexOf(@"<!-- just added -->");
			int EJA = TempHTML.IndexOf(@"<!-- /just added -->",BJA);
			int TJA = EJA-BJA;

			string TempJA;
			TempJA = TempHTML.Substring(BJA,TJA);

			MatchCollection mc = Regex.Matches(TempJA,@"<a\shref=.(?<movieurl>.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach(Match m in mc)
			{
				JAMovieUrl[i] = m.Groups["movieurl"].Value;
				JAMovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
		}
		void ShowJustAdded()
		{
			ResetViews();
			mwview = false;
			jaview = true;
			allview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\background.png");
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5903));

			
			listview.Clear();
			int i = 0;
			while(JAMovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				item.Label = JAMovieName[i];
				Utils.SetDefaultIcons(item);
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
			listview.Focus=true;
		}

		void GetMostWatched()
		{
			GetWebPage("http://movies.yahoo.com/trailers/", out TempHTML);
		
			if(TempHTML == null || TempHTML == string.Empty)
				return;

			int BMW = TempHTML.IndexOf(@"<!-- most watched -->");
			int EMW = TempHTML.IndexOf(@"<!-- /most watched -->",BMW);
			int TMW = EMW-BMW;

			string TempMW;
			TempMW = TempHTML.Substring(BMW,TMW);

			MatchCollection mc = Regex.Matches(TempMW,@"<a\shref=.(?<movieurl>.*).>(?<moviename>.*)</a>");

			int i = 0;

			foreach(Match m in mc)
			{
				MWMovieUrl[i] = m.Groups["movieurl"].Value;
				MWMovieName[i] = m.Groups["moviename"].Value;
				i++;
			}
		}
		void ShowMostWatched()
		{
			ResetViews();
			mwview = true;
			jaview = false;
			allview = false;
			letterview = false;
			poster.SetFileName(GUIGraphicsContext.Skin+@"\media\background.png");
			GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5904));

			
			listview.Clear();
			int i = 0;
			while(MWMovieName[i] !=null)
			{
				GUIListItem item = new GUIListItem();
				item.IsFolder = true;
				item.Label = MWMovieName[i];
				Utils.SetDefaultIcons(item);
				listview.Add(item);
				i++;
			}
			listview.SelectedListItemIndex = Prev_SelectedItem;
		}

		void GetCastInfo(string url)
		{
			TempHTML="";
			string casturl;
			string cast = "";

			casturl = "http://movies.yahoo.com/"+url;
			casturl = casturl.Replace("trailer","cast");

			GetWebPage(casturl, out TempHTML);

			if(TempHTML == null || TempHTML == string.Empty)
				return;

			MatchCollection mc = Regex.Matches(TempHTML, @"<A\sHRef=./shop.*>(?<cast>.*)</a>");

			foreach(Match m in mc)
			{
				if(cast.Equals(""))
				{
					cast = m.Groups["cast"].Value;
				}
				else
				{
					cast = cast.Insert(cast.Length,", ");
					cast = cast.Insert(cast.Length, m.Groups["cast"].Value);
				}
			}
			GUIPropertyManager.SetProperty("#cast", cast);
		}

		void ShowLabelsFalse()
		{
			label0.Visible=false;
			label1.Visible=false;
			label2.Visible=false;
			label3.Visible=false;
			label4.Visible=false;
			label5.Visible=false;
			label6.Visible=false;
			castarea.Visible=false;
			label8.Visible=false;
			plotarea.Visible=false;
			btntoggleplot.Visible=false;
			buttonOne.NavigateUp = 3;
			btntogglecast.Visible=false;
			btnletter.NavigateDown =2;

		}
		void ShowLabelsTrue()
		{
			label0.Visible=true;
			label1.Visible=true;
			label2.Visible=true;
			label3.Visible=true;
			label4.Visible=true;
			label5.Visible=true;
			label6.Visible=true;
			label8.Visible=true;
			btntoggleplot.Visible=true;
			buttonOne.NavigateUp = 6;
			btntogglecast.Visible=true;
			btnletter.NavigateDown =5;
			if(castview==true)
				ToggleButtonCast();
			else
				ToggleButtonPlot();

		}

		void GetGUIProperties()
		{
			ptitle = GUIPropertyManager.GetProperty("#title");
			pgenre = GUIPropertyManager.GetProperty("#genre");
			pruntime = GUIPropertyManager.GetProperty("#runtime");
			preleasedate = GUIPropertyManager.GetProperty("#year");
			pplot = GUIPropertyManager.GetProperty("#plot");
			pcast = GUIPropertyManager.GetProperty("#cast");
		}
		void SetGUIProperties()
		{
			GUIPropertyManager.SetProperty("#title", ptitle);
			GUIPropertyManager.SetProperty("#genre", pgenre);
			GUIPropertyManager.SetProperty("#runtime", pruntime);
			GUIPropertyManager.SetProperty("#year", preleasedate);
			GUIPropertyManager.SetProperty("#plot", pplot);
			GUIPropertyManager.SetProperty("#cast", pcast);
		}
		void LoadSettings()
		{
			if(tview==true)
				ShowTrailers();
			if(cview==true)
				ShowClips();
			if(mview==true)
				ShowMore();
			if(plotview==true)
				ToggleButtonPlot();
			if(castview==true)
				ToggleButtonCast();
		}
	}
}


