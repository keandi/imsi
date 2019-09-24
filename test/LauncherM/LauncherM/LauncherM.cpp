// LauncherM.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include "pch.h"
#include <iostream>
#include "MemoryM.h"

void RealRun()
{
#pragma region 람다 
	auto fnFindPos = [](LPWSTR str, WCHAR it, size_t beginPos = 0) {
		if (str == NULL) { return -1; }

		size_t length = wcslen(str);
		if (length == 0) { return -1; }

		size_t loop = beginPos;
		for (; loop < length; loop++)
		{
			if (str[loop] == it) { return (int)loop; }
		}

		return -1;
	};

	auto fnGetParam = [&fnFindPos](LPWSTR lpwzCmdLine, WCHAR word) {
		if (lpwzCmdLine == NULL) { return CStringW(); }

		size_t length = wcslen(lpwzCmdLine);
		if (length == 0) { return CStringW(); }

		int nFlagPos, nWordPos;
		int nBeginPos = 0;
		do
		{
			nFlagPos = -1, nWordPos = -1;

			nFlagPos = fnFindPos(lpwzCmdLine, L'/', (size_t)nBeginPos);
			if (nFlagPos < 0) { return CStringW(); }

			nWordPos = fnFindPos(lpwzCmdLine, word, (size_t)nFlagPos);
			if (nWordPos < 0) { return CStringW(); }

			nBeginPos = nWordPos + 1;
		} while (nWordPos != nFlagPos + 1);

		int nLastPos = fnFindPos(lpwzCmdLine, L' ', (size_t)nWordPos);
		if (nLastPos < 0)
		{
			nLastPos = (int)length - 1;
		}
		else
		{
			nLastPos -= 1;
		}

		if (nLastPos <= nWordPos) { return CStringW(); }

		//
		CStringW strResult;
		for (int nLoop = nWordPos + 1; nLoop <= nLastPos; nLoop++)
		{
			strResult += lpwzCmdLine[nLoop];
		}

		return strResult;
	};
#pragma endregion

#pragma region 인자값 받기
	LPWSTR lpwzCmdLine = ::GetCommandLineW();
	CStringW strProgram = fnGetParam(lpwzCmdLine, L'p');
	CStringW strData = fnGetParam(lpwzCmdLine, L'd');

	if (strProgram.IsEmpty() == true || strData.IsEmpty() == true) { return; }
#pragma endregion

#pragma region 실행 인자 만들기
	size_t memSize = (strProgram.GetLength() + strProgram.GetLength() + 128) * 2;
	BYTE* pBufferMem = new BYTE[memSize];
	memset(pBufferMem, 0x00, memSize);
	CMemoryM argMem(pBufferMem, memSize);
	WCHAR* pwArg = (WCHAR*)pBufferMem;

#pragma region Data 생성
	size_t dataLength = ((size_t)strData.GetLength() + 128) * sizeof(WCHAR);
	BYTE* pDataBuffer = new BYTE[dataLength];
	memset(pDataBuffer, 0x00, dataLength);
	CMemoryM dataMem(pDataBuffer, dataLength);
	WCHAR* pwData = (WCHAR*)pDataBuffer;
	{
		int nDataPos = 0;
		for (int nLoop = 0; nLoop < strData.GetLength(); nLoop+=2, nDataPos++)
		{
			pwData[nDataPos] = strData.GetAt(nLoop);
		}
	}

	size_t realDataLength = wcslen(pwData);
#pragma endregion

#pragma region 데이터 인자
	CMemoryM argData;
	argData.Create(dataMem.GetSize());

	::swprintf_s(argData.ToPWCHAR(), argData.GetWCHARSize(), L"%s %d", pwData, realDataLength);

#pragma endregion

	::swprintf_s(pwArg, memSize / sizeof(WCHAR), L"%s %s %d", (LPCWSTR)strProgram, pwData, realDataLength);
#pragma endregion

#pragma region 실행하기
	//ShellExecuteW(NULL, L"open", pwArg, NULL, NULL, SW_SHOWNORMAL);
	//ShellExecuteW(NULL, L"open", L"Notepad.exe", L"1234 4", NULL, SW_SHOWNORMAL);
	ShellExecuteW(NULL, L"open", strProgram, argData.ToPWCHAR(), NULL, SW_SHOWNORMAL);
#pragma endregion


	//::MessageBoxW(NULL, pwArg, NULL, NULL);
	//::MessageBoxW(NULL, argData.ToPWCHAR(), NULL, NULL);
}

int main()
{
    //std::cout << "Hello World!\n";
	RealRun();
	return 0;
}

// 프로그램 실행: <Ctrl+F5> 또는 [디버그] > [디버깅하지 않고 시작] 메뉴
// 프로그램 디버그: <F5> 키 또는 [디버그] > [디버깅 시작] 메뉴

// 시작을 위한 팁: 
//   1. [솔루션 탐색기] 창을 사용하여 파일을 추가/관리합니다.
//   2. [팀 탐색기] 창을 사용하여 소스 제어에 연결합니다.
//   3. [출력] 창을 사용하여 빌드 출력 및 기타 메시지를 확인합니다.
//   4. [오류 목록] 창을 사용하여 오류를 봅니다.
//   5. [프로젝트] > [새 항목 추가]로 이동하여 새 코드 파일을 만들거나, [프로젝트] > [기존 항목 추가]로 이동하여 기존 코드 파일을 프로젝트에 추가합니다.
//   6. 나중에 이 프로젝트를 다시 열려면 [파일] > [열기] > [프로젝트]로 이동하고 .sln 파일을 선택합니다.


int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpszCmdParam, int nCmdShow)
{
	RealRun();
	return 0;

}