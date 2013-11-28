//
//  Bryce Booth
//  Auckland
//  New Zealand
//
//
//  File Name :   WinClock.inl
//
//  Author    :   Bryce Booth
//  Mail      :   brycebooth@hotmail.com
//


// Local Includes


// Library Includes


// Implementation


float 
CClock::GetDeltaTick() const
{
	return ((float)m_fDeltaTick);
}


float  
CClock::GetFramesPerSecond() const
{
	return (m_fFramesPerSecond);
}