using System.Linq.Expressions;
using UnityEngine;

namespace NervesOfDarkness;

public class MeltingCloudHandler : MonoBehaviour
{
    [SerializeField]
    private OWAudioSource _audio;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private GameObject _frozenItem;

    [SerializeField]
    private GameObject _frozenRecord;

    [SerializeField]
    private Transform _frozenCloud;

    [SerializeField]
    private Transform _itemTransform;

    [SerializeField]
    private Transform _campfireRoot;

    private NoDCampfire _campfire;

    public bool isCampfireCloud;

    public bool isInTrigger;

    public bool isItemInteractible;

    public bool hasMelted;

    public bool hasStartedMelting;

    private bool _hasBeenInit = false; // THIS HAS BEEN ADDED

    public void Start()
    {
        hasMelted = false;
        hasStartedMelting = false;
        isInTrigger = false;
        if (_frozenItem != null && _frozenItem.activeSelf)
        {
            ToggleInteraction(false);
        }
        else
        {
            ToggleInteraction(true);
        }
        _campfire = _campfireRoot.GetComponentInChildren<NoDCampfire>();
        if (_campfire != null)
        {
            isCampfireCloud = true;
        }
        else
        {
            isCampfireCloud = false;
        }

        _hasBeenInit = true; // THIS HAS BEEN ADDED
    }

    public void ToggleInteraction(bool boolToSet)
    {
        if (!_hasBeenInit || isItemInteractible != boolToSet) // THIS HAS BEEN CHANGED
        {
            if (_frozenRecord != null && _frozenRecord.activeSelf)
            {
                InteractReceiver _frozenItemInteract = _frozenRecord.GetComponent<InteractReceiver>();
                if (boolToSet)
                {
                    _frozenItemInteract.EnableInteraction();
                }
                else
                {
                    _frozenItemInteract.DisableInteraction();
                }
            }

            if (_frozenItem != null && _frozenItem.activeSelf)
            {
                OWItem _frozenItemInteract = _frozenItem.GetComponent<OWItem>();
                _frozenItemInteract.EnableInteraction(boolToSet);
            }
            isItemInteractible = boolToSet;
        }
    }

    public void Update()
    {
        if (!hasMelted && TimeLoop.GetSecondsElapsed() < 670)
        {
            if (isCampfireCloud && _campfire.GetState() == NoDCampfire.State.LIT)
            {
                StartMelt(0.01929f); //0.01929 should mean it finishes melting in 5 mins and 11 seconds.
            } else if (isInTrigger && Locator.GetPlayerTransform().gameObject.GetComponent<JetpackThrusterModel>().IsTranslationalThrusterFiring())
            {
                StartMelt(0.1f);
            }
            else if (isInTrigger && Locator.GetShipTransform().gameObject.GetComponent<ShipThrusterModel>().IsTranslationalThrusterFiring())
            {
                StartMelt(0.2f);
            } else
            {
                PauseMelt();
            }
        } else if (TimeLoop.GetSecondsElapsed() > 670)
        {
            StartMelt(1f);
        }

        if (_frozenCloud.localScale.y <= 0.0146844f && _frozenCloud.localScale.y > 0 &&
            _frozenItem != null && _frozenItem.activeSelf)
        {
            ToggleInteraction(true);
        }

        if (_frozenCloud.localScale.Equals(Vector3.zero))
        {
            _animator.speed = 1f;
            _audio?.Stop();
            hasMelted = true;
            if (_frozenItem != null && _frozenItem.activeSelf)
            {
                ToggleInteraction(false);
            }
        }

        if (_itemTransform != null && _itemTransform.localPosition.y < -125.189f)
        {
            Destroy(_itemTransform.gameObject);
        }
    }

    public void StartMelt(float speed)
    {
        _animator.speed = speed;
        if (!hasStartedMelting)
        {
            _animator.Play("NoD_CloudMelt", 0);
            hasStartedMelting = true;
        }
        if (!_audio.isPlaying)
        {
            _audio?.Play();
        }
    }

    public void PauseMelt()
    {
        _animator.speed = 0f;
        if (_audio.isPlaying)
        {
            _audio?.Stop();
        }
    }

    public float GetCurrentPlaybackLength()
    {
        int layer = 0;
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(layer);
        float normalizedTime = state.normalizedTime; // cycles (1.3 = 30% into 2nd loop)
        AnimatorClipInfo[] clips = _animator.GetCurrentAnimatorClipInfo(layer);
        if (clips.Length > 0)
        {
            AnimationClip clip = clips[0].clip;
            float clipLength = clip.length;
            return (state.normalizedTime % 1f) * clipLength;
        } else
        {
            NervesOfDarkness.WriteLine("WARNING in MeltingCloudHandler.cs: Clip Array Length is not greater than 0! GetCurrentPlaybackLength() may not be functioning properly." +
                                        "\nArray Length: " + clips.Length,OWML.Common.MessageType.Warning);
            return 0;
        }
    }

    public virtual void OnTriggerEnter(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            isInTrigger = true;
        }
    }

    public virtual void OnTriggerExit(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            isInTrigger = false;
        }
    }
}