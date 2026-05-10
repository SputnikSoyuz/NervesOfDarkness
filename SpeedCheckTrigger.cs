using UnityEngine;
using NewHorizons.Utility;

namespace NervesOfDarkness;

public class SpeedCheckTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject _volumeToDisable;
    [SerializeField]
    private float minSpeed;
    [SerializeField]
    private float maxSpeed;

    private OWRigidbody _planetRigidbody;

    public bool isInTrigger;

    public void Start()
    {
        isInTrigger = false;
        _planetRigidbody = SearchUtilities.Find("BlowingBehemoth_Body").GetComponent<OWRigidbody>();
    }

    public void Update()
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