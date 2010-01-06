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

namespace System.Windows.Input
{
  public class RoutedCommand : ICommand
  {
    #region Constructors

    static RoutedCommand()
    {
      TargetProperty = DependencyProperty.RegisterAttached("Target", typeof (IInputElement), typeof (RoutedCommand));
    }

    public RoutedCommand()
    {
      _name = string.Empty;
    }

    public RoutedCommand(string name, Type declaringType) : this(name, declaringType, null) {}

    public RoutedCommand(string name, Type declaringType, InputGestureCollection inputGestures)
    {
      _name = name;
      _declaringType = declaringType;
      _inputGestures = inputGestures;
    }

    #endregion Constructors

    #region Methods

    public bool Execute()
    {
      return Execute(null, null);
    }

    public bool Execute(IInputElement target)
    {
      return Execute(target, null);
    }

    public bool Execute(object data)
    {
      return Execute(null, data);
    }

    public bool Execute(IInputElement target, object data)
    {
      // returns response from PreviewExecute event
      throw new NotImplementedException();
    }

    public static IInputElement GetTarget(DependencyObject d)
    {
      return (IInputElement)d.GetValue(TargetProperty);
    }

    public bool QueryEnabled(IInputElement target)
    {
      // returns the state of the target's IsEnabled property
      // by sending the QueryEnabled event to the target.
      return target.IsEnabled;
    }

    public static void SetTarget(DependencyObject d, IInputElement target)
    {
      d.SetValue(TargetProperty, target);
    }

    #endregion Methods

    #region Properties

    public Type DeclaringType
    {
      get { return _declaringType; }
    }

    public InputGestureCollection InputGestures
    {
      get { return _inputGestures; }
    }

    public bool IsBlockedByRM
    {
      get { return _isBlockedByRM; }
      set { _isBlockedByRM = false; }
    }

    // TODO: Query target's IsEnabledProperty value
    public bool IsEnabled
    {
      get { return _isEnabled; }
    }

    public string Name
    {
      get { return _name; }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty TargetProperty;

    #endregion Properties (Dependency)

    #region Fields

    private Type _declaringType;
    private InputGestureCollection _inputGestures;
    private string _name;
    private bool _isBlockedByRM;
    private bool _isEnabled = true;

    #endregion Fields
  }
}