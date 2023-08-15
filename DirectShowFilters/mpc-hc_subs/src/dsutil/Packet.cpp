/*
 * (C) 2006-2018 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#include "stdafx.h"
#include "Packet.h"

//
// CPacketQueue
//

void CPacketQueue2::Add(CAutoPtr<CPacket> p)
{
	std::unique_lock<std::mutex> lock(m_mutex);
	if (p) {
		m_size += p->size();
	}
	emplace_back(p);
}

CAutoPtr<CPacket> CPacketQueue2::Remove()
{
	std::unique_lock<std::mutex> lock(m_mutex);
	ASSERT(!empty());
	CAutoPtr<CPacket> p = front(); pop_front();
	if (p) {
		m_size -= p->size();
	}
	return p;
}

void CPacketQueue2::RemoveSafe(CAutoPtr<CPacket>& p, size_t& count)
{
	std::unique_lock<std::mutex> lock(m_mutex);
	count = size();

	if (count) {
		p = front(); pop_front();
		if (p) {
			m_size -= p->size();
		}
	}
}

void CPacketQueue2::RemoveAll()
{
	std::unique_lock<std::mutex> lock(m_mutex);
	m_size = 0;
	clear();
}

const size_t CPacketQueue2::GetCount()
{
	std::unique_lock<std::mutex> lock(m_mutex);
	return size();
}

const size_t CPacketQueue2::GetSize()
{
	std::unique_lock<std::mutex> lock(m_mutex);
	return m_size;
}

const REFERENCE_TIME CPacketQueue2::GetDuration()
{
	std::unique_lock<std::mutex> lock(m_mutex);
	return !empty() ? (back()->rtStop - front()->rtStart) : 0;
}
