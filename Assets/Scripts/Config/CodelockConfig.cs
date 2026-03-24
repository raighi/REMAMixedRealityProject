using UnityEngine;

/// <summary>
/// Configuration d'un verrou à code.
/// Créer via Assets > Create > Dungeon > CodeLock Config.
/// </summary>
[CreateAssetMenu(fileName = "CodeLockConfig", menuName = "Dungeon/CodeLock Config")]
public class CodeLockConfig : ScriptableObject
{
    [Header("Code")]
    [Tooltip("Le code correct (ex: 427)")]
    public string CorrectCode = "427";

    [Tooltip("Nombre de digits attendus (déduit automatiquement de CorrectCode si 0)")]
    public int CodeLength = 0;

    [Header("Interaction")]
    [Tooltip("Distance max pour voir le bouton d'interaction")]
    public float InteractionDistance = 3f;

    [Header("Feedback")]
    [Tooltip("Durée d'affichage du message de succès/échec (secondes)")]
    public float FeedbackDuration = 1.5f;

    /// <summary>
    /// Retourne la longueur effective du code.
    /// </summary>
    public int EffectiveCodeLength => CodeLength > 0 ? CodeLength : CorrectCode.Length;
}