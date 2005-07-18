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
			Open(0);
		}

		public void Open(int mixerIndex)
		{
			lock(this)
			{
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
				_isMuted = (int)GetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Mute) == 1;
				_volume = (int)GetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Volume);
			}
		}

		object GetValue(MixerComponentType componentType, MixerControlType controlType)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine();

			mixerLine.Size = Marshal.SizeOf(mixerLine);
			mixerLine.Destination = 0; 
			mixerLine.Source = 0; 
			mixerLine.LineId = 0; 
			mixerLine.Status = MixerLineStatusFlags.Disconnected; 
			mixerLine.dwUser = 0; 
			mixerLine.ComponentType = MixerComponentType.DestinationSpeakers; 
			mixerLine.Channels = 0; 
			mixerLine.Connections = 0; 
			mixerLine.Controls = 0; 
			mixerLine.ShortName = string.Empty; 
			mixerLine.Name = string.Empty; 
			mixerLine.Type = MixerLineTargetType.None; 
			mixerLine.DeviceId = 0;
			mixerLine.ManufacturerId = 0; 
			mixerLine.ProductId = 0 ; 
			mixerLine.DriverVersion = 0;
			mixerLine.ProductName = string.Empty; 

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.OpenControl.1");
			
			MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls();

			mixerLineControls.Size = Marshal.SizeOf(mixerLineControls);
			mixerLineControls.LineId = mixerLine.LineId;
			mixerLineControls.ControlType = Convert.ToUInt32(controlType);
			mixerLineControls.Controls = 1;
			mixerLineControls.Data = Marshal.AllocCoTaskMem(152);
			mixerLineControls.DataSize = 152;

//			if(MixerNativeMethods.mixerGetLineControlsA(_handle, ref mixerLineControls, MixerLineControlFlags.OneByType) != MixerError.None)
//				throw new InvalidOperationException("Mixer.OpenControl.2");

			MixerError errorX = MixerNativeMethods.mixerGetLineControlsA(_handle, ref mixerLineControls, MixerLineControlFlags.OneByType);

			MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl)); 

			MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails();

			mixerControlDetails.Size = Marshal.SizeOf(mixerControlDetails); 
			mixerControlDetails.ControlId = mixerControl.ControlId; 
			mixerControlDetails.Data = Marshal.AllocCoTaskMem(4); 
			mixerControlDetails.Channels = 1;
			mixerControlDetails.Item = 0;
			mixerControlDetails.DataSize = Marshal.SizeOf(4);

			MixerError error = MixerNativeMethods.mixerGetControlDetailsA(_handle, ref mixerControlDetails, 0);

			int value = Marshal.ReadInt32(mixerControlDetails.Data);

			Marshal.FreeCoTaskMem(mixerLineControls.Data);
			Marshal.FreeCoTaskMem(mixerControlDetails.Data);

			return value;
		}

		void SetValue(MixerComponentType componentType, MixerControlType controlType, bool controlValue)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine();

			mixerLine.Size = Marshal.SizeOf(mixerLine);
			mixerLine.ComponentType = componentType;

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.1");
			
			MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls();

			mixerLineControls.Size = Marshal.SizeOf(mixerLineControls);
			mixerLineControls.LineId = mixerLine.LineId;
			mixerLineControls.ControlType = Convert.ToUInt32(controlType);
			mixerLineControls.Controls = 1;
			mixerLineControls.Data = Marshal.AllocCoTaskMem(152);
			mixerLineControls.DataSize = 152;

			if(MixerNativeMethods.mixerGetLineControlsA(_handle, ref mixerLineControls, MixerLineControlFlags.OneByType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.2");

			MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl)); 

			MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails();

			mixerControlDetails.Size = Marshal.SizeOf(mixerControlDetails); 
			mixerControlDetails.ControlId = mixerControl.ControlId; 
			mixerControlDetails.Data = Marshal.AllocCoTaskMem(4);
			mixerControlDetails.Channels = 1;
			mixerControlDetails.Item = 0;
			mixerControlDetails.DataSize = 4;

			Marshal.WriteInt32(mixerControlDetails.Data, controlValue ? 1 : 0);

			MixerError error = MixerNativeMethods.mixerSetControlDetails(_handle, ref mixerControlDetails, 0);

			Marshal.FreeCoTaskMem(mixerLineControls.Data);
			Marshal.FreeCoTaskMem(mixerControlDetails.Data);
		}

		void SetValue(MixerComponentType componentType, MixerControlType controlType, int controlValue)
		{
			MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine();

			mixerLine.Size = Marshal.SizeOf(mixerLine);
			mixerLine.ComponentType = componentType;

			if(MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.1");
			
			MixerNativeMethods.MixerLineControls mixerLineControls = new MixerNativeMethods.MixerLineControls();

			mixerLineControls.Size = Marshal.SizeOf(mixerLineControls);
			mixerLineControls.LineId = mixerLine.LineId;
			mixerLineControls.ControlType = Convert.ToUInt32(controlType);
			mixerLineControls.Controls = 1;
			mixerLineControls.Data = Marshal.AllocCoTaskMem(152);
			mixerLineControls.DataSize = 152;

			if(MixerNativeMethods.mixerGetLineControlsA(_handle, ref mixerLineControls, MixerLineControlFlags.OneByType) != MixerError.None)
				throw new InvalidOperationException("Mixer.SetValue.2");

			MixerNativeMethods.MixerControl mixerControl = (MixerNativeMethods.MixerControl)Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl)); 

			MixerNativeMethods.MixerControlDetails mixerControlDetails = new MixerNativeMethods.MixerControlDetails();

			mixerControlDetails.Size = Marshal.SizeOf(mixerControlDetails); 
			mixerControlDetails.ControlId = mixerControl.ControlId; 
			mixerControlDetails.Data = Marshal.AllocCoTaskMem(4);
			mixerControlDetails.Channels = 1;
			mixerControlDetails.Item = 0;
			mixerControlDetails.DataSize = 4;

			Marshal.WriteInt32(mixerControlDetails.Data, controlValue);

			MixerError error = MixerNativeMethods.mixerSetControlDetails(_handle, ref mixerControlDetails, 0);

			Marshal.FreeCoTaskMem(mixerLineControls.Data);
			Marshal.FreeCoTaskMem(mixerControlDetails.Data);
		}

		void OnLineChanged(object sender, MixerEventArgs e)
		{		
			if(LineChanged != null)
				LineChanged(sender, e);
		}
		
		void OnControlChanged(object sender, MixerEventArgs e)
		{
			_isMuted = (int)GetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Mute) == 1;
			_volume = (int)GetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Volume);

			if(ControlChanged != null)
				ControlChanged(sender, e);
		}

		#endregion Methods

		#region Properties

		public bool IsMuted
		{
			get { lock(this) return _isMuted; }
			set { lock(this) _isMuted = value; SetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Mute, _isMuted); }
		}

		public IntPtr Handle
		{
			get { lock(this) return _handle; }
		}

		public int Volume
		{
			get { lock(this) return _volume; }
			set { lock(this) _volume = Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)); SetValue(MixerComponentType.DestinationSpeakers, MixerControlType.Volume, _volume); }
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

		IntPtr						_handle;
		bool						_isMuted;
		static MixerEventListener	_mixerEventListener;
		int							_volume;

		#endregion Fields
	}
}
