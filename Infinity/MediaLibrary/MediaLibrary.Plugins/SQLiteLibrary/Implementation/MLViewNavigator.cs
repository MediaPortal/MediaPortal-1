using System;
using System.IO; 
using System.Net;
using System.Data;
using System.Text;
using System.Threading;
using System.Collections;
using MediaLibrary;

namespace SQLiteLibrary
{
	public class MLViewNavigator : IMLViewNavigator
	{
		

		#region MLViewNavigator Members

        private bool _EnableFilters;
        private bool _SkipSingleChoice;	//Get/set whether the navigator should skip single choices.
        private string _BlankChoiceText;	//This is a string to use when there are blank values in the view.
        private string _DefaultImage;		//An image to use when there are no images.
        private string _DefaultMode;		//The default mode to use when the current step does not have a mode.
        private string _ViewsMode;		//Sets the mode that is returned by CurrentMode when showing the list of views.
		private int curLevel;
        private IMLSection Section;
        private IMLItemList _Items;
		private IMLView curView;        //Hld the current view in the navigation
		private IMLViewStep curStep;    //hold the current step in the navigation
		private Stack Filters;			//Hold the previous filter item 
		private Stack TagHistory;		//Hold the previous selected items 
		private Stack ImgHistory;       //Hold the previous selected images
		private MLHashItem CustomFilters;	//For adding global filters to AND with the current view
		
		#endregion

		#region MLViewNavigator Properties

        public bool EnableFilters
        {
            get { return _EnableFilters; }
            set { _EnableFilters = value; }
        }

        public bool SkipSingleChoice
        {
            get { return _SkipSingleChoice; }
            set { _SkipSingleChoice = value; }
        }

        public IMLItemList Items
        {
            get { return _Items; }
        }

        public string BlankChoiceText
        {
            get { return _BlankChoiceText; }
            set { _BlankChoiceText = value; }
        }

        public string DefaultImage
        {
            get { return _DefaultImage; }
            set { _DefaultImage = value; }
        }

        public string DefaultMode
        {
            get { return _DefaultMode; }
            set { _DefaultMode = value; }
        }

        public string ViewsMode
        {
            get { return _ViewsMode; }
            set { _ViewsMode = value; }
        }

        public string SectionName
        {
            get { return Section.Name; }
        }

		//If TRUE the View Navigator is at the bottom of the hierarchy, that is, at the last step defined for the view.
		public bool AtBottom 
		{
			get 
			{ 
				if(AtViews)
					return false;
				else
					return (Level >= curView.Count); 
			}
		}


		//If TRUE the View Navigator is at the top of the hierarchy, that is, at the first step defined for the view.
		public bool AtTop 
		{
			get{return (Level == 1);}
		}


		//Returns TRUE if the view navigator is currently showing the list of views.
		public bool AtViews 
		{
			get {return (Level == 0); }
		}


		//Returns TRUE if the navigator can go back one step.
		public bool CanGoBack 
		{
			get { return (Level > 1 || (Level == 1 && Section.ViewCount > 1)); }
		}


		//The current level of the navigator. At the top, it is 0.
		public int Level 
		{
			get
			{
				return this.curLevel + 1;
			}
		}
	

		//The number of current choices.
		public int Count
		{
			get 
			{
			    if(!AtViews)
			        return Items.Count; 
			    else
				    return Section.ViewCount;
			}
		}


		//Returns the current mode as defined in the view steps or the default mode if there is none.
		public string CurrentMode
		{
			get
			{
				if(AtViews)
					return ViewsMode;
				else if(curStep != null)
					if(!IsNOE(curStep.Mode))
						return curStep.Mode;
				return DefaultMode;
			}
		}

		//Returns the name of the current tag that is being displayed by the navigator or "views" if the navigator is showing a list of views.
		public string CurrentTag
		{
			get
			{
				if(AtViews)
					return "views";
				else if(curStep != null && !IsNOE(curStep.GroupTag))
					return curStep.GroupTag;
				return "<name>";
			}
		}


		//Returns the name of the current view in use or an empty string if the navigator is showing a list of views.
		public string CurrentView
		{
			get
			{
				if (!AtViews)
					return curView.Name;
				return "";
			}
		}


		//The navigator attempts to track the image for the last choice made. This allows us to display, for example, the cover for the current album when looking at its tracks.
		public string LastImage
		{
			get
			{
				if (ImgHistory.Count >= 1)
					return ImgHistory.Peek().ToString();
				return "";
			}
		}


		//Returns the sort tag for the current step
		public string SortBy
		{
			get
			{
				if(!AtViews && curStep != null && !IsNOE(curStep.SortTag))
					return curStep.SortTag;
				return CurrentTag;
			}
		}


		//returns if the sort is ascending
		public bool SortAsc
		{
			get
			{
				if(curStep != null)
					return curStep.SortAscending;
				return true;
			}
		}


		//Returns a subtitle for display purposes. This is usually the tag value for the next to last choice made.
		public string Subtitle
		{
			get
			{
				if (TagHistory.Count >= 2)
					return TagHistory.ToArray()[1].ToString();
				return "";
			}
		}


		//Returns a title for display purposes. This is usually the tag value for the last choice made.
		public string Title
		{
			get
			{
				if (TagHistory.Count >= 1)
					return TagHistory.Peek().ToString();
				return "";
			}
		}

        /// <summary>
        /// This is mostly for debugging purposes, as it shows the current "path" and the criteria used so far. You can see the value of this property in the views tester in the configuration application.
        /// </summary>
        public string CriteriaPath
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// For debugging purposes.
        /// </summary>
        public string DebugText
        {
            get { 
                throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// For debugging purposes.
        /// </summary>
        IMLHashItem IMLViewNavigator.Filters 
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

		
		#endregion

		#region MLViewNavigator Constructors

		public MLViewNavigator(IMLSection Section)
		{	
			//Initalize system objects
			this.Section = Section;	
  
			curView = null;
			curStep = null;
			curLevel = -1;

			//Setup filter info
			Filters = new Stack();						//create stack to hold filters
			TagHistory = new Stack();					//create history stack
			ImgHistory = new Stack();

			if(Section.ViewCount == 0)
			{
				IMLView newView = Section.AddNewView("All");
				IMLViewStep newStep = newView.AddNewStep("");
				Section.RefreshViews();
			}
            CustomFilters = new MLHashItem();
            Refresh();
		}

		#endregion
	
		#region MLViewNavigator Methods

		public string Images(int Index)
		{
			if (AtViews)
				return "";
			else if(!IsNOE(Items[Index].ImageFile))
				return Items[Index].ImageFile;	
			else
				return DefaultImage;
		}

		public string Choices(int Index)
		{
			if (AtViews)
				return Section.Views(Index).Name;
			else if(AtBottom)
			{
				if(!IsNOE(Items[Index].Name))
                    return Items[Index].Name;
				else
					return BlankChoiceText;
			}
			else 
			{
				if(!IsNOE(Items[Index].Tags["caption"]))
					return Convert.ToString(Items[Index].Tags["caption"]);
				else
					return BlankChoiceText;
			}
		}
		
		public bool Select(int Index)
		{
			if(!AtBottom)
			{
				if(AtViews)
				{
					curView = Section.Views(Index);
				}
				else	
				{	//Store the group by parameter and value in the Filter list by pushing it on the stack
					string filter = "";
					string groupTag = FixSQL(this.CurrentTag);
					string groupVal = FixSQL(FixBlankChoice(this.Choices(Index)));
					if(curStep.GroupFunction == "index")
						filter = "({" + groupTag + "} LIKE '" + groupVal + "%' OR {" + groupTag + "} LIKE '%|" + groupVal + "%')";
					else
						filter = "({" + groupTag + "} LIKE '" + groupVal + "' OR {" + groupTag + "} LIKE '%|" + groupVal + "|%')";
					Filters.Push(filter);
				}
				TagHistory.Push(this.Choices(Index));	//push the selected item onto the stack
				ImgHistory.Push(this.Images(Index));     //push the selected image onto the stack

				curLevel++;		//go to the next step
				curStep = curView.Steps(curLevel);
				Refresh();		//Run query for new step
				if(SkipSingleChoice && this.Count == 1)
					return Select(0) || true;
				return true;
			}
			return false;
		}

		public bool Back()
		{
			if(CanGoBack)
			{			
				if(Level > 1)
					Filters.Pop();
				TagHistory.Pop();
				ImgHistory.Pop();
				curLevel--;		//go to the previous step
				if(AtViews)
					curStep = null;
				else
					curStep = curView.Steps(curLevel);
				Refresh();
				if(SkipSingleChoice && this.Count == 1)
					return Back() || true;
				return true;
			}
			return false;
		}

		public void Refresh()
		{
			if(!this.AtViews)
			{
				string Filter = GetFilter();
                string GroupBy;
                string OrderBy;
                if(this.CurrentTag == "<name>")
				    GroupBy = "item_name";
                else
                    GroupBy = "{" + this.CurrentTag + "}";
                if (this.SortBy == "<name>")
                    OrderBy = "item_name";
                else
                    OrderBy = "{" + this.SortBy + "}";
				

				_Items = Section.CustomSearch(Filter,GroupBy,curStep.GroupFunction,OrderBy, curStep.SortType,SortAsc);	
			}
			else
			{
				if (Section.ViewCount < 1)
				{
					;   //todo create a new view of all
				}
				else if (Section.ViewCount == 1)
					Select(0);
			}
		}

        private IMLItemList GetItems()
        {
            if (!this.AtViews)
            {
                string Filter = GetFilter();
                string OrderBy;
                if (this.SortBy == "<name>")
                    OrderBy = "item_name";
                else
                    OrderBy = "{" + this.SortBy + "}";
                return Section.CustomSearch(GetFilter(), null, null, OrderBy, curStep.SortType, SortAsc);
            }
            return null;

        }

		public IMLItemList GetAllItemsAtOrBelowHere() 
		{
			return GetItems();
		}

		public IMLItemList GetAllItemsForChoice(int Index)
		{
			if(!AtBottom)
			{
				if(AtViews)
					curView = Section.Views(Index);
				else	//Store the group by parameter and value in the Filter list by pushing it on the stack
				{	
					string filter = "";
					string groupTag = FixSQL(this.CurrentTag);
					string groupVal = FixSQL(FixBlankChoice(this.Choices(Index)));
					if(curStep.GroupFunction == "index")
						filter = "({" + groupTag + "} LIKE '" + groupVal + "%' OR {" + groupTag + "} LIKE '%|" + groupVal + "%')";
					else
						filter = "({" + groupTag + "} LIKE '" + groupVal + "' OR {" + groupTag + "} LIKE '%|" + groupVal + "|%')";
					Filters.Push(filter);	//push the selected filter onto the stack
				}
				curLevel++;							//go to the next step
				IMLItemList myList = GetItems();	//Run query for new step
				curLevel--;
				Filters.Pop();
				return myList;	
			}
			else
			{	//case when we are at the bottom view, just return the selected item
                MLItemList myList = new MLItemList();
                myList.Items.Add(Items[Index]);
                return myList as IMLItemList;
			}
		}

		public void AddCustomFilter(string ActionName, string filter)
		{
			if(EnableFilters)
			{
				CustomFilters[ActionName] = filter.Replace("[","{").Replace("]","}");
			}
        }

        #region Helper Functions
 
        private string GetFilter()
		{
			string filter = "";
			if(!AtViews)
			{
				//apply the view filter
				foreach(object obj in Filters)
				{
					if(filter == "")
						filter = Convert.ToString(obj);
					else
						filter += " AND " + Convert.ToString(obj);
				}
				//apply the custom filter for the view
				for(int i = 0; i < CustomFilters.Count; i++)
				{
					if(!IsNOE(CustomFilters[i]))
					{
						if(filter == "")
							filter = Convert.ToString(CustomFilters[i]);
						else
							filter += " AND " + Convert.ToString(CustomFilters[i]);
					}
				}
				//Apply the global filter for the view
				if(!IsNOE(curView.Filter))
				{
					if(filter == "")
						filter = curView.Filter;
					else
						filter += " AND " + curView.Filter;
				}
			}
			return filter;
		}

		private string FixBlankChoice(string str) 
		{
			if(str == BlankChoiceText)
				return "";
			return str;
        }

        private bool IsNOE(string str)
        {
            return (str == null || str == string.Empty);
        }
        
        private string FixSQL(string str)
        {
            return str.Replace("'", "''");
        }
        
        private bool IsNOE(object obj)
        {
            string str = Convert.ToString(obj);
            return (str == null || str == string.Empty);
        }
        
        private string FixSQL(object obj)
        {
            string str = Convert.ToString(obj);
            return str.Replace("'", "''");
        }

        #endregion

        #endregion


    }
}
