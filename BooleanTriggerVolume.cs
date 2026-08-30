using UnityEngine;

namespace NervesOfDarkness;

public class BooleanTriggerVolume : MonoBehaviour
{
    [SerializeField]
    private int triggerID;
    private bool isInTrigger = false;

    public bool GetTriggerStatus()
    {
        return isInTrigger;
    }

    public int GetTriggerID()
    {
        return triggerID;
    }

    public void SetTriggerID(int _triggerID)
    {
        triggerID = _triggerID;
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