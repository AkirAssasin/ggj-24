using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float m_edgeX;
    [SerializeField] float m_minZoomOrthoSize, m_maxZoomOrthoSize, m_maxFollow;

    Vector2 m_originalPosition;
    float m_originalZ;

    [System.NonSerialized] public float NormalizedZoomLevel = 0;

    Transform m_transform;
    Camera m_camera;

    void Awake()
    {
        m_transform = GetComponent<Transform>();
        m_camera = GetComponent<Camera>();
    }

    void Start()
    {
        m_originalPosition = m_transform.position;
        m_originalZ = m_transform.position.z;
    }

    void Update()
    {
        m_originalPosition.x = m_edgeX - Mathf.Lerp(m_maxZoomOrthoSize, m_minZoomOrthoSize, m_maxFollow) * m_camera.aspect;
        Vector2 currentPosition = Vector2.Lerp(m_originalPosition, GameManager.Instance.Player.Position, NormalizedZoomLevel * m_maxFollow);
        m_transform.position = new Vector3(currentPosition.x, currentPosition.y, m_originalZ);
        m_camera.orthographicSize = Mathf.Lerp(m_minZoomOrthoSize, m_maxZoomOrthoSize, NormalizedZoomLevel);
    }
}
