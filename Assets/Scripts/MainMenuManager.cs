using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public enum MenuType { Title, GoodEnd, BadEnd }
    public static MenuType Type = MenuType.Title;

    [SerializeField] GameObject m_makeMeLaughTitle, m_makeMeCryTitle, m_anyKeyPrompt;
    [SerializeField] TextMeshProUGUI m_endScreenText;
    [SerializeField] float m_laughLingerDuration, m_anyKeyWaitDuration, m_delayPerKey;
    [SerializeField] AudioClipGroup m_titleSwitchAudioClip;

    [TextArea, SerializeField] string m_goodEndText, m_badEndText;

    Akir.Coroutine m_animCoroutine;
    AudioSource m_audioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_animCoroutine = new Akir.Coroutine(this);
        m_audioSource = GetComponent<AudioSource>();

        m_anyKeyPrompt.SetActive(false);
        if (Type == MenuType.Title)
        {
            m_endScreenText.gameObject.SetActive(false);
            m_makeMeCryTitle.SetActive(false);
            m_makeMeLaughTitle.SetActive(true);
            m_animCoroutine.Start(TitleScreenCoroutine());
        }
        else
        {
            m_endScreenText.gameObject.SetActive(true);
            m_makeMeCryTitle.SetActive(false);
            m_makeMeLaughTitle.SetActive(false);
            m_animCoroutine.Start(EndScreenCoroutine(Type == MenuType.GoodEnd ? m_goodEndText : m_badEndText));
        }
    }

    IEnumerator TitleScreenCoroutine()
    {
        //make me laugh
        Akir.WaitForSeconds wait = new Akir.WaitForSeconds(m_laughLingerDuration);
        yield return wait;

        //make me cry
        m_titleSwitchAudioClip.PlayOneShot(m_audioSource);
        m_makeMeCryTitle.SetActive(true);
        m_makeMeLaughTitle.SetActive(false);

        //wait
        yield return wait.Reuse(m_anyKeyWaitDuration);
        m_anyKeyPrompt.SetActive(true);
    }

    IEnumerator EndScreenCoroutine(string text)
    {
        Akir.WaitForSeconds wait = new Akir.WaitForSeconds(m_delayPerKey);

        string currentText = "";
        for (int X = 0; X < text.Length; ++X)
        {
            currentText += text[X];
            m_endScreenText.text = currentText;
            yield return wait.Reuse(m_delayPerKey);
        }

        //wait
        yield return wait.Reuse(m_anyKeyWaitDuration);
        m_anyKeyPrompt.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && !m_animCoroutine.Running)
        {
            SceneManager.LoadScene(1);
        }
    }
}
