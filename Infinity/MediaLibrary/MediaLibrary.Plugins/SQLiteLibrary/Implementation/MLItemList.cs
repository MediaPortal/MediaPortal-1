using System;
using MediaLibrary;
using System.Collections;

namespace SQLiteLibrary
{
	public class MLItemList : IMLItemList
	{
		// Private Fields
        public IMLItemDataSource Database;
		public ArrayList Items;

		// Constructor
        public MLItemList()
		{
            this.Database = null;
			this.Items = new ArrayList();
		}

		// Indexer
		public IMLItem this[int Index]
		{
			get 
			{
                return (Items[Index]) as IMLItem; 
			}
		}

		// Properties
		public int Count
		{
			get { return Items.Count; }
		}

		public bool IsReadOnly
		{
            get { return Database == null; }
		}
	}
}
