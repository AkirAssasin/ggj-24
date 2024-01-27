using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// audio clip group
[CreateAssetMenu(fileName = "New Audio Clip Group", menuName = "Scriptable Objects/Audio Clip Group")]
public class AudioClipGroup : ScriptableObject
{
    // audio clips
    [SerializeField] List<AudioClip> m_clips;

    // audio volume
    [SerializeField] float m_volume = 1f;

    // time restriction
    [SerializeField] float m_minInterval = 0f;
    float m_lastPlayedTime = 0;

    // play
    public void PlayOneShot(AudioSource audioSource)
    {
        if (Time.time >= m_lastPlayedTime && Time.time - m_lastPlayedTime < m_minInterval) return;
        m_lastPlayedTime = Time.time;
        audioSource.PlayOneShot(m_clips[Random.Range(0, m_clips.Count)], m_volume);
    }
}
