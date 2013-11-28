//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//
//  File Name :   File.inl
//
//  Author    :   Bryce Booth
//  Mail      :   bryce.booth@mediadesign.school.nz
//


// Library Includes


// Local Includes


// Implementation


void
CFile::SetReadPointer(uint _uiPosition)
{
	m_FileStream.seekg(_uiPosition);
}


void
CFile::SetWritePointer(uint _uiPosition)
{
	m_FileStream.seekp(_uiPosition);
}


uint
CFile::GetSize() const
{
	return (m_uiSize);
}


int
CFile::GetReadPointerPosition()
{
	return (static_cast<int>(m_FileStream.tellg()));
}


int
CFile::GetWritePointerPosition()
{
	return (static_cast<int>(m_FileStream.tellp()));
}