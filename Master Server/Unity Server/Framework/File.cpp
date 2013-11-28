//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//  (c) 2011 Bryce Booth
//
//  File Name :   File.cpp
//
//  Author    :   Bryce Booth
//  Mail      :   bryce.booth@mediadesign.school.nz
//


// Library Includes


// Local Includes


// This Include
#include "File.h"


// Static Variables


// Implementation


/********************************
            Public
*********************************/





/*---------------------------------------------------------------------------------------------------------------------------
*
* File constructors
*
*---------------------------------------------------------------------------------------------------------------------------*/

CFile::CFile()
: m_uiSize(0)
{
	// Empty
}





/*---------------------------------------------------------------------------------------------------------------------------
*
* File deconstruction
*
*---------------------------------------------------------------------------------------------------------------------------*/

CFile::~CFile()
{
	Deinitialise();
}





bool
CFile::Open(const char* _kcpFile, bool _bBinary, bool _bRead, bool _bWrite, bool _bClearContents)
{
	uint uiFlags = 0;


	if (_bBinary)
	{
		uiFlags |= std::fstream::binary;
	}


	if (_bRead)
	{
		uiFlags |= std::fstream::in;
	}


	if (_bWrite)
	{
		uiFlags |= std::fstream::out;
	}


	if (_bClearContents)
	{
		uiFlags |= std::fstream::trunc;
	}


	m_FileStream.open(_kcpFile, uiFlags);

	
	// Set file size
	long begin, end;
	begin = static_cast<long>(m_FileStream.tellg());
	m_FileStream.seekg (0, std::ios::end);
	end = static_cast<long>(m_FileStream.tellg());
    m_uiSize = (end-begin);


	SetReadPointer(0);
	SetWritePointer(0);

	
	return (m_FileStream.is_open());
}





bool
CFile::Read(uint _uiNumBytes, char* _cpBuffer)
{
	m_FileStream.read(reinterpret_cast<char*>(_cpBuffer), _uiNumBytes);


	return (m_FileStream.bad());
}





bool
CFile::Write(c_char* _kcpData, uint _uiNumBytes)
{
	m_FileStream.write(_kcpData, _uiNumBytes);
	m_FileStream << std::endl;


	return (m_FileStream.bad());
}





/********************************
            Protected
*********************************/






/********************************
            Private
*********************************/





void
CFile::Deinitialise()
{
	m_FileStream.close();
}