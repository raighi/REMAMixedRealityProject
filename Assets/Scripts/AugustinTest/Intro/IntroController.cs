using UnityEngine;

public class IntroController
{
    private BackgroundIntroView backgroundIntroView;
    private TextIntroModel textIntroModel;
    private NewUniversalConnectionManager universalConnectionManager;

    public IntroController(TextIntroModel model, BackgroundIntroView view, NewUniversalConnectionManager connectionManager)
    {
        textIntroModel = model;
        backgroundIntroView = view;
        universalConnectionManager = connectionManager;

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
        else if (!textIntroModel.IsConnected())
        {
            universalConnectionManager.Connect();
            Debug.Log("Fin de l’introduction → lancement de la suite du jeu");
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
}