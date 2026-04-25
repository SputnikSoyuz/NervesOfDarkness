using UnityEngine;
using NewHorizons.Utility;

namespace NervesOfDarkness;

public class LowerLayerPuzzleTrigger : MonoBehaviour
{
    [SerializeField]
    private SphereCollider _volumeToDisable;

    private OWRigidbody _planetRigidbody;

    public bool isInTrigger;

    public void Start()
    {
        isInTrigger = false;
        _planetRigidbody = SearchUtilities.Find("BlowingBehemoth_Body").GetComponent<OWRigidbody>();
    }

    public void Update()
    {
        if (isInTrigger && NervesOfDarkness.Instance.IsAccelerationWithinRange(1, 10, Locator.GetPlayerBody(), _planetRigidbody))
        {
            if (_volumeToDisable.enabled)
            {
                NervesOfDarkness.WriteLine("Gravity Volume Disabled.", OWML.Common.MessageType.Success);
                _volumeToDisable.enabled = false;
            }
        } 
        else
        {
            if (!_volumeToDisable.enabled)
            {
                NervesOfDarkness.WriteLine("Gravity Volume Enabled.", OWML.Common.MessageType.Success);
                _volumeToDisable.enabled = true;
            }
        }
    }

    public virtual void OnTriggerEnter(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            NervesOfDarkness.WriteLine("TriggerEntered", OWML.Common.MessageType.Success);
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