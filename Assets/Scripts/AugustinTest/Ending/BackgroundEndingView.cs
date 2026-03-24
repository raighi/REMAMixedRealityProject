using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BackgroundEndingView : MonoBehaviour
{
    public float typingSpeed = 0.02f;
    
    [Header("UI Text")]
    public TMP_Text introText;

    private Coroutine typingCoroutine;

    // ─────────────────────────────────────────────
    // Typing Coroutine
    // ─────────────────────────────────────────────

    public void TypeText(string fullText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextCoroutine(fullText));
    }

    private IEnumerator TypeTextCoroutine(string text)
    {
        introText.text = "";

        foreach (char c in text)
        {
            introText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

}