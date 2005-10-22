/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Serialization;

using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A class which implements a group
	/// A group can hold 1 or more controls
	/// and apply an animation to the entire group
	/// </summary>
	public class GUIGroup : GUIControl, ISupportInitialize, IAddChild
	{
		#region Constructors

		public GUIGroup(int parentId) : base(parentId)
		{
		}

		#endregion Constructors

		#region Methods

		public override void OnInit()
		{
			_startAnimation = true;
			_animator = new Animator(_animatorType);
		}

		public void AddControl(GUIControl control)
		{
			LogicalChildren.Add(control);
		}

		public override void Render(float timePassed)
		{
			if(GUIGraphicsContext.Animations)
			{
				if(_animator != null)
				{
					if(_startAnimation)
					{
						_startAnimation = false;
						StorePosition();
					}

					foreach(GUIControl control in LogicalChildren)
					{
						if(control != null)
							control.Animate(timePassed, _animator);
					}

					_animator.Advance(timePassed);
				}
			}

			foreach(GUIControl control in LogicalChildren)
				control.Render(timePassed);
			
			if(_animator != null && _animator.IsDone())
			{
				ReStorePosition();
				_animator = null;
			}
		}

		public override void FreeResources()
		{
			if(_animator != null)
			{
				ReStorePosition();
				_animator = null;
			}

			foreach(GUIControl control in LogicalChildren)
				control.FreeResources();
		}

		public override void AllocResources()
		{
			foreach(GUIControl control in LogicalChildren)
				control.AllocResources();
		}

		public override void PreAllocResources()
		{
			foreach(GUIControl control in LogicalChildren)
				control.PreAllocResources();
		}

		public override GUIControl GetControlById(int ID)
		{
			foreach(GUIControl control in LogicalChildren)
			{
				GUIControl childControl = control.GetControlById(ID);

				if(childControl != null)
					return childControl;
			}

			return null;
		}

		public override bool NeedRefresh()
		{
			foreach(GUIControl control in LogicalChildren)
			{
				if(control.NeedRefresh())
					return true;
			}

			return false;
		}

		public override bool HitTest(int x, int y, out int controlID, out bool focused)
		{
			controlID=-1;
			focused=false;

			for(int index = LogicalChildren.Count - 1; index >= 0; index--)
			{
				if((LogicalChildren[index]).HitTest(x, y, out controlID, out focused))
					return true;
			}

			return false;
		}

		public override void OnAction(Action action)
		{
			foreach(GUIControl control in LogicalChildren)
			{
				if(control.Focus)
					control.OnAction(action);
			}
		}

		public void Remove(int controlId)
		{
			foreach(GUIControl control in LogicalChildren)
			{
				if(control is GUIGroup)
				{
					((GUIGroup)control).Remove(controlId);
					break;
				}
				else if(control.GetID == controlId)
				{
					LogicalChildren.Remove(control);
					break;
				}
			}
		}

		public int GetFocusControlId()
		{
			foreach(GUIControl control in LogicalChildren)
			{
				if(control is GUIGroup)
				{
					int focusedId = ((GUIGroup)control).GetFocusControlId();

					if(focusedId != -1)
						return focusedId;
				}
				else if(control.Focus)
				{
					return control.GetID;
				}
			}

			return -1;
		}

		public override void DoUpdate()
		{
			foreach(GUIControl control in LogicalChildren)
				control.DoUpdate();
		}
    
		public override void StorePosition()
		{
			foreach(GUIControl control in LogicalChildren)
				control.StorePosition();
      
			base.StorePosition();
		}

		public override void ReStorePosition()
		{
			foreach(GUIControl control in LogicalChildren)
				control.ReStorePosition();
      
			base.ReStorePosition();
		}

		public override void Animate(float timePassed,Animator animator)
		{
			foreach(GUIControl control in LogicalChildren)
				control.Animate(timePassed, animator);

			base.Animate(timePassed, animator);
		}

		#endregion Methods

		#region Properties

		public Animator.AnimationType Animation
		{
			get { return _animatorType; }
			set { _animatorType = value; }
		}

		public int Count
		{
			get { return LogicalChildren.Count; }
		}

		public GUIControl this[int index]
		{
			get { return LogicalChildren[index]; }
		}

		/// <summary>
		/// Property to get/set the id of the window 
		/// to which this control belongs
		/// </summary>
		public override int WindowId
		{
			get { return base.WindowId; }
			set { base.WindowId = value; foreach(GUIControl control in LogicalChildren) control.WindowId = value; }
		}

		#endregion Properties

		////////////////////////////
		
		#region Methods

		void IAddChild.AddChild(object value)
		{
			if(value is GUIControl == false)
				return;

			LogicalChildren.Add((GUIControl)value);
		}

		void IAddChild.AddText(string text)
		{
		}

		protected void Arrange()
		{
			if(_beginInitCount != 0)
				return;

			if(_layout == null)
				return;

			this.Size = _layout.Measure(this, this.Size);

			_layout.Arrange(this);
		}

		protected override Size ArrangeOverride(Rect finalRect)
		{
			this.Location = finalRect.Location;
			this.Size = finalRect.Size;

			if(_layout == null)
				return this.Size;

			_layout.Arrange(this);

			return finalRect.Size;
		}

		void ISupportInitialize.BeginInit()
		{
			_beginInitCount++;
		}

		void ISupportInitialize.EndInit()
		{
			if(--_beginInitCount == 0)
				Arrange();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if(_layout == null)
				return Size.Empty;

			_layout.Measure(this, this.Size);

			return this.Size = _layout.Size;
		}

		#endregion Methods

		#region Properties

		public ILayout Layout
		{
			get { return _layout; }
			set { _layout = value; }
		}

		#endregion Properties

		#region Fields

		Animator					_animator;
		int							_beginInitCount = 0;

		[XMLSkinElement("layout")]
		ILayout						_layout;

		[XMLSkinElement("animation")]
		Animator.AnimationType		_animatorType = Animator.AnimationType.None;

		bool						_startAnimation;

		#endregion Fields
	}
}