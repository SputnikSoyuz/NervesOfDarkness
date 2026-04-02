using NewHorizons.Utility.Files;
using UnityEngine;

namespace NervesOfDarkness;

public class Recorder : MonoBehaviour
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
    private Transform _attentionPoint;

    [SerializeField]
    private InteractReceiver _interactReceiver;

    [SerializeField]
    private string _fileName;

    [SerializeField]
    private Vector3 _attentionPointOffset = Vector3.zero;

    [SerializeField]
    private bool _turnOffFlashlight = true;

    [SerializeField]
    private bool _turnOnFlashlight = true;

    private bool _wasFlashlightOn;

    public bool isListening;

    public void OnValidate()
    {
        if (_turnOnFlashlight && !_turnOffFlashlight)
        {
            _turnOnFlashlight = false;
        }
    }

    public void Awake()
    {
        isListening = false;
        _attentionPoint = ((_attentionPoint == null) ? base.transform : _attentionPoint);
        _interactReceiver.OnPressInteract += OnPressInteract;
        GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
        base.enabled = false;
    }

    public void Start()
    {
        NervesOfDarkness.Instance.NewHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
        AudioUtilities.SetAudioClip(_audioSource, "assets/Audio/Recordings/"+_fileName, NervesOfDarkness.Instance); // sets audio clip
        _audioSource.Play();
        _audioSource.Stop();
    }

    public void OnDestroy()
    {
        GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
    }

    public void OnStarSystemLoaded(string system)
    {
        if (system == "SputnikSoyuz.SalvagedStardust")
        {
            _interactReceiver.SetPromptText(UITextType.RecordingPrompt);
        }
    }

    public void Update()
    {
        if (isListening)
        {
            if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) ||
            OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
            {
                ButtonPress();
                StopListening();
            } else if (!_audioSource.isPlaying && isListening)
            {
                StopListening();
            }
        }
    }

    private void OnPressInteract()
    {
        ButtonPress();
        StartListening();
    }

    private void ButtonPress()
    {
        _buttonAnimator.Play("NoD_Recorder_Button_Press", 0);
        _buttonAudioSource?.PlayOneShot(global::AudioType.TapeRecorder_Stop, 1f);
    }

    private void StartListening()
    {
        OWInput.ChangeInputMode(InputMode.None);
        base.enabled = true;
        Locator.GetToolModeSwapper().UnequipTool();
        _wasFlashlightOn = Locator.GetFlashlight().IsFlashlightOn();
        if (_wasFlashlightOn && _turnOffFlashlight)
        {
            Locator.GetFlashlight().TurnOff(playAudio: false);
        }
        if (_attentionPoint != null && !PlayerState.InZeroG())
        {
            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, 2f);
        }
        if (PlayerState.InZeroG())
        {
            Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody().GetReferenceFrame());
        }
        if (!isListening)
        {
            _audioSource.Play();
            _animator.Play("NoD_Recorder_Animated");
            isListening = true;
        }
    }

    private void StopListening()
    {
        OWInput.ChangeInputMode(InputMode.Character);
        if (!base.enabled)
        {
            return;
        }
        base.enabled = false;
        _interactReceiver.ResetInteraction();
        Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
        if (_wasFlashlightOn && _turnOffFlashlight && _turnOnFlashlight)
        {
            Locator.GetFlashlight().TurnOn(playAudio: false);
        }

        if (PlayerState.InZeroG())
        {
            Autopilot component = Locator.GetPlayerBody().GetComponent<Autopilot>();
            if (component.enabled)
            {
                component.Abort();
            }
        }
        if (isListening)
        {
            _audioSource.Stop();
            _animator.Play("NoD_Recorder_Static", 0);
            isListening = false;
        }
    }

    private void OnPlayerDeath(DeathType deathType)
    {
        if (base.enabled)
        {
            StopListening();
        }
    }
}