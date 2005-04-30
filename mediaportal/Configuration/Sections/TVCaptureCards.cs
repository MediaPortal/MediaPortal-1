using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.Configuration.Sections
{
	public class TVCaptureCards : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPListView cardsListView;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.ColumnHeader columnHeader4;

		//
		// Privare members
		//
		ArrayList captureCards = new ArrayList();

		public TVCaptureCards() : this("Capture Cards")
		{
		}

		public TVCaptureCards(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Load capture cards
			//
			LoadCaptureCards();

			// 
			// Populate the list view
			//
			PopulateListView();
		}

		private void PopulateListView()
		{
			cardsListView.Items.Clear();
			foreach(TVCaptureDevice captureCard in captureCards)
			{
				AddCaptureCard(captureCard);
			}
		}

		private void AddCaptureCard(TVCaptureDevice card)
		{
			//cardsListView.Items.Clear();
			ListViewItem listItem = new ListViewItem(new string[] { card.VideoDevice, 
                                  card.FriendlyName,                                                                
																	card.UseForTV.ToString(),
																	card.UseForRecording.ToString()
																  } );

			listItem.Tag = card;

			cardsListView.Items.Add(listItem);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.deleteButton = new System.Windows.Forms.Button();
      this.editButton = new System.Windows.Forms.Button();
      this.addButton = new System.Windows.Forms.Button();
      this.cardsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.cardsListView);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(384, 352);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.deleteButton.Enabled = false;
      this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.deleteButton.Location = new System.Drawing.Point(176, 312);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.editButton.Enabled = false;
      this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.editButton.Location = new System.Drawing.Point(96, 312);
      this.editButton.Name = "editButton";
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.addButton.Location = new System.Drawing.Point(16, 312);
      this.addButton.Name = "addButton";
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // cardsListView
      // 
      this.cardsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cardsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                    this.columnHeader1,
                                                                                    this.columnHeader4,
                                                                                    this.columnHeader2,
                                                                                    this.columnHeader3});
      this.cardsListView.FullRowSelect = true;
      this.cardsListView.Location = new System.Drawing.Point(16, 24);
      this.cardsListView.Name = "cardsListView";
      this.cardsListView.Size = new System.Drawing.Size(352, 280);
      this.cardsListView.TabIndex = 0;
      this.cardsListView.View = System.Windows.Forms.View.Details;
      this.cardsListView.DoubleClick += new System.EventHandler(this.cardsListView_DoubleClick);
      this.cardsListView.SelectedIndexChanged += new System.EventHandler(this.cardsListView_SelectedIndexChanged);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Capture card";
      this.columnHeader1.Width = 144;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Television";
      this.columnHeader2.Width = 63;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Recording";
      this.columnHeader3.Width = 64;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Friendly name";
      this.columnHeader4.Width = 77;
      // 
      // TVCaptureCards
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVCaptureCards";
      this.Size = new System.Drawing.Size(400, 368);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void addButton_Click(object sender, System.EventArgs e)
		{
			//find unique ID for the new card
			int cardid=1;
			bool found=false;
			do
			{
				found=false;
				foreach (ListViewItem item in cardsListView.Items)
				{
					TVCaptureDevice dev = item.Tag as TVCaptureDevice;
					if (dev.ID==cardid)
					{
						found=true;
						cardid++;
						break;
					}
				}
			} while (found);

			EditCaptureCardForm editCard = new EditCaptureCardForm(cardid, true,null);

			DialogResult dialogResult = editCard.ShowDialog(this);
			if(dialogResult == DialogResult.OK)
			{
				AddCaptureCard(editCard.CaptureCard);

				editCard.CaptureCard.ID = cardid;
				captureCards.Add(editCard.CaptureCard);
			}
			SaveSettings();
		}

		private void editButton_Click(object sender, System.EventArgs e)
		{
			foreach(ListViewItem listItem in cardsListView.SelectedItems)
			{
				TVCaptureDevice device=listItem.Tag as TVCaptureDevice;
				EditCaptureCardForm editCard = new EditCaptureCardForm(device.ID,false,device);
        editCard.CaptureCard = device;

        DialogResult dialogResult = editCard.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
          TVCaptureDevice captureCard = editCard.CaptureCard;

					listItem.Tag = captureCard;

					listItem.SubItems[0].Text = captureCard.VideoDevice;
          listItem.SubItems[1].Text = captureCard.FriendlyName;
					listItem.SubItems[2].Text = captureCard.UseForTV.ToString();
					listItem.SubItems[3].Text = captureCard.UseForRecording.ToString();
        }
			}		
			SaveSettings();
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			int itemCount = cardsListView.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				//
				// Fetch device
				//
				TVCaptureDevice card = cardsListView.Items[cardsListView.SelectedIndices[0]].Tag as TVCaptureDevice;

				TVDatabase.DeleteCard(card.ID);
				//
				// Remove it from the internal list
				//
				captureCards.Remove(card);

				//
				// Remove from the list view
				//
				cardsListView.Items.RemoveAt(cardsListView.SelectedIndices[0]);
			}
			SaveSettings();

			LoadCaptureCards();
			PopulateListView();
		}

		public void LoadCaptureCards()
		{
			if(File.Exists("capturecards.xml"))
			{
				using(FileStream fileStream = new FileStream("capturecards.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
          try
          {
            //
            // Create Soap Formatter
            //
            SoapFormatter formatter = new SoapFormatter();

            //
            // Serialize
            //
						captureCards = new ArrayList();
            captureCards = (ArrayList)formatter.Deserialize(fileStream);
						for (int i=0; i < captureCards.Count; i++)
						{
							((TVCaptureDevice)captureCards[i]).ID=(i+1);
							((TVCaptureDevice)captureCards[i]).LoadDefinitions();
						}
            //
            // Finally close our file stream
            //
            fileStream.Close();
          }
          catch
          {
            MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
				}
			}
		}

		public override void SaveSettings()
		{

			using(FileStream fileStream = new FileStream("capturecards.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				//
				// Create Soap Formatter
				//
				SoapFormatter formatter = new SoapFormatter();

        //
        // Fetch list of capture cards
        //
        ArrayList availableCards = new ArrayList();

        foreach(ListViewItem listItem in cardsListView.Items)
        {
          availableCards.Add(listItem.Tag);
        }

				//
				// Serialize
				//
				formatter.Serialize(fileStream, availableCards);

				//
				// Finally close our file stream
				//
				fileStream.Close();
			}
		}

		private void cardsListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);		
		}

    private void cardsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      editButton.Enabled = deleteButton.Enabled = (cardsListView.SelectedItems.Count > 0);
    }
	}
}

