using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    public interface IMLSystem
    {
        IMLHashItem NewHashItem();

        IMLHashItemList NewHashItemList();

        IMLHashItemList GetInstalledPlugins(string PluginType);

        string GetRootDirectory(string CreateSubdirectory);

        string GetDataDirectory(string CreateSubdirectory);

        string GetLibraryDirectory(string CreateSubdirectory);

        string GetPluginsDirectory(string CreateSubdirectory);
    }
}
