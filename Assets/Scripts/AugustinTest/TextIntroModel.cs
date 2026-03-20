
[System.Serializable]
public class TextIntroModel
{
    private TextSequence textSequence;
    private int currentIndex = 0;

    public TextIntroModel(TextSequence textSequence)
    {
        this.textSequence = textSequence;
    }

    public string GetCurrentText()
    {
        return textSequence.messages[currentIndex];
    }

    public bool CanGoPreviousText()
    {
        return currentIndex > 0;
    }

    public bool CanGoNextText()
    {
        return currentIndex < textSequence.messages.Length -1;
    }

    public void GoPreviousText()
    {
        currentIndex -= 1;
    }

    public void GoNextText()
    {
        currentIndex += 1;
    }
}