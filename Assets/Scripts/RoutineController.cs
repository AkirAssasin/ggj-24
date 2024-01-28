using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoutineScheduleHandler
{
    readonly RoutineSchedule m_schedule;
    readonly GameObject m_checklistItem;
    readonly TextMeshProUGUI m_checklistItemTextMesh;

    public RoutineScheduleHandler(RoutineSchedule schedule, GameObject checklistItem)
    {
        m_schedule = schedule;
        m_checklistItem = checklistItem;

        m_checklistItemTextMesh = m_checklistItem.GetComponent<TextMeshProUGUI>();
        m_checklistItemTextMesh.text = m_schedule.m_checklistText;
    }

    public void Complete()
    {
        m_checklistItemTextMesh.fontStyle = FontStyles.Strikethrough;
        m_checklistItemTextMesh.color = Color.gray;

        GameManager.Instance.GainSanity(m_schedule.m_sanityGainOnComplete);

        foreach (RoutineSchedule addThisSchedule in m_schedule.m_triggerThese)
        {
            GameManager.Instance.AddThisRoutineSchedule(addThisSchedule);
        }

        if (m_schedule.m_skipHoursOnComplete > 0)
        {
            GameManager.Instance.FastForwardTime(m_schedule.m_skipHoursOnComplete);
        }
    }

    public bool TryToExpire(int currentHour)
    {
        if (m_schedule.m_endHour <= currentHour)
        {
            GameManager.Instance.RemoveChecklistItem(m_checklistItem);
            return true;
        }
        return false;
    }
}

public class RoutineController : MonoBehaviour
{
    //interactable
    [field:SerializeField] public int InteractionPointerCount { get; private set; }
    List<RoutineScheduleHandler> m_scheduleHandlers = new List<RoutineScheduleHandler>();

    //spawner (spawn rate is maxed out at min range)
    [SerializeField] float m_minSpawnDetectionRange, m_maxSpawnDetectionRange, m_secondsBetweenSpawn;
    float m_spawnProgress;

    //component
    Transform m_transform;

    void Awake()
    {
        m_transform = GetComponent<Transform>();
    }

    public void AddSchedule(RoutineSchedule addThisSchedule, GameObject checklistItem)
    {
        m_scheduleHandlers.Add(new RoutineScheduleHandler(addThisSchedule, checklistItem));
    }

    public void ResetSchedulesAndSpawns()
    {
        m_scheduleHandlers.Clear();
        m_spawnProgress = 0;
    }

    public void Complete()
    {
        if (GameManager.Instance.IsFastForwarding || m_scheduleHandlers.Count == 0) return;
        m_scheduleHandlers[0].Complete();
        m_scheduleHandlers.RemoveAt(0);
    }

    public void CheckExpiredSchedules()
    {
        //check if expired
        int currentHour = GameManager.Instance.GetCurrentHour();
        for (int X = m_scheduleHandlers.Count - 1; X > -1; --X)
        {
            if (m_scheduleHandlers[X].TryToExpire(currentHour)) m_scheduleHandlers.RemoveAt(X);
        }
    }

    public bool IsInteractable() => (m_scheduleHandlers.Count > 0);

    void Update()
    {
        if (GameManager.Instance.IsFastForwarding) return;

        Vector2 position = m_transform.position;
        float sqDistanceToPlayer = (GameManager.Instance.Player.Position - position).sqrMagnitude;

        float minSq = m_minSpawnDetectionRange * m_minSpawnDetectionRange;
        float maxSq = m_maxSpawnDetectionRange * m_maxSpawnDetectionRange;
        float t = 1f - Mathf.Clamp01((sqDistanceToPlayer - minSq) / (maxSq - minSq));
        t *= 1f - GameManager.Instance.GetSanity();

        //do spawn
        m_spawnProgress += Time.deltaTime * t / m_secondsBetweenSpawn;
        if (m_spawnProgress >= 1f)
        {
            m_spawnProgress -= 1f;
            if (GameManager.Instance.Player.IsLatched)
            {
                GameManager.Instance.SpawnEnemy(m_transform.position, t);
            }
            else
            {
                GameManager.Instance.Player.LatchOn(1f);
            }
        }

        GameManager.Instance.SetHighestCue(t * m_spawnProgress);
    }

    public void GetInteractionBarRange(out float min, out float max)
    {
        float width = Random.Range(0.1f, 0.3f);
        min = Random.Range(0f, 1f - width);
        max = min + width;
    }
}
