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
    private Vector3 _attentionPointOffset = Vector3.zero;

    [SerializeField]
    private bool _turnOffFlashlight = true;

    [SerializeField]
    private bool _turnOnFlashlight = true;

    private bool _wasFlashlightOn;

    private bool _timeFrozen;

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
        AudioUtilities.SetAudioClip(_audioSource, "assets/Audio/Recordings/test1.wav", NervesOfDarkness.Instance); // sets audio clip
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
        if (!(_audioSource != null) || OWInput.GetInputMode() != InputMode.Dialogue)
        {
            return;
        }
        if (isListening)
        {
            if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) ||
            OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
            {
                NervesOfDarkness.WriteLine("Audio forced stop by player.", OWML.Common.MessageType.Success);
                ButtonPress();
                StopListening();
            }
        }
    }

    private void OnPressInteract()
    {
        NervesOfDarkness.WriteLine("Tried to interact.", OWML.Common.MessageType.Success);
        ButtonPress();
        StartListening();
    }

    private void ButtonPress()
    {
        NervesOfDarkness.WriteLine("Tried to press button.", OWML.Common.MessageType.Success);
        _buttonAnimator.Play("NoD_Recorder_Button_Press", 0);
        _buttonAudioSource?.PlayOneShot(global::AudioType.TapeRecorder_Stop, 1f);
    }

    private void StartListening()
    {
        NervesOfDarkness.WriteLine("Start Listening.", OWML.Common.MessageType.Success);
        OWInput.ChangeInputMode(InputMode.None);
        base.enabled = true;
        NervesOfDarkness.WriteLine("Player can no longer move.", OWML.Common.MessageType.Success);
        if (!_timeFrozen && PlayerData.GetFreezeTimeWhileReadingConversations() && !Locator.GetGlobalMusicController().IsEndTimesPlaying())
        {
            NervesOfDarkness.WriteLine("Time is Frozen", OWML.Common.MessageType.Success);
            _timeFrozen = true;
            OWTime.Pause(OWTime.PauseType.Reading);
        }
        Locator.GetToolModeSwapper().UnequipTool();
        NervesOfDarkness.WriteLine("Item unequipped", OWML.Common.MessageType.Success);
        /*(if (this.StartListening != null)
        {
            this.StartListening();
        }*/
        _wasFlashlightOn = Locator.GetFlashlight().IsFlashlightOn();
        if (_wasFlashlightOn && _turnOffFlashlight)
        {
            Locator.GetFlashlight().TurnOff(playAudio: false);
        }
        NervesOfDarkness.WriteLine("Flashlight Handled.", OWML.Common.MessageType.Success);
        if (_attentionPoint != null && !PlayerState.InZeroG())
        {
            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, 2f);
            NervesOfDarkness.WriteLine("Locked on!", OWML.Common.MessageType.Success);
        }
        if (PlayerState.InZeroG() && !_timeFrozen)
        {
            Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody().GetReferenceFrame());
            NervesOfDarkness.WriteLine("Locked on! (Zero G)", OWML.Common.MessageType.Success);
        }
        if (!isListening)
        {
            NervesOfDarkness.WriteLine("Start the listening!", OWML.Common.MessageType.Success);
            _audioSource.Play();
            _animator.Play("NoD_Recorder_Animated", 0);
            isListening = true;
        }
    }

    private void StopListening()
    {
        NervesOfDarkness.WriteLine("Stopping the Audio...", OWML.Common.MessageType.Success);
        OWInput.ChangeInputMode(InputMode.Character);
        if (!base.enabled)
        {
            return;
        }
        base.enabled = false;
        if (_timeFrozen)
        {
            _timeFrozen = false;
            OWTime.Unpause(OWTime.PauseType.Reading);
        }
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