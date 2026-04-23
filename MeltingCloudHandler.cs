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

    private OWItem _frozenItemInteract;

    public bool isInTrigger;

    public bool hasMelted;

    public bool hasStartedMelting;

    public void Start()
    {
        hasMelted = false;
        hasStartedMelting = false;
        isInTrigger = false;

        if (_frozenItem != null)
        {
            _frozenItemInteract = _frozenItem.GetComponent<OWItem>();
            _frozenItemInteract.EnableInteraction(false);
        }
    }

    public void Update()
    {
        var startMelting = TimeLoop.GetSecondsElapsed() > 670;
        var midpointDuration = TimeLoop.GetSecondsElapsed() > 674;
        var endDuration = TimeLoop.GetSecondsElapsed() > 676;

        if (isInTrigger && !Locator.GetPlayerTransform().gameObject.GetComponent<JetpackThrusterModel>().IsTranslationalThrusterFiring() && TimeLoop.GetSecondsElapsed() < 670 && _audio.isPlaying)
        {
            _audio?.Stop();
        }

        if (startMelting && !hasStartedMelting)
        {
            _animator.Play("NoD_CloudMelt", 0);
            _audio.Play();

            hasStartedMelting = true;
            if (_frozenItem != null && !_frozenItemInteract.IsInteractable())
            {
                _frozenItemInteract.EnableInteraction(true);
            }
        }

        if (midpointDuration)
        {
            //ItemStartFalling(_frozenItem.transform, 100);
        }

        if (endDuration)
        {
            _audio?.Stop();
            hasMelted = true;
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