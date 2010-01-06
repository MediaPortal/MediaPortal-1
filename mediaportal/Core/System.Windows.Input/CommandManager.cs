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

using System.Collections;

namespace System.Windows.Input
{
  public sealed class CommandManager
  {
    #region Constructors

    private CommandManager() {}

    #endregion Constructors

    #region Events

    public static event EventHandler StatusInvalidated;

    #endregion Events

    #region Methods

    public static void InvalidateStatus()
    {
      // post idle item in dispatcher to query command status

      if (StatusInvalidated != null)
      {
        StatusInvalidated(null, EventArgs.Empty);
      }
    }

    public static void RegisterClassCommandBinding(Type type, CommandBinding commandBinding)
    {
      if (type == null)
      {
        throw new ArgumentNullException("type");
      }

      CommandBindingCollection bindings = _commandBindings[type] as CommandBindingCollection;

      if (bindings == null)
      {
        _commandBindings[type] = bindings = new CommandBindingCollection();
      }

      bindings.Add(commandBinding);
    }

//		public static void RegisterClassInputBinding(Type type, InputBinding inputBinding)
//		{
//			if(type == null)
//				throw new ArgumentNullException("type");
//
//			if(inputBinding == null)
//				_inputBindings.Remove(type);
//			else
//				_inputBindings[type] = inputBinding;
//		}

    #endregion Methods

    #region Fields

    private static Hashtable _commandBindings = new Hashtable(50);
    private static Hashtable _inputBindings = new Hashtable(50);

    #endregion Fields
  }
}