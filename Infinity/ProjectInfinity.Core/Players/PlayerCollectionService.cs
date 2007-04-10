using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Players
{
  public class PlayerCollectionService :  IPlayerCollectionService
  {
    #region variables
    List<IPlayer> _players = new List<IPlayer>();
    #endregion


    #region IPlayerCollection Members

    /// <summary>
    /// Adds the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void Add(IPlayer player)
    {
      _players.Add(player);
    }

    /// <summary>
    /// Removes the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void Remove(IPlayer player)
    {
      _players.Remove(player);
    }

    /// <summary>
    /// Removes at the player at the specified index
    /// </summary>
    /// <param name="index">The index.</param>
    public void RemoveAt(int index)
    {
      _players.RemoveAt(index);
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
      foreach (IPlayer player in _players)
      {
        player.Close();
      }
      _players.Clear();
    }

    /// <summary>
    /// Gets the <see cref="PlayerService.IPlayer"/> at the specified index.
    /// </summary>
    /// <value></value>
    public IPlayer this[int index]
    {
      get
      {
        return _players[index];
      }
    }

    /// <summary>
    /// Gets the player count.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get
      {
        return _players.Count;
      }
    }

    #endregion
  }
}
