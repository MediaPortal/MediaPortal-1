using System;
using System.Windows.Forms;

namespace MPTail
{
  public class RingBufferedRichTextBox : RichTextBox
  {
    private long ringBufferSize = 1024*1024*3; // we init with 3 mb

    #region constructor

    public RingBufferedRichTextBox()
    {
      TextChanged += new EventHandler(RingBufferedRichTextBox_TextChanged);
    }

    #endregion

    private void RingBufferedRichTextBox_TextChanged(object sender, EventArgs e)
    {
      if (TextLength > ringBufferSize)
        Text = Text.Remove(0, (int) (TextLength - ringBufferSize));
    }

    public long RingBufferSizeInMB
    {
      get { return (ringBufferSize/1024)/1024; }
      set { ringBufferSize = 1024*1024*value; }
    }
  }
}