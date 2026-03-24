using UnityEngine;

public class EndingBootstrapper : MonoBehaviour
{
    private EndingController endingController;
    private TextEndingModel textEndingModel;

    [Header("Text Data")]
    public TextSequence textSequence;

    [Header("View")]
    public BackgroundEndingView backgroundEndingView;

    [Header("Interaction")]
    public EndingInteractions endingInteractions;

    private void Start()
    {
        textEndingModel = new TextEndingModel(textSequence);
        endingController = new EndingController(textEndingModel, backgroundEndingView);
        endingInteractions.Initialize(endingController);
    }
}