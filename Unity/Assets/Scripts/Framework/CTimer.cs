//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Timer.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CTimer
{

// Member Types


// Member Functions
    
    // public:


    public void Start(float _fInitialDelay, float _fInterval, int _iNumRepeats = 0) // _iNumRepeats = -1 = Infinit
    {
        if (_fInterval > 1.0f / 30.0f)
        {
            m_fInterval = _fInitialDelay + _fInterval;
            m_iNumRepeats = _iNumRepeats;
            m_bActive = true;
        }
        else
        {
            Debug.LogError("The interval is too small. This could lead to unwanted behavior");
        }
    }


    public void Stop()
    {
        m_fInterval = 0.0f;
        m_iNumRepeats = 0;
        m_bActive = false;
        m_bPaused = false;
    }


    public void Update(float _fDeltatick)
    {
        if ( m_bActive &&
            !m_bPaused)
        {
            m_fTime -= _fDeltatick;


            while (m_fTime <= 0.0f)
            {
                m_fTime += m_fInterval;


                if (m_iNumRepeats > 0)
                {
                    -- m_iNumRepeats;


                    if (m_iNumRepeats == 0)
                    {
                        Stop();
                    }
                }
            }
        }
    }


    public void Pause()
    {
        m_bPaused = true;
    }


    public void Resume()
    {
        m_bPaused = false;
    }


    public bool IsPaused()
    {
        return (m_bPaused);
    }


    public bool IsActive()
    {
        return (m_bActive);
    }


    public static void UpdateTimers(float _fDeltatick)
    {
        foreach (CTimer cTimer in s_aTimers)
		{
			cTimer.Update(_fDeltatick);
		}
    }


    // protected:


    // private:



// Member Variables
    
    // protected:


    // private:


    float m_fInterval = 0.0f;
    float m_fTime = 0.0f;


    int m_iNumRepeats = 0;


    bool m_bPaused = false;
    bool m_bActive = false;


    static List<CTimer> s_aTimers = new List<CTimer>();


};
