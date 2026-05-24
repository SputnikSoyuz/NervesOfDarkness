using HarmonyLib;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace NervesOfDarkness
{
    public class NervesOfDarkness : ModBehaviour
    {
        public static NervesOfDarkness Instance;
        public INewHorizons NewHorizons;
        public bool hasDebugWarpedOnce = false;
        private Transform parent;
        GameObject[] trees = new GameObject[1200];
        public void Awake()
        {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(NervesOfDarkness)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);
            NewHorizons.GetBodyLoadedEvent().AddListener(OnBodyLoaded);
            NewHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);

            new Harmony("SputnikSoyuz.NervesOfDarkness").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public static void WriteLine(string text, MessageType messageType = MessageType.Message)
        {
            Instance.ModHelper.Console.WriteLine(text, messageType);
        }

        private void OnBodyLoaded(string body)
        {
            if (body == "Divine Devourer")
            {
                var divineDevourer = NewHorizons.GetPlanet("Divine Devourer").transform;
                var ringRenderer = divineDevourer.Find("Sector/Ring").GetComponent<MeshRenderer>();
                ringRenderer.sharedMaterial.name = "DivineDevourerRing";
                ringRenderer.sharedMaterial.renderQueue = 3000;
                var blackHoleRenderer = divineDevourer.Find("Sector/BlackHole/BlackHoleRenderer").GetComponent<MeshRenderer>();
                blackHoleRenderer.sharedMaterial.name = "DivineDevourerBlackHole";
                blackHoleRenderer.sharedMaterial.shader = Shader.Find("Outer Wilds/Effects/Singularity");
                blackHoleRenderer.sharedMaterial.renderQueue = 3001;
            }
        }

        private void OnStarSystemLoaded(string system)
        {
            if (system == "SputnikSoyuz.SalvagedStardust")
            {
                //Tree Stuff
                int[] badTreeIndexes = { 0, 46, 91, 92, 95, 101, 102, 108, 119, 154, 178, 184, 185, 264, 289, 312, 321, 346, 352, 353, 372, 376, 397, 410, 472, 507, 552, 562, 567, 589, 596, 612, 624, 658, 668, 718, 722, 727, 728, 766, 767, 784, 786, 831, 843, 881, 873, 875, 895, 905, 919, 932, 965, 978, 981, 982, 996, 1001, 1016, 1037, 1039, 1059, 1073, 1074, 1084, 1090, 1109, 1119, 1147, 1181 };
                parent = SearchUtilities.Find("MaroonMeadows_Body/Sector/Trees").transform;
                for (int i = 0; i < trees.Length; i++)
                {
                    trees[i] = parent.GetChild(i).gameObject;
                    trees[i].name = ("Tree" + i).ToString();
                }
                for (int i = 0; i < badTreeIndexes.Length; i++)
                {
                    Destroy(trees[badTreeIndexes[i]]);
                }
            } else if (system == "SolarSystem")
            {
                SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_Village/Sector_StartingCamp/Characters_StartingCamp/Villager_HEA_Slate").AddComponent<SlateDialogueHandler>();
            }
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        }

        public IEnumerator WarpPlayer(float blinkTime, SpawnPoint spawn)
        {
            if (blinkTime <= 0)
            {
                blinkTime = 0.5f; // constant for blink time
                
            }
            float animTime = blinkTime / 2f; // constant for blink animation time

            PlayerCameraEffectController cameraEffectController = FindObjectOfType<PlayerCameraEffectController>(); // gets camera controller
            PlayerSpawner _spawner; // for spawning the player
            
            //Close Eyes
            cameraEffectController.CloseEyes(animTime); //Close Eyes
            yield return new WaitForSeconds(animTime);  // waits until animation stops to proceed to next line
            GlobalMessenger.FireEvent("PlayerBlink"); // fires an event for the player blinking

            //Warp Player
            _spawner = GameObject.FindGameObjectWithTag("Player").GetRequiredComponent<PlayerSpawner>(); // gets player spawner
            _spawner.DebugWarp(spawn); // warps you to vessel

            //Open Eyes
            cameraEffectController.OpenEyes(animTime, false); //Open Eyes
            yield return new WaitForSeconds(animTime); //  waits until animation stops to proceed to next line
        }
    }
}
