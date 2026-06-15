using UnityEngine;
using System.Collections.Generic;

namespace NervesOfDarkness;

public class ToggleLightsTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform lights;
    [SerializeField]
    private bool turnsOn;
    private Transform sector;
    private List<GameObject> objectsToEnable = new List<GameObject>();
    private List<GameObject> objectsToDisable = new List<GameObject>();
    
    public void Start()
    {
        sector = this.GetAttachedOWRigidbody().transform.Find("Sector");
        SetupObjectArray(false, sector);
        SetupObjectArray(true, lights);
    }

    public void SetupObjectArray(bool isEnabling, Transform parentObject)
    {
        if (parentObject != null)
        {
            List<GameObject> tempChildren = new List<GameObject>();
            for (int i = 0; i < parentObject.childCount; i++)
            {
                tempChildren.Add(parentObject.GetChild(i).gameObject);
            }

            foreach (GameObject child in tempChildren)
            {
                if (isEnabling)
                {
                    if (child.name == "Spot Light" || child.name == "Point Light")
                    {
                        objectsToEnable.Add(child);
                    }
                    foreach (var obj in objectsToEnable)
                    {
                        if (obj == null)
                        {
                            NervesOfDarkness.WriteLine("ToggleLightsTrigger.cs ERROR! Object to enable is null! Stopping loop!", OWML.Common.MessageType.Error);
                            break;
                        }
                        obj.SetActive(false);
                    }
                }
                else
                {
                    if (child.name == "AmbientLight" || child.name == "FogSphere" || child.name == "Effects" || child.name == "VisorRainEffectVolume")
                    {
                        objectsToDisable.Add(child);
                    }
                    foreach (var obj in objectsToEnable)
                    {
                        if (obj == null)
                        {
                            NervesOfDarkness.WriteLine("ToggleLightsTrigger.cs ERROR! Object to disable is null! Stopping loop!", OWML.Common.MessageType.Error);
                            break;
                        }
                        obj.SetActive(true);
                    }
                }

            }
        }
        
    }

    public void ToggleLights(bool isOn)
    {
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(isOn);
        }
        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(!isOn);
        }
    }

    public virtual void OnTriggerEnter(Collider hitCollider)
    {
        //checks if player collides with the trigger volume
        if (hitCollider.CompareTag("PlayerDetector") && enabled)
        {
            ToggleLights(turnsOn);
        }
    }
}