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
        double dFreq=(double)m_graph.Channel ;
        dFreq/=1000000d;
        Text=String.Format("Radio tuning:{0:#,##} MHz", m_graph.Channel );
      }
      catch(Exception)
      {
        MessageBox.Show("Failed to perform autotuning, the tuner card did not supply the data needed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
		}
	}
}
