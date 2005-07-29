using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Mixer
{
	public sealed class Mixer : IDisposable
	{
		#region Events

		public event MixerEventHandler	LineChanged;
		public event MixerEventHandler	ControlChanged;

		#endregion Events

		#region Methods

		public void Close()
		{
			lock(this)
			{
				if(_handle == IntPtr.Zero)
					return;

				MixerNativeMethods.mixerClose(_handle);

				_handle = IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			Close();
		}

		public void Open()
		{
			Open(0, false);
		}

		public void Open(int mixerIndex, bool isSpeakers)
		{
			lock(this)
			{
				if(isSpeakers == false)
				{
					MediaPortal.GUI.Library.Log.Write("Using digital");
					_componentType = MixerComponentType.DestinationDigital;
				}

				if(_mixerEventListener == null)
				{
					_mixerEventListener = new MixerEventListener();
					_mixerEventListener.Start();
					_mixerEventListener.LineChanged += new MixerEventHandler(OnLineChanged);
					_mixerEventListener.ControlChanged += new MixerEventHandler(OnControlChanged);
				}

				MixerNativeMethods.MixerControl mc = new MixerNativeMethods.MixerControl();

				mc.Size = 0;
				mc.ControlId = 0;
				mc.ControlType = MixerControlType.Volume;
				mc.fdwControl = 0;
				mc.MultipleItems = 0;
				mc.ShortName = string.Empty;
				mc.Name = string.Empty;
				mc.Minimum = 0;
				mc.Maximum = 0;
				mc.Reserved = 0;
			
				IntPtr handle = IntPtr.Zero;

				MixerError error = MixerNativeMethods.mixerOpen(ref handle, mixerIndex, _mixerEventListener.Handle, 0, MixerFlags.CallbackWindow);
				
				if(error != MixerError.None)
					throw new InvalidOperationException();

				_handle = handle;
				_isMuted = (int)GetValue(_componentType, MixerControlType.Mute) == 1;
				_volume = (int)GetValue(_componentType, MixerControlType.Volume);
			}
		}

		object GetValue(MixerComponentType componentType, MixerControlType controlType)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine(componentType);

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.OpenControl.1");
			
			using(MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls(mixerLine.LineId, controlType))
			{
				MixerNativeMethods.mixerGetLineControlsA(_handle, mixerLineControls, MixerLineControlFlags.OneByType);
				MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl));

				using(MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails(mixerControl.ControlId))
				{
					MixerNativeMethods.mixerGetControlDetailsA(_handle, mixerControlDetails, 0);

					return Marshal.ReadInt32(mixerControlDetails.Data);
				}
			}
		}

		void SetValue(MixerComponentType componentType, MixerControlType controlType, bool controlValue)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine(componentType);

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.1");
			
			using(MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls(mixerLine.LineId, controlType))
			{
				if(MixerNativeMethods.mixerGetLineControlsA(_handle, mixerLineControls, MixerLineControlFlags.OneByType) != MixerError.None)
					throw new InvalidOperationException("Mixer.SetValue.2");

				MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl)); 

				using(MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails(mixerControl.ControlId))
				{
					Marshal.WriteInt32(mixerControlDetails.Data, controlValue ? 1 : 0);
					MixerNativeMethods.mixerSetControlDetails(_handle, mixerControlDetails, 0);
				}
			}
		}

		void SetValue(MixerComponentType componentType, MixerControlType controlType, int controlValue)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine(componentType);

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.1");
			
			using(MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls(mixerLine.LineId, controlType))
			{
				if(MixerNativeMethods.mixerGetLineControlsA(_handle, mixerLineControls, MixerLineControlFlags.OneByType) != MixerError.None)
					throw new InvalidOperationException("Mixer.SetValue.2");

				MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl)); 

				using(MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails(mixerControl.ControlId))
				{
					Marshal.WriteInt32(mixerControlDetails.Data, controlValue);
					MixerNativeMethods.mixerSetControlDetails(_handle, mixerControlDetails, 0);
				}
			}
		}

		void OnLineChanged(object sender, MixerEventArgs e)
		{		
			if(LineChanged != null)
				LineChanged(sender, e);
		}
		
		void OnControlChanged(object sender, MixerEventArgs e)
		{
			_isMuted = (int)GetValue(_componentType, MixerControlType.Mute) == 1;
			_volume = (int)GetValue(_componentType, MixerControlType.Volume);

			if(ControlChanged != null)
				ControlChanged(sender, e);
		}

		#endregion Methods

		#region Properties

		public bool IsMuted
		{
			get { lock(this) return _isMuted; }
			set { lock(this) _isMuted = value; SetValue(_componentType, MixerControlType.Mute, _isMuted); }
		}

		public IntPtr Handle
		{
			get { lock(this) return _handle; }
		}

		public int Volume
		{
			get { lock(this) return _volume; }
			set { lock(this) _volume = Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)); SetValue(_componentType, MixerControlType.Volume, _volume); }
		}

		public int VolumeMaximum
		{
			get { return 65535; }
		}

		public int VolumeMinimum
		{
			get { return 0; }
		}

		#endregion Properties

		#region Fields

		MixerComponentType			_componentType = MixerComponentType.DestinationSpeakers;
		IntPtr						_handle;
		bool						_isMuted;
		static MixerEventListener	_mixerEventListener;
		int							_volume;

		#endregion Fields
	}
}
