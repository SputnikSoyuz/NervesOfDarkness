using NewHorizons.Utility.Files;
using UnityEngine;

namespace NervesOfDarkness;

public class TheBox : MonoBehaviour
{
    [SerializeField]
    private OWAudioSource _audioSource;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private OWAudioSource _buttonAudioSource;

    [SerializeField]
    private Animator _buttonAnimator;

    [SerializeField]
    private InteractReceiver _interactReceiver;

    private bool isDoorOpen;

    public void Awake()
    {
        base.enabled = false;
        _interactReceiver.OnPressInteract += OnPressInteract;
    }

    public void Start()
    {
        NervesOfDarkness.Instance.NewHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
        isDoorOpen = true;
    }

    public void OnStarSystemLoaded(string system)
    {
        if (system == "SputnikSoyuz.SalvagedStardust")
        {
            _interactReceiver.SetPromptText(UITextType.PressPrompt);
        }
    }
    private void OnPressInteract()
    {
        ButtonPress();
        if (isDoorOpen)
        {
            _animator.Play("boxdoor", 0);
            isDoorOpen = false;
        } else
        {
            _animator.Play("boxdooropen", 0);
            isDoorOpen = true;
        }
        _audioSource?.PlayOneShot(global::AudioType.SecretPassage_Loop, 1f);
    }

    private void ButtonPress()
    {
        _buttonAnimator.Play("boxbutton", 0);
        _buttonAudioSource?.PlayOneShot(global::AudioType.NonDiaUIAffirmativeSFX, 1f);
    }
}