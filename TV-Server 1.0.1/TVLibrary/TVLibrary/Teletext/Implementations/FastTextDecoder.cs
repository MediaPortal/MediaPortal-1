/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
namespace TvLibrary.Teletext
{
  /// <summary>
  /// Fast text decoder
  /// </summary>
  public class FastTextDecoder
  {
    #region variables
    int _redPage = -1;
    int _greenPage = -1;
    int _yellowPage = -1;
    int _bluePage = -1;
    int _whitePage = -1;
    #endregion

    #region public members
    ///<summary>
    /// Decodes the fast text data
    ///</summary>
    ///<param name="pageData">Teletext page data</param>
    public void Decode(byte[] pageData)
    {
      _redPage = -1;
      _greenPage = -1;
      _yellowPage = -1;
      _bluePage = -1;
      _whitePage = -1;

      int maxRows = pageData.Length / 42;
      if (maxRows < 1)
        return;

      int pageNumber = 0;
      for (int rowNr = 0; rowNr < maxRows; rowNr++)
      {
        int packetNumber = Hamming.GetPacketNumber(rowNr * 42, ref pageData);
        if (packetNumber == 0)
          pageNumber = Hamming.GetPageNumber(rowNr * 42, ref pageData);
        if (packetNumber == 27)
        {
          DecodePacket27(pageNumber, rowNr * 42, pageData);
          return;
        }
      }
    }
    #endregion

    #region private members
    void DecodePacket27(int pageNumber, int offset, byte[] pageData)
    {
      offset += 3;
      // Links 0 through 5
      for (long index = 0; index < 6; index++)
      {
        // Retrieve page address
        byte[] linkData = new byte[6];

        for (int i = 0; i < 6; i++)
        {
          linkData[i] = Hamming.Decode[pageData[offset]];
          offset++;
        }

        byte pageUnits = linkData[0];
        byte pageTens = linkData[1];

        byte m1 = (byte)(linkData[3] >> 3);
        byte m2 = (byte)((linkData[5] & 0x4) >> 2);
        byte m3 = (byte)(linkData[5] >> 3);
        byte m = (byte)((m3 << 2) + (m2 << 1) + m1);

        // Magazine is complemented
        int Magazine = pageNumber / 0x100;
        if (Magazine == 8)
          Magazine = 0;

        byte linkMagazine = (byte)(m ^ (Magazine % 8));

        // Magazine encoded as 0 is magazine 8
        if (linkMagazine == 0)
        {
          linkMagazine = 8;
        }
        int pageNr = linkMagazine * 0x100 + pageTens * 0x10 + pageUnits;
        if (!IsDecimalPage(pageNr))
        {
          pageNr = -1;
        }
        switch (index)
        {
          case 0:
            _redPage = pageNr;
            break;
          case 1:
            _greenPage = pageNr;
            break;
          case 2:
            _yellowPage = pageNr;
            break;
          case 3:
            _bluePage = pageNr;
            break;
          case 5:
            _whitePage = pageNr;
            break;
        }
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the pagenumber for the red button.
    /// </summary>
    /// <value>The red page number</value>
    public int Red
    {
      get { return _redPage; }
    }

    /// <summary>
    /// Gets the pagenumber for the green button.
    /// </summary>
    /// <value>The green page number</value>
    public int Green
    {
      get { return _greenPage; }
    }

    /// <summary>
    /// Gets the pagenumber for the yellow button.
    /// </summary>
    /// <value>The yellow page number</value>
    public int Yellow
    {
      get { return _yellowPage; }
    }

    /// <summary>
    /// Gets the pagenumber for the blue button.
    /// </summary>
    /// <value>The blue page number</value>
    public int Blue
    {
      get { return _bluePage; }
    }

    /// <summary>
    /// Gets the pagenumber for the white button.
    /// </summary>
    /// <value>The white page number.</value>
    public int White
    {
      get { return _whitePage; }
    }
    /// <summary>
    /// Determines whether the page is a decimal page or hexadecimal page
    /// </summary>
    /// <param name="i">The pagenumber</param>
    /// <returns>
    /// 	<c>true</c> if the page is a decimal page; otherwise, <c>false</c>.
    /// </returns>
    static bool IsDecimalPage(int i)
    {
      return ((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90);
    }
    #endregion
  }
}
