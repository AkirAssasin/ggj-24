using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float m_walkSpeed, m_latchedSpeedMultiplier;

    //latching mechanic
    public bool IsLatched { get; private set; } = false;
    [SerializeField] GameObject m_quickTimeUI;

    [SerializeField] float m_quickTimeProgressPerInput;
    [SerializeField] float m_quickTimeLoss;
    float m_quickTimeProgress;

    //components
    Rigidbody2D m_rigidbody;
    Slider m_quickTimeSlider;
    public Vector2 Position => m_rigidbody.position;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_quickTimeSlider = m_quickTimeUI.GetComponent<Slider>();
    }

    public void Start()
    {
        m_quickTimeUI.SetActive(false);
    }

    void Update()
    {
        //update latching
        if (IsLatched) UpdateLatch();

        //get normalized input
        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        //set as velocity
        float speed = m_walkSpeed;
        if (IsLatched) speed *= m_latchedSpeedMultiplier;
        m_rigidbody.velocity = input * speed;
    }

    void UpdateLatch()
    {
        //update le quicktime event
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //spam click to progress
            m_quickTimeProgress += m_quickTimeProgressPerInput;
        }
        if (m_quickTimeProgress >= 1f)
        {
            //we done
            GameManager.Instance.SpawnEnemy(Position + Random.insideUnitCircle);
            m_quickTimeUI.SetActive(false);
            IsLatched = false;
            
            GameManager.Instance.Camera.NormalizedZoomLevel = 0;
        }
        else
        {
            //we aint done
            m_quickTimeProgress = Mathf.Max(0, m_quickTimeProgress - m_quickTimeLoss * Time.deltaTime);
            m_quickTimeSlider.value = m_quickTimeProgress;

            GameManager.Instance.Camera.NormalizedZoomLevel = 1f - m_quickTimeProgress * 0.5f;
        }
    }

    public void LatchOn()
    {
        //begin le quicktime event
        m_quickTimeProgress = 0;
        m_quickTimeUI.SetActive(true);
        IsLatched = true;
    }
}
