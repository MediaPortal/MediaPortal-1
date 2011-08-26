using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests.Mocks
{
  public class TVControllerMocks
  {
    public static void CardPresent(int cardId, TVController controller)
    {
      Isolate.WhenCalled(() => controller.CardPresent(cardId)).WillReturn(true);
    }

    public static void CardNotPresent(int cardId, TVController controller)
    {
      Isolate.WhenCalled(() => controller.CardPresent(cardId)).WillReturn(false);
    }
  }
}
