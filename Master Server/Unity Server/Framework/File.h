//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name   :   File.h
//  Description :   --------------------------
//
//  Author      :   Bryce Booth
//  Mail        :   bryce.booth@mediadesign.school.nz
//


#pragma once


#ifndef __File_H__
#define __File_H__


// Library Includes
#include <iostream>
#include <fstream>
#include <string>


// Local Includes
#include "Defines/DataTypes.h"


// Types


// Prototypes


class CFile
{

	// Member Functions
public:


	 CFile();
	~CFile();


	bool Open(c_char* _kcpFile, bool _bBinary, bool _bRead, bool _bWrite, bool _bClearContents);


	bool Read(uint _uiNumBytes, char* _cpBuffer);
	bool Write(c_char* _kcpData, uint _uiNumBytes);


	// Inline Functions
	inline void SetReadPointer(uint _uiPosition);
	inline void SetWritePointer(uint _uiPosition);


	inline uint GetSize() const;
	inline int GetReadPointerPosition();
	inline int GetWritePointerPosition();


protected:


private:


	void Deinitialise();


	CFile(const CFile& _krFile);
	CFile& operator = (const CFile& _krFile);


	// Member Variables
protected:


private:


	std::fstream m_FileStream;


	uint m_uiSize;


};


#include "Inline/File.inl"


#endif //__File_H__