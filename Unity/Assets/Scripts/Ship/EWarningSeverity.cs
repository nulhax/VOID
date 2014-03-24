using UnityEngine;
using System.Collections;

public enum EWarningType : int
{
	INVALID,
	
	Atmosphere,
	Power,
	Engine,
	ProximityObject,

	MAX
}

public enum EWarningSeverity : int
{
	INVALID,
	
	Minor,
	Major,
	Critical,

	MAX
}

public struct TWarningInstance
{
	public TWarningInstance(EWarningType _WarningType, EWarningSeverity _WarningSeverity)
	{
		m_WarningType = _WarningType;
		m_WarningSeverity = _WarningSeverity;
	}
	
	public EWarningType m_WarningType;
	public EWarningSeverity m_WarningSeverity;
}
