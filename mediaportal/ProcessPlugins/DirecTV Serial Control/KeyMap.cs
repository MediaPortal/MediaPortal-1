/* 
 *	Copyright (C) 2005-2008 Team MediaPortal - micheloe, patrick, diehard2
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

using System;
using System.Collections.Generic;
using System.Text;

namespace DirecTV
{
  public class KeyMap
  {
    public string KeyMapName = string.Empty;
    public byte RIGHT;
    public byte LEFT;
    public byte UP;
    public byte DOWN;
    public byte FAVORITE;
    public byte SELECT;
    public byte ENTER;
    public byte EXIT;
    public byte KEY_9;
    public byte KEY_8;
    public byte KEY_7;
    public byte KEY_6;
    public byte KEY_5;
    public byte KEY_4;
    public byte KEY_3;
    public byte KEY_2;
    public byte KEY_1;
    public byte KEY_0;
    public byte DASH;
    public byte CH_UP;
    public byte CH_DOWN;
    public byte POWER;
    public byte JUMP;
    public byte GUIDE;
    public byte MENU;
    public byte INFO;
    public byte ACTIVE;
    public byte LIST;
    public byte BACK;

    public KeyMap(
      string name,
      byte right,
      byte left,
      byte up,
      byte down,
      byte favorite,
      byte select,
      byte enter,
      byte exit,
      byte key9,
      byte key8,
      byte key7,
      byte key6,
      byte key5,
      byte key4,
      byte key3,
      byte key2,
      byte key1,
      byte key0,
      byte dash,
      byte chup,
      byte chdn,
      byte pwr,
      byte jump,
      byte guide,
      byte menu,
      byte info,
      byte active,
      byte list,
      byte back
      )
    {
      KeyMapName = name;
      RIGHT = right;
      LEFT = left;
      UP = up;
      DOWN = down;
      FAVORITE = favorite;
      SELECT = select;
      ENTER = enter;
      EXIT = exit;
      KEY_9 = key9;
      KEY_8 = key8;
      KEY_7 = key7;
      KEY_6 = key6;
      KEY_5 = key5;
      KEY_4 = key4;
      KEY_3 = key3;
      KEY_2 = key2;
      KEY_1 = key1;
      KEY_0 = key0;
      DASH = dash;
      CH_UP = chup;
      CH_DOWN = chdn;
      POWER = pwr;
      JUMP = jump;
      GUIDE = guide;
      MENU = menu;
      INFO = info;
      ACTIVE = active;
      LIST = list;
      BACK = back;
    }
  }
}
