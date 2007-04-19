using System;
using MediaLibrary;
using System.Collections;
using Microsoft.Win32;

namespace SQLiteLibrary
{
	public class MLSection : IMLSection
	{
		#region Members

		public ArrayList _Views; //public for serialization
		private string SectionName;
        private string SectionPath;
        private IMLItemDataSource _Database;

        

		#endregion
		
		#region Properties

        #region Read Only Properties

        public int ItemCount
        {
            get { return _Database.ItemCount(); }
        }

		public int ViewCount
		{
            get { return _Views.Count; }
        }

        #endregion

        #region Read/Write Properties

        public IMLItemDataSource Database
        {
            get { return _Database; }
            set 
            { 
                _Database = value;
                RefreshViews();
            }
        }

        public string FileName
        {
            get { return SectionPath; }
            set { SectionPath = value; }
        }

        public string Name
        {
            get { return SectionName; }
            set { SectionName = value; }
        }

        #endregion

        #endregion

        #region Constructors

        public MLSection()
		{
            this.SectionPath = "";
			this.SectionName = "";
            this._Database = null;
            this._Views = new ArrayList();
		}

		#endregion

		#region Methods

		#region Item Functions - Status: 85%

		public IMLItem AddNewItem(string ItemName, string ItemSource)
		{
            MLItem Item = new MLItem();
			Item.Name = ItemName;
			Item.Location = ItemSource;
            Item._Database = this._Database;
            return Item;
		}

        public void DeleteItem(IMLItem Item)
		{
			if(Item != null)
                _Database.DeleteItem(Item);
		}

		public void DeleteAllItems()
		{
			_Database.DeleteAllItems();
		}

        public IMLItem Items(int Index)
		{
            throw new Exception("The method or operation is not implemented.");
		}

		public IMLItemList GetReadOnlyItems()
		{
            return _Database.GetAllItems();
		}

		public void RefreshItems()
		{
            _Database.Refresh();
		}

		public IMLViewNavigator GetViewNavigator()
		{
			return (new MLViewNavigator(this)) as IMLViewNavigator;
		}

		#endregion

		#region Tag Functions - Status: 100%
		
		public bool AddNewTag(string TagName)
		{
            return _Database.AddNewTag(TagName);
		}
	
		public bool DeleteTag(string TagName)
		{
            return _Database.DeleteTag(TagName);
		}
	
		public bool RenameTag(string OldTagName, string NewTagName)
		{
            return _Database.RenameTag(OldTagName, NewTagName);
		}
		
		public string[] GetTagNames()
		{
            return _Database.GetTagNames();
		}
		
		public string[] GetTagValues(string TagName)
		{
            return _Database.GetTagValues(TagName);
		}
	
		#endregion

		#region View Functions - Status: 100%

		public IMLView AddNewView(string ViewName)
		{
            MLView View = new MLView();
            View.Name = ViewName;
            this.Database.AddNewView(View);
            View.Database = this.Database;
            _Views.Add(View);
            return View as IMLView;			
		}

		public void DeleteView(IMLView View)
		{
            if (_Database.DeleteView(View))
                _Views.Remove(View);
		}

        public void DeleteAllViews()
		{
            _Database.DeleteAllViews();
            _Views.Clear();
		}

		public void RefreshViews()
		{
			_Views.Clear();
            IMLView[] views = this.Database.GetViews();
            foreach (MLView View in views)
            {
                View.Database = this.Database;
                _Views.Add(View);
            }
			_Views.Sort();
		}

		public IMLView Views(int Index)
		{
			return _Views[Index] as IMLView;
		}

		#endregion

		#region Transaction Functions - Status: 100%

		public bool BeginUpdate()
		{
            return _Database.BeginUpdate();
		}

		public void CancelUpdate()
		{
            _Database.CancelUpdate();
		}

		public void EndUpdate()
		{
            _Database.EndUpdate();
		}

		#endregion

		#region Find Item Functions - Status: 100%
		
		public IMLItem FindItemByExternalID(string ExternalID)
		{
            MLItem Item = _Database.FindItem("item_ext_id", ExternalID) as MLItem;
            if(Item == null)
                return null;
            Item._Database = this._Database;
            return Item;
		}

		public IMLItem FindItemByID(int ItemID)
		{
            MLItem Item = _Database.FindItem("item_id", Convert.ToString(ItemID)) as MLItem;
            if (Item == null)
                return null; 
            Item._Database = this._Database;
            return Item;
		}

		public IMLItem FindItemByLocation(string Location)
		{
            MLItem Item = _Database.FindItem("item_location", Location) as MLItem;
            if (Item == null)
                return null; 
            Item._Database = this._Database;
            return Item;
		}

        public int[] GetAllItemIDs()
        {
            return _Database.GetAllItemIDs();
        }

		#endregion

		#region Search Functions - Status: 100%
	
		public IMLItemList SearchAll(string SearchString)
		{
            return _Database.Search(null, SearchString);
		}

        public IMLItemList SearchByImage(string SearchString)
		{
            return _Database.Search("item_image", SearchString);
		}

        public IMLItemList SearchByLocation(string SearchString)
		{
            return _Database.Search("item_location", SearchString);
		}

        public IMLItemList SearchByTag(string TagName, string SearchString)
		{
            return _Database.Search(TagName, SearchString);
		}

        public IMLItemList SearchByTitle(string SearchString)
		{
            return _Database.Search("item_name", SearchString);
		}

        public IMLItemList CustomSearch(string Filter, string GroupBy, string GroupFunc, string OrderBy, string OrderType, bool Asc)
		{
            return _Database.CustomSearch(Filter, GroupBy, GroupFunc, OrderBy, OrderType, Asc);
		}


		#endregion

        public IMLDataSet GetDataSet()
        {
            MLDataSet ds = _Database.GetDataSet() as MLDataSet;
            ds._Database = this._Database;
            return ds;
        }

		#endregion
	}
}
