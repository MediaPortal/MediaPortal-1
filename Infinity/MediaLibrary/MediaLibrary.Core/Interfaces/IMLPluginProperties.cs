using System;
using System.Collections;
using System.Text;

namespace MediaLibrary
{
    public interface IMLPluginProperties : IEnumerable
    {
        IMLPluginProperty this[int Index]
        {
            get;
        }

        IMLPluginProperty this[string Key]
        {
            get;
        }

        int Count 
        { 
            get;
        }

        IMLPluginProperty AddNew(string PropertyName);

        bool Contains(string Key);
    }
}
