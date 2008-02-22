using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MPTail
{
  public class RingBufferedRichTextBox: RichTextBox
  {
    private long ringBufferSize=1024*1024*3; // we init with 3 mb

    public RingBufferedRichTextBox()
    {
      this.TextChanged+=new EventHandler(RingBufferedRichTextBox_TextChanged);
    }

    void  RingBufferedRichTextBox_TextChanged(object sender, EventArgs e)
    {
 	    if (this.TextLength>ringBufferSize)
        this.Text=this.Text.Remove(0,(int)(this.TextLength-ringBufferSize));
    }
    public long RingBufferSizeInMB
    {
      get { return (ringBufferSize / 1024) / 1024; }
      set { ringBufferSize = 1024 * 1024 * value; }
    }
  }
}
