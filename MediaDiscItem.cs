using NewHorizons.Utility.Files;
using UnityEngine;

namespace NervesOfDarkness;

public class MediaDiscItem : OWItem
{
    private Animator _animator;

    [SerializeField]
    private OWAudioSource _audio;
    [SerializeField]
    private SphereCollider _collider;

    [SerializeField]
    private string _fileName;

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
        _collider.enabled = false;
        _animator = this.transform.parent.GetComponent<Animator>();
        _animator.Play("NoD_DriveSocketIn", 0);
    }

    public override void PlayUnsocketAnimation()
    {
        _collider.enabled = true;
        _animator.Play("NoD_DriveSocketOut", 0);
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
        //_animator.ResetToOriginalPositionRotation();
    }

    public override string GetDisplayName()
    {
        return UITextLibrary.GetString(UITextType.ItemScrollPrompt);
    }
}
