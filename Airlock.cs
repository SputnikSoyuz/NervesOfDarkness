using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NervesOfDarkness;

public class Airlock : MonoBehaviour
{
    [SerializeField]
    private OWAudioSource _gateAudio;

    [SerializeField]
    private OWAudioSource _doorAudio;

    [SerializeField]
    private OWAudioSource _buttonAudio;

    [SerializeField]
    private Animator _gateAnim;

    [SerializeField]
    private Animator _doorAnim;

    [SerializeField]
    private Animator _buttonAnim;

    [SerializeField]
    private InteractReceiver _interactReceiverInside1;


    [SerializeField]
    private InteractReceiver _interactReceiverInside2;

    [SerializeField]
    private InteractReceiver _interactReceiverOutside;

    [SerializeField]
    private GameObject[] _redLights;

    [SerializeField]
    private GameObject[] _blueLights;

    [SerializeField]
    private GameObject _interiorLights;

    [SerializeField]
    private BooleanTriggerVolume triggerVolume;

    private bool isDoorOpen = false;
    private bool isGateOpen = false;
    private bool areLightsOn = false;
    private bool hasChangedLights = false;

    private Transform sector;
    private List<GameObject> objectsToEnable = new List<GameObject>();
    private List<GameObject> objectsToDisable = new List<GameObject>();

    public void Awake()
    {
        _interactReceiverInside1.OnPressInteract += OnPressInteractInside1;
        _interactReceiverInside2.OnPressInteract += OnPressInteractInside2;
        _interactReceiverOutside.OnPressInteract += OnPressInteractOutside;
    }

    public void Start()
    {
        sector = this.GetAttachedOWRigidbody().transform.Find("Sector");
        SetupObjectArray(false, sector);
        SetupObjectArray(true, _interiorLights.transform);
        ToggleLights(false);
        _gateAnim.Play("NoD_AirlockGateInside_ClosedSTATIC", 0);
        _doorAnim.Play("NoD_AirlockGateOutside_ClosedSTATIC", 0);
        _buttonAnim.Play("NoD_Airlock_Button_Static", 0);
        _interactReceiverOutside.SetPromptText(UITextType.OpenPrompt);
        _interactReceiverInside1.SetPromptText(UITextType.OpenPrompt);
        _interactReceiverInside2.SetPromptText(UITextType.OpenPrompt);
    }

    //Method that doesn't load!
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
                            NervesOfDarkness.WriteLine("Airlock.cs ERROR! Object to enable is null! Stopping loop!", OWML.Common.MessageType.Error);
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
                            NervesOfDarkness.WriteLine("Airlock.cs ERROR! Object to disable is null! Stopping loop!", OWML.Common.MessageType.Error);
                            break;
                        }
                        obj.SetActive(true);
                    }
                }

            }
        } else {
            NervesOfDarkness.WriteLine(this.gameObject.name + " Airlock.cs ERROR! Parent object is null!", OWML.Common.MessageType.Error);
        }
    }

    private void OnPressInteractInside1()
    {
        StopAllCoroutines();
        _buttonAnim.Play("NoD_Airlock_Button_Press", 0);
        _buttonAudio?.PlayOneShot(global::AudioType.NonDiaUIAffirmativeSFX, 1f);
        StartCoroutine(DoorSetup(true));
    }

    private void OnPressInteractInside2()
    {
        StopAllCoroutines();
        _buttonAnim.Play("NoD_Airlock_Button_Press", 0);
        _buttonAudio?.PlayOneShot(global::AudioType.NonDiaUIAffirmativeSFX, 1f);
        StartCoroutine(DoorSetup(true));
    }

    private void OnPressInteractOutside()
    {
        StopAllCoroutines();
        StartCoroutine(DoorSetup(false));
    }

    private IEnumerator DoorSetup (bool isInside){
        //NervesOfDarkness.WriteLine("STAGE 1 | AreLightsOn: " + areLightsOn, OWML.Common.MessageType.Success);
        hasChangedLights = false;
        //NervesOfDarkness.WriteLine("STAGE 2 | AreLightsOn: " + areLightsOn, OWML.Common.MessageType.Success);
        _interactReceiverInside1.DisableInteraction();
        _interactReceiverInside2.DisableInteraction();
        _interactReceiverOutside.DisableInteraction();
        if (isInside) {   
            yield return new WaitForSeconds(1);
        }
        ToggleDoors(isInside);
        yield return new WaitForSeconds(1);
        if (!isInside && triggerVolume.GetTriggerStatus())
        {
            NervesOfDarkness.WriteLine("Lights should be turning on.", OWML.Common.MessageType.Success);
            ToggleInteriorLights(true);
            hasChangedLights = true;
        }
        //NervesOfDarkness.WriteLine("STAGE 3 | AreLightsOn: " + hasChangedLights, OWML.Common.MessageType.Success);
        yield return new WaitForSeconds(4);
        ToggleDoors(isInside);
        yield return new WaitForSeconds(0.5f);
        if (!isInside && triggerVolume.GetTriggerStatus())
        {
            NervesOfDarkness.WriteLine("Lights should be turning off.", OWML.Common.MessageType.Success);
            ToggleInteriorLights(false);
            hasChangedLights = true;
        }
        //NervesOfDarkness.WriteLine("STAGE 4 | AreLightsOn: " + hasChangedLights, OWML.Common.MessageType.Success);
        _interactReceiverInside1.EnableInteraction();
        _interactReceiverInside2.EnableInteraction();
        _interactReceiverOutside.EnableInteraction();
    }

    private void ToggleDoors(bool isInside){
        if (isInside)
        {
            if (isGateOpen)
            {
                _gateAnim.Play("NoD_AirlockGateInside_Closed", 0);
                isGateOpen = false;
                //ToggleLights(false);
                _gateAudio?.PlayOneShot(global::AudioType.Airlock_Close, 1f);
            }
            else
            {
                _gateAnim.Play("NoD_AirlockGateInside_Open", 0);
                //ToggleLights(true);
                isGateOpen = true;
                _gateAudio?.PlayOneShot(global::AudioType.Airlock_Open, 1f);
            }
        } else {
            if (isDoorOpen)
            {
                _doorAnim.Play("NoD_AirlockGateOutside_Closed", 0);
                isDoorOpen = false;
                ToggleLights(false);
                _doorAudio?.PlayOneShot(global::AudioType.Airlock_Close, 1f);
            }
            else
            {
                _doorAnim.Play("NoD_AirlockGateOutside_Open", 0);
                ToggleLights(true);
                isDoorOpen = true;
                _doorAudio?.PlayOneShot(global::AudioType.Airlock_Open, 1f);
            } 
        }
    }

    private void ToggleLights(bool isRed){
        foreach (var light in _redLights)
        {
            light.SetActive(isRed);
        }
        foreach (var light in _blueLights)
        {
            light.SetActive(!isRed);
        }
    }

    private void ToggleInteriorLights(bool isOn)
    {
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(isOn);
        }
        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(!isOn);
        }
        areLightsOn = isOn;
    }
}