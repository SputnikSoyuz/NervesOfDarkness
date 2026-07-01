using UnityEngine;

namespace NervesOfDarkness;

public class SpeedCheckTrigger : MonoBehaviour
{
    [SerializeField]
    public GameObject _volumeToDisable;
    [SerializeField]
    public float minSpeed;
    [SerializeField]
    public float maxSpeed;

    private OWRigidbody _planetRigidbody;

    public bool isInTrigger;

    public void Start()
    {
        isInTrigger = false;
        _planetRigidbody = this.GetAttachedOWRigidbody();
        NervesOfDarkness.WriteLine("Speed Check Rigidbody: " + _planetRigidbody.name);
    }

    public void Update()
    {
        if (_volumeToDisable != null)
        {
            if (isInTrigger && CheckSpeedLimit(minSpeed, maxSpeed, Locator.GetPlayerBody(), _planetRigidbody))
            {
                if (_volumeToDisable.activeSelf)
                {
                    _volumeToDisable.SetActive(false);
                }
            }
            else
            {
                if (!_volumeToDisable.activeSelf)
                {
                    _volumeToDisable.SetActive(true);
                }
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
    public bool CheckSpeedLimit(float min, float max, OWRigidbody _activeBody, OWRigidbody _currentReferenceFrame)
    {
        Vector3 relativeVelocity = -_activeBody.GetRelativeVelocity(_currentReferenceFrame);
        Vector3 direction = (_currentReferenceFrame.GetPosition() - _activeBody.GetPosition()).normalized;
        float target = Mathf.Abs(Vector3.Dot(relativeVelocity, direction));

        if (target < max && target > min)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}