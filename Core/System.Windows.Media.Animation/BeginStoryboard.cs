#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Windows;
using System.Windows.Serialization;

namespace System.Windows.Media.Animation
{
	public sealed class BeginStoryboard : TriggerAction, IAddChild
	{
		#region Constructors

		static BeginStoryboard()
		{
			StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(BeginStoryboard));
		}

		public BeginStoryboard()
		{
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is Storyboard == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(Storyboard)));

			SetValue(StoryboardProperty, child);
		}

		void IAddChild.AddText(string text)
		{
		}

		#endregion Methods

		#region Properties

		public HandoffBehavior HandoffBehavior
		{
			get { return _handoffBehavior; }
			set { _handoffBehavior = value; }
		}

		public Storyboard Storyboard
		{
			get { return (Storyboard)GetValue(StoryboardProperty); }
			set { SetValue(StoryboardProperty, value); }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		#endregion Properties

		#region Properties (Dependency)

		public static readonly DependencyProperty StoryboardProperty;

		#endregion Properties (Dependency)

		#region Fields

		HandoffBehavior				_handoffBehavior;
		string						_name = string.Empty;

		#endregion Fields
	}
}
