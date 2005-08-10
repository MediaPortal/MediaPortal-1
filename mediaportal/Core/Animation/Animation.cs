using System;
using System.ComponentModel;

using MediaPortal.Dispatcher;

namespace MediaPortal.Animations
{
	public abstract class Animation
	{
		#region Events

		/// <summary>Occurs when this timeline begins an active duration.</summary>
		public event EventHandler Begun;

		/// <summary>Occurs when this timeline's active duration ends.</summary>
		public event EventHandler Ended;

		/// <summary>Occurs when this timeline, its ancestors, or any of its active child timelines becomes paused.</summary>
//		public event EventHandler Paused;

		/// <summary>Occurs whenever this timeline repeats its simple duration.</summary>
		public event EventHandler Repeated;

		/// <summary>Occurs when this timeline resumes playing after being paused.</summary>
//		public event EventHandler Resumed;

		/// <summary>Occurs when this timeline reverses direction.</summary>
//		public event EventHandler Reversed;

		/// <summary>Occurs when this timeline's progress or simple time changes as a result of a .</summary>
//		public event EventHandler Seeked;

		/// <summary>Occurs whenever the value of the CurrentGlobalSpeed property changes.</summary>
//		public event EventHandler SpeedChanged;

		#endregion Events

		#region Methods

		public void Attach(object target, DependencyBinding binding)
		{
			if(binding == null)
				throw new ArgumentNullException("binding");

			if(target == null)
				throw new ArgumentNullException("target");

			_target = target;
			_binding = binding;
		}

		public void Tick(int tickCurrent)
		{
			if(_isPaused)
				return;

			if(_isAnimating == false)
				return;

			if(tickCurrent > _beginTime + _duration)
			{
				EventHandler eventHandler = null;

				if(_repeatBehavior.Equals(RepeatBehavior.Forever))
				{
					eventHandler = Repeated;
				}
				else if(_repeatBehavior.IsIterationCount)
				{
					eventHandler = Repeated;

					if(++_iterationCount == (_repeatBehavior.IterationCount * (_isAutoReverse ? 2 : 1)))
						_isAnimating = false;
				}
				else if(tickCurrent > _beginTime + _repeatBehavior.RepeatDuration)
				{
					eventHandler = Ended;
					_isAnimating = false;
				}

				if(eventHandler != null)
					eventHandler(this, EventArgs.Empty);

				if(_isAnimating)
				{
					_beginTime = tickCurrent;
					_isReversed = _isAutoReverse ? !_isReversed : _isReversed;
				} 
			}

			_binding.SetValue(_target, this.Value);
		}

		internal void AnimationWorker(object sender, DoWorkEventArgs e)
		{
			lock(this)
			{
				_iterationCount = 0;
				_isReversed = false;
				_isAnimating = true;
				_beginTime = AnimationTimer.Tick;

				if(Begun != null)
					Begun(this, EventArgs.Empty);
			}
		}

		#endregion Methods

		#region Properties

		public double AccelerationRatio
		{ 
			get { return _accelerationRatio; }
			set { _accelerationRatio = value; }
		}

		public bool AutoReverse
		{ 
			get { return _isAutoReverse; }
			set { _isAutoReverse = value; }
		}

		public double BeginTime
		{
			get { return _beginTime; }
			set { _beginTime = value; }
		}
		
		public AnimationCollection Children
		{ 
			get { return _children == null ? _children = new AnimationCollection(this) : _children; }
		}

		public double CutoffTime
		{
			get { return _cutoffTime; }
			set { _cutoffTime = value; }
		}

		public double DecelerationRatio
		{ 
			get { return _decelerationRatio; }
			set { _decelerationRatio = value; }
		}

		public Duration Duration
		{
			get { return _duration; }
			set { _duration = value; }
		}

		public string Path
		{
			get { return ""; }
			set { }
		}

		public RepeatBehavior RepeatBehavior
		{ 
			get { return _repeatBehavior; }
			set { _repeatBehavior = value; }
		}

		public ClockController InteractiveController
		{
			get { return _interactiveController == null ? _interactiveController = new ClockController(this) : _interactiveController; }
		}

		public bool IsAnimating
		{
			get { return _isAnimating; }
		}

		public bool IsReversed
		{
			get { return _isReversed; }
		}

		public Easing Easing
		{
			get { return _easingStyle; }
			set { _easingStyle = value; }
		}

		public abstract object Value { get; }

		#endregion Properties

		#region Fields

		double						_accelerationRatio;
		double						_beginTime;
		bool						_isAutoReverse;
		bool						_isAnimating;
		AnimationCollection			_children;
		double						_cutoffTime;
		double						_decelerationRatio;
		Duration					_duration = new Duration();
		RepeatBehavior				_repeatBehavior;
		object						_target;
		DependencyBinding			_binding;
		ClockController				_interactiveController;
		bool						_isReversed;
		int							_iterationCount = 0;
		bool						_isPaused = false;
		Easing						_easingStyle;

		#endregion Fields
	}
}