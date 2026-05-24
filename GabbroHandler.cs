using UnityEngine;
using OWML.Common;

namespace NervesOfDarkness;

public class GabbroHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject gabbro;
    [SerializeField]
    private GameObject noGabbro;

    public void Start()
	{
        if (PlayerData.GetPersistentCondition("NoD_LOOP_2") == true) //Only runs if the player spends 2 loops in the new system.
        {
            NervesOfDarkness.WriteLine("NoD: Loop 3+", MessageType.Success);
            gabbro.SetActive(true); //Enable Gabbro
            noGabbro.SetActive(false); //Disable Missing Gabbro GameObject
        }
        else
        {
            if (PlayerData.GetPersistentCondition("NoD_LOOP_1") == true)
            {
                PlayerData.SetPersistentCondition("NoD_LOOP_2", true); //Set Loop 2
                NervesOfDarkness.WriteLine("NoD: Loop 2", MessageType.Success);

            }
            else
            {
                PlayerData.SetPersistentCondition("NoD_LOOP_1", true); //Set Loop 1
                NervesOfDarkness.WriteLine("NoD: Loop 1", MessageType.Success);
            }
            gabbro.SetActive(false); //Disable Gabbro
            noGabbro.SetActive(true); //Enable Missing Gabbro GameObject
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
