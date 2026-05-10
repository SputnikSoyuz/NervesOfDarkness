using UnityEngine;
using System.Collections;
using NewHorizons.Utility;
using NewHorizons.Components.Volumes;

namespace NervesOfDarkness
{
    public class WarpCoreHeatExposure : MonoBehaviour
    {
        public GameObject warpCore;
        public WarpCoreItem warpCoreItem;
        public GameObject warpSingularity;
        public BlackHoleWarpVolume warpSingularityScript;
        public BlackHoleWarpVolume warpSingularityDefaultScript;
        public SingularityController singularityFX;
        public PlayerSectorDetector playerSectorDetector;
        public bool hasBeenExposed;
        public bool areValidConditions;
        private Coroutine exposeCoroutine = null;
        public Sector itemSector;
        public OWItem heldItem;

        public void Start()
        {
            playerSectorDetector = Locator.GetPlayerSectorDetector();
            hasBeenExposed = false;
            warpCore = this.gameObject;
            warpCoreItem = this.gameObject.GetComponent<WarpCoreItem>();
            warpSingularityDefaultScript = SearchUtilities.Find("Sun_Body/Sector_SUN/NoDWarpSingularity").GetComponentInChildren<BlackHoleWarpVolume>();
        }

        public void Update()
        {
            areValidConditions = AreValidConditions();

            if (!areValidConditions)
            {  // bad conditions -> stop the coroutine (CancelExposure handles whether it's even running to begin with)
                hasBeenExposed = false;
                CancelExposure();
            }

            if (areValidConditions && !hasBeenExposed)
            { // good condition and coroutine not already running -> start coroutine
                hasBeenExposed = true;
                exposeCoroutine = StartCoroutine(ExposeToHeat());
            }
        }

        private bool AreValidConditions()
        {
            if (playerSectorDetector.IsWithinSector(Sector.Name.Ship))
            {
                return false;
            }

            if (IsHeldByPlayer())
            {
                return (playerSectorDetector.IsWithinSector(Sector.Name.VolcanicMoon));
            }

            if (warpCore != null)
            {
                return (warpCoreItem.GetSector().GetName().Equals(Sector.Name.VolcanicMoon));
            }

            return false;
        }

        public bool IsHeldByPlayer()
        {
            heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();

            if (heldItem != null && heldItem.Equals(warpCoreItem))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator ExposeToHeat()
        {
            yield return new WaitForSeconds(10);
            Locator.GetShipLogManager().RevealFact("NoD_WARPCORE_E");
            singularityFX = Instantiate(SearchUtilities.Find("CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthSurface/Sector_TimeLoopExperiment/Interactables_TimeLoopExperiment/WarpCoreExperiment/SingularityEffects/Singularity_BlackHole/SingularityController_BlackHole"), warpCore.transform).GetComponent<SingularityController>();
            singularityFX.enabled = true;
            singularityFX._startActive = true;
            singularityFX._targetRadius = 1;
            singularityFX.CollapseImmediate();
            singularityFX.Create();
            warpSingularity = Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/NoDWarpSingularity"), warpCore.transform);
            warpSingularityScript = GetComponentInChildren<BlackHoleWarpVolume>();
            warpSingularityScript.TargetSolarSystem = "SputnikSoyuz.SalvagedStardust";
            if (IsHeldByPlayer())
            {
                warpSingularity.transform.parent = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM").transform;
            }
        }

        public void CancelExposure()
        {
            if (exposeCoroutine != null)
            {
                StopCoroutine(exposeCoroutine);
                exposeCoroutine = null;
            }
        }
    }
}
