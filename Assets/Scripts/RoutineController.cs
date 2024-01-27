using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoutineController : MonoBehaviour
{
    [SerializeField] Color m_checklistCompleteColor, m_checklistColor;
    [SerializeField, TextArea] string m_checklistText;

    //interactable
    [field:SerializeField] public int InteractionPointerCount { get; private set; }
    bool m_isInteractable;

    //sanity
    [field:SerializeField] public float m_sanityGainOnComplete;
    [field:SerializeField] public bool m_endDayOnComplete;

    //component
    Transform m_transform;
    GameObject m_checklistItem;
    TextMeshProUGUI m_checklistItemTextMesh;

    void Awake()
    {
        m_transform = GetComponent<Transform>();
    }

    public void Initialize(GameObject checklistItem)
    {
        m_checklistItem = checklistItem;
        m_checklistItemTextMesh = m_checklistItem.GetComponent<TextMeshProUGUI>();
        m_checklistItemTextMesh.text = m_checklistText;
    }

    public void Disable()
    {
        enabled = false;
        m_checklistItem.SetActive(false);

        m_isInteractable = false;
    }

    public void Enable()
    {
        enabled = true;
        m_checklistItem.SetActive(true);
        m_checklistItemTextMesh.fontStyle = FontStyles.Normal;
        m_checklistItemTextMesh.color = m_checklistColor;

        m_isInteractable = true;
    }

    public void Complete()
    {
        if (GameManager.Instance.IsEnteringNextDay || !m_isInteractable) return;

        m_checklistItemTextMesh.fontStyle = FontStyles.Strikethrough;
        m_checklistItemTextMesh.color = m_checklistCompleteColor;

        m_isInteractable = false;
        GameManager.Instance.GainSanity(m_sanityGainOnComplete);

        if (m_endDayOnComplete) GameManager.Instance.EndDay();
    }

    void Update()
    {
        if (m_isInteractable)
        {
            Vector2 position = m_transform.position;
            float sqDistanceToPlayer = (GameManager.Instance.Player.Position - position).sqrMagnitude;
            GameManager.Instance.Player.CheckNearestRoutine(this, sqDistanceToPlayer);
        }
    }

    public void GetInteractionBarRange(out float min, out float max)
    {
        float width = Random.Range(0.1f, 0.3f);
        min = Random.Range(0f, 1f - width);
        max = min + width;
    }
}
