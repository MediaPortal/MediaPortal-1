using System;
using System.ComponentModel;

using MediaPortal.Dispatcher;

namespace MediaPortal.Animation
{
	public sealed class ClockController
	{
		#region Constructors

		private ClockController()
		{
		}

		internal ClockController(Animation animation)
		{
			_animation = animation;
		}

		#endregion Constructors

		#region Methods

		public void Begin()
		{
			_job = new Job();
			_job.DoWork += new DoWorkEventHandler(_animation.AnimationWorker);
			_job.Dispatch();
		}

		public void BeginIn(double beginIn)
		{
			_job = new Job();
			_job.DoWork += new DoWorkEventHandler(_animation.AnimationWorker);
			_job.Dispatch((int)beginIn);
		}

		#endregion Methods

		#region Fields
		
		Animation				_animation;
		Job						_job;

		#endregion Fields
	}
}
