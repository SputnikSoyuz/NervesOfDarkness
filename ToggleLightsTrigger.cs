using UnityEngine;
using System.Collections.Generic;

namespace NervesOfDarkness;

public class ToggleLightsTrigger : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectsToEnable = new List<GameObject>();
    private List<GameObject> objectsToDisable = new List<GameObject>();
    private Transform sector;

    public void Start()
    {
        sector = this.GetAttachedOWRigidbody().transform.Find("Sector");

        List<GameObject> sectorChildren = new List<GameObject>();
        for (int i = 0; i < sector.childCount; i++)
        {
            sectorChildren.Add(sector.GetChild(i).gameObject);
        }

        foreach (GameObject child in sectorChildren)
        {
            if (child.name == "AmbientLight" || child.name == "FogSphere" || child.name == "Effects" || child.name == "VisorRainEffectVolume")
            {
                objectsToDisable.Add(child);
            }
        }

        foreach (var obj in objectsToEnable)
        {
            if (obj == null)
            {
                NervesOfDarkness.WriteLine("ERROR! Object to enable is null! Stopping loop!", OWML.Common.MessageType.Error);
                break;
            }
            obj.SetActive(false);
        }
    }

    public virtual void OnTriggerEnter(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj == null)
                {
                    NervesOfDarkness.WriteLine("ERROR! Object to enable is null! Stopping loop!", OWML.Common.MessageType.Error);
                    break;
                }
                obj.SetActive(true);
            }
        }
    }

    public virtual void OnTriggerExit(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                obj.SetActive(true);
            }
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj == null)
                {
                    NervesOfDarkness.WriteLine("ERROR! Object to enable is null! Stopping loop!", OWML.Common.MessageType.Error);
                    break;
                }
                obj.SetActive(false);
            }
        }
    }
}