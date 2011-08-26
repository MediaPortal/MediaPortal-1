using System;
namespace PersonalTVGuide
{
  public interface IWishItem
  {
    string Name { get; set; }
    bool AutoRecord { get; set; }
    int Rating { get; set; }
    bool SearchInDescription { get; set; }
    bool SearchInGenre { get; set; }
    bool SearchInTitle { get; set; }
  }
}
