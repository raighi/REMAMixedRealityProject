using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Config")]
public class AudioConfig : ScriptableObject
{
    [Header("Ambiance")]
    public AudioClip dungeonAmbientLoop;

    [Header("Interactions")]
    public AudioClip grabSound;
    public AudioClip dropSound;
    public AudioClip doorOpenSound;

    [Header("Keypad")]
    public AudioClip keypadBeep;
    public AudioClip keypadValid;
    public AudioClip keypadError;
}