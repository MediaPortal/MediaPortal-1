using System;
using System.Collections;
using System.Windows.Forms;

using DShowNET;
using MediaPortal.Player;
namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for RadioAutoTuningForm.
	/// </summary>
	public class RadioAutoTuningForm : AutoTuningForm
	{
		int		currentChannel = 0;
		RadioGraph m_graph=null;

		public RadioAutoTuningForm(RadioGraph graph)
		{
			this.m_graph = graph;

			//
			// Setup progress bar
			//
			SetStep(100000);
			SetInterval(87500000, 108000000);
		}

		public override void OnStartTuning(int startValue)
		{
			m_graph.Tune(startValue);
		}

		public override void OnStopTuning()
		{
      Text=String.Format("Radio tuning");
		}

		private new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).BeginInit();
			// 
			// cancelButton
			// 
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			// 
			// groupBox1
			// 
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			// 
			// itemsListBox
			// 
			this.itemsListBox.Name = "itemsListBox";
			// 
			// progressBar
			// 
			this.progressBar.Name = "progressBar";
			// 
			// startButton
			// 
			this.startButton.Name = "startButton";
			// 
			// stopButton
			// 
			this.stopButton.Name = "stopButton";
			// 
			// tunerTimer
			// 
			this.tunerTimer.Enabled = false;
			// 
			// RadioAutoTuningForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 222);
			this.Name = "RadioAutoTuningForm";
			this.Text = "Scan for radio channels";
			((System.ComponentModel.ISupportInitialize)(this.tunerTimer)).EndInit();

		}

		public override void OnPerformTuning(int stepSize)
		{
      try
      {
        if(m_graph.SignalPresent == true)
        {
          //
          // We have found a channel!
          //
          RadioStation newStation =new RadioStation(currentChannel, m_graph.Channel);
          newStation.Name=String.Format("Station{0}", currentChannel++);
          newStation.Type="Radio";
          newStation.Frequency=m_graph.Channel;
          AddItem(newStation);
        }
      }
      catch(NotSupportedException)
      {
        // Unable to read signal strength, step to next frequency
      }

			//
			// Move forward
			//
      try
      {
        m_graph.Tune(m_graph.Channel + stepSize);
        Text=String.Format("Radio tuning:{0:} Hz", (int)m_graph.Channel );
      }
      catch(Exception)
      {
        MessageBox.Show("Failed to perform autotuning, the tuner card did not supply the data needed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
		}
	}
}
