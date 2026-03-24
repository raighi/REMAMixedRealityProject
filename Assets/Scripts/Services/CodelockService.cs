/// <summary>
/// Service pur : validation du code.
/// Aucune dépendance Unity, testable unitairement.
/// </summary>
public class CodeLockService
{
    private readonly CodeLockConfig _config;

    public CodeLockService(CodeLockConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Vérifie une tentative de code.
    /// </summary>
    public CodeAttemptResult ValidateCode(string attempt, bool alreadyUnlocked)
    {
        if (alreadyUnlocked)
            return CodeAttemptResult.AlreadyUnlocked;

        if (attempt.Length != _config.EffectiveCodeLength)
            return CodeAttemptResult.WrongLength;

        if (attempt == _config.CorrectCode)
            return CodeAttemptResult.Correct;

        return CodeAttemptResult.Incorrect;
    }
}