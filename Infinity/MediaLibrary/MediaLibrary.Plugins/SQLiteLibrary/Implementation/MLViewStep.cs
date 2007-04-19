using System;
using MediaLibrary;
using System.Collections;

namespace SQLiteLibrary
{
	public class MLViewStep : IMLViewStep
    {
        #region Members

        private IMLItemDataSource _Database;

        
        private int _ViewID;
        private int _ViewStepID;
        private string _GroupFunction;
        private string _GroupTag;
        private string _Mode;
        private bool _SortAscending;
        private string _SortTag;
        private string _SortType;


        #endregion

        #region Properties

        public IMLItemDataSource Database
        {
            get { return _Database; }
            set { _Database = value; }
        }

        public int ViewID
		{
            get { return _ViewID; }
            set { _ViewID = value; }
		}
		public int ViewStepID
		{
			get { return _ViewStepID; }
            set { _ViewStepID = value; }
		}
		public string GroupFunction
		{
			get
			{
                return _GroupFunction;
			}
            set
            {
                _GroupFunction = value;
                Update("group_func", value);
            }
		}
		public string GroupTag
		{
			get
			{
                return _GroupTag;
			}
            set
            {

                _GroupTag = value;
                Update("group_tag", value);
            }
		}
		public string Mode
		{
			get
			{
                return _Mode;
			}
            set
            {
                _Mode = value;
                Update("mode", value);
            }
		}
		public bool SortAscending
		{
			get
			{
                return _SortAscending;
			}
            set
            {
                _SortAscending = value;
                if (_SortAscending)
                    Update("sort_asc", "yes");
                else
                    Update("sort_asc", "no");
            }
		}
		public string SortTag
		{
			get
			{
                return _SortTag;
			}
            set
            {
                _SortTag = value;
                Update("sort_tag", value);
            }
		}
		public string SortType
		{
			get
			{
                return _SortType;
			}
            set
            {
                _SortType = value;
                Update("sort_type", value);
            }
        }

        #endregion

        #region Constructors

        public MLViewStep()
		{
            this._Database = null;
            this._ViewID = 0;
            this._ViewStepID = 0;
            this._GroupFunction = String.Empty;
            this._GroupTag = String.Empty;
            this._Mode = String.Empty;
            this._SortAscending = true;
            this._SortTag = String.Empty;
            this._SortType = String.Empty;
        }

        #endregion

        private void Update(string Key, string Value)
        {
            if (_Database != null)
                _Database.UpdateStep(Key, Value, this);
        }
    }
}
