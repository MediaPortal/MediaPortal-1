#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Windows.Serialization;

namespace System.Windows
{
  public class EventTrigger : TriggerBase, IAddChild
  {
    #region Constructors

    public EventTrigger() {}

    public EventTrigger(RoutedEvent routedEvent)
    {
      _routedEvent = routedEvent;
    }

    #endregion Constructors

    #region Methods

    protected virtual void AddChild(object child)
    {
      if (child == null)
      {
        throw new ArgumentNullException("child");
      }

      if (child is TriggerAction == false)
      {
        throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof (TriggerAction)));
      }

      if (_actions == null)
      {
        _actions = new TriggerActionCollection();
      }

      _actions.Add((TriggerAction)child);
    }

    protected virtual void AddText(string text) {}

    void IAddChild.AddChild(object child)
    {
      AddChild(child);
    }

    void IAddChild.AddText(string text)
    {
      AddText(text);
    }

    #endregion Methods

    #region Properties

    public TriggerActionCollection Actions
    {
      get
      {
        if (_actions == null)
        {
          _actions = new TriggerActionCollection();
        }
        return _actions;
      }
    }

    public RoutedEvent RoutedEvent
    {
      get { return _routedEvent; }
      set { _routedEvent = value; }
    }

    public string SourceName
    {
      get { return _sourceName; }
      set { _sourceName = value; }
    }

    #endregion Properties

    #region Fields

    private TriggerActionCollection _actions;
    private RoutedEvent _routedEvent;
    private string _sourceName;

    #endregion Fields
  }
}