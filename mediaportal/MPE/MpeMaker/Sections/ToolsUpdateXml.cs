using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeMaker.Sections
{
    public partial class ToolsUpdateXml : UserControl, ISectionControl
    {
        public PackageClass Package { get; set; }

        public ToolsUpdateXml()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        /// <summary>
        /// Handles the Click event of the btn_gen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btn_gen_Click(object sender, EventArgs e)
        {
            string xmlFile = textBox1.Text;
            ExtensionCollection list = new ExtensionCollection();
            if (File.Exists(xmlFile))
                list = ExtensionCollection.Load(xmlFile);
            list.Add(Package);
            list.Save(xmlFile);
        }

        public void Set(PackageClass pak)
        {
            Package = pak;
        }

        public PackageClass Get()
        {
            throw new NotImplementedException();
        }
    }
}
