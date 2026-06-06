using NewHorizons;
using System.Collections;
using UnityEngine;

namespace NervesOfDarkness;

public class DisplayDevice : MonoBehaviour
{
    [SerializeField]
    public float startingWavelength;
    [SerializeField]
	public float desiredWavelength;
    [SerializeField]
    public Transform joystick;
	[SerializeField]
	public GameObject projection;
    [SerializeField]
    public MeshRenderer[] projectionRenderers;
	[SerializeField]
	public MeshRenderer liquidRenderer;
    [SerializeField]
    public ParticleSystem formingLiquid;
    [SerializeField]
	public ParticleSystem[] risingLiquids;
    [SerializeField]
    public InteractReceiver interactReceiver;
    [SerializeField]
	public OWAudioSource audioJoystick;
    [SerializeField]
    public AudioSource audioDisplay;
    [SerializeField]
    private Transform attentionPoint;
    [SerializeField]
    private float scaleSpeed = 0.5f;

    private float currentWavelength;
    private int phase = 0;
    private Material liquidShader;
    private bool isControlling;
    private int random1, random2;
    private Material[] projectionShaders = new Material[5];
    private bool isGrowing;
    private float targetScale = 0;
    private float targetDisplacement = 0;
    private float targetFluidDisplacement = 0;

    public void Awake()
    {
        isControlling = false;
        isGrowing = false;
        attentionPoint = ((attentionPoint == null) ? base.transform : attentionPoint);
        GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
    }
    public void Start()
    {
        projection.SetActive(false);
        formingLiquid.Stop();
        foreach (var liquid in risingLiquids)
        {
            liquid.Stop();
        }
        interactReceiver.OnPressInteract += OnPressInteract;

        currentWavelength = startingWavelength;
        audioJoystick.pitch = (currentWavelength / 80) + 0.5f;
        
        Material[] matTemp = liquidRenderer.materials;
        liquidShader = matTemp[0];
        liquidShader.SetFloat("_Displacement", 0);
        joystick.Rotate(startingWavelength, 0, 0);



        for (int i = 0; i < projectionRenderers.Length; i++)
        {
            matTemp = projectionRenderers[i].materials;
            projectionShaders[i] = matTemp[0];
            projectionShaders[i].SetFloat("_Displacement", 2);
        }

        do
        {
            random1 = Random.Range(0, risingLiquids.Length);
        } while (random1 == 2);

        do
        {
            random2 = Random.Range(0, risingLiquids.Length);
        } while (random2 == random1 && random2 == 2);
    }

    public void HandleLiquids()
    {
        //NervesOfDarkness.WriteLine("Phase Var: " + phase);
        switch (phase)
        {
            case 0:
                StopAllCoroutines();
                StartCoroutine(ToggleParticles(false, risingLiquids));
                StartCoroutine(ToggleFormingParticle(false));
                NervesOfDarkness.WriteLine("CURRENT PHASE 0");
                break;
            case 1:
                StopAllCoroutines();
                StartCoroutine(ToggleParticles(false, risingLiquids));
                StartCoroutine(ToggleParticles(true, risingLiquids[random1]));
                StartCoroutine(ToggleFormingParticle(false));
                NervesOfDarkness.WriteLine("CURRENT PHASE 1");
                break;
            case 2:
                StopAllCoroutines();
                StartCoroutine(ToggleParticles(false, risingLiquids));
                StartCoroutine(ToggleParticles(false, risingLiquids[random1]));
                StartCoroutine(ToggleFormingParticle(false));
                NervesOfDarkness.WriteLine("CURRENT PHASE 2");
                break;
            case 3:
                StopAllCoroutines();
                StartCoroutine(ToggleParticles(true, risingLiquids));
                StartCoroutine(ToggleFormingParticle(false));
                NervesOfDarkness.WriteLine("CURRENT PHASE 3");
                break;
            default: //any other case
                StopAllCoroutines();
                StartCoroutine(ToggleParticles(true, risingLiquids));
                StartCoroutine(ToggleFormingParticle(true));
                NervesOfDarkness.WriteLine("CURRENT PHASE 4+");
                break;
        }
    }

    public void HandleFluid(float max)
    {
        targetFluidDisplacement += (Time.deltaTime * 0.5f) * (isGrowing ? 1 : -1);
        targetFluidDisplacement = Mathf.Clamp(targetFluidDisplacement, liquidShader.GetFloat("_Displacement"), max);
        liquidShader.SetFloat("_Displacement", targetFluidDisplacement);
    }

    public IEnumerator ToggleParticles(bool isOn, params ParticleSystem[] particles)
    {
        var tempMain = particles[0].main;

        if (isOn)
        {
            foreach (var liquid in particles)
            {
                tempMain = liquid.main;
                tempMain.loop = true;
                liquid.Play();
                tempMain.simulationSpeed = 3;
            }
            yield return new WaitForSeconds(0.666666666667f);
            foreach (var liquid in particles)
            {
                tempMain.simulationSpeed = 1;
            }
        }
        else
        {
            foreach (var liquid in particles)
            {
                tempMain = liquid.main;
                tempMain.loop = false;
                liquid.Stop();
            }
            yield return new WaitForSeconds(2f);
            foreach (var liquid in particles)
            {
                liquid.Stop();
            }
        }
    }

    public IEnumerator ToggleFormingParticle(bool isOn)
    {
        var tempMain = formingLiquid.main;
        if (isOn)
        {
            tempMain.loop = true;
            formingLiquid.Play();
            yield return new WaitForSeconds(0f);
        } else
        {
            tempMain.loop = false;
            yield return new WaitForSeconds(2f);
            formingLiquid.Stop();
        }
    }

    public void ProjectionToggle()
    {
        targetScale += (Time.deltaTime * scaleSpeed) * (isGrowing ? 1 : -1);
        targetScale = Mathf.Clamp(targetScale, 0f, 0.5f);
        projection.transform.localScale = Vector3.one * targetScale;

        targetDisplacement -= (Time.deltaTime * scaleSpeed * 3.7f) * (isGrowing ? 1 : -1);
        targetDisplacement = Mathf.Clamp(targetDisplacement, 0.15f, 2f);
        foreach (var shader in projectionShaders)
        {
            shader.SetFloat("_Displacement", targetDisplacement);
        }
        projection.SetActive(targetScale > 0);
    }

    public void Update()
    {
        if (isControlling)
        {
            if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) ||
OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
            {
                StopControlling();
            }
            else if (OWInput.IsPressed(InputLibrary.toolOptionDown))
            {
                if (!audioJoystick.isPlaying)
                {
                    audioJoystick.PlayOneShot();
                }
                if (currentWavelength >= 0)
                {
                    audioDisplay.pitch = (currentWavelength / 80) + 0.5f;
                    currentWavelength--;
                }
            }
            else if (OWInput.IsPressed(InputLibrary.toolOptionUp))
            {
                if (!audioJoystick.isPlaying)
                {
                    audioJoystick.PlayOneShot();
                }
                if (currentWavelength <= 120)
                {
                    audioDisplay.pitch = (currentWavelength / 80) + 0.5f;
                    currentWavelength++;
                }
            }
            joystick.localEulerAngles = new Vector3(currentWavelength, 0, 0);

            string debugActives = "";
            for (int i = 0; i < risingLiquids.Length; i++)
            {
                debugActives += ("\nLiquid " + i + ": " + (risingLiquids[i].isPlaying ? "Active" : "Inactive"));
            }

            string projectionDisplacement = "";
            for (int i = 0; i < projectionShaders.Length; i++)
            {
                projectionDisplacement += ($"\nProjection Displacement {i}: " + projectionShaders[i].GetFloat("_Displacement"));
            }

            /*NervesOfDarkness.WriteLine("\nJoystick Rotation Euler: " + joystick.rotation.eulerAngles.x
                        + "\nJoystick Local Rotation Euler: " + joystick.localRotation.eulerAngles.x
                        + "\nEuler Changed To: " + joystick.localEulerAngles.ToString()
                        + "\nPhase: " + phase
                        + "\nProjection Scale: " + projection.transform.localScale.x
                        + projectionDisplacement
                        + "\nDisplacement: " + liquidShader.GetFloat("_Displacement")
                        + "\nCurrent Wavelength: " + currentWavelength
                        + "\nDesired Wavelength: " + desiredWavelength
                        + "\nStarting Wavelength: " + startingWavelength
                        + "\nPitch Start: " + startingWavelength * (3 / 120)
                        + "\nPitch: " + audioDisplay.pitch
                        + "\nRising Liquids: "
                        + debugActives, OWML.Common.MessageType.Success);*/

            if (Mathf.Abs(currentWavelength - desiredWavelength) <= 10)
            {
                if (phase != 4)
                {
                    phase = 4;
                    isGrowing = true;
                    HandleLiquids();
                }
                HandleFluid(1.5f);
            }
            else if (Mathf.Abs(currentWavelength - desiredWavelength) <= 20)
            {
                if (phase != 3)
                {
                    phase = 3;
                    isGrowing = false;
                    HandleLiquids();
                }
                HandleFluid(1f);
            }
            else if (Mathf.Abs(currentWavelength - desiredWavelength) <= 30)
            {
                if (phase != 2)
                {
                    phase = 2;
                    isGrowing = false;
                    HandleLiquids();
                }
                HandleFluid(0.75f);
            }
            else if (Mathf.Abs(currentWavelength - desiredWavelength) <= 40)
            {
                if (phase != 1)
                {
                    phase = 1;
                    isGrowing = false;
                    HandleLiquids();
                    liquidShader.SetFloat("_Displacement", 0.5f);
                }
                HandleFluid(0.5f);
            } 
            else
            {
                if (phase != 0)
                {
                    phase = 0;
                    isGrowing = false;
                    HandleLiquids();
                    liquidShader.SetFloat("_Displacement", 0.0f);
                }
                HandleFluid(0.0f);
            }

            ProjectionToggle();
        }
    }

    public void OnDestroy()
    {
        GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
    }

    private void StartControlling()
    {
        OWInput.ChangeInputMode(InputMode.None);
        base.enabled = true;
        Locator.GetToolModeSwapper().UnequipTool();
        if (attentionPoint != null && !PlayerState.InZeroG())
        {
            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(attentionPoint, Vector3.zero, 2f);
        }
        if (PlayerState.InZeroG())
        {
            Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody().GetReferenceFrame());
        }
        if (!isControlling)
        {
            isControlling = true;
        }
    }

    private void OnPressInteract()
    {
        StartControlling();
    }

    private void StopControlling()
    {
        OWInput.ChangeInputMode(InputMode.Character);
        if (!base.enabled)
        {
            return;
        }
        base.enabled = false;
        interactReceiver.ResetInteraction();
        Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();

        if (PlayerState.InZeroG())
        {
            Autopilot component = Locator.GetPlayerBody().GetComponent<Autopilot>();
            if (component.enabled)
            {
                component.Abort();
            }
        }
        if (isControlling)
        {
            isControlling = false;
        }
    }

    private void OnPlayerDeath(DeathType deathType)
    {
        if (base.enabled)
        {
            StopControlling();
        }
    }
}
