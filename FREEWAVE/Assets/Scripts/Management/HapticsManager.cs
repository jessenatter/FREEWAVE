using UnityEngine;
using UnityEngine.InputSystem;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance { get; private set; }

    Coroutine activePulseRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDisable()
    {
        StopAllHaptics();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            PauseHaptics();
        else
            ResumeHaptics();
    }

    void OnApplicationQuit()
    {
        StopAllHaptics();
    }

    public static void Play(float lowFrequency, float highFrequency, float duration)
    {
        EnsureInstance();
        Instance.PlayInternal(lowFrequency, highFrequency, duration);
    }

    public static void PlayLight(float duration = 0.08f)
    {
        Play(0.2f, 0.35f, duration);
    }

    public static void PlayMedium(float duration = 0.12f)
    {
        Play(0.45f, 0.55f, duration);
    }

    public static void PlayHeavy(float duration = 0.18f)
    {
        Play(0.8f, 0.9f, duration);
    }

    public static void Stop()
    {
        EnsureInstance();
        Instance.StopAllHaptics();
    }

    public static void PauseHaptics()
    {
        if (Gamepad.current == null)
            return;

        Gamepad.current.PauseHaptics();
    }

    public static void ResumeHaptics()
    {
        if (Gamepad.current == null)
            return;

        Gamepad.current.ResumeHaptics();
    }

    static void EnsureInstance()
    {
        if (Instance != null)
            return;

        Instance = FindFirstObjectByType<HapticsManager>();
        if (Instance != null)
            return;

        GameObject hapticsObject = new GameObject(nameof(HapticsManager));
        Instance = hapticsObject.AddComponent<HapticsManager>();
    }

    void PlayInternal(float lowFrequency, float highFrequency, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        float low = Mathf.Clamp01(lowFrequency);
        float high = Mathf.Clamp01(highFrequency);
        float safeDuration = Mathf.Max(0f, duration);

        if (activePulseRoutine != null)
            StopCoroutine(activePulseRoutine);

        gamepad.SetMotorSpeeds(low, high);

        if (safeDuration <= 0f)
            return;

        activePulseRoutine = StartCoroutine(StopAfterDuration(safeDuration));
    }

    System.Collections.IEnumerator StopAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopAllHaptics();
    }

    void StopAllHaptics()
    {
        if (activePulseRoutine != null)
        {
            StopCoroutine(activePulseRoutine);
            activePulseRoutine = null;
        }

        if (Gamepad.current != null)
            Gamepad.current.ResetHaptics();
    }
}
