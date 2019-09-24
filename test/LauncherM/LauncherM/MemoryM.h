#pragma once
class CMemoryM
{
public:
	CMemoryM(BYTE* pMemory = NULL, size_t size = 0);
	~CMemoryM();

private:
	BYTE* m_pMemory = NULL;
	size_t m_Size = 0;

public:
	void Set(BYTE* pMemory, size_t size);
	void Clear();
	void Create(size_t size, BYTE init = 0x00);

public:
	size_t GetWCHARSize();

public:
	inline size_t GetSize() { return m_Size; }
	inline WCHAR* ToPWCHAR() { return (WCHAR*)m_pMemory; }
};

