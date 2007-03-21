#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Xml;

namespace MediaPortal.GUI.Library
{
  public enum AnimationType
  {
    Unfocus = -3,
    Hidden,
    WindowClose,
    None,
    WindowOpen,
    Visible,
    Focus,
  };

  public enum EffectType
  {
    None = 0,
    Fade,
    Slide,
    Rotate,
    Zoom
  };
  public enum AnimationProcess
  {
    None = 0,
    Normal,
    Reverse
  };
  public enum AnimationState
  {
    None = 0,
    Delayed,
    InProcess,
    StateApplied
  };

  public class VisualEffect : ICloneable
  {
    AnimationType _type;
    EffectType _effect;
    AnimationProcess _queuedProcess;
    AnimationState _currentState;
    AnimationProcess _currentProcess;
    int _condition;      // conditions that must be satisfied in order for this // animation to be performed
    // animation variables
    float _acceleration;
    float _startX;
    float _startY;
    float _endX;
    float _endY;
    float _centerX;
    float _centerY;
    int _startAlpha;
    int _endAlpha;

    // timing variables
    float _amount;
    uint _start;
    uint _length;
    uint _delay;

    bool _isReversible;    // whether the animation is reversible or not
    TransformMatrix _matrix = new TransformMatrix();

    const float DEGREE_TO_RADIAN = 0.01745329f;

    public VisualEffect()
    {
      Reset();
    }
    public void Reset()
    {
      _type = AnimationType.None;
      _effect = EffectType.None;
      _currentState = AnimationState.None;
      _currentProcess = _queuedProcess = AnimationProcess.None;
      _amount = 0;
      _delay = _start = _length = 0;
      _startX = _startY = _endX = _endY = 0;
      _centerX = _centerY = 0;
      _startAlpha = 0;
      _endAlpha = 100;
      _acceleration = 0;
      _condition = 0;
      _isReversible = true;
    }
    float GetFloat(string text)
    {
      bool useCommas = false;
      float fTest = 123.12f;
      string test = fTest.ToString();
      if (test.IndexOf(",") >= 0)
        useCommas = true;
      if (useCommas) 
        text = text.Replace(".", ",");
      else
        text = text.Replace(",", ".");
      return float.Parse(text);
    }
    void GetPosition(string text, ref float x, ref float y)
    {
//      Log.Info("GetPos:{0}", text);
      x = y = 0;
      if (text == null) return;
      int pos = text.IndexOf(",");
      if (pos >= 0)
      {
        x = float.Parse(text.Substring(0, pos));
        y = float.Parse(text.Substring(pos + 1));
      }
      else
      {
        x = float.Parse(text);
      }
    }
    public bool Create(XmlNode node)
    {
      
      string animType = node.InnerText.ToLower();
      if (String.Compare(animType, "visible", true) == 0)
        _type = AnimationType.Visible;
      else if (String.Compare(animType, "hidden", true) == 0)
        _type = AnimationType.Hidden;
      else if (String.Compare(animType, "visiblechange", true) == 0)
        _type = AnimationType.Visible;
      else if (String.Compare(animType, "focus", true) == 0)
        _type = AnimationType.Focus;
      else if (String.Compare(animType, "unfocus", true) == 0)
        _type = AnimationType.Unfocus;
      else if (String.Compare(animType, "windowopen", true) == 0)
        _type = AnimationType.WindowOpen;
      else if (String.Compare(animType, "windowclose", true) == 0)
        _type = AnimationType.WindowClose;
      if (_type == AnimationType.None)
      {
        Log.Error("Control has invalid animation type");
        return false;
      }
      XmlNode nodeAttribute = node.Attributes.GetNamedItem("condition");
      if (nodeAttribute != null)
      {
        string conditionString = nodeAttribute.Value;
        _condition = GUIInfoManager.TranslateString(conditionString);
      }
      nodeAttribute = node.Attributes.GetNamedItem("effect");
      if (nodeAttribute == null) return false;
      string effectType = nodeAttribute.Value;
      // effect type
      if (String.Compare(effectType, "fade") == 0)
        _effect = EffectType.Fade;
      else if (String.Compare(effectType, "slide") == 0)
        _effect = EffectType.Slide;
      else if (String.Compare(effectType, "rotate") == 0)
        _effect = EffectType.Rotate;
      else if (String.Compare(effectType, "zoom") == 0)
        _effect = EffectType.Zoom;
      // time and delay
      nodeAttribute = node.Attributes.GetNamedItem("time");
      UInt32.TryParse(nodeAttribute.Value.ToString(), out _length);

      nodeAttribute = node.Attributes.GetNamedItem("delay");
      if (nodeAttribute != null)
        UInt32.TryParse(nodeAttribute.Value.ToString(), out _delay);

      //_length = (uint)(_length * g_SkinInfo.GetEffectsSlowdown());
      //_delay = (uint)(_delay * g_SkinInfo.GetEffectsSlowdown());

      // reversible (defaults to true)
      nodeAttribute = node.Attributes.GetNamedItem("reversible");
      if (nodeAttribute != null)
      {
        string reverse = nodeAttribute.Value;
        if (String.Compare(reverse, "false") == 0)
          _isReversible = false;
      }

      // acceleration of effect
      //float accel;
      nodeAttribute = node.Attributes.GetNamedItem("acceleration");
      if (nodeAttribute != null)
      {
        _acceleration = GetFloat(nodeAttribute.Value.ToString());
      }


      // slide parameters
      if (_effect == EffectType.Slide)
      {
        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null)
        {
          string startPos = nodeAttribute.Value;
          GetPosition(startPos, ref _startX, ref _startY);
        }
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null)
        {
          string endPos = nodeAttribute.Value;
          GetPosition(endPos, ref _endX, ref _endY);
        }
        // scale our parameters
        GUIGraphicsContext.ScaleHorizontal(ref _startX );
        GUIGraphicsContext.ScaleVertical(ref _startY);
        GUIGraphicsContext.ScaleHorizontal(ref _endX);
        GUIGraphicsContext.ScaleVertical(ref _endY);
      }
      else if (_effect == EffectType.Fade)
      {
        // alpha parameters
        if (_type < 0)
        { // out effect defaults
          _startAlpha = 100;
          _endAlpha = 0;
        }
        else
        { // in effect defaults
          _startAlpha = 0;
          _endAlpha = 100;
        }
        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null) _startAlpha = Int32.Parse(nodeAttribute.Value.ToString());
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null) _endAlpha = Int32.Parse(nodeAttribute.Value.ToString());

        if (_startAlpha > 100) _startAlpha = 100;
        if (_endAlpha > 100) _endAlpha = 100;
        if (_startAlpha < 0) _startAlpha = 0;
        if (_endAlpha < 0) _endAlpha = 0;
      }
      else if (_effect == EffectType.Rotate)
      {
        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null) _startX = float.Parse(nodeAttribute.Value.ToString());
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null) _endX = float.Parse(nodeAttribute.Value);

        // convert to a negative to account for our reversed vertical axis
        _startX *= -1;
        _endX *= -1;

        nodeAttribute = node.Attributes.GetNamedItem("center");
        if (nodeAttribute != null)
        {
          string centerPos = nodeAttribute.Value;
          GetPosition(centerPos, ref _centerX, ref _centerY);
          GUIGraphicsContext.ScaleHorizontal(ref _centerX);
          GUIGraphicsContext.ScaleVertical(ref _centerY);
        }
      }
      else // if (effect == EffectType.Zoom)
      {
        // effect defaults
        _startX = _startY = 100;
        _endX = _endY = 100;

        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null)
        {
          string start = nodeAttribute.Value;
          GetPosition(start, ref _startX, ref _startY);
          if (_startX == 0) _startX = 100;
          if (_startY == 0) _startY = 100;
        }
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null)
        {
          string endLine = nodeAttribute.Value;
          GetPosition(endLine, ref _endX, ref _endY);
          if (_endX == 0) _endX = 100;
          if (_endY == 0) _endY = 100;
        }
        nodeAttribute = node.Attributes.GetNamedItem("center");
        if (nodeAttribute != null)
        {
          string center = nodeAttribute.Value;
          GetPosition(center, ref _centerX, ref _centerY);
          GUIGraphicsContext.ScaleHorizontal(ref _centerX);
          GUIGraphicsContext.ScaleVertical(ref _centerY);
        }
      }
      return true;
    }
    // creates the reverse animation
    void CreateReverse(VisualEffect anim)
    {
      _acceleration = -anim._acceleration;
      _startX = anim._endX;
      _startY = anim._endY;
      _endX = anim._startX;
      _endY = anim._startY;
      _endAlpha = anim._startAlpha;
      _startAlpha = anim._endAlpha;
      _centerX = anim._centerX;
      _centerY = anim._centerY;
      _type = (AnimationType)(-(int)anim._type);
      _effect = anim._effect;
      _length = anim._length;
      _isReversible = anim._isReversible;
    }

    public void Animate(uint time, bool hasRendered)
    {
      // First start any queued animations
      if (_queuedProcess == AnimationProcess.Normal)
      {
        if (_currentProcess == AnimationProcess.Reverse)
          _start = (uint)(time - (int)(_length * _amount));  // reverse direction of effect
        else
          _start = time;
        _currentProcess = AnimationProcess.Normal;
      }
      else if (_queuedProcess == AnimationProcess.Reverse)
      {
        if (_currentProcess == AnimationProcess.Normal)
          _start = (uint)(time - (int)(_length * (1 - _amount))); // turn around direction of effect
        else
          _start = time;
        _currentProcess = AnimationProcess.Reverse;
      }
      // reset the queued state once we've rendered
      // Note that if we are delayed, then the resource may not have been allocated as yet
      // as it hasn't been rendered (is still invisible).  Ideally, the resource should
      // be allocated based on a visible state, rather than a bool on/off, then only rendered
      // if it's in the appropriate state (ie allow visible = NO, DELAYED, VISIBLE, and allocate
      // if it's not NO, render if it's VISIBLE)  The alternative, is to just always render
      // the control while it's in the DELAYED state (comes down to the definition of the states)
      if (hasRendered || _queuedProcess == AnimationProcess.Reverse || (_currentState == AnimationState.Delayed && _type > 0))
        _queuedProcess = AnimationProcess.None;
      // Update our animation process
      if (_currentProcess == AnimationProcess.Normal)
      {
        if (time - _start < _delay)
        {
          _amount = 0.0f;
          _currentState = AnimationState.Delayed;
        }
        else if (time - _start < _length + _delay)
        {
          _amount = (float)(time - _start - _delay) / _length;
          _currentState = AnimationState.InProcess;
        }
        else
        {
          _amount = 1.0f;
          _currentState = AnimationState.StateApplied;
        }
      }
      else if (_currentProcess == AnimationProcess.Reverse)
      {
        if (time - _start < _length)
        {
          _amount = 1.0f - (float)(time - _start) / _length;
          _currentState = AnimationState.InProcess;
        }
        else
        {
          _amount = 0.0f;
          _currentState = AnimationState.StateApplied;
        }
      }
    }
    public void SetCenter(float x, float y)
    {
      if (_effect == EffectType.Zoom || _effect == EffectType.Rotate)
      {
        if (_centerX == 0) _centerX = x;
        if (_centerY == 0) _centerY = y;
      }
    }

    public void RenderAnimation(ref TransformMatrix matrix)
    {
      // If we have finished an animation, reset the animation state
      // We do this here (rather than in Animate()) as we need the
      // _currentProcess information in the UpdateStates() function of the
      // window and control classes.

      // Now do the real animation
      if (_currentProcess != AnimationProcess.None)
      {
        float offset = _amount * (_acceleration * _amount + 1.0f - _acceleration);
        if (_effect == EffectType.Fade)
        {
          _matrix.SetFader(((float)(_endAlpha - _startAlpha) * _amount + _startAlpha) * 0.01f);
        }
        else if (_effect == EffectType.Slide)
        {
          _matrix.SetTranslation((_endX - _startX) * offset + _startX, (_endY - _startY) * offset + _startY);
        }
        else if (_effect == EffectType.Rotate)
        {
          _matrix.SetTranslation(_centerX, _centerY);
          _matrix.multiplyAssign(TransformMatrix.CreateRotation(((_endX - _startX) * offset + _startX) * DEGREE_TO_RADIAN));
          _matrix.multiplyAssign(TransformMatrix.CreateTranslation(-_centerX, -_centerY));
        }
        else if (_effect == EffectType.Zoom)
        {
          float scaleX = ((_endX - _startX) * offset + _startX) * 0.01f;
          float scaleY = ((_endY - _startY) * offset + _startY) * 0.01f;
          _matrix.SetTranslation(_centerX, _centerY);
          _matrix.multiplyAssign(TransformMatrix.CreateScaler(scaleX, scaleY));
          _matrix.multiplyAssign(TransformMatrix.CreateTranslation(-_centerX, -_centerY));
        }
      }
      if (_currentState == AnimationState.StateApplied)
        _currentProcess = AnimationProcess.None;

      if (_currentState != AnimationState.None)
        matrix.multiplyAssign(_matrix);
    }

    public void ResetAnimation()
    {
      _currentProcess = AnimationProcess.None;
      _queuedProcess = AnimationProcess.None;
      _currentState = AnimationState.None;
    }
    public bool IsReversible
    {
      get
      {
        return _isReversible;
      }
    }

    public AnimationProcess QueuedProcess
    {
      get
      {
        return _queuedProcess;
      }
      set
      {
        _queuedProcess = value;
      }
    }

    public AnimationProcess CurrentProcess
    {
      get
      {
        return _currentProcess;
      }
      set
      {
        _currentProcess = value;
      }
    }
    public AnimationType AnimationType
    {
      get
      {
        return _type;
      }
      set
      {
         _type=value;
       }
     }
    public AnimationState CurrentState
    {
      get
      {
        return _currentState;
      }
      set
      {
        _currentState = value;
      }
    }
    public int Condition
    {
      get
      {
        return _condition;
      }
    }
    public EffectType Effect
    {
      get
      {
        return _effect;
      }
      set
      {
        _effect = value;
      }
    }
    public float CenterX
    {
      get
      {
        return _centerX;
      }
      set
      {
        _centerX = value;
      }
    }
    public float CenterY
    {
      get
      {
        return _centerY;
      }
      set
      {
        _centerY = value;
      }
    }
    public float StartX
    {
      get
      {
        return _startX;
      }
      set
      {
        _startX = value;
      }
    }
    public float EndX
    {
      get
      {
        return _endX;
      }
      set
      {
        _endX = value;
      }
    }
    public float StartY
    {
      get
      {
        return _startY;
      }
      set
      {
        _startY = value;
      }
    }
    public float EndY
    {
      get
      {
        return _endY;
      }
      set
      {
        _endY = value;
      }
    }
    public float Acceleration
    {
      get
      {
        return _acceleration;
      }
      set
      {
        _acceleration = value;
      }
    }
    public float Amount
    {
      get
      {
        return _amount;
      }
      set
      {
        _amount = value;
      }
    }

    #region ICloneable Members

    public object Clone()
    {
      VisualEffect effect = new VisualEffect();
      effect._type = _type;
      effect._effect = _effect;
      effect._queuedProcess = _queuedProcess;
      effect._currentState = _currentState;
      effect._currentProcess = _currentProcess;
      effect._condition = _condition;
      effect._acceleration = _acceleration;
      effect._startX = _startX;
      effect._startY = _startY;
      effect._endX = _endX;
      effect._endY = _endY;
      effect._centerX = _centerX;
      effect._centerY = _centerY;
      effect._startAlpha = _startAlpha;
      effect._endAlpha = _endAlpha;
      effect._amount = _amount;
      effect._start = _start;
      effect._length = _length;
      effect._delay = _delay;
      effect._isReversible = _isReversible;
      effect._matrix = (TransformMatrix)_matrix.Clone();
      return effect;
    }

    #endregion
  }
}
