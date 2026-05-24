using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Configs/PlayerSoundsConfig")]
public class PlayerSoundsConfig : ScriptableObject
{
    [SerializeField] private AudioClip[] _defaultSounds;
    public FootstepSounds[] FootstepSounds;
    public AudioClip SlidingSound;

    private Dictionary<string, AudioClip[]> _surfaceSoundMap;

    private void OnEnable()
    {
        _surfaceSoundMap = new Dictionary<string, AudioClip[]>();

        foreach(var footstepSound in FootstepSounds)
        {
            if (string.IsNullOrWhiteSpace(footstepSound.SurfaceName)) continue;
            if (footstepSound.Clips == null || footstepSound.Clips.Length == 0) continue;

            _surfaceSoundMap[footstepSound.SurfaceName.Trim()] = footstepSound.Clips;
        }
    }

    public bool TryGetSurfaceSounds(string surfaceName, out AudioClip[] clips)
    {
        if (!string.IsNullOrWhiteSpace(surfaceName) &&
            _surfaceSoundMap != null &&
            _surfaceSoundMap.TryGetValue(surfaceName.Trim(), out clips) &&
            clips != null && clips.Length > 0)
        {
            return true;
        }

        clips = _defaultSounds;
        return clips != null && clips.Length > 0;
    }
}

[Serializable]
public struct FootstepSounds
{
    public string SurfaceName;
    public AudioClip[] Clips;
}
