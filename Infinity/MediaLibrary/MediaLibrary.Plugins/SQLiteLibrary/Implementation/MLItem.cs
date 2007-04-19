using System;
using MediaLibrary;
using System.Collections;


namespace SQLiteLibrary
{
	public class MLItem : IMLItem
	{
		#region Members

		private IMLHashItem _Item;
        public IMLItemDataSource _Database;
        private int item_id;
        private string item_name;
        private string item_location;
        private string item_ext_id;
        private DateTime item_date;
        private DateTime created;
        private DateTime modified;
        private string item_image;

		#endregion

        #region Properties

        #region Read Only Properties

        public bool IsReadOnly
        {
            get { return _Database == null; }
        }

        #endregion

        #region Read/Write Properties

        public DateTime DateCreated
        {
            get { return created; }
            set { created = value; }
        }

        public DateTime DateChanged
        {
            get { return modified; }
            set { modified = value; }
        }

        public string ExternalID
        {
            get { return item_ext_id; }
            set { item_ext_id = value; }
        }
        public int ID
        {
            get { return item_id; }
            set { item_id = value; }
        }
        public string ImageFile
        {
            get { return item_image; }
            set { item_image = value; }
        }
        public string Location
        {
            get { return item_location; }
            set { item_location = value; }
        }
        public string Name
        {
            get { return item_name; }
            set { item_name = value; }
        }
        public IMLHashItem Tags
        {
            get { return _Item; }
            set { _Item = value; }
        }
        public DateTime TimeStamp
        {
            get { return item_date; }
            set { item_date = value; }
        }

        #endregion

        #endregion

        #region Constructors

        public MLItem()
		{
            this.item_id = 0;
            this.item_date = DateTime.Now;
            this.created = DateTime.Now;
            this.modified = DateTime.Now;
            this.item_name = string.Empty;
            this.item_location = string.Empty;
            this.item_ext_id = string.Empty;
            this.item_image = string.Empty;
            this._Database = null;
            this._Item = (new MLHashItem()) as IMLHashItem;
		}

		#endregion

		#region Methods

		public void SaveTags()
		{
			if(!IsReadOnly)
			{
				if(this.ID < 1)
                    this._Database.AddNewItem(this);
				else
					this._Database.UpdateItem(this);
			}
		}

		#endregion
	}
}
