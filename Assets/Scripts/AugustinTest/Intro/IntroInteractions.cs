using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class IntroInteractions : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Input Interactions")]
    [SerializeField] private InputActionReference previousInputAction;
    [SerializeField] private InputActionReference nextInputAction;

    private IntroController introController;

    public void Initialize(IntroController controller)
    {
        introController = controller;
    }

    private void OnEnable()
    {
        // UI Buttons
        previousButton.onClick.AddListener(OnPreviousPressed);
        nextButton.onClick.AddListener(OnNextPressed);

        // Input Actions
        previousInputAction.action.performed += OnPreviousAction;
        nextInputAction.action.performed += OnNextAction;

        previousInputAction.action.Enable();
        nextInputAction.action.Enable();
    }

    private void OnDisable()
    {
        // UI Buttons
        previousButton.onClick.RemoveListener(OnPreviousPressed);
        nextButton.onClick.RemoveListener(OnNextPressed);

        // Input Actions
        previousInputAction.action.performed -= OnPreviousAction;
        nextInputAction.action.performed -= OnNextAction;

        previousInputAction.action.Disable();
        nextInputAction.action.Disable();
    }

    private void OnNextPressed() => introController.ProceedIntro();
    private void OnPreviousPressed() => introController.GoBackIntro();

    private void OnNextAction(InputAction.CallbackContext ctx) => introController.ProceedIntro();
    private void OnPreviousAction(InputAction.CallbackContext ctx) => introController.GoBackIntro();
}