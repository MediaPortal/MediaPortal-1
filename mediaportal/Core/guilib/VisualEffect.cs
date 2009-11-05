#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
    Conditional,
  } ;

  public enum EffectType
  {
    None = 0,
    Fade,
    Slide,
    RotateX,
    RotateY,
    RotateZ,
    Zoom
  } ;

  public enum AnimationProcess
  {
    None = 0,
    Normal,
    Reverse
  } ;

  public enum AnimationState
  {
    None = 0,
    Delayed,
    InProcess,
    StateApplied
  } ;

  public enum AnimationRepeat
  {
    None = 0,
    Pulse,
    Loop
  } ;

  public enum ClockHandleType
  {
    None = 0,
    Hour ,
    Minute,
    Second
  } ;


  public class VisualEffect : ICloneable
  {
    private AnimationType _type;
    private EffectType _effect;
    private AnimationProcess _queuedProcess;
    private AnimationState _currentState;
    private AnimationProcess _currentProcess;
    private AnimationRepeat _repeatAnim;
    private Tweener _tweener;
    private int _condition; // conditions that must be satisfied in order for this // animation to be performed
    // animation variables
    //float _acceleration;
    private float _startX;
    private float _startY;
    private float _endX;
    private float _endY;
    private float _centerX;
    private float _centerY;
    private int _startAlpha;
    private int _endAlpha;

    // timing variables
    private float _amount;
    private uint _start;
    private uint _length;
    private uint _delay;

    // Clock Animation vars
    private int _savedMinute; 
    private int _savedHour; 
    private ClockHandleType _clockHandle;

    private bool _isReversible; // whether the animation is reversible or not
    private bool _lastCondition;
    private TransformMatrix _matrix = new TransformMatrix();

    private const float DEGREE_TO_RADIAN = 0.01745329f;

    public VisualEffect()
    {
      Reset();
    }

    public void Reset()
    {
      _tweener = null;
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
      //_acceleration = 0;
      _condition = 0;
      _isReversible = true;
      _lastCondition = false;
      _repeatAnim = AnimationRepeat.None;
      _clockHandle = ClockHandleType.None;
      _savedMinute = -1;
      _savedHour = -1;
    }

    private float GetFloat(string text)
    {
      bool useCommas = false;
      float fTest = 123.12f;
      string test = fTest.ToString();
      if (test.IndexOf(",") >= 0)
      {
        useCommas = true;
      }
      if (useCommas)
      {
        text = text.Replace(".", ",");
      }
      else
      {
        text = text.Replace(",", ".");
      }
      return float.Parse(text);
    }

    private void GetPosition(string text, ref float x, ref float y)
    {
      //      Log.Info("GetPos:{0}", text);
      x = y = 0;
      if (text == null)
      {
        return;
      }
      text = text.ToLower();
      text = text.Replace("screencenterx", GUIGraphicsContext.OutputScreenCenter.X.ToString());
      text = text.Replace("screencentery", GUIGraphicsContext.OutputScreenCenter.Y.ToString());

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
      if (String.Compare(animType, "visible", true) == 0 || String.Compare(animType, "visiblechange", true) == 0)
      {
        _type = AnimationType.Visible;
      }
      else if (String.Compare(animType, "hidden", true) == 0)
      {
        _type = AnimationType.Hidden;
      }
      else if (String.Compare(animType, "focus", true) == 0)
      {
        _type = AnimationType.Focus;
      }
      else if (String.Compare(animType, "unfocus", true) == 0)
      {
        _type = AnimationType.Unfocus;
      }
      else if (String.Compare(animType, "windowopen", true) == 0)
      {
        _type = AnimationType.WindowOpen;
      }
      else if (String.Compare(animType, "windowclose", true) == 0)
      {
        _type = AnimationType.WindowClose;
      }
      else if (String.Compare(animType, "conditional", true) == 0)
      {
        _type = AnimationType.Conditional;
        SetInitialCondition();
      }
      if (_type == AnimationType.None)
      {
        Log.Error(String.Format("Control has invalid animation type [{0} on {1}]", animType, node.Name));
        return false;
      }
      XmlNode nodeAttribute = node.Attributes.GetNamedItem("condition");
      if (nodeAttribute != null)
      {
        string conditionString = nodeAttribute.Value;
        _condition = GUIInfoManager.TranslateString(conditionString);
      }
      nodeAttribute = node.Attributes.GetNamedItem("effect");
      if (nodeAttribute == null)
      {
        return false;
      }
      string effectType = nodeAttribute.Value;
      // effect type
      if (String.Compare(effectType, "fade", true) == 0)
      {
        _effect = EffectType.Fade;
      }
      else if (String.Compare(effectType, "slide", true) == 0)
      {
        _effect = EffectType.Slide;
      }
      else if (String.Compare(effectType, "rotate", true) == 0)
      {
        _effect = EffectType.RotateZ;
      }
      else if (String.Compare(effectType, "rotatey", true) == 0)
      {
        _effect = EffectType.RotateY;
      }
      else if (String.Compare(effectType, "rotatex", true) == 0)
      {
        _effect = EffectType.RotateX;
      }
      else if (String.Compare(effectType, "zoom", true) == 0)
      {
        _effect = EffectType.Zoom;
      }
      if (_effect == EffectType.None)
      {
        Log.Error(String.Format("Control has invalid effect type [{0} on {1}]", effectType, node.Name));
        return false;
      }
      // time and delay
      nodeAttribute = node.Attributes.GetNamedItem("time");
      UInt32.TryParse(nodeAttribute.Value.ToString(), out _length);

      nodeAttribute = node.Attributes.GetNamedItem("delay");
      if (nodeAttribute != null)
      {
        UInt32.TryParse(nodeAttribute.Value.ToString(), out _delay);
      }

      //_length = (uint)(_length * g_SkinInfo.GetEffectsSlowdown());
      //_delay = (uint)(_delay * g_SkinInfo.GetEffectsSlowdown());

      _tweener = null;
      nodeAttribute = node.Attributes.GetNamedItem("tween");
      if (nodeAttribute != null)
      {
        string tweenMode = nodeAttribute.Value;
        if (tweenMode == "linear")
        {
          _tweener = new LinearTweener();
        }
        else if (tweenMode == "quadratic")
        {
          _tweener = new QuadTweener();
        }
        else if (tweenMode == "cubic")
        {
          _tweener = new CubicTweener();
        }
        else if (tweenMode == "sine")
        {
          _tweener = new SineTweener();
        }
        else if (tweenMode == "back")
        {
          _tweener = new BackTweener();
        }
        else if (tweenMode == "circle")
        {
          _tweener = new CircleTweener();
        }
        else if (tweenMode == "bounce")
        {
          _tweener = new BounceTweener();
        }
        else if (tweenMode == "elastic")
        {
          _tweener = new ElasticTweener();
        }
        nodeAttribute = node.Attributes.GetNamedItem("easing");
        if (nodeAttribute != null && _tweener != null)
        {
          string easing = nodeAttribute.Value;
          if (easing == "in")
          {
            _tweener.Easing = TweenerType.EASE_IN;
          }
          else if (easing == "out")
          {
            _tweener.Easing = TweenerType.EASE_OUT;
          }
          else if (easing == "inout")
          {
            _tweener.Easing = TweenerType.EASE_INOUT;
          }
        }
      }

      // acceleration of effect
      //float accel;
      nodeAttribute = node.Attributes.GetNamedItem("acceleration");
      if (nodeAttribute != null)
      {
        float acceleration = GetFloat(nodeAttribute.Value.ToString());
        if (_tweener == null)
        {
          if (acceleration != 0.0f)
          {
            _tweener = new QuadTweener(acceleration);
            _tweener.Easing = TweenerType.EASE_IN;
          }
          else
          {
            _tweener = new LinearTweener();
          }
        }
      }


      // reversible (defaults to true)
      nodeAttribute = node.Attributes.GetNamedItem("reversible");
      if (nodeAttribute != null)
      {
        string reverse = nodeAttribute.Value;
        if (String.Compare(reverse, "false") == 0)
        {
          _isReversible = false;
        }
      }


      // conditional parameters
      if (_type == AnimationType.Conditional)
      {
        nodeAttribute = node.Attributes.GetNamedItem("pulse");
        if (nodeAttribute != null)
        {
          string reverse = nodeAttribute.Value;
          if (String.Compare(reverse, "true") == 0)
          {
            _repeatAnim = AnimationRepeat.Pulse;
          }
        }
        nodeAttribute = node.Attributes.GetNamedItem("loop");
        if (nodeAttribute != null)
        {
          string reverse = nodeAttribute.Value;
          if (String.Compare(reverse, "true") == 0)
          {
            _repeatAnim = AnimationRepeat.Loop;
          }
        }

        // Analog Clock animation
        nodeAttribute = node.Attributes.GetNamedItem("clockhandle");
        if (nodeAttribute != null)
        {
          string clockType = nodeAttribute.Value;
          if (String.Compare(clockType, "second") == 0)
          {
            _clockHandle = ClockHandleType.Second;
          }
          else if (String.Compare(clockType, "minute") == 0)
          {
            _clockHandle = ClockHandleType.Minute;
          }
          else if (String.Compare(clockType, "hour") == 0)
          {
            _clockHandle = ClockHandleType.Hour;
          }
          else
          {
            _clockHandle = ClockHandleType.None;
          }
        }
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
        GUIGraphicsContext.ScaleHorizontal(ref _startX);
        GUIGraphicsContext.ScaleVertical(ref _startY);
        GUIGraphicsContext.ScaleHorizontal(ref _endX);
        GUIGraphicsContext.ScaleVertical(ref _endY);
      }
      else if (_effect == EffectType.Fade)
      {
        // alpha parameters
        if (_type < 0)
        {
          // out effect defaults
          _startAlpha = 100;
          _endAlpha = 0;
        }
        else
        {
          // in effect defaults
          _startAlpha = 0;
          _endAlpha = 100;
        }
        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null)
        {
          _startAlpha = Int32.Parse(nodeAttribute.Value.ToString());
        }
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null)
        {
          _endAlpha = Int32.Parse(nodeAttribute.Value.ToString());
        }

        if (_startAlpha > 100)
        {
          _startAlpha = 100;
        }
        if (_endAlpha > 100)
        {
          _endAlpha = 100;
        }
        if (_startAlpha < 0)
        {
          _startAlpha = 0;
        }
        if (_endAlpha < 0)
        {
          _endAlpha = 0;
        }
      }
      else if (_effect == EffectType.RotateZ || _effect == EffectType.RotateX || _effect == EffectType.RotateY)
      {
        nodeAttribute = node.Attributes.GetNamedItem("start");
        if (nodeAttribute != null)
        {
          _startX = float.Parse(nodeAttribute.Value);
        }
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null)
        {
          _endX = float.Parse(nodeAttribute.Value);
        }

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
          if (_startX == 0)
          {
            _startX = 100;
          }
          if (_startY == 0)
          {
            _startY = 100;
          }
        }
        nodeAttribute = node.Attributes.GetNamedItem("end");
        if (nodeAttribute != null)
        {
          string endLine = nodeAttribute.Value;
          GetPosition(endLine, ref _endX, ref _endY);
          if (_endX == 0)
          {
            _endX = 100;
          }
          if (_endY == 0)
          {
            _endY = 100;
          }
        }
        nodeAttribute = node.Attributes.GetNamedItem("center");
        if (nodeAttribute != null)
        {
          string center = nodeAttribute.Value;
          GetPosition(center, ref _centerX, ref _centerY);
          GUIGraphicsContext.ScaleHorizontal(ref _centerX);
          GUIGraphicsContext.ScaleVertical(ref _centerY);
        }
        else
        {
          /*
          // no center specified
          // calculate the center position...
          if (_startX != 0)
          {
            float scale = _endX / _startX;
            if (scale != 1)
              _centerX = (_endPosX - scale * _startPosX) / (1 - scale);
          }
          if (_startY != 0)
          {
            float scale = _endY / _startY;
            if (scale != 1)
              _centerY = (_endPosY - scale * _startPosY) / (1 - scale);
          }*/
        }
      }
      return true;
    }

    public void Animate(uint time, bool startAnim)
    {
      // First start any queued animations
      if (_queuedProcess == AnimationProcess.Normal)
      {
        if (_currentProcess == AnimationProcess.Reverse)
        {
          _start = (uint) (time - (int) (_length*_amount)); // reverse direction of animation
        }
        else
        {
          _start = time;
        }
        _currentProcess = AnimationProcess.Normal;
      }
      else if (_queuedProcess == AnimationProcess.Reverse)
      {
        if (_currentProcess == AnimationProcess.Normal)
        {
          _start = (uint) (time - (int) (_length*(1 - _amount))); // turn around direction of animation
        }
        else if (_currentProcess == AnimationProcess.None)
        {
          _start = time;
        }
        _currentProcess = AnimationProcess.Reverse;
      }
      // reset the queued state once we've rendered to ensure allocation has occured
      if (startAnim || _queuedProcess == AnimationProcess.Reverse)
        // || (_currentState == ANI_STATE_DELAYED && _type > 0))
      {
        _queuedProcess = AnimationProcess.None;
      }

      if (_type == AnimationType.Conditional)
      {
        UpdateCondition();
      }

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
          _amount = (float) (time - _start - _delay)/_length;
          _currentState = AnimationState.InProcess;
        }
        else
        {
          _amount = 1.0f;
          if (_repeatAnim == AnimationRepeat.Pulse && _lastCondition)
          {
            // pulsed anims auto-reverse
            _currentProcess = AnimationProcess.Reverse;
            _start = time;
          }
          else if (_repeatAnim == AnimationRepeat.Loop && _lastCondition)
          {
            // looped anims start over
            _amount = 0.0f;
            _start = time;
          }
          else
          {
            _currentState = AnimationState.StateApplied;
          }
        }
      }
      else if (_currentProcess == AnimationProcess.Reverse)
      {
        if (time - _start < _length)
        {
          _amount = 1.0f - (float) (time - _start)/_length;
          _currentState = AnimationState.InProcess;
        }
        else
        {
          _amount = 0.0f;
          if (_repeatAnim == AnimationRepeat.Pulse && _lastCondition)
          {
            // pulsed anims auto-reverse
            _currentProcess = AnimationProcess.Normal;
            _start = time;
          }
          else
          {
            _currentState = AnimationState.StateApplied;
          }
        }
      }
    }


    public void RenderAnimation(ref TransformMatrix matrix)
    {
      // If we have finished an animation, reset the animation state
      // We do this here (rather than in Animate()) as we need the
      // currentProcess information in the UpdateStates() function of the
      // window and control classes.

      // Now do the real animation
      if (_currentProcess != AnimationProcess.None)
      {
        Calculate();
      }
      if (_currentState == AnimationState.StateApplied)
      {
        _currentProcess = AnimationProcess.None;
        _queuedProcess = AnimationProcess.None;
      }
      if (_currentState != AnimationState.None)
      {
        matrix.multiplyAssign(_matrix);
      }
    }

    private void Calculate()
    {
      // If we have finished an animation, reset the animation state
      // We do this here (rather than in Animate()) as we need the
      // _currentProcess information in the UpdateStates() function of the
      // window and control classes.

      float offset = _amount;
      if (_tweener != null)
      {
        offset = _tweener.Tween(_amount, 0.0f, 1.0f, 1.0f);
      }

      if (_effect == EffectType.Fade)
      {
        _matrix.SetFader(((float) (_endAlpha - _startAlpha)*offset + _startAlpha)*0.01f);
      }
      else if (_effect == EffectType.Slide)
      {
        _matrix.SetTranslation((_endX - _startX)*offset + _startX, (_endY - _startY)*offset + _startY, 0);
      }
      else if (_effect == EffectType.RotateX)
      {
        _matrix.SetXRotation(((_endX - _startX)*offset + _startX)*DEGREE_TO_RADIAN, _centerX, _centerY, 1.0f);
      }
      else if (_effect == EffectType.RotateY)
      {
        _matrix.SetYRotation(((_endX - _startX)*offset + _startX)*DEGREE_TO_RADIAN, _centerX, _centerY, 1.0f);
      }
      else if (_effect == EffectType.RotateZ)
      {
        if (_clockHandle != ClockHandleType.None)
        {
          SetClock();
        }
        _matrix.SetZRotation(((_endX - _startX)*offset + _startX)*DEGREE_TO_RADIAN, _centerX, _centerY,
                             GUIGraphicsContext.PixelRatio);
      }
      else if (_effect == EffectType.Zoom)
      {
        float scaleX = ((_endX - _startX)*offset + _startX)*0.01f;
        float scaleY = ((_endY - _startY)*offset + _startY)*0.01f;
        _matrix.SetScaler(scaleX, scaleY, _centerX, _centerY);
      }
    }

    private void SetClock()
    {
      DateTime currentTime = DateTime.Now;

      if (_clockHandle == ClockHandleType.Second)
      {
        _startX = (float)Math.Ceiling(currentTime.Second/60.0*360.0);
        _endX = _startX;
      }
      else if (_clockHandle == ClockHandleType.Minute)
      {
        _savedMinute = currentTime.Minute;
        _startX = (float)Math.Ceiling(currentTime.Minute / 60.0 * 360.0);
        _endX = _startX;
      }
      else if (_savedHour != currentTime.Hour)
      {
        _savedHour = currentTime.Hour;
        _startX = (float)Math.Ceiling((currentTime.Hour / 12.0 * 360.0) + (currentTime.Minute / 60.0 * 30.0));
        _endX = _startX;
      }
    }

    public void ResetAnimation()
    {
      _currentProcess = AnimationProcess.None;
      _queuedProcess = AnimationProcess.None;
      _currentState = AnimationState.None;
    }

    public void ApplyAnimation()
    {
      _queuedProcess = AnimationProcess.None;
      if (_repeatAnim == AnimationRepeat.Pulse)
      {
        // pulsed anims auto-reverse
        _amount = 1.0f;
        _currentProcess = AnimationProcess.Reverse;
        _currentState = AnimationState.InProcess;
      }
      else if (_repeatAnim == AnimationRepeat.Loop)
      {
        // looped anims start over
        _amount = 0.0f;
        _currentProcess = AnimationProcess.Normal;
        _currentState = AnimationState.InProcess;
      }
      else
      {
        _currentProcess = AnimationProcess.Normal;
        _currentState = AnimationState.StateApplied;
        _amount = 1.0f;
      }
      Calculate();
    }

    public void UpdateCondition()
    {
      bool condition = GUIInfoManager.GetBool(_condition, 0);
      if (condition && !_lastCondition)
      {
        _queuedProcess = AnimationProcess.Normal;
      }
      else if (!condition && _lastCondition)
      {
        if (_isReversible)
        {
          _queuedProcess = AnimationProcess.Reverse;
        }
        else
        {
          ResetAnimation();
        }
      }
      _lastCondition = condition;
    }

    public void SetInitialCondition()
    {
      _lastCondition = GUIInfoManager.GetBool(_condition, 0);
      if (_lastCondition)
      {
        ApplyAnimation();
      }
      else
      {
        ResetAnimation();
      }
    }

    private void QueueAnimation(AnimationProcess process)
    {
      _queuedProcess = process;
    }

    public void SetCenter(float x, float y)
    {
      if (_effect == EffectType.Zoom || _effect == EffectType.RotateZ)
      {
        if (_centerX == 0.0f)
        {
          _centerX = x;
        }
        if (_centerY == 0.0f)
        {
          _centerY = y;
        }
      }
    }

    public bool IsReversible
    {
      get { return _isReversible; }
    }

    public AnimationProcess QueuedProcess
    {
      get { return _queuedProcess; }
      set { _queuedProcess = value; }
    }

    public AnimationProcess CurrentProcess
    {
      get { return _currentProcess; }
      set { _currentProcess = value; }
    }

    public AnimationType AnimationType
    {
      get { return _type; }
      set { _type = value; }
    }

    public AnimationState CurrentState
    {
      get { return _currentState; }
      set { _currentState = value; }
    }

    public int Condition
    {
      get { return _condition; }
    }

    public EffectType Effect
    {
      get { return _effect; }
      set { _effect = value; }
    }

    public float CenterX
    {
      get { return _centerX; }
      set { _centerX = value; }
    }

    public float CenterY
    {
      get { return _centerY; }
      set { _centerY = value; }
    }

    public float StartX
    {
      get { return _startX; }
      set { _startX = value; }
    }

    public float EndX
    {
      get { return _endX; }
      set { _endX = value; }
    }

    public float StartY
    {
      get { return _startY; }
      set { _startY = value; }
    }

    public float EndY
    {
      get { return _endY; }
      set { _endY = value; }
    }

    //public float Acceleration
    //{
    //  get
    //  {
    //    return _acceleration;
    //  }
    //  set
    //  {
    //    _acceleration = value;
    //  }
    //}
    public float Amount
    {
      get { return _amount; }
      set { _amount = value; }
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
      //effect._acceleration = _acceleration;
      effect._startX = _startX;
      effect._startY = _startY;
      effect._endX = _endX;
      effect._endY = _endY;
      effect._centerX = _centerX;
      effect._centerY = _centerY;
      effect._startAlpha = _startAlpha;
      effect._repeatAnim = _repeatAnim;
      effect._endAlpha = _endAlpha;
      effect._lastCondition = _lastCondition;
      effect._amount = _amount;
      effect._start = _start;
      effect._length = _length;
      effect._delay = _delay;
      effect._isReversible = _isReversible;
      effect._matrix = (TransformMatrix) _matrix.Clone();
      effect._tweener = _tweener;
      effect._clockHandle = _clockHandle;
      effect._savedMinute = _savedMinute;
      effect._savedHour = _savedHour;
      return effect;
    }

    #endregion
  }
}