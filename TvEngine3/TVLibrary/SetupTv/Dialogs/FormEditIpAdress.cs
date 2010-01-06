using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TvControl;

namespace SetupTv.Sections
{
  public partial class FormEditIpAdress : Form
  {
    private string _hostName = "";
    private int _portNo = 554;

    public FormEditIpAdress()
    {
      InitializeComponent();
    }

    private void FormEditIpAdress_Load(object sender, EventArgs e)
    {
      List<string> ipAdresses = RemoteControl.Instance.ServerIpAdresses;
      mpComboBox1.Items.Clear();
      int selected = 0;
      int counter = 0;
      foreach (string ipAdress in ipAdresses)
      {
        mpComboBox1.Items.Add(ipAdress);
        if (String.Compare(ipAdress, HostName, true) == 0)
        {
          selected = counter;
        }
        counter++;
      }
      mpComboBox1.SelectedIndex = selected;
      PortNoNumericTextBox.Value = PortNo;
    }

    private void button2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (mpComboBox1.SelectedIndex >= 0)
      {
        HostName = mpComboBox1.SelectedItem.ToString();
        PortNo = PortNoNumericTextBox.Value;
      }
      DialogResult = DialogResult.OK;
      Close();
    }

    public string HostName
    {
      get { return _hostName; }
      set { _hostName = value; }
    }

    public int PortNo
    {
      get { return _portNo; }
      set { _portNo = value; }
    }
  }
}