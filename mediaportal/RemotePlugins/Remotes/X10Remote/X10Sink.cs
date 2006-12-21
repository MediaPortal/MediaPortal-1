using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;

namespace MediaPortal.InputDevices
{
  class X10Sink:X10._DIX10InterfaceEvents
  {
    
    bool _logverbose = false;
    InputHandler _inputHandler = null;

   
    public X10Sink(InputHandler Handler, bool logverbose)
    {
      _inputHandler = Handler;
    }

    #region _DIX10InterfaceEvents Members

    public void X10Command(string bszCommand, X10.EX10Command eCommand, int lAddress, X10.EX10Key EKeyState, int lSequence, X10.EX10Comm eCommandType, object varTimestamp)
    {
     
      if (EKeyState == X10.EX10Key.X10KEY_ON || EKeyState == X10.EX10Key.X10KEY_REPEAT)
      {
        _inputHandler.MapAction((int)Enum.Parse(typeof(X10.EX10Command), eCommand.ToString()));
        if (_logverbose)
        {
          Log.Info("X10Remote: Command Start --------------------------------------------");
          Log.Info("X10Remote: bszCommand   = {0}", bszCommand.ToString());
          Log.Info("X10Remote: eCommand     = {0} - {1}", (int)Enum.Parse(typeof(X10.EX10Command), eCommand.ToString()), eCommand.ToString());
          Log.Info("X10Remote: eCommandType = {0}", eCommandType.ToString());
          Log.Info("X10Remote: eKeyState    = {0}", EKeyState.ToString());
          Log.Info("X10Remote: lAddress     = {0}", lAddress.ToString());
          Log.Info("X10Remote: lSequence    = {0}", lSequence.ToString());
          Log.Info("X10Remote: varTimestamp = {0}", varTimestamp.ToString());
          Log.Info("X10Remote: Command End ----------------------------------------------");
        }

      }
      
    }

    public void X10HelpEvent(int hwndDialog, int lHelpID)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
}
