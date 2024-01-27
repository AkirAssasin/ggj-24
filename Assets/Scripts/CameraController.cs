using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float MinZoomOrthoSize, MaxZoomOrthoSize, MaxFollow;

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
        Vector2 currentPosition = Vector2.Lerp(m_originalPosition, GameManager.Instance.Player.Position, NormalizedZoomLevel * MaxFollow);
        m_transform.position = new Vector3(currentPosition.x, currentPosition.y, m_originalZ);
        m_camera.orthographicSize = Mathf.Lerp(MinZoomOrthoSize, MaxZoomOrthoSize, NormalizedZoomLevel);
    }
}
