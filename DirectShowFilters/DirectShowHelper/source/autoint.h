// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.


// Used for detecting the real frame duration
// Origin of this clas is MPC-HC
class CAutoInt
{
public:

	int m_Int;

	CAutoInt()
	{
		m_Int = 0;
	}
	CAutoInt(int _Other)
	{
		m_Int = _Other;
	}

	operator int()
	{
		return m_Int;
	}

	CAutoInt &operator ++()
	{
		++m_Int;
		return *this;
	}
};
