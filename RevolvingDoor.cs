using System.Collections;
using UnityEngine;

namespace NervesOfDarkness
{
    public class RevolvingDoor : MonoBehaviour
    {
        private Vector3 _localAxis = new Vector3(0, 1, 0);
        private float _degreesPerSecond = 0;
        

        [Header("Active Sector (Optional)")]
        [SerializeField]
        private Sector _sector;

        [SerializeField]
        private DynamicOccupantMask _sectorOccupantMask;

        [SerializeField]
        private NoiseSensor noiseSensor;

        [SerializeField]
        private float _maxDistance = 3;

        [SerializeField]
        private float speedMultiple = 10;

        [SerializeField]
        private float fadeDuration = 3;

        private Transform _transform;

        private Quaternion _localRotation;

        private ThrusterModel _thrusterModel;

        public void Awake()
        {
            _transform = base.transform;
            _localRotation = base.transform.localRotation;
            _localAxis.Normalize();
            if (_sector != null)
            {
                _sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
                base.enabled = false;
            }
            noiseSensor.OnClosestAudibleNoise += OnClosestAudibleNoise;
        }

        public void OnDestroy()
        {
            if (_sector != null)
            {
                _sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
            }
        }

        public void OnSectorOccupantsUpdated()
        {
            base.enabled = _sector.ContainsAnyOccupants(_sectorOccupantMask.GetMask());
        }

        public void OnClosestAudibleNoise(NoiseMaker noiseMaker)
        {
            
            _thrusterModel = Locator.GetPlayerBody().GetComponent<JetpackThrusterModel>();
            StopAllCoroutines();
            if ((noiseMaker.GetNoiseOrigin() - base.transform.position).sqrMagnitude < _maxDistance * _maxDistance)
            {
                StartCoroutine(FadeToZero());
                _degreesPerSecond = _thrusterModel.GetLocalAcceleration().magnitude * speedMultiple;
            } else
            {
                StartCoroutine(FadeToZero());
                _degreesPerSecond = 0;
            }
        }
        public IEnumerator FadeToZero()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _degreesPerSecond = Mathf.Lerp(_degreesPerSecond, 0f, elapsed / fadeDuration);
                yield return null;
            }
            _degreesPerSecond = 0f;
        }

        public void Update()
        {
            _localRotation *= Quaternion.AngleAxis(_degreesPerSecond * Time.deltaTime, _localAxis);
            _transform.localRotation = _localRotation;
        }
    }

}
