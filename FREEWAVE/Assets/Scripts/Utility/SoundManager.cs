using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlaySound(string name, float volume, float pitchRange = 0f)
    {
        EnsureInstance();
        Instance.PlaySoundInternal(name, volume, pitchRange);
    }

    public static void PlaySound(float volume, float pitchRange = 0f, params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            Debug.LogWarning("SoundManager.PlaySound was called with no sound names.");
            return;
        }

        EnsureInstance();

        string selectedName = names.Length == 1
            ? names[0]
            : names[Random.Range(0, names.Length)];

        Instance.PlaySoundInternal(selectedName, volume, pitchRange);
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        Instance = FindFirstObjectByType<SoundManager>();

        if (Instance != null)
        {
            return;
        }

        GameObject soundManagerObject = new GameObject(nameof(SoundManager));
        Instance = soundManagerObject.AddComponent<SoundManager>();
    }

    private void PlaySoundInternal(string name, float volume, float pitchRange)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/{name}");

        if (clip == null)
        {
            Debug.LogWarning($"Sound clip not found at Resources/Sounds/{name}");
            return;
        }

        float clampedPitchRange = Mathf.Max(0f, pitchRange);
        float pitch = clampedPitchRange > 0f
            ? Random.Range(1f - clampedPitchRange, 1f + clampedPitchRange)
            : 1f;

        GameObject soundObject = new GameObject($"Sound_{name}");
        soundObject.transform.position = transform.position;

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.pitch = pitch;
        audioSource.spatialBlend = 0f;

        audioSource.Play();

        float pitchSafe = Mathf.Max(0.01f, Mathf.Abs(pitch));
        Destroy(soundObject, clip.length / pitchSafe + 0.1f);
    }
}
