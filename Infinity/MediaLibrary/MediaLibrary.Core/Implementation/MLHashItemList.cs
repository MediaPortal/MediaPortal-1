using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace MediaLibrary
{
    [Serializable]
    public class MLHashItemList : IMLHashItemList
    {
        [XmlIgnore]
        private ArrayList unsync_list;

        // this is public because otherwise we cant xmlserialize it
        // TODO: can we use the this[] indexer instead???
        [XmlElementAttribute(Type = typeof(MLHashItem))]
        public ArrayList Property;

        public MLHashItemList()
        {
            unsync_list = new ArrayList();
            Property = ArrayList.Synchronized(unsync_list);
        }

        #region old MLHashItemList interface

        [XmlIgnore]
        public IMLHashItem this[int Index]
        {
            get
            {
                return Property[Index] as IMLHashItem;
            }
            set 
            {
                Property[Index] = value;
            }
        }

        public void Add(IMLHashItem Item)
        {
            Property.Add((MLHashItem)Item);
        }

        public void AddFromList(IMLHashItemList SourceList)
        {
            IMLHashItem imic;

            if (SourceList != null)
            {

                MLHashItemList sourcelist = SourceList as MLHashItemList;

                // TODO: is a deep copy really needed?
                // shallow copy would be
                // Property.AddRange(sourcelist.Property);
                lock (sourcelist.Property.SyncRoot)
                {
                    foreach (IMLHashItem imi in sourcelist.Property)
                    {
                        imic = new MLHashItem();
                        imic.CopyFrom(imi);
                        Property.Add(imic);
                    }
                }
            }
        }

        public IMLHashItem AddNew()
        {
            MLHashItem imi = new MLHashItem();
            Property.Add(imi);
            return imi;
        }

        public void Clear()
        {
            Property.Clear();
        }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return Property.Count;
            }

        }

        public IMLHashItemList CreateCopy()
        {
            MLHashItemList iml = new MLHashItemList();
            MLHashItem imic;

            lock (Property.SyncRoot)
            {
                foreach (MLHashItem imi in Property)
                {
                    imic = new MLHashItem();
                    imic.CopyFrom(imi);
                    iml.Property.Add(imic);
                }
            }
            return iml;
        }

        public void Exchange(int OldIndex, int NewIndex)
        {
            IMLHashItem imi = Property[OldIndex] as IMLHashItem;
            Property[OldIndex] = Property[NewIndex];
            Property[NewIndex] = imi;
        }

        public IMLHashItem FindItem(string Key, object Value)
        {
            IMLHashItem retimi = null;

            lock (Property.SyncRoot)
            {
                foreach (IMLHashItem imi in Property)
                {
                    if (imi.Contains(Key))
                    {
                        if (imi[Key] == Value)
                        {
                            retimi = imi;
                            break;
                        }
                    }
                }
            }
            return retimi;
        }

        public int FindItemIndex(IMLHashItem Item)
        {
            IMLHashItem imi;
            int i;
            int reti = -1;

            lock (Property.SyncRoot)
            {
                for (i = 0; i < Property.Count; i++)
                {
                    imi = (IMLHashItem)Property[i];
                    if (imi == Item)
                    {
                        reti = i;
                        break;
                    }
                }
            }
            return reti;
        }

        public int FindItemIndex(string Key, object Value)
        {
            IMLHashItem imi;
            int i;
            int reti = -1;

            lock (Property.SyncRoot)
            {
                for (i = 0; i < Property.Count; i++)
                {
                    imi = (IMLHashItem)Property[i];
                    if (imi.Contains(Key))
                    {
                        if (imi[Key] == Value)
                        {
                            reti = i;
                            break;
                        }
                    }
                }
            }
            return -1;
        }

        public void Remove(int Index)
        {
            Property.RemoveAt(Index);
        }

        public void Remove(IMLHashItem Item)
        {
            Property.Remove(Item);
        }

        public void Sort(string SortKey, bool SortAscending)
        {
            IComparer myItemComparer;

            myItemComparer = new ItemComparer(SortKey, SortAscending);

            Property.Sort(myItemComparer);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new ItemIterator(this);
        }

        #endregion

        #region private stuff

        private class ItemComparer : IComparer
        {
            private string _SortKey;
            private bool _SortAscending;

            public ItemComparer(string SortKey, bool SortAscending)
            {
                _SortKey = SortKey;
                _SortAscending = SortAscending;
            }

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(object x, object y)
            {
                if (_SortAscending)
                    return ((new CaseInsensitiveComparer()).Compare(((IMLHashItem)x)[_SortKey], ((IMLHashItem)y)[_SortKey]));
                else
                    return ((new CaseInsensitiveComparer()).Compare(((IMLHashItem)y)[_SortKey], ((IMLHashItem)x)[_SortKey]));
            }
        }

        public class ItemIterator : IEnumerator
        {
            private int index = -1;
            private IMLHashItemList MyCollection;

            public ItemIterator(IMLHashItemList MyCollection)
            {
                this.MyCollection = MyCollection;
            }

            public bool MoveNext()
            {
                index++;
                if (index < MyCollection.Count)
                {
                    return true;
                }
                else
                {
                    index = -1;
                    return false;
                }
            }

            public object Current
            {
                get
                {
                    if (index <= -1)
                    {
                        throw new InvalidOperationException();
                    }
                    return MyCollection[index];
                }
            }

            public void Reset()
            {
                index = -1;
            }
        }

        #endregion
    }
}