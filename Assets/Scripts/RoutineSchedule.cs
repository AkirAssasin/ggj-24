using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Routine Schedule", menuName = "Scriptable Objects/Routine Schedule")]
public class RoutineSchedule : ScriptableObject
{
    public string m_where;
    public int m_startHour;
    public int m_endHour;
    public string m_checklistText;

    [Header("On Complete")]
    public float m_sanityGainOnComplete;
    public int m_skipHoursOnComplete;
    public List<RoutineSchedule> m_triggerThese;
}
