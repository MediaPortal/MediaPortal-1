using System;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// Utility class for <see cref="Byte"/> arrays
  /// </summary>
  public static class ByteArray
  {
    /// <summary>
    /// Compares 2 byte arrays and returns whether they are equal.
    /// </summary>
    /// <param name="bytes1">The byte array to compare</param>
    /// <param name="bytes2">The byte array to compare to</param>
    /// <returns>A <b>bool</b> indicating whether they are equal</returns>
    public static bool AreEqual(byte[] bytes1, byte[] bytes2)
    {
      // If both are null, they're equal
      if (bytes1 == null && bytes2 == null)
      {
        return true;
      }
      // If either but not both are null, they're not equal
      if (bytes1 == null || bytes2 == null)
      {
        return false;
      }
      // If both instances point to the same object, they are equal
      if (bytes1.Equals(bytes2))
      {
        return true;
      }
      //If they lengths differ they are not equal
      if (bytes1.Length != bytes2.Length)
      {
        return false;
      }
      //Byte per byte comparison
      for (int i = 0; i < bytes1.Length; i++)
      {
        if (bytes1[i] != bytes2[i])
        {
          return false;
        }
      }
      return true;
    }

  }
}
