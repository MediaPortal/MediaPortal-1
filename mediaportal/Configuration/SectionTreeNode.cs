using System;
using System.Windows.Forms;

namespace MediaPortal.Configuration
{
	public class SectionTreeNode : TreeNode
	{
		public SectionSettings Section
		{
			get { return section; }
		}
		SectionSettings section;

		public SectionTreeNode(SectionSettings section)
		{
			this.section = section;
			this.Text = section.Text;
		}

		public override string ToString()
		{
			return section.ToString();
		}
	}

}
