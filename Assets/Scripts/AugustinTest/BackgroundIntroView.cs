using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BackgroundIntroView : MonoBehaviour
{
    public float typingSpeed = 0.02f;
    
    [Header("UI Text")]
    public TMP_Text introText;

    [Header("UI Previous Button")]
    public Button previousButton;
    public TMP_Text previousButtonText;

    [Header("UI Next Button")]
    public Button nextButton;
    public TMP_Text nextButtonText;

    private Color activeColor = Color.white;
    private Color inactiveColor = Color.gray;
    private Color connectColor = Color.red;
    private int offsetspace = 3;

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
    
    // ─────────────────────────────────────────────
    // UI Buttons
    // ─────────────────────────────────────────────

    public void EnableNextButton()
    {
        if (nextButton != null)
        {
            nextButton.image.color = activeColor;
            nextButtonText.text = new string(' ', offsetspace) + "Next";
        }
        else
        {
            Debug.LogWarning("NextButton n'est pas assigné !");
        }
    }

    public void SetNextButtonForConnection()
    {
        if (nextButton != null)
        {
            nextButton.image.color = connectColor;
            nextButtonText.text = new string(' ', offsetspace) + "Connect";
        }
        else
        {
            Debug.LogWarning("NextButton n'est pas assigné !");
        }
    }

    public void EnablePreviousButton()
    {
        if (previousButton != null)
        {
            previousButton.image.color = activeColor;
            previousButtonText.text = new string(' ', offsetspace) + "Previous";
        }
        else
        {
            Debug.LogWarning("PreviousButton n'est pas assigné !");
        }
    }

    public void DisablePreviousButton()
    {
        if (previousButton != null)
        {
            previousButton.image.color = inactiveColor;
            previousButtonText.text = "";
        }
        else
        {
            Debug.LogWarning("PreviousButton n'est pas assigné !");
        }
    }
}