#include "pch.h"
#include "MemoryM.h"

CMemoryM::CMemoryM(BYTE* pMemory/* = NULL*/, size_t size /*= 0*/)
{
	Set(pMemory, size);
}

CMemoryM::~CMemoryM()
{
	Clear();
}

void CMemoryM::Set(BYTE* pMemory, size_t size)
{
	Clear();
	m_pMemory = pMemory;
	m_Size = size;
}

void CMemoryM::Clear()
{
	if (m_pMemory != NULL)
	{
		for (size_t loop = 0; loop < m_Size; loop++)
		{
			m_pMemory[loop] = 0x00;
		}

		delete[]m_pMemory;
		m_pMemory = NULL;
	}
	m_Size = 0;
}

void CMemoryM::Create(size_t size, BYTE init)
{
	Clear();
	if (size == 0) { return; }

	m_pMemory = new BYTE[size];
	memset(m_pMemory, init, size);
	m_Size = size;
}

size_t CMemoryM::GetWCHARSize()
{
	if (m_Size == 0) { return 0; }
	return m_Size / sizeof(WCHAR);
}
