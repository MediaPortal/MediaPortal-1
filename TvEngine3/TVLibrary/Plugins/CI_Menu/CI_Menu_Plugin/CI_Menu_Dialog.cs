using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SetupTv;
using SetupTv.Sections;
using TvControl;
using TvDatabase;
using TvEngine;
using TvEngine.Events;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations;
using TvLibrary.Implementations.DVB;


namespace TvEngine
{
    [CLSCompliant(false)]
    public partial class CI_Menu_Dialog : SetupTv.SectionSettings, ICiMenuCallbacks
    {
      readonly TvCardCollection _tvcards = new TvCardCollection(null);

      CiMenuState ciMenuState   = CiMenuState.Closed;
      int         ciMenuChoices = 0;
      
        public CI_Menu_Dialog()
        {
          InitializeComponent();
        }

        private void CI_Menu_Dialog_Load(object sender, EventArgs e)
        {
          GetTvCards();
          SetButtonState();
        }

        private void InitMenu()
        {
          Title.Text = Subtitle.Text = BottomText.Text = CiRequest.Text = "";
          Choices.Items.Clear();
        }

        private void GetTvCards()
        {
          cbxTvCards.Items.Clear();
          foreach (ITVCard card in _tvcards.Cards)
          {
            if (card.CardType == CardType.DvbC || card.CardType == CardType.DvbS || card.CardType == CardType.DvbT)
            {
              cbxTvCards.Items.Add(card);
            }
          }
          if (_tvcards.Cards.Count > 0)
          {
            cbxTvCards.SelectedIndex = 0;
          }
        }

        public TvCardDvbBase ActiveCard
        {
          get
          {
            if (cbxTvCards.SelectedItem == null) return null;

            return cbxTvCards.SelectedItem as TvCardDvbBase;
          }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
          try
          {
            if (ActiveCard != null)
            {
              // Check if card is initialized, CA is only available after set tunerfilter
              if (ActiveCard.ConditionalAccess == null)
              {
                ActiveCard.BuildGraph();
                ActiveCard.StopGraph();
              }

              if (ActiveCard.ConditionalAccess.CiMenu != null && ActiveCard.ConditionalAccess.IsCamReady() == true)
              {
                ActiveCard.ConditionalAccess.CiMenuHandler = this;
                ActiveCard.ConditionalAccess.CiMenu.EnterCIMenu();
              }
              else
              {
                MessageBox.Show("The selected card doesn't support CI menu or CAM not ready");
              }
            }
          }
          catch (Exception ex)
          {
            Log.Write(ex);
          }
        }

        private void btnCloseMenu_Click(object sender, EventArgs e)
        {
          try
          {
            if (ActiveCard != null)
            {
              if (ciMenuState == CiMenuState.NoChoices)
              {
                ActiveCard.ConditionalAccess.CiMenu.SelectMenu(0);
                ciMenuState = CiMenuState.Closed;
              }
              if (ciMenuState == CiMenuState.Ready)
              {
                ActiveCard.ConditionalAccess.CiMenu.CloseCIMenu();
                ciMenuState = CiMenuState.Closed;
              }
              if (ciMenuState == CiMenuState.Request)
              {
                ActiveCard.ConditionalAccess.CiMenu.SendMenuAnswer(true, null);
                ciMenuState = CiMenuState.Ready;
              }
              SetButtonState();
            }
          }         
          catch (Exception ex)
          {
            Log.Write(ex);
          }
        }

        private void btnSendAnswer_Click(object sender, EventArgs e)
        {
          try
          {
            if (ActiveCard != null)
            {
              if (ciMenuState == CiMenuState.Ready && Choices.SelectedIndex != -1) 
              {
                ActiveCard.ConditionalAccess.CiMenu.SelectMenu(Convert.ToByte(Choices.SelectedIndex + 1));
              }
              if (ciMenuState == CiMenuState.Request)
              {
                ActiveCard.ConditionalAccess.CiMenu.SendMenuAnswer(false, CiAnswer.Text);
                ciMenuState = CiMenuState.Ready;
              }
              SetButtonState();
            }
          }
          catch (Exception ex)
          {
            Log.Write(ex);
          }
        }

        private void SetButtonState()
        {
          cbxTvCards.Enabled    =  ciMenuState == CiMenuState.Closed; // disallow changes, when CI Menu open
          btnOk.Enabled         =  ciMenuState == CiMenuState.Closed;
          btnCloseMenu.Enabled  = (ciMenuState == CiMenuState.Ready || ciMenuState == CiMenuState.Request || ciMenuState == CiMenuState.NoChoices);
          btnSendAnswer.Enabled = (ciMenuState == CiMenuState.Ready || ciMenuState == CiMenuState.Request);
          grpCIMenu.Enabled     =  ciMenuState != CiMenuState.Closed;
          CiRequest.Visible     =  ciMenuState == CiMenuState.Request;
          CiAnswer.Visible      =  ciMenuState == CiMenuState.Request;
          if (ciMenuState == CiMenuState.Closed) InitMenu();
        }

        #region ICiMenu Member
        public int OnCiMenu(string lpszTitle, string lpszSubTitle, string lpszBottom, int nNumChoices)
        {
          try
          {
            ciMenuState = CiMenuState.Opened;
            Title.Text        = lpszTitle;
            Subtitle.Text     = lpszSubTitle;
            BottomText.Text   = lpszBottom;
            ciMenuChoices     = nNumChoices;
            
            Choices.Items.Clear();

            // no choices then we are ready yet
            if (nNumChoices == 0)
            {
              ciMenuState = CiMenuState.NoChoices;
              SetButtonState();
            }
          }
          catch (Exception e)
          {
            Log.Write(e);
          }
          return 0;
        }

        public int OnCiMenuChoice(int nChoice, string lpszText)
        {
          try
          {
            Choices.Items.Add(new CiMenuEntry(nChoice+1, lpszText) );
            
            // all Choices shown? then menu is ready
            if (nChoice + 1 == ciMenuChoices)
            {
              ciMenuState = CiMenuState.Ready;
              SetButtonState();
            }
          }
          catch (Exception e)
          {
            Log.Write(e);
          }

          return 0;
        }

        public int OnCiCloseDisplay(int nDelay)
        {
          try
          {
            ciMenuState = CiMenuState.Closed;
            SetButtonState();
          }
          catch (Exception e)
          {
            Log.Write(e);
          }
          return 0;
        }

        public int OnCiRequest(bool bBlind, uint nAnswerLength, string lpszText)
        {
          try
          {
            ciMenuState = CiMenuState.Request;
            SetButtonState();
            CiRequest.Text = String.Format("{0} ({1} Zeichen)", lpszText, nAnswerLength);
            CiAnswer.MaxLength = (int)nAnswerLength;
            CiAnswer.Text = "";
            CiAnswer.Focus();
          }
          catch (Exception e)
          {
            Log.Write(e);
          }
          return 0;
        }

        #endregion
    }
}
