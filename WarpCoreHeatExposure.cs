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
        //private float lerpDuration = 1.5f;

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
        { // Name this however you want
            if (IsHeldByPlayer())
            {
                return (playerSectorDetector.IsWithinSector(Sector.Name.SunStation) || playerSectorDetector.IsWithinSector(Sector.Name.VolcanicMoon));
            }

            if (warpCore != null)
            {
                NervesOfDarkness.WriteLine("The Sector: " + warpCoreItem.GetSector().ToString(), OWML.Common.MessageType.Success);
                return (warpCoreItem.GetSector().GetName().Equals(Sector.Name.SunStation) || warpCoreItem.GetSector().GetName().Equals(Sector.Name.VolcanicMoon));
            }

            return false; // I don't know what happens by default, guessing false
        }

        public bool IsHeldByPlayer()
        {
            heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();

            if (heldItem != null && heldItem.Equals(warpCoreItem))
            {
                //NervesOfDarkness.WriteLine("The two are equal.", OWML.Common.MessageType.Success);
                return true;
            }
            else
            {
                //NervesOfDarkness.WriteLine("The two are NOT equal.", OWML.Common.MessageType.Success);
                return false;
            }
        }

        public IEnumerator ExposeToHeat()
        {
            NervesOfDarkness.WriteLine("Warp Core Exposed to Heat", OWML.Common.MessageType.Success);
            yield return new WaitForSeconds(10);
            NervesOfDarkness.WriteLine("Wait is done!", OWML.Common.MessageType.Success);
            singularityFX = Instantiate(SearchUtilities.Find("CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthSurface/Sector_TimeLoopExperiment/Interactables_TimeLoopExperiment/WarpCoreExperiment/SingularityEffects/Singularity_BlackHole/SingularityController_BlackHole"), warpCore.transform).GetComponent<SingularityController>(); //This might not be working.
            singularityFX.enabled = true;
            singularityFX._startActive = true;
            singularityFX._targetRadius = 1;
            singularityFX.CollapseImmediate();
            singularityFX.Create();
            warpSingularity = Instantiate(SearchUtilities.Find("Sun_Body/Sector_SUN/NoDWarpSingularity"), warpCore.transform); //This might not be working.
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
                NervesOfDarkness.WriteLine("Coroutine is not null! Cancelling!", OWML.Common.MessageType.Success);
                StopCoroutine(exposeCoroutine);
                exposeCoroutine = null;
            }
        }
    }
}
