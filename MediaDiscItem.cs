using NewHorizons.Utility.Files;
using UnityEngine;

namespace NervesOfDarkness;

public class MediaDiscItem : OWItem
{
    [SerializeField]
    private TransformAnimator _animator;

    [SerializeField]
    private OWAudioSource _audio;

    [SerializeField]
    private string _fileName;

    private const float _animDuration = 0.9f;

    private const float _animDegrees = -105f;

    private const float _animOffsetZ = 0.8f;

    public override void Awake()
    {
        _type = ItemType.Scroll;
        base.Awake();
    }

    public void Start()
    {
        AudioUtilities.SetAudioClip(_audio, "assets/Audio/Recordings/"+_fileName, NervesOfDarkness.Instance); // sets audio clip
    }

    public override void PlaySocketAnimation()
    {
        _animator.transform.localPosition = Vector3.forward * 0.8f;
        _animator.TranslateToOriginalLocalPosition(0.9f);
        _animator.transform.localEulerAngles = Vector3.forward * -105f;
        _animator.RotateToOriginalLocalRotation(0.9f);
    }

    public override void PlayUnsocketAnimation()
    {
        _animator.TranslateToLocalPosition(Vector3.forward * 0.8f, 0.9f);
        _animator.RotateToLocalEulerAngles(Vector3.forward * -105f, 0.9f);
    }

    public void PlayAudio()
    {
        _audio.Play();
    }

    public void StopAudio()
    {
        _audio.Stop();
    }

    public override void OnCompleteUnsocket()
    {
        _animator.ResetToOriginalPositionRotation();
    }

    public override string GetDisplayName()
    {
        return UITextLibrary.GetString(UITextType.ItemScrollPrompt);
    }
}
