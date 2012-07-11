using System;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface ITeletextManagement
  {
    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="user">USer</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    bool IsGrabbingTeletext(IUser user);

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    bool HasTeletext(string userName);

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    TimeSpan TeletextRotation(string userName, int pageNumber);

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="onOff">turn on/off teletext grabbing</param>
    void GrabTeletext(string userName, bool onOff);

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    byte[] GetTeletextPage(string userName, int pageNumber, int subPageNumber);

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    int SubPageCount(string userName, int pageNumber);

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the red button</returns>
    int GetTeletextRedPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the green button</returns>
    int GetTeletextGreenPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    int GetTeletextYellowPageNumber(string userName);

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    int GetTeletextBluePageNumber(string userName);
  }
}