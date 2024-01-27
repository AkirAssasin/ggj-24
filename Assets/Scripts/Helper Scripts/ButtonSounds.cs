using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    // audio clip groups
    public AudioClipGroup m_enterAudio, m_successAudio, m_failAudio;

    // component references
    AudioSource m_audioSource;
    Selectable m_selectable;

    // awake
    void Awake()
    {
        // get references
        m_audioSource = GetComponent<AudioSource>();
        m_selectable = GetComponent<Selectable>();
    }

    // enter and exit audio
    public void OnPointerExit(PointerEventData eventData) => OnPointerEnter(eventData);
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_selectable.interactable) m_enterAudio.PlayOneShot(m_audioSource);
    }

    // click audio
    public void OnPointerClick(PointerEventData eventData) => (m_selectable.interactable ? m_successAudio : m_failAudio).PlayOneShot(m_audioSource);
}
