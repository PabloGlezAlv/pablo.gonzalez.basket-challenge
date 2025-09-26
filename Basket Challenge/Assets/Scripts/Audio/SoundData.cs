using UnityEngine;

public enum SoundType
{
    SFX_BallShoot,
    SFX_BallBounce,
    SFX_Score,
    SFX_Miss,
    SFX_UIClick,
    SFX_GameStart,
    SFX_GameEnd,
    Music_Background
}

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public SoundType soundType;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}