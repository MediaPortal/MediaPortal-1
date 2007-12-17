using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;

namespace ProjectInfinity.Settings
{
  /// <summary>
  /// Sample settings class wich will implement your own settings object in your code/plugin
  /// Only public properties are stored/retrieved
  /// </summary>
  public class MySampleSettingsClass 
  {
    private int _myInt;
    private string _myString;
    private string _anotherString;
    private List<int> _alist = new List<int>();

    /// <summary>
    /// Default Ctor
    /// </summary>
    public MySampleSettingsClass()
    {
    }
    /// <summary>
    /// Scope and default value attribute
    /// </summary>
    [Setting(SettingScope.Global,"55555")]
    public int MyInt
    {
      get { return this._myInt; }
      set { this._myInt = value; }
    }
    [Setting(SettingScope.User,"myStringDefaultValue")]
    public string MyString
    {
      get { return this._myString; }
      set { this._myString = value; }
    }
    [Setting(SettingScope.User, "anotherStringDefaultValue")]
    public string AnotherString
    {
      get { return this._anotherString; }
      set { this._anotherString = value; }
    }
    [Setting(SettingScope.User, "")]
    public List<int> AList
    {
      get { return this._alist; }
      set { this._alist = value; }
    }
  }
}
