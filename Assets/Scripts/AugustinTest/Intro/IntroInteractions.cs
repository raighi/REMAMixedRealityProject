using UnityEngine;
using UnityEngine.UI;

public class IntroInteractions : MonoBehaviour
{
    [Header("UI Component")]
    public Button previousButton;
    public Button nextButton;

    [Header("App Controller")]
    private IntroController introController;

    public void Initialize(IntroController controller)
    {
        introController = controller;
    }

    private void OnEnable()
    {
        previousButton.onClick.AddListener(OnPreviousPressed);
        nextButton.onClick.AddListener(OnNextPressed);
    }

    private void OnDisable()
    {
        previousButton.onClick.RemoveListener(OnPreviousPressed);
        nextButton.onClick.RemoveListener(OnNextPressed);
    }

    private void OnNextPressed()
    {
        introController.ProceedIntro();
    }

    private void OnPreviousPressed()
    {
        introController.GoBackIntro();
    }
}