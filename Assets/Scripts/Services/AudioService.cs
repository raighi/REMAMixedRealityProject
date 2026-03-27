using UnityEngine;
using UnityEngine.Audio;

public class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    [SerializeField] private AudioConfig config;
    [SerializeField] private AudioMixerGroup ambianceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource ambianceSource;
    private bool isVR = true; // À lier à ton système de détection VR/AR

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAmbianceSource();
    }

    private void SetupAmbianceSource()
    {
        ambianceSource = gameObject.AddComponent<AudioSource>();
        ambianceSource.outputAudioMixerGroup = ambianceGroup;
        ambianceSource.loop = true;
        ambianceSource.spatialBlend = 0f; // L'ambiance reste toujours en 2D (globale)
        ambianceSource.clip = config.dungeonAmbientLoop;
        ambianceSource.Play();
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        // Création d'un GameObject temporaire pour le son
        GameObject sfxObject = new GameObject("SFX_" + clip.name);
        sfxObject.transform.position = position;

        AudioSource source = sfxObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = sfxGroup;
        
        // Gestion VR vs AR
        source.spatialBlend = isVR ? 1f : 0f;
        
        source.Play();

        // Destruction automatique après la fin du clip
        Destroy(sfxObject, clip.length);
    }
}