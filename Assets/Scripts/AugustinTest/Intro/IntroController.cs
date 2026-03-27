using UnityEngine;
/*
public class IntroController
{
    private BackgroundIntroView backgroundIntroView;
    private TextIntroModel textIntroModel;

    public IntroController(TextIntroModel model, BackgroundIntroView view)
    {
        textIntroModel = model;
        backgroundIntroView = view;

        RefreshView();
    }

    private void RefreshView()
    {
        backgroundIntroView.TypeText(textIntroModel.GetCurrentText());

        if (textIntroModel.CanGoPreviousText())
            backgroundIntroView.EnablePreviousButton();
        else
            backgroundIntroView.DisablePreviousButton();

        if (textIntroModel.CanGoNextText())
            backgroundIntroView.EnableNextButton();
        else
            backgroundIntroView.SetNextButtonForConnection();
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
            Debug.Log("Fin de l’introduction → lancer la suite du jeu");
        }
    }

    public void GoBackIntro()
    {
        if (textIntroModel.CanGoPreviousText())
        {
            textIntroModel.GoPreviousText();
            RefreshView();
        }
    }
}*/