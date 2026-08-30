using UnityEngine;
using OWML.Common;
using NewHorizons.Utility;

namespace NervesOfDarkness;

public class GabbroHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject gabbro;
    [SerializeField]
    private GameObject noGabbro;
    private GameObject gabbroShip;
    private Campfire campfire;

    public void Start()
	{
        gabbroShip = SearchUtilities.Find("MaroonMeadows_Body/Sector/GabbroShip"); //Find and Get Gabbro's Ship
        campfire = SearchUtilities.Find("MaroonMeadows_Body/Sector/Prefab_HEA_Campfire/Controller_Campfire").GetComponent<Campfire>(); //Find and Get the Campfire near Gabbro
        if (PlayerData.GetPersistentCondition("NoD_LOOP_2") == true) //Only runs if the player spends 2 loops in the new system.
        {
            //NervesOfDarkness.WriteLine("NoD: Loop 3+", MessageType.Success);
            gabbro.SetActive(true); //Enable Gabbro
            noGabbro.SetActive(false); //Disable Missing Gabbro GameObject
            gabbroShip.SetActive(true); //Enable Gabbro's Ship
            campfire.SetState(Campfire.State.LIT); //Lights campfire at the start of the loop when Gabbro's there.
        }
        else
        {
            if (PlayerData.GetPersistentCondition("NoD_LOOP_1") == true)
            {
                PlayerData.SetPersistentCondition("NoD_LOOP_2", true); //Set Loop 2
                //NervesOfDarkness.WriteLine("NoD: Loop 2", MessageType.Success);

            }
            else
            {
                PlayerData.SetPersistentCondition("NoD_LOOP_1", true); //Set Loop 1
                //NervesOfDarkness.WriteLine("NoD: Loop 1", MessageType.Success);
            }
            gabbro.SetActive(false); //Disable Gabbro
            noGabbro.SetActive(true); //Enable Missing Gabbro GameObject
            gabbroShip.SetActive(false); //Disable Gabbro's Ship
        }
    }

    //DEBUG: Sets loop back to Loop 1 and KILLS THE PLAYER.
    public void ForceLoopOne()
    {
        PlayerData.SetPersistentCondition("NoD_LOOP_1", false);
        PlayerData.SetPersistentCondition("NoD_LOOP_2", false);
        Locator.GetDeathManager().KillPlayer(DeathType.Default);
    }
}
