using UnityEngine;

public class EndingController
{
    private BackgroundEndingView backgroundIntroView;
    private TextEndingModel textIntroModel;

    public EndingController(TextEndingModel model, BackgroundEndingView view)
    {
        textIntroModel = model;
        backgroundIntroView = view;

        RefreshView();
    }

    private void RefreshView()
    {
        backgroundIntroView.TypeText(textIntroModel.GetCurrentText());
    }

    public void ProceedIntro()
    {
        if (textIntroModel.CanGoNextText())
        {
            textIntroModel.GoNextText();
            RefreshView();
        }
        else
        {
            Debug.Log("Fin du jeu");
        }
    }
}