using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponent<Slider>();

        InitializeSlider();
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void InitializeSlider()
    {
        volumeSlider.value = AudioManager.Instance.GetMasterVolume();
    }

    private void OnVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }
}