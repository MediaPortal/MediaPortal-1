using System;
using MediaLibrary;
using System.Collections;

namespace SQLiteLibrary
{
	public class MLView : IMLView, IComparable
	{
		public ArrayList _Steps; //public for serialization
		public IMLHashItem _View;//public for serialization
        private IMLItemDataSource _Database; //Not public in interface

        
        private string _Filter;
        private string _Name;
        private int _ID;
        private int _Order;
        private string _Custom1;
        private string _Custom2;


        public IMLItemDataSource Database
        {
            get { return _Database; }
            set 
            {
                foreach (MLViewStep Step in _Steps)
                    Step.Database = value;
                _Database = value; 
            }
        }

        public int Count
        {
            get { return _Steps.Count; }
        }

        public string Filter
        {
            get { return _Filter; }
            set 
            { 
                _Filter = value;
                Update("view_filter",value); 
            }
        }
        
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                Update("view_name", value);
            }
        }
        
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        
        public int Order
        {
            get { return _Order; }
            set
            {
                _Order = value;
                Update("view_order", value.ToString());
            }
        }
        
        public string Custom1
        {
            get { return _Custom1; }
            set
            {
                _Custom1 = value;
                Update("custom_1", value);
            }
        }
        
        public string Custom2
        {
            get { return _Custom2; }
            set
            {
                _Custom2 = value;
                Update("custom_2", value);
            }
        }

		public MLView()
		{
            this._Filter = String.Empty;
            this._Name = String.Empty;
            this._ID = 0;
            this._Order = 0;
            this._Custom1 = String.Empty;
            this._Custom2 = String.Empty;
            this._Database = null;
			this._Steps = new ArrayList();
		}
		
		public IMLViewStep Steps(int Index)
		{
			return _Steps[Index] as MLViewStep;
		}

		public IMLViewStep AddNewStep(string GroupTag)
		{
            MLViewStep Step = new MLViewStep();
            Step.GroupTag = GroupTag;
            Step.ViewID = this.ID;
            _Database.AddNewStep(Step as IMLViewStep);
            Step.Database = this._Database;
            _Steps.Add(Step);
            return Step as IMLViewStep;
		}

        public bool DeleteStep(IMLViewStep Step)
        {
            if (_Database.DeleteStep(Step))
            {
                _Steps.Remove(Step);
                return true;
            }
            return false;
        }
        
        public void DeleteAllSteps()
        {
            _Database.DeleteSteps(this);
            _Steps.Clear();
        }
		
        public int CompareTo(object obj)
		{
			MLView temp = (MLView)obj;
			if(this.Order > temp.Order)
			{
				return 1;
			}
			else
			{
				if(temp.Order == this.Order)
					return 0;
				else
					return -1;
			}
		}

        private void Update(string Key, string Value)
        {
            if (_Database != null)
                _Database.UpdateView(Key, Value, this);
        }

        public bool MoveUp()
        {
            throw new Exception("The method or operation is not implemented.");

        }

        public bool MoveDown()
        {
            throw new Exception("The method or operation is not implemented.");
        }

	}
}
