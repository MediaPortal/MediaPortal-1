using System;
namespace MediaPortal.Profile
{
    public interface ISettingsProvider
    {
        string FileName { get; }
        object GetValue(string section, string entry);
        void RemoveEntry(string section, string entry);
        void Save();
        void SetValue(string section, string entry, object value);
    }
}
