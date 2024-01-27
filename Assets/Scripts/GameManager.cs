using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //static instance
    public static GameManager Instance { get; private set; }

    //player reference
    [field:SerializeField] public PlayerController Player { get; private set; }
    [field:SerializeField] public CameraController Camera { get; private set; }

    //enemies
    [SerializeField] GameObject m_enemyPrefab;
    readonly List<EnemyController> m_enemies = new List<EnemyController>();

    //checklist
    [SerializeField] RectTransform m_checklistParent;
    [SerializeField] GameObject m_checklistItemPrefab;

    //routines
    [SerializeField] List<RoutineController> m_routines;

    //sanity
    [SerializeField] float m_initialSanity, m_sanityLossPerDay;
    float m_sanityPrevDay, m_sanity;

    [SerializeField] AnimationCurve m_sanityFadeCurve;
    [SerializeField] CanvasGroup m_sanityBlackoutGroup;
    [SerializeField] TextMeshProUGUI m_sanityTextMesh;
    [SerializeField] float m_sanityBlackoutFadeDuration;
    [SerializeField] float m_sanityTextDuration;
    Akir.Coroutine m_endDayCoroutine;
    public bool IsEnteringNextDay => m_endDayCoroutine.Running;

    //time
    [SerializeField] int m_startHour, m_endHour;
    [SerializeField] float m_minutesPerSecond;
    [SerializeField] TextMeshProUGUI m_timeTextMesh;
    [SerializeField] Slider m_timeBar;
    float m_currentMinute;
    int m_endOfDayMinutes;

    //cues
    float m_highestCue;

    void Awake()
    {
        if (Instance != null) throw new System.Exception();
        Instance = this;

        m_sanityPrevDay = m_sanity = m_initialSanity;

        m_endDayCoroutine = new Akir.Coroutine(this);

        m_endOfDayMinutes = (m_endHour - m_startHour) * 60;
    }

    void Start()
    {
        foreach (RoutineController routine in m_routines)
        {
            GameObject checklistItem = Instantiate(m_checklistItemPrefab, m_checklistParent);
            routine.Initialize(checklistItem);
        }
        StartDay();
    }

    void StartDay()
    {
        //reset time
        m_currentMinute = 0;
        SetTimeBarAndText();

        //reset routine
        foreach (RoutineController routine in m_routines)
        {
            routine.Enable();
        }

        //reset enemies
        for (int X = m_enemies.Count - 1; X > -1; --X)
        {
            m_enemies[X].Pool();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void GainSanity(float amount)
    {
        m_sanity = Mathf.Clamp01(m_sanity + amount);
    }

    public void SpawnEnemy(Vector2 position)
    {
        EnemyController enemy = EnemyController.GetFromPool(m_enemyPrefab);
        m_enemies.Add(enemy);
        enemy.Initialize(position);
    }

    public void UnregisterEnemy(EnemyController enemy) => m_enemies.Remove(enemy);

    void SetTimeBarAndText()
    {
        //set time UI
        m_timeBar.value = m_currentMinute / m_endOfDayMinutes;
        int hours = (int)(m_currentMinute / 60);
        int minutes = (int)(m_currentMinute - hours * 60);
        hours += m_startHour;
        
        string meridian;
        if (hours >= 12)
        {
            hours -= 12;
            meridian = "PM";
        }
        else meridian = "AM";
        if (hours == 0) hours = 12;

        m_timeTextMesh.text = $"{hours:00}:{minutes:00} {meridian}";
    }

    public void SetHighestCue(float cue)
    {
        m_highestCue = Mathf.Max(m_highestCue, cue);
    }

    void UpdateCue()
    {
        m_highestCue = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //normal day stuff
        if (!IsEnteringNextDay)
        {
            //advance time
            m_currentMinute += m_minutesPerSecond * Time.deltaTime;
            if (m_currentMinute >= m_endOfDayMinutes)
            {
                //you didn't bedge in time
                m_currentMinute = m_endOfDayMinutes;
                EndDay();
            }
            SetTimeBarAndText();
        }

        //update cue
        UpdateCue();
    }

    //set sanity text
    void SetSanityText(float t)
    {
        int currentSanity = (int)(100 * Mathf.Lerp(m_sanityPrevDay, m_sanity, t));
        m_sanityTextMesh.text = $"{currentSanity:00}%";
    }

    //advance to next day
    public void EndDay()
    {
        //cannot end day while day is ending
        if (IsEnteringNextDay) return;

        //set initial sanity text
        SetSanityText(0);

        //lose sanity
        m_sanity -= m_sanityLossPerDay;

        //start end
        IEnumerator EndDayCoroutine()
        {
            //fade in
            yield return new RunForDuration(m_sanityBlackoutFadeDuration, t =>
            {
                m_sanityBlackoutGroup.alpha = m_sanityFadeCurve.Evaluate(t);
            });

            //sanity display
            yield return new RunForDuration(m_sanityTextDuration, t => SetSanityText(t * t));
            m_sanityPrevDay = m_sanity;

            //start day
            StartDay();

            //fade out
            yield return new RunForDuration(m_sanityBlackoutFadeDuration, t =>
            {
                m_sanityBlackoutGroup.alpha = 1f - m_sanityFadeCurve.Evaluate(t);
            });
        }
        m_endDayCoroutine.Start(EndDayCoroutine());
    }
}
