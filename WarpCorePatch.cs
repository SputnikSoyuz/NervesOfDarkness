using HarmonyLib;

namespace NervesOfDarkness;

[HarmonyPatch]
public class WarpCorePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WarpCoreItem), nameof(WarpCoreItem.Awake))]
    private static void Awake_Patch(WarpCoreItem __instance)
    {
        if (NervesOfDarkness.Instance.NewHorizons.GetCurrentStarSystem().Equals("SolarSystem") && __instance.GetWarpCoreType() == WarpCoreType.Black)
        { 
            __instance.gameObject.AddComponent<WarpCoreHeatExposure>(); //Adds new heat exposure behaviour.
        }
    }
}
