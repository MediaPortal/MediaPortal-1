using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Players
{
  public interface IPlayerCollectionService 
  {
    /// <summary>
    /// Adds the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    void Add(IPlayer player);
    /// <summary>
    /// Removes the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    void Remove(IPlayer player);
    /// <summary>
    /// Removes at the player at the specified index
    /// </summary>
    /// <param name="index">The index.</param>
    void RemoveAt(int index);
    /// <summary>
    /// Clears the collection.
    /// </summary>
    void Clear();
    /// <summary>
    /// Gets the <see cref="PlayerService.IPlayer"/> at the specified index.
    /// </summary>
    /// <value></value>
    IPlayer this[int index] { get;}
    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    int Count { get;}


  }
}
