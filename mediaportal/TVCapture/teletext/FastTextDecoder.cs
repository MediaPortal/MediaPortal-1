#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

namespace MediaPortal.TV.Teletext
{
  public class FastTextDecoder
  {
    private int _redPage = -1;
    private int _greenPage = -1;
    private int _yellowPage = -1;
    private int _bluePage = -1;
    private int _whitePage = -1;

    public FastTextDecoder()
    {
    }

    public void Decode(byte[] pageData)
    {
      _redPage = -1;
      _greenPage = -1;
      _yellowPage = -1;
      _bluePage = -1;
      _whitePage = -1;

      int maxRows = pageData.Length/42;
      if (maxRows < 1)
      {
        return;
      }

      int pageNumber = 0;
      for (int rowNr = 0; rowNr < maxRows; rowNr++)
      {
        int packetNumber = Hamming.GetPacketNumber(rowNr*42, ref pageData);
        if (packetNumber == 0)
        {
          pageNumber = Hamming.GetPageNumber(rowNr*42, ref pageData);
        }
        if (packetNumber == 27)
        {
          DecodePacket27(pageNumber, rowNr*42, ref pageData);
          return;
        }
      }
    }

    private void DecodePacket27(int pageNumber, int offset, ref byte[] pageData)
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
        byte s1 = (byte) (linkData[2]);
        byte s2 = (byte) (linkData[3] & 0x7);
        byte s3 = (byte) (linkData[4]);
        byte s4 = (byte) (linkData[5] & 0x3);

        byte m1 = (byte) (linkData[3] >> 3);
        byte m2 = (byte) ((linkData[5] & 0x4) >> 2);
        byte m3 = (byte) (linkData[5] >> 3);
        byte m = (byte) ((m3 << 2) + (m2 << 1) + m1);

        // Magazine is complemented
        int Magazine = pageNumber/0x100;
        if (Magazine == 8)
        {
          Magazine = 0;
        }

        byte linkMagazine = (byte) (m ^ (Magazine%8));

        // Magazine encoded as 0 is magazine 8
        if (linkMagazine == 0)
        {
          linkMagazine = 8;
        }
        int pageNr = linkMagazine*0x100 + pageTens*0x10 + pageUnits;
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

    public int Red
    {
      get { return _redPage; }
    }

    public int Green
    {
      get { return _greenPage; }
    }

    public int Yellow
    {
      get { return _yellowPage; }
    }

    public int Blue
    {
      get { return _bluePage; }
    }

    public int White
    {
      get { return _whitePage; }
    }

    private bool IsDecimalPage(int i)
    {
      return (bool) (((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90));
    }
  }
}