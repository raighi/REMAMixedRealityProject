using UnityEngine;

public class IntroBootstrapper : MonoBehaviour
{
    private IntroController introController;
    private TextIntroModel textIntroModel;

    [Header("Text Data")]
    public TextSequence textSequence;

    [Header("View")]
    public BackgroundIntroView backgroundIntroView;

    [Header("Interaction")]
    public IntroInteractions introInteractions;

    private void Start()
    {
        textIntroModel = new TextIntroModel(textSequence);
        introController = new IntroController(textIntroModel, backgroundIntroView);
        introInteractions.Initialize(introController);
    }
}