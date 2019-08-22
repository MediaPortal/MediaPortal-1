using System;
using System.Windows.Forms;

namespace TsPacketChecker
{
  public partial class OpenStreamDialog : Form
  {
    private string _address;
    private int _port;
    private TransmissionMode _transmissionMode;

    public string Address { get { return _address; } set { _address = value; } }
    public int Port { get { return _port; } set { _port = value; } }
    public TransmissionMode TransmissionMode { get { return _transmissionMode; } set { _transmissionMode = value; } }

    public OpenStreamDialog()
    {
      InitializeComponent();
    }

    private void OpenStreamDialog_Load(object sender, EventArgs e)
    {
      comboBox1.Items.Add(TransmissionMode.Unicast);
      comboBox1.Items.Add(TransmissionMode.Multicast);
      comboBox1.SelectedIndex = 0;
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
      if ((!string.IsNullOrEmpty(AddressTextBox.Text)) || (!string.IsNullOrEmpty(PortTextBox.Text)))
      {
        _address = AddressTextBox.Text;
        _port = Int32.Parse(PortTextBox.Text);
        _transmissionMode = (TransmissionMode)comboBox1.SelectedItem;
        DialogResult = DialogResult.OK;
      }
      else
      {
        MessageBox.Show("Address and Port Number are needed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void CaneltButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }
  }
}
