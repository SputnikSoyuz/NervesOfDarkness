using NewHorizons.Utility;
using System;
using System.Collections;
using UnityEngine;

namespace NervesOfDarkness;
public class NoDCampfire : MonoBehaviour
{
    public enum State
    {
        UNLIT = 0,
        LIT = 1,
        SMOLDERING = 2
    }

    public delegate void NoDCampfireEvent(NoDCampfire fire);

    [SerializeField]
    private State _initialState = State.LIT;

    [SerializeField]
    private Sector _sector;

    [Space]
    [SerializeField]
    private float _heatConeBottom;

    [SerializeField]
    private float _heatConeTop;

    [SerializeField]
    private float _heatConeRadius;

    [SerializeField]
    private float _heatFalloffDistance;

    [Space]
    [SerializeField]
    private float _logSphereCenter;

    [SerializeField]
    private float _logSphereRadius;

    [SerializeField]
    private float _rockHeight;

    [Space]
    [SerializeField]
    protected OWAudioSource _audio;

    [SerializeField]
    protected OWAudioSource _oneShotAudio;

    [SerializeField]
    private OWLightController _lightController;

    [SerializeField]
    private OWRenderer[] _litRenderers;

    [SerializeField]
    private OWRenderer[] _hideWhileSmolderingRenderers;

    [SerializeField]
    private ParticleSystem[] _smolderingParticles;

    [SerializeField]
    private ParticleSystem[] _litParticles;

    [SerializeField]
    private MeshRenderer _flames;

    [SerializeField]
    private MeshRenderer _embers;

    [SerializeField]
    private MeshRenderer _ash;

    [Space]
    [SerializeField]
    private SingleInteractionVolume _interactVolume;

    [SerializeField]
    private HazardVolume _hazardVolume;

    private PlayerLockOnTargeting _lockOnTargeting;

    protected State _state;

    private bool _playerInSector;

    private float _litFraction;

    private bool _windBlew;

    public event NoDCampfireEvent OnCampfireStateChange;

    public Sector frigidFogSector;

    public PlayerSectorDetector playerSectorDetector;

    protected virtual void Awake()
    {
        if (_interactVolume != null)
        {
            _interactVolume.OnPressInteract += OnPressInteract;
        }
        if (_sector != null)
        {
            _sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
        }
    }

    protected virtual void Start()
    {
        if (Locator.GetPlayerTransform() != null)
        {
            _lockOnTargeting = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
        }
        if (_audio != null)
        {
            _audio.SetLocalVolume(0f);
        }
        base.enabled = false;
        SetState(_initialState, forceStateUpdate: true);
        base.enabled = false;
        frigidFogSector = SearchUtilities.Find("FrigidFog_Body/Sector").GetComponent<Sector>();
        playerSectorDetector = Locator.GetPlayerSectorDetector();
        _windBlew = false;
        if (this.GetAttachedOWRigidbody().name == "FrigidFog_Body")
        {
            StartCoroutine(Extinguish(true));
        }
    }

    protected virtual void OnDestroy()
    {
        if (_interactVolume != null)
        {
            _interactVolume.OnPressInteract -= OnPressInteract;
        }
        if (_sector != null)
        {
            _sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
        }
    }

    public Sector GetSector()
    {
        return _sector;
    }

    public State GetState()
    {
        return _state;
    }

    public void SetInteractionEnabled(bool enabled)
    {
        if (_interactVolume != null)
        {
            if (enabled)
            {
                _interactVolume.EnableInteraction();
            }
            else
            {
                _interactVolume.DisableInteraction();
            }
        }
    }

    public void PlayOneShot(AudioType audioType)
    {
        _oneShotAudio.PlayOneShot(audioType);
    }

    public void SetInitialState(State initialState)
    {
        _initialState = initialState;
    }

    public void SetState(State newState, bool forceStateUpdate = false)
    {
        if (!(_state != newState || forceStateUpdate))
        {
            return;
        }
        _state = newState;
        _hazardVolume.SetVolumeActivation(_state == State.LIT);
        switch (_state)
        {
            case State.LIT:
                {
                    for (int j = 0; j < _litRenderers.Length; j++)
                    {
                        _litRenderers[j].SetActivation(active: true);
                    }
                    if (_oneShotAudio != null && !forceStateUpdate)
                    {
                        _oneShotAudio.PlayOneShot(AudioType.TH_Campfire_Ignite);
                    }
                    _flames.enabled = true;
                    break;
                }
            case State.SMOLDERING:
                {
                    for (int k = 0; k < _hideWhileSmolderingRenderers.Length; k++)
                    {
                        _hideWhileSmolderingRenderers[k].SetActivation(active: false);
                    }
                    break;
                }
            case State.UNLIT:
                {
                    for (int i = 0; i < _litRenderers.Length; i++)
                    {
                        _litRenderers[i].SetActivation(active: false);
                    }
                    break;
                }
        }
        CheckParticleActivation(forceStateUpdate);
        CheckLoopingAudioActivation();
        if (_state == State.LIT)
        {
            _lightController.FadeTo(1f, 1f);
        }
        else
        {
            _lightController.FadeTo(0f, forceStateUpdate ? 0f : 1f);
        }
        UITextType promptID = UITextType.LightCampfirePrompt;
        if (_state == State.LIT)
        {
            promptID = UITextType.RoastingExtinguishPrompt;
        }
        if (_interactVolume != null)
        {
            _interactVolume.ChangePrompt(promptID);
        }
        if (forceStateUpdate)
        {
            switch (_state)
            {
                case State.LIT:
                    SetLitFraction(1f);
                    break;
                case State.SMOLDERING:
                    SetLitFraction(0.4f);
                    break;
                case State.UNLIT:
                    SetLitFraction(0f);
                    break;
            }
        }
        if (!forceStateUpdate && this.OnCampfireStateChange != null)
        {
            this.OnCampfireStateChange(this);
        }
        base.enabled = true;
    }

    public bool CheckStickIntersection(Vector3 stickPivotPos, Vector3 stickPivotForward, out Vector3 intersectPoint)
    {
        return OWMath.RaySphereIntersection(stickPivotPos, stickPivotForward, base.transform.position + base.transform.up * _logSphereCenter, _logSphereRadius, out intersectPoint);
    }

    public float GetRockHeight()
    {
        return _rockHeight;
    }

    public float GetHeatAtPosition(Vector3 worldPosition)
    {
        Vector3 vector = base.transform.TransformPoint(new Vector3(0f, _heatConeBottom, 0f));
        Vector3 vector2 = base.transform.TransformPoint(new Vector3(0f, _heatConeTop, 0f));
        Vector3 vector3 = worldPosition - vector;
        Vector3 vector4 = vector + Vector3.ProjectOnPlane(vector3, base.transform.up).normalized * _heatConeRadius;
        Vector3 from = vector2 - vector4;
        Vector3 to = worldPosition - vector4;
        float num = OWMath.Angle(axis: Vector3.Cross(base.transform.up, vector3), from: from, to: to);
        float value = to.magnitude * Mathf.Sin((float)Math.PI / 180f * num);
        float num2 = Mathf.InverseLerp(_heatFalloffDistance, 0f, value);
        float num3 = Mathf.Lerp(0f, 25.1f, num2 * num2);
        if (_state != State.LIT)
        {
            num3 *= 0.5f;
        }
        return num3;
    }

    protected virtual bool CheckUnequipToolWhileSleeping()
    {
        return true;
    }

    private void OnPressInteract()
    {
        if (_state == State.LIT)
        {
            StartCoroutine(Extinguish(false));
        }
        else
        {
            _flames.gameObject.SetActive(true);
            SetState(State.LIT);
            _windBlew = false;
            Locator.GetFlashlight().TurnOff(playAudio: false);
        }
    }

    public void Update()
    {
        float num = 0f;
        switch (_state)
        {
            case State.LIT:
                num = 1f;
                break;
            case State.SMOLDERING:
                num = 0.4f;
                break;
            case State.UNLIT:
                num = 0f;
                break;
        }
        if (_litFraction != num)
        {
            SetLitFraction(Mathf.MoveTowards(_litFraction, num, Time.deltaTime));
        }

        if (this.GetAttachedOWRigidbody().name == "FrigidFog_Body" && GetState() == State.LIT && !_windBlew)
        {
            StartCoroutine(Extinguish(true));
        }
    }

    private IEnumerator Extinguish(bool isWind)
    {
        if (isWind)
        {
            _windBlew = true;
            yield return new WaitForSeconds(2);
        }
        if (_state == State.LIT)
        {
            SetState(State.UNLIT);
            _oneShotAudio.PlayOneShot(AudioType.ProjectorTotem_Blow);
            yield return new WaitForSeconds(0.5f);
            _flames.gameObject.SetActive(false);
        }
    }

    private void SetLitFraction(float fraction)
    {
        _litFraction = fraction;
        Vector2 textureOffset = _flames.material.GetTextureOffset("_MainTex");
        textureOffset.y = 1f - _litFraction;
        _flames.material.SetTextureOffset("_MainTex", textureOffset);
        if (_embers != null)
        {
            _embers.material.SetColor("_EmissionColor", new Color(_litFraction * 1.5f, _litFraction * 1.5f, _litFraction * 1.5f));
        }
        if (_ash != null)
        {
            _ash.material.SetColor("_EmissionColor", new Color(_litFraction * 1.5f, _litFraction * 1.5f, _litFraction * 1.5f));
        }
    }

    private void CheckParticleActivation(bool forceStateUpdate = false)
    {
        for (int i = 0; i < _smolderingParticles.Length; i++)
        {
            _smolderingParticles[i].Stop();
            if (forceStateUpdate)
            {
                _smolderingParticles[i].Clear();
            }
        }
        for (int j = 0; j < _litParticles.Length; j++)
        {
            _litParticles[j].Stop();
            if (forceStateUpdate)
            {
                _litParticles[j].Clear();
            }
        }
        if (!(_sector == null) && !_playerInSector)
        {
            return;
        }
        for (int k = 0; k < _smolderingParticles.Length; k++)
        {
            if (_state == State.SMOLDERING)
            {
                _smolderingParticles[k].Play();
            }
        }
        for (int l = 0; l < _litParticles.Length; l++)
        {
            if (_state == State.LIT)
            {
                _litParticles[l].Play();
            }
        }
    }

    private void CheckLoopingAudioActivation()
    {
        if (_audio == null)
        {
            return;
        }
        if (_sector != null && !_playerInSector)
        {
            _audio.FadeOut(2f);
            return;
        }
        switch (_state)
        {
            case State.LIT:
                _audio.FadeIn(0.5f);
                break;
            case State.SMOLDERING:
                _audio.FadeIn(0.5f, fadeFromNothing: false, randomizePlayhead: false, 0.5f);
                break;
            case State.UNLIT:
                _audio.FadeOut(0.5f);
                break;
        }
    }

    private void OnSectorOccupantsUpdated()
    {
        bool flag = _sector.ContainsOccupant(DynamicOccupant.Player);
        if (_playerInSector != flag)
        {
            _playerInSector = flag;
            CheckParticleActivation();
            CheckLoopingAudioActivation();
            base.enabled = _playerInSector;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (OWGizmos.IsDirectlySelected(base.gameObject))
        {
            Gizmos.color = Color.red;
            Vector3 vector = base.transform.TransformPoint(new Vector3(0f, _heatConeBottom, 0f));
            Vector3 to = base.transform.TransformPoint(new Vector3(0f, _heatConeTop, 0f));
            OWGizmos.DrawWireCircle(vector, base.transform.up, _heatConeRadius);
            Gizmos.DrawLine(vector, to);
            Gizmos.DrawLine(vector + base.transform.right * _heatConeRadius, to);
            Gizmos.DrawLine(vector - base.transform.right * _heatConeRadius, to);
            Gizmos.DrawLine(vector + base.transform.forward * _heatConeRadius, to);
            Gizmos.DrawLine(vector - base.transform.forward * _heatConeRadius, to);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(base.transform.position + base.transform.up * _logSphereCenter, _logSphereRadius);
            OWGizmos.DrawWireCircle(base.transform.position + base.transform.up * _rockHeight, base.transform.up, 0.8f);
        }
    }
}
