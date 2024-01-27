using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //static instance
    public static GameManager Instance { get; private set; }

    //player reference
    [field:SerializeField] public PlayerController Player { get; private set; }
    [field:SerializeField] public CameraController Camera { get; private set; }

    //enemies
    [SerializeField] GameObject m_enemyPrefab;

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

    void Awake()
    {
        if (Instance != null) throw new System.Exception();
        Instance = this;

        m_sanityPrevDay = m_sanity = m_initialSanity;

        m_endDayCoroutine = new Akir.Coroutine(this);
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
        foreach (RoutineController routine in m_routines)
        {
            routine.Enable();
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
        enemy.Initialize(position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SpawnEnemy(Random.insideUnitCircle * 5f);
    }

    //set sanity text
    void SetSanityText(float t)
    {
        int currentSanity = (int)(100 * Mathf.Lerp(m_sanityPrevDay, m_sanity, t));
        m_sanityTextMesh.text = currentSanity + "%";
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

            //fade out
            yield return new RunForDuration(m_sanityBlackoutFadeDuration, t =>
            {
                m_sanityBlackoutGroup.alpha = 1f - m_sanityFadeCurve.Evaluate(t);
            });

            //start day
            StartDay();
        }
        m_endDayCoroutine.Start(EndDayCoroutine());
    }
}
