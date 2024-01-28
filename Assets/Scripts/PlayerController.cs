using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //movement speed
    [SerializeField] float m_walkSpeed, m_latchedSpeedMultiplier;

    //latching mechanic
    public bool IsLatched { get; private set; } = false;
    [SerializeField] GameObject m_quickTimeUI;

    [SerializeField] float m_quickTimeProgressPerInput;
    [SerializeField] float m_quickTimeLoss;
    float m_quickTimeProgress;

    //interaction
    readonly List<RoutineController> m_touchingTheseRoutines = new List<RoutineController>();
    RoutineController m_interactingWithThis = null;

    [SerializeField] GameObject m_interactionPromptGO;
    [SerializeField] GameObject m_interactionBarParent;
    [SerializeField] RectTransform m_interactionIndicatorRect;
    [SerializeField] RectTransform m_interactionPointerParentRect;

    [SerializeField] float m_barProgressSpeed;
    float m_barValue, m_barRangeMin, m_barRangeMax;

    [SerializeField] GameObject m_interactionPointerPrefab;
    [SerializeField] float m_interactionPointerOffset;
    int m_interactionPointerCount;
    readonly List<GameObject> m_interactionPointers = new List<GameObject>();

    [SerializeField] float m_barStunRecoverySpeed;
    float m_barProgressSpeedMultiplier = 1f;

    //components
    Animator m_animator;
    Rigidbody2D m_rigidbody;
    Slider m_quickTimeSlider;
    public Vector2 Position => m_rigidbody.position;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_quickTimeSlider = m_quickTimeUI.GetComponent<Slider>();
    }

    public void Start()
    {
        m_quickTimeUI.SetActive(false);
        m_interactionPromptGO.SetActive(false);
        m_interactionBarParent.SetActive(false);
    }

    void Update()
    {
        //update latching
        if (IsLatched) UpdateLatch();

        //recover stun
        if (m_barProgressSpeedMultiplier < 1)
        {
            m_barProgressSpeedMultiplier = Mathf.Min(1f, m_barProgressSpeedMultiplier + Time.deltaTime * m_barStunRecoverySpeed);
        }

        //update interaction
        if (m_interactingWithThis != null)
        {
            UpdateInteraction();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            for (int X = 0; X < m_touchingTheseRoutines.Count; ++X)
            {
                if (m_touchingTheseRoutines[X].IsInteractable())
                {
                    StartInteraction(m_touchingTheseRoutines[X]);
                    break;
                }
            }
        }

        //get input
        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float inputLength = input.magnitude;
        if (inputLength > Mathf.Epsilon)
        {
            //stop interacting
            CancelInteraction();

            //set as velocity
            float speed = m_walkSpeed;
            if (IsLatched) speed *= m_latchedSpeedMultiplier;
            m_rigidbody.velocity = input / inputLength * speed;

            //set walking
            m_animator.SetBool("IsWalking", true);
        }
        else
        {
            //set not walking
            m_animator.SetBool("IsWalking", false);
            m_rigidbody.velocity = Vector2.zero;
        }
    }

    void CancelInteraction()
    {
        m_interactingWithThis = null;
        m_interactionBarParent.SetActive(false);
    }

    void UpdateInteraction()
    {
        if (GameManager.Instance.IsFastForwarding || !m_interactingWithThis.IsInteractable())
        {
            CancelInteraction();
            return;
        }

        m_barValue += Time.deltaTime * m_barProgressSpeedMultiplier;
        float actualBarValue = Mathf.PingPong(m_barValue, 1f);

        Vector2 pointerAnchor = m_interactionPointerParentRect.anchorMin;
        pointerAnchor.x = actualBarValue;
        m_interactionPointerParentRect.anchorMin = pointerAnchor;
        m_interactionPointerParentRect.anchorMax = pointerAnchor;

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (m_barRangeMin <= actualBarValue && actualBarValue <= m_barRangeMax)
            {
                --m_interactionPointerCount;
                m_interactionPointers[m_interactionPointerCount].SetActive(false);
                if (m_interactionPointerCount == 0)
                {
                    m_interactingWithThis.Complete();
                    CancelInteraction();
                }
                else SetNewBarRange();
            }
            else m_barProgressSpeedMultiplier = 0;
        }
    }

    void StartInteraction(RoutineController interactWithThis)
    {
        //cancel previous interaction
        if (m_interactingWithThis != null) CancelInteraction();

        //check if interaction is instant
        if (interactWithThis.InteractionPointerCount <= 0)
        {
            interactWithThis.Complete();
            return;
        }

        //not instant
        m_interactingWithThis = interactWithThis;
        m_interactionBarParent.SetActive(true);
        
        //set up pointers
        m_interactionPointerCount = m_interactingWithThis.InteractionPointerCount;
        for (int X = 0; X < m_interactionPointerCount; ++X)
        {
            if (X >= m_interactionPointers.Count)
            {
                GameObject newPointer = Instantiate(m_interactionPointerPrefab, m_interactionPointerParentRect);
                newPointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, X * m_interactionPointerOffset);
                m_interactionPointers.Add(newPointer);
            }
            m_interactionPointers[X].SetActive(true);
        }
        for (int X = m_interactionPointerCount; X < m_interactionPointers.Count; ++X)
        {
            m_interactionPointers[X].SetActive(false);
        }

        //set bar range
        SetNewBarRange();
    }

    void SetNewBarRange()
    {
        m_barValue = 0f;
        m_interactingWithThis.GetInteractionBarRange(out m_barRangeMin, out m_barRangeMax);
        m_interactionIndicatorRect.anchorMin = new Vector2(m_barRangeMin, 0f);
        m_interactionIndicatorRect.anchorMax = new Vector2(m_barRangeMax, 1f);
    }

    void UpdateLatch()
    {
        //update le quicktime event
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //spam click to progress
            m_quickTimeProgress += m_quickTimeProgressPerInput;

            //also cancel interaction
            CancelInteraction();
        }
        if (m_quickTimeProgress >= 1f || GameManager.Instance.IsFastForwarding)
        {
            //we done
            CancelLatch();
        }
        else
        {
            //we aint done
            m_quickTimeProgress = Mathf.Max(0, m_quickTimeProgress - m_quickTimeLoss * Time.deltaTime);
            m_quickTimeSlider.value = m_quickTimeProgress;

            GameManager.Instance.Camera.NormalizedZoomLevel = 1f - m_quickTimeProgress * 0.5f;
        }
    }

    void CancelLatch()
    {
        if (IsLatched)
        {
            GameManager.Instance.SpawnEnemy(Position + Random.insideUnitCircle);
            m_quickTimeUI.SetActive(false);
            IsLatched = false;
            m_animator.SetBool("IsLatched", false);
            
            GameManager.Instance.Camera.NormalizedZoomLevel = 0;
        }
    }

    public void LatchOn()
    {
        //begin le quicktime event
        m_quickTimeProgress = 0;
        m_quickTimeUI.SetActive(true);
        IsLatched = true;
        m_animator.SetBool("IsLatched", true);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        RoutineController routine = collision.GetComponent<RoutineController>();
        if (routine != null)
        {
            m_touchingTheseRoutines.Add(routine);
            m_interactionPromptGO.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        RoutineController routine = collision.GetComponent<RoutineController>();
        if (routine != null)
        {
            m_touchingTheseRoutines.RemoveAll(r => r == routine);
            m_interactionPromptGO.SetActive(m_touchingTheseRoutines.Count > 0);
        }
    }
}
