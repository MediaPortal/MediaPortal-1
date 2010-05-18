using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVServiceTests.Mocks
{
  public class CardManager
  {
    private static int _currentCardId = 0;

    public static int GetNextAvailCardId ()
    {
      _currentCardId++;
      return _currentCardId;
    }
  }
}
