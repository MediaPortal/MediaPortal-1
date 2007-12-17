using System;
using System.Collections;
using System.Text;

namespace MediaLibrary.Settings
{
    public class MLPluginProperties : IMLPluginProperties 
    {
        MLHashItem Properties;

        public IMLPluginProperty this[int Index]
        {
            get
            {
                return Properties[Index] as IMLPluginProperty;
            }
        }

        public IMLPluginProperty this[string Key]
        {
            get
            {
                return Properties[Key] as IMLPluginProperty;
            }
        }

        public int Count
        {
            get{return Properties.Count;}
        }

        public MLPluginProperties()
        {
            Properties = new MLHashItem();
        }

        public IMLPluginProperty AddNew(string PropertyName)
        {
            MLPluginProperty newProp = new MLPluginProperty();
            newProp.Name = PropertyName;
            Properties[PropertyName] = newProp;
            return newProp;
        }

        public bool Contains(string Key)
        {
            return Properties.Contains(Key);
        }


        public virtual IEnumerator GetEnumerator()
        {
            return new MLPluginPropertyIterator(this);
        }

        public class MLPluginPropertyIterator : IEnumerator
        {
            private int index = -1;
            private IMLPluginProperties MyCollection;

            public MLPluginPropertyIterator(IMLPluginProperties MyCollection)
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
    }
}
