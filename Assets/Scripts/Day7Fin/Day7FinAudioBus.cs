using UnityEngine;

/// <summary>
/// Shared static helpers to control chase music and other audio cues unique to day7_fin.
/// </summary>
public static class Day7FinAudioBus
{
    private static AudioSource cachedChaseSource;

    public static void RegisterChaseSource(AudioSource source)
    {
        cachedChaseSource = source;
    }

    public static void StopChaseMusic()
    {
        if (cachedChaseSource != null && cachedChaseSource.isPlaying)
        {
            cachedChaseSource.Stop();
        }
    }
}

