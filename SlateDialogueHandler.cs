using NewHorizons.Utility;
using UnityEngine;

namespace NervesOfDarkness
{
    public class SlateDialogueHandler : MonoBehaviour
    {
        private GameObject slateDialogue;

        public void Start()
        {
            //PlayerData.SetPersistentCondition("NoD_HornfelsIntro", false);
            slateDialogue = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_Village/Sector_StartingCamp/Characters_StartingCamp/Villager_HEA_Slate/NoD_SlateRemote");
        }

        public void Update()
        {
            if (PlayerData.GetPersistentCondition("NoD_HornfelsIntro") && slateDialogue != null)
            {
                Destroy(slateDialogue);
            }
        }
    }
}