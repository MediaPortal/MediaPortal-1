#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;
using Timer = System.Timers.Timer;

namespace MediaPortal.GUI.Sudoku
{
  [PluginIcons("GUISudoku.Sudoku.gif", "GUISudoku.SudokuDisabled.gif")]
  public class GUISudoku : GUIInternalWindow, ISetupForm, IShowPlugin
  {
    private enum SkinControlIDs
    {
      BTN_NEW_GAME = 2,
      BTN_HELP_ONCE = 3,
      BTN_RESET_GAME = 4,
      BTN_SOLVE = 5,
      BTN_BLOCK_INVALID_MOVES = 6,
      BTN_SHOW_INVALID_MOVES = 7,
      BTN_LEVEL = 8,
      BTN_CLEAR = 9,
    }

    private enum LevelName
    {
      Kids = 0,
      Easy = 1,
      Medium = 2,
      Hard = 3
    }

    public class Highscore
    {
      protected string name;
      protected int score;
      //      protected int rating = 0;
      //      protected int totalSec = 0;

      public string Name
      {
        get { return name; }
        set { name = value; }
      }

      public int Score
      {
        get { return score; }
        set { score = value; }
      }

      public Highscore(string _name, int _score)
      {
        this.name = _name;
        this.score = _score;
      }
    }

    private sealed class ScoreComparer : IComparer<Highscore>
    {
      public int Compare(Highscore item1, Highscore item2)
      {
        return item2.Score - item1.Score;
      }
    }

    #region Serialization

    [Serializable]
    public class Settings
    {
      protected bool m_bShow;
      protected bool m_bBlock;
      protected int m_bLevel;
      protected int _filter;
      protected bool _showCandidates;
      protected List<Highscore> m_highScore = new List<Highscore>();

      public Settings()
      {
        m_bShow = false;
        m_bBlock = false;
        m_bLevel = (int)LevelName.Easy;
        _filter = -1;
        _showCandidates = false;
      }

      [XmlElement("Show")]
      public bool Show
      {
        get { return m_bShow; }
        set { m_bShow = value; }
      }

      [XmlElement("Block")]
      public bool Block
      {
        get { return m_bBlock; }
        set { m_bBlock = value; }
      }

      [XmlElement("Level")]
      public int Level
      {
        get { return m_bLevel; }
        set { m_bLevel = value; }
      }

      [XmlElement("Highscores")]
      public List<Highscore> HighScore
      {
        get { return m_highScore; }

        set { m_highScore = value; }
      }

      [XmlElement("Filter")]
      public int Filter
      {
        get { return _filter; }
        set { _filter = value; }
      }

      [XmlElement("ShowCandidates")]
      public bool ShowCandidates
      {
        get { return _showCandidates; }
        set { _showCandidates = value; }
      }

      public void Load()
      {
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          m_bShow = xmlreader.GetValueAsBool("NumberPlace", "showerrormoves", false);
          m_bBlock = xmlreader.GetValueAsBool("NumberPlace", "blockerrormoves", false);
          m_bLevel = xmlreader.GetValueAsInt("NumberPlace", "level", 1);
          _showCandidates = xmlreader.GetValueAsBool("NumberPlace", "showcandidates", false);
          m_highScore.Clear();
          for (int i = 1; i < 4; i++)
          {
            string name = xmlreader.GetValueAsString("NumberPlace", "name" + i, string.Empty);
            int score = xmlreader.GetValueAsInt("NumberPlace", "score" + i, 0);
            if (!name.Equals(string.Empty))
            {
              m_highScore.Add(new Highscore(name, score));
            }
          }
        }
      }

      public void Save()
      {
        using (Profile.Settings xmlwriter = new Profile.MPSettings())
        {
          xmlwriter.SetValueAsBool("NumberPlace", "showerrormoves", m_bShow);
          xmlwriter.SetValueAsBool("NumberPlace", "blockerrormoves", m_bBlock);
          xmlwriter.SetValue("NumberPlace", "level", m_bLevel);
          xmlwriter.SetValueAsBool("NumberPlace", "showcandidates", _showCandidates);
          for (int i = 1; i <= m_highScore.Count && i < 4; i++)
          {
            xmlwriter.SetValue("NumberPlace", "name" + i, m_highScore[i - 1].Name);
            xmlwriter.SetValue("NumberPlace", "score" + i, m_highScore[i - 1].Score);
          }
        }
      }
    }

    #endregion Serialization

    private Grid grid = new Grid(3);
    private static Random random = new Random(DateTime.Now.Millisecond);

    [SkinControl((int)SkinControlIDs.BTN_NEW_GAME)] protected GUIButtonControl btnNewGame = null;
    [SkinControl((int)SkinControlIDs.BTN_HELP_ONCE)] protected GUIButtonControl btnHelpOnce = null;
    [SkinControl((int)SkinControlIDs.BTN_RESET_GAME)] protected GUIButtonControl btnResetGame = null;
    [SkinControl((int)SkinControlIDs.BTN_SOLVE)] protected GUIButtonControl btnSolve = null;
    [SkinControl((int)SkinControlIDs.BTN_BLOCK_INVALID_MOVES)] protected GUICheckButton btnBlockInvalidMoves = null;
    [SkinControl((int)SkinControlIDs.BTN_SHOW_INVALID_MOVES)] protected GUICheckButton btnShowInvalidMoves = null;
    [SkinControl((int)SkinControlIDs.BTN_LEVEL)] protected GUIButtonControl btnLevel = null;
    [SkinControl((int)SkinControlIDs.BTN_CLEAR)] protected GUIButtonControl btnClear = null;


    private static readonly string pluginConfigFileName = "mynumberplace";

    protected static long m_dwCellIncorrectTextColor = 0xFFFF0000;
    protected static long m_dwTextColor = 0xFFFFFFFF;

    private Timer timer = new Timer();
    private TimeSpan totalTime = new TimeSpan(1000);
    private DateTime startTime;
    private string strSeconds = "00";
    private string strMinutes = "00";
    private string strHours = "";
    private int gameRating;
    private bool gameRunning = false;
    private bool isScoreGame = false;
    private bool _nextNumberIsToggle = false;
    private bool _nextNumberIsFilter = false;

    private Settings _Settings = new Settings();

    public GUISudoku()
    {
      GetID = (int)Window.WINDOW_NUMBERPLACE;
    }


    public override bool Init()
    {
      // pre-register the control class so that the factory knows how to create it
      GUIControlFactory.RegisterControl("cell", typeof (CellControl));

      // Create our skin xml file
      CreateSkinXML(GetWindowId(), GUIGraphicsContext.Theme);

      this.timer.Elapsed += new ElapsedEventHandler(OnTimer_Tick);

      // Load the skin xml file
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\" + pluginConfigFileName + ".xml"));
    }

    /// <summary>
    /// Create the skin xml files
    /// </summary>
    /// <param name="PluginID">the ID of the plugin</param>
    /// <param name="SkinDirectory">the directory where the skin xml files live</param>
    private void CreateSkinXML(int PluginID, string SkinDirectory)
    {
      Assembly asm = Assembly.GetExecutingAssembly();
      string[] resourceNames = asm.GetManifestResourceNames();
      foreach (string resource in resourceNames)
      {
        // If the embedded resource is a skin file
        if (resource.IndexOf("skin") > 0)
        {
          // Extract the extension of the resource
          int lastDot = resource.LastIndexOf(".");
          string extension = resource.Substring(lastDot);

          // Now replace all the '.'s with '\'s
          string name = resource.Remove(lastDot, resource.Length - lastDot);
          name = name.Replace("numberplace_mp.skin", "");
          name = name.Replace(".", "" + Path.DirectorySeparatorChar);

          // Finally put together the appropriate name for the skin file
          string fileName = SkinDirectory + name + extension;

          if (!File.Exists(fileName))
          {
            // Delete the skin file if it already exists - not
            //File.Delete(fileName);

            // Now create the skin file only if it not exists
            Stream writer = File.Create(fileName);

            // Copy the contents of the embedded resource into the skin file
            Stream reader = asm.GetManifestResourceStream(resource);

            int data = reader.ReadByte();
            while (data != -1)
            {
              writer.WriteByte((byte)data);
              data = reader.ReadByte();
            }
            writer.Flush();
            writer.Close();
            reader.Close();
          }
        }
      }
    }

    protected override void OnPageLoad()
    {
      try
      {
        _Settings.Load();
        ShowInvalid();

        if (_Settings.Show)
        {
          GUIControl.SelectControl(GetID, ((int)SkinControlIDs.BTN_SHOW_INVALID_MOVES));
        }
        else
        {
          GUIControl.DeSelectControl(GetID, ((int)SkinControlIDs.BTN_SHOW_INVALID_MOVES));
        }

        if (_Settings.Block)
        {
          GUIControl.SelectControl(GetID, ((int)SkinControlIDs.BTN_BLOCK_INVALID_MOVES));
        }
        else
        {
          GUIControl.DeSelectControl(GetID, ((int)SkinControlIDs.BTN_BLOCK_INVALID_MOVES));
        }

        for (int i = 1; i < 4; i++)
        {
          GUIPropertyManager.SetProperty("#numberplace.name" + i, " ");
          GUIPropertyManager.SetProperty("#numberplace.score" + i, " ");
        }
        if (gameRunning)
        {
          ResumeTimer();
        }

        UpdateButtonStates();
        base.OnPageLoad();
      }
      catch (Exception e1)
      {
        Log.Error("GUINumberPlace: Exception occured - {0}", e1.Message);
      }
    }

    private void ClearGrid()
    {
      grid.Reset();
      StopTimer();
      gameRunning = false;

      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          cntlFoc.editable = true;
          cntlFoc.CellValue = 0;
          cntlFoc.SolutionValue = 0;
          cntlFoc.M_dwDisabledColor = 0xFF000000;
        }
      }
    }

    private void ResetCandidates()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          cntlFoc.ClearCandidates();
          //if (cntlFoc.CellValue == 0)
          {
            for (int i = 1; i <= 9; i++)
            {
              cntlFoc.SetCandidate(i);
            }
            cntlFoc.ShowCandidates = _Settings.ShowCandidates;
          }
        }
      }
      CheckCandidates();
    }

    private void ClearCandidates()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          cntlFoc.ClearCandidates();
        }
      }
      CheckCandidates();
    }

    private void CheckCandidates()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          if (cntlFoc.CellValue == 0)
          {
            IList candidates = grid.Possibilities(row, column);
            for (int i = 1; i <= 9; i++)
              //foreach (int candidate in candidates)
            {
              if (!candidates.Contains(i))
              {
                cntlFoc.RemoveCandidate(i);
              }
            }
            cntlFoc.HighlightCandidate(_Settings.Filter);

            if ((_Settings.Filter == 0) && (candidates.Count == 2))
            {
              cntlFoc.Highlight = true;
            }

            cntlFoc.ShowCandidates = _Settings.ShowCandidates;
          }
        }
      }
    }

    private bool GridIsComplete()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          if (cntlFoc.CellValue == 0)
          {
            return false;
          }
        }
      }
      return true;
    }

    private void ShowInvalid()
    {
      if (_Settings.Show)
      {
        CellControl.M_dwCellIncorrectTextColor = m_dwCellIncorrectTextColor;
      }
      else
      {
        CellControl.M_dwCellIncorrectTextColor = m_dwTextColor;
      }
    }

    private bool SolvedCorrect()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          if ((cntlFoc.CellValue != cntlFoc.SolutionValue) && cntlFoc.editable)
          {
            return false;
          }
        }
      }
      return true;
    }

    private Grid GenerateLevel(Grid puzzle, int minrating, int maxrating)
    {
      int i = 0, rating = 0;

      while (i < 100)
      {
        i++;
        puzzle = Solver.Generate(3);
        rating = Solver.Rate(puzzle);
        if (rating > minrating && rating < maxrating)
        {
          break;
        }
        if (i == 99)
        {
          Log.Debug(
            "GUINumberPlace: None of the generated games where hard enough - aborting with new game at rating: {0}",
            rating);
          break;
        }
      }
      Log.Info("GUINumberPlace: New game generated after {1} loops, rating: {0}", rating, i);
      return puzzle;
    }

    private void UpdateButtonStates()
    {
      string textLine = GUILocalizeStrings.Get(19107); // Level:
      switch ((LevelName)_Settings.Level)
      {
        case LevelName.Kids:
          textLine += GUILocalizeStrings.Get(19115); // kids
          break;
        case LevelName.Easy:
          textLine += GUILocalizeStrings.Get(19108); // easy
          break;
        case LevelName.Medium:
          textLine += GUILocalizeStrings.Get(19109); // medium
          break;
        case LevelName.Hard:
          textLine += GUILocalizeStrings.Get(19110); // difficult
          break;
      }
      GUIControl.SetControlLabel(GetID, btnLevel.GetID, textLine);
    }

    private void ResetGame()
    {
      for (int row = 0; row < grid.CellsInRow; row++)
      {
        for (int column = 0; column < grid.CellsInRow; column++)
        {
          int cellControlId = (1000 * (row + 1)) + column;
          CellControl cntlFoc = (CellControl)GetControl(cellControlId);
          if (cntlFoc.editable)
          {
            cntlFoc.CellValue = 0;
            grid.cells[row, column] = 0;
          }
        }
      }
      ResetCandidates();
    }

    private void Result()
    {
      GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      if (SolvedCorrect())
      {
        StopTimer();
        gameRunning = false;
        int score = (100000000 / gameRating) / (totalTime.Hours * 3600 + totalTime.Minutes * 60 + totalTime.Seconds);

        dlg.SetHeading(GUILocalizeStrings.Get(19111)); // Game Over
        dlg.SetLine(1, GUILocalizeStrings.Get(19112)); // Congratulation!
        dlg.SetLine(2, GUILocalizeStrings.Get(19113)); // You have solved the game correctly.

        if ((_Settings.HighScore.Count < 3 || score > _Settings.HighScore[2].Score) && isScoreGame)
        {
          //dlg.SetLine(3, "New Highscore! Your Name?");
          dlg.DoModal(GUIWindowManager.ActiveWindow);

          string name = GetPlayerName();
          _Settings.HighScore.Add(new Highscore(name, score));
          _Settings.HighScore.Sort(new ScoreComparer());
          _Settings.Save();
        }
        else
        {
          dlg.DoModal(GUIWindowManager.ActiveWindow);
        }

        Log.Info("GUINumberPlace: Solved in: {0} game Rating: {2} Score: {1}",
                 (totalTime.Hours * 3600 + totalTime.Minutes * 60 + totalTime.Seconds), score, gameRating);

        //Utils.PlaySound("notify.wav", false, true);
      }
      else
      {
        dlg.SetHeading(GUILocalizeStrings.Get(19111)); // Game Over
        dlg.SetLine(1, GUILocalizeStrings.Get(19114)); // Sorry, but your solution is wrong.
        dlg.SetLine(2, string.Empty);
        dlg.SetLine(3, string.Empty);
        dlg.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    #region Timer Functions

    public void OnTimer_Tick(object sender, EventArgs e)
    {
      if (gameRunning)
      {
        totalTime = DateTime.Now.Subtract(startTime);
      }

      strSeconds = "";
      strMinutes = "";
      strHours = "";
      if (totalTime.Seconds < 10)
      {
        strSeconds = "0";
      }
      strSeconds += totalTime.Seconds;
      if (totalTime.Minutes < 10)
      {
        strMinutes = "0";
      }
      strMinutes += totalTime.Minutes;
      if (totalTime.Hours > 0)
      {
        strHours = totalTime.Hours.ToString();
      }
    }

    private void ResumeTimer()
    {
      timer.Enabled = true;
      startTime = DateTime.Now.Subtract(totalTime);
    }

    private void StartTimer()
    {
      startTime = DateTime.Now;
      timer.Interval = 500;
      timer.Enabled = true;

      totalTime = new TimeSpan(0);
      strSeconds = "00";
      strMinutes = "00";
      strHours = "";
    }

    private void StopTimer()
    {
      timer.Enabled = false;
      OnTimer_Tick(null, null);
    }

    #endregion

    private string GetPlayerName()
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return null;
      }
      keyboard.Reset();
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        return keyboard.Text;
      }
      return "unknown";
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnSolve)
      {
        // Solve grid
        Grid solution = Solver.Solve(grid);
        if (solution != null)
        {
          for (int row = 0; row < grid.CellsInRow; row++)
          {
            for (int column = 0; column < grid.CellsInRow; column++)
            {
              int cellControlId = (1000 * (row + 1)) + column;
              CellControl cntlFoc = (CellControl)GetControl(cellControlId);
              if (cntlFoc.editable)
              {
                cntlFoc.CellValue = solution.cells[row, column];
                cntlFoc.M_dwDisabledColor = m_dwCellIncorrectTextColor;
                cntlFoc.editable = false;
              }
            }
          }
        }
      }
      else if (control == btnNewGame)
      {
        //new game
        GUIWaitCursor.Show();
        int minrating = 0;
        int maxrating = 0;
        ClearGrid();
        Grid puzzle = new Grid();
        switch ((LevelName)_Settings.Level)
        {
          case LevelName.Kids:
            minrating = 550;
            maxrating = 999;
            break;
          case LevelName.Easy:
            minrating = 450;
            maxrating = 650;
            break;
          case LevelName.Medium:
            minrating = 250;
            maxrating = 550;
            break;
          case LevelName.Hard:
            minrating = 0;
            maxrating = 250;
            break;
        }
        puzzle = GenerateLevel(puzzle, minrating, maxrating);
        gameRating = Solver.Rate(puzzle);
        //puzzle = Solver.Generate(3);
        Grid solution = Solver.Solve(puzzle);
        if ((LevelName)_Settings.Level == LevelName.Easy)
        {
          puzzle = Solver.FillOutCells(puzzle, solution, 10);
          gameRating = Solver.Rate(puzzle);
        }
        else if ((LevelName)_Settings.Level == LevelName.Kids)
        {
          puzzle = Solver.FillOutCells(puzzle, solution, 20);
          gameRating = Solver.Rate(puzzle) * 2;
        }

        for (int row = 0; row < grid.CellsInRow; row++)
        {
          for (int column = 0; column < grid.CellsInRow; column++)
          {
            int cellControlId = (1000 * (row + 1)) + column;
            CellControl cntlFoc = (CellControl)GetControl(cellControlId);
            cntlFoc.CellValue = puzzle.cells[row, column];
            if (cntlFoc.CellValue > 0)
            {
              cntlFoc.editable = false;
            }
            else
            {
              cntlFoc.SolutionValue = solution.cells[row, column];
            }
          }
        }
        grid = puzzle;
        ResetCandidates();
        GUIWaitCursor.Hide();
        StartTimer();
        gameRunning = true;
        if (_Settings.Show || _Settings.Block)
        {
          isScoreGame = false;
        }
        else
        {
          isScoreGame = true;
        }
      }
      else if (control == btnBlockInvalidMoves)
      {
        _Settings.Block = btnBlockInvalidMoves.Selected;
        if (btnBlockInvalidMoves.Selected)
        {
          if (btnShowInvalidMoves.Selected)
          {
            _Settings.Show = btnShowInvalidMoves.Selected = false;
          }
          isScoreGame = false;
        }
        _Settings.Save();
      }
      else if (control == btnClear)
      {
        ClearGrid();
        ResetCandidates();
      }
      else if (control == btnShowInvalidMoves)
      {
        _Settings.Show = btnShowInvalidMoves.Selected;
        if (btnShowInvalidMoves.Selected)
        {
          if (btnBlockInvalidMoves.Selected)
          {
            _Settings.Block = btnBlockInvalidMoves.Selected = false;
          }
          isScoreGame = false;
        }
        ShowInvalid();
        _Settings.Save();
      }
      else if (control == btnLevel)
      {
        switch ((LevelName)_Settings.Level)
        {
          case LevelName.Kids:
            _Settings.Level = (int)LevelName.Easy;
            break;
          case LevelName.Easy:
            _Settings.Level = (int)LevelName.Medium;
            break;
          case LevelName.Medium:
            _Settings.Level = (int)LevelName.Hard;
            break;
          case LevelName.Hard:
            _Settings.Level = (int)LevelName.Kids;
            break;
        }
        UpdateButtonStates();
        _Settings.Save();
      }
      else if (control == btnHelpOnce)
      {
        int candidateIndex = random.Next(81 - grid.CountFilledCells());
        int m = -1, row = 0, column = 0;
        isScoreGame = false;

        for (row = 0; row < 9 && m < candidateIndex; row++)
        {
          for (column = 0; column < 9 && m < candidateIndex; column++)
          {
            int cellControlId = (1000 * (row + 1)) + column;
            CellControl cntlFoc = (CellControl)GetControl(cellControlId);
            if (cntlFoc.editable == true && cntlFoc.CellValue == 0)
            {
              m++;
              if (m == candidateIndex)
              {
                cntlFoc.CellValue = cntlFoc.SolutionValue;
                grid.cells[row, column] = cntlFoc.SolutionValue;
              }
            }
          }
          CheckCandidates();
        }
      }
      else if (control == btnResetGame)
      {
        ResetGame();
      }
      base.OnClicked(controlId, control, actionType);
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SELECT_ITEM || action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
      {
        int controlId = GetFocusControlId();
        if (controlId >= 1000 && controlId <= 9008)
        {
          // Show dialog
          CellControl cntlFoc = (CellControl)GetControl(controlId);
          int row = (controlId / 1000) - 1;
          int column = controlId % 1000;

          if (cntlFoc.editable)
          {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
            if (dlg != null)
            {
              dlg.Reset();
              dlg.SetHeading(GUILocalizeStrings.Get(19116)); // Cell value

              for (int index = 1; index < 10; index++)
              {
                dlg.Add("");
              }
              dlg.Add(GUILocalizeStrings.Get(19117)); // Clear cell
              dlg.SelectedLabel = cntlFoc.CellValue - 1;
              dlg.DoModal(GetWindowId());
              if (dlg.SelectedLabel < 0)
              {
                return;
              }
              else
              {
                if (dlg.SelectedId == 10)
                {
                  cntlFoc.CellValue = 0;
                  grid.cells[row, column] = 0;
                }
                else
                {
                  if (!_Settings.Block || cntlFoc.SolutionValue == dlg.SelectedId)
                  {
                    cntlFoc.CellValue = dlg.SelectedId;
                    grid.cells[row, column] = dlg.SelectedId;

                    if (this.GridIsComplete())
                    {
                      this.Result();
                    }
                  }
                }
              }
            }
            CheckCandidates();
          }
        }
      }
      else if (action.wID == Action.ActionType.ACTION_KEY_PRESSED ||
               (action.wID >= Action.ActionType.REMOTE_0 && action.wID <= Action.ActionType.REMOTE_9))
      {
        int controlId = GetFocusControlId();
        if (controlId >= 1000 && controlId <= 9008)
        {
          CellControl cntlFoc = (CellControl)GetControl(controlId);
          int row = (controlId / 1000) - 1;
          int column = controlId % 1000;

          if (cntlFoc != null)
          {
            if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
            {
              if (action.m_key.KeyChar == 8)
              {
                cntlFoc.CellValue = 0;
                grid.cells[row, column] = 0;
              }
              else if (action.m_key.KeyChar == 35) // #
              {
                _nextNumberIsToggle = true;
              }
              else if (action.m_key.KeyChar == 42) // *
              {
                _nextNumberIsFilter = true;
              }
            }
            else if (action.wID >= Action.ActionType.REMOTE_0 && action.wID <= Action.ActionType.REMOTE_9)
            {
              int value = (action.wID - Action.ActionType.REMOTE_0);

              if (!_nextNumberIsToggle && !_nextNumberIsFilter)
              {
                if (value == 0 || !_Settings.Block || cntlFoc.SolutionValue == value)
                {
                  cntlFoc.CellValue = value;
                  grid.cells[row, column] = value;

                  if (this.GridIsComplete())
                  {
                    this.Result();
                  }
                }
              }
              else if (_nextNumberIsToggle)
              {
                if (value > 0)
                {
                  if (cntlFoc.IsCandidate(value))
                  {
                    cntlFoc.RemoveCandidate(value);
                  }
                  else
                  {
                    cntlFoc.SetCandidate(value);
                  }
                }
                _nextNumberIsToggle = false;
              }
              else if (_nextNumberIsFilter)
              {
                _Settings.Filter = value;
                _Settings.ShowCandidates = true;
                _nextNumberIsFilter = false;
              }
            }
            CheckCandidates();
          }
        }
      }
      else if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        StopTimer();
      }
      else if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        ShowContextMenu();
      }
      else if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        StopTimer();
      }
      base.OnAction(action);
    }

    private void ShowFilterMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(19120); // Filter Candidates

      dlg.Add(GUILocalizeStrings.Get(19122)); // Filter 1
      dlg.Add(GUILocalizeStrings.Get(19123)); // Filter 2
      dlg.Add(GUILocalizeStrings.Get(19124)); // Filter 3
      dlg.Add(GUILocalizeStrings.Get(19125)); // Filter 4
      dlg.Add(GUILocalizeStrings.Get(19126)); // Filter 5
      dlg.Add(GUILocalizeStrings.Get(19127)); // Filter 6
      dlg.Add(GUILocalizeStrings.Get(19128)); // Filter 7
      dlg.Add(GUILocalizeStrings.Get(19129)); // Filter 8
      dlg.Add(GUILocalizeStrings.Get(19130)); // Filter 9
      dlg.Add(GUILocalizeStrings.Get(19131)); // Filter pairs
      dlg.Add(GUILocalizeStrings.Get(19132)); // Filter off

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 10:
          _Settings.Filter = 0;
          break;
        case 11:
          _Settings.Filter = -1;
          _Settings.ShowCandidates = true;
          break;
        default:
          _Settings.Filter = dlg.SelectedId;
          _Settings.ShowCandidates = true;
          break;
      }
      CheckCandidates();
    }

    private void ShowToggleCandidatesMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(19133); // Add/Remove Candidate

      dlg.Add(GUILocalizeStrings.Get(19134)); // Toggle 1
      dlg.Add(GUILocalizeStrings.Get(19135)); // Toggle 2
      dlg.Add(GUILocalizeStrings.Get(19136)); // Toggle 3
      dlg.Add(GUILocalizeStrings.Get(19137)); // Toggle 4
      dlg.Add(GUILocalizeStrings.Get(19138)); // Toggle 5
      dlg.Add(GUILocalizeStrings.Get(19139)); // Toggle 6
      dlg.Add(GUILocalizeStrings.Get(19140)); // Toggle 7
      dlg.Add(GUILocalizeStrings.Get(19141)); // Toggle 8
      dlg.Add(GUILocalizeStrings.Get(19142)); // Toggle 9

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      int controlId = GetFocusControlId();
      if (controlId >= 1000 && controlId <= 9008)
      {
        CellControl cntlFoc = (CellControl)GetControl(controlId);

        if (cntlFoc != null)
        {
          if (cntlFoc.IsCandidate(dlg.SelectedId))
          {
            cntlFoc.RemoveCandidate(dlg.SelectedId);
          }
          else
          {
            cntlFoc.SetCandidate(dlg.SelectedId);
          }
        }
      }

      CheckCandidates();
    }

    private void ShowCandidatesMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(19143); // Candidates

      dlg.Add(GUILocalizeStrings.Get(19144)); // Show candidates
      dlg.Add(GUILocalizeStrings.Get(19145)); // Hide candidates
      dlg.Add(GUILocalizeStrings.Get(19146)); // Clear candidates
      dlg.Add(GUILocalizeStrings.Get(19147)); // Reset candidates

      int controlId = GetFocusControlId();
      if (controlId >= 1000 && controlId <= 9008)
      {
        CellControl cntlFoc = (CellControl)GetControl(controlId);
        if (cntlFoc != null)
        {
          dlg.Add(GUILocalizeStrings.Get(19151)); // Add/remove candidates
        }
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 1:
          _Settings.ShowCandidates = true;
          _Settings.Save();
          break;
        case 2:
          _Settings.ShowCandidates = false;
          _Settings.Save();
          break;
        case 3:
          ClearCandidates();
          break;
        case 4:
          ResetCandidates();
          break;
        case 5:
          ShowToggleCandidatesMenu();
          break;
      }
      CheckCandidates();
    }

    private void ShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(19148); // Assistance

      dlg.Add(GUILocalizeStrings.Get(19149)); // Filters
      dlg.Add(GUILocalizeStrings.Get(19150)); // Candidates

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 1:
          ShowFilterMenu();
          break;
        case 2:
          ShowCandidatesMenu();
          break;
      }
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          return;
        }
      }

      if (!String.IsNullOrEmpty(strHours))
      {
        GUIPropertyManager.SetProperty("#numberplace.time", strHours + ":" + strMinutes + ":" + strSeconds);
      }
      else
      {
        GUIPropertyManager.SetProperty("#numberplace.time", strMinutes + ":" + strSeconds);
      }
      //GUIPropertyManager.SetProperty("#selecteditem", strMinutes + ":" + strSeconds);

      for (int i = 0; i < _Settings.HighScore.Count; i++)
      {
        GUIPropertyManager.SetProperty("#numberplace.name" + (i + 1), _Settings.HighScore[i].Name);
        GUIPropertyManager.SetProperty("#numberplace.score" + (i + 1), (_Settings.HighScore[i].Score).ToString());
      }
    }

    #region ISetupForm Members

    public int GetWindowId()
    {
      return GetID;
    }

    public void ShowPlugin()
    {
      MessageBox.Show("Nothing to setup.");
    }

    public string PluginName()
    {
      return "Sudoku";
    }

    public string Description()
    {
      return "Play Sudoku during commercials!";
    }

    public string Author()
    {
      return "Cosmo, IMOON, rtv, mPod";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(19101); // Sudoku
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = "hover_" + pluginConfigFileName + ".png";
      return true;
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
  }
}