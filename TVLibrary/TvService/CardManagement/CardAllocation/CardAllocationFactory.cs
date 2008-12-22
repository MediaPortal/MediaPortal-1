using System;
using System.Collections.Generic;
using System.Text;

namespace TvService
{
  public class CardAllocationFactory
  {
    static public ICardAllocation Create(bool simple)
    {
      if (simple)
      {
        return new SimpleCardAllocation();
      }
      return new AdvancedCardAllocation();
    }
  }
}
