using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary;

namespace MyVideos
{
  public class ImportMovies : IMLImportPlugin
  {
    public bool GetProperties(IMLPluginProperties _properties)
    {
      IMLPluginProperty _prop = _properties.AddNew("Title");
      {
        _prop.Caption = "Title of the movie";
        _prop.DataType = "string";
        _prop.DefaultValue = String.Empty;
        _prop.HelpText = "";
        _prop.IsMandatory = true;
      }

      _prop = _properties.AddNew("Size");
      {
        _prop.Caption = "Movie filesize";
        _prop.DataType = "string";
        _prop.DefaultValue = String.Empty;
        _prop.HelpText = "";
        _prop.IsMandatory = false;
      }

      _prop = _properties.AddNew("Path");
      {
        _prop.Caption = "Path to the movie";
        _prop.DataType = "string";
        _prop.DefaultValue = String.Empty;
        _prop.HelpText = "";
        _prop.IsMandatory = true;
      }

      return true;
    }

    public bool SetProperties(IMLHashItem _properties, out string _errorText)
    {
      _errorText = "";
      return true;
    }

    public bool ValidateProperties(IMLPluginProperties _properties, IMLHashItem _propertyValues)
    {
      return true;
    }

    public bool EditCustomProperty(IntPtr _window, string _propertyName, ref string _value)
    {
      return true;
    }

    public bool Import(IMLSection _section, IMLImportProgress _progress)
    {
      _section.BeginUpdate();

      for (int i = 0; i < 20; i++)
      {
        IMLItem item = _section.AddNewItem("item-" + i, "blah");
        item.Tags["Title"] = "Title_" + i;
        item.Tags["Size"] = "Size_" + i;
        item.Tags["Path"] = "Path_" + i;

        item.SaveTags();
        if (!_progress.Progress(50, item.Name))
          return false;
      }

      _section.EndUpdate();
      return true;
    }
  }
}
