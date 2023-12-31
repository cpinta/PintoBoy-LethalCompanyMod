using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;
using GameNetcodeStuff;
using UnityEngine;
using Unity.Netcode;
using LethalLib;
using System.Reflection;
using LethalLib.Modules;
using Object = UnityEngine.Object;
using HarmonyLib.Tools;
using BepInEx.Configuration;
using PintoMod.Assets.Scripts;
using PintoMod.Assets.Scripts.LethalJumpany;

namespace PintoMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    public class Pinto_ModBase : BaseUnityPlugin
    {
        public const string MODGUID = "Pinta.PintoBoy";
        public const string MODNAME = "PintoBoy";
        public const string MODVERSION = "1.0.3";

        private readonly Harmony harmony = new Harmony(MODGUID);

        public ManualLogSource logger;

        public static ConfigEntry<float>
            config_PintoboyRarity;

        public static readonly Lazy<Pinto_ModBase> Instance = new Lazy<Pinto_ModBase>(() => new Pinto_ModBase());

        public static AssetBundle pintoBundle; 

        public static Item itemPintoBoyPrefab;
        public static Material matOffScreen;
        public static Material matOnScreen;

        public static Item itemLJCartridgePrefab;
        public static LethalJumpany gameLethalJumpanyPrefab;
        public static GameObject ljSpiderPrefab;
        public static GameObject ljSlimePrefab;
        public static GameObject ljLootbugPrefab;



        public int currentId = 0;


        public static string basePath = "assets/pintoboy";
        public static string devicePath = $"{basePath}/device";
        public static string gamesPath = $"{basePath}/games";
        public static string ljBasePath = $"{gamesPath}/lethal jumpany";
        public static string ljAudioPath = $"{ljBasePath}/audio/";
        public static string ljSpritesPath = $"{ljBasePath}/2d";

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);



            ConfigSetup();
            LoadBundle();
            SetVariables();

            harmony.PatchAll(typeof(Pinto_ModBase));

            // Unity Netcode Weaver
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            Debug.Log("Pintoboy initialized");
        }

        private void ConfigSetup()
        {
            config_PintoboyRarity = Config.Bind("Pintoboy Rarity", "Value", 25f, "How rare is the PintoBoy");
        }

        private void SetVariables()
        {

            itemPintoBoyPrefab.canBeGrabbedBeforeGameStart = true;
            itemPintoBoyPrefab.isScrap = true;
            itemPintoBoyPrefab.canBeInspected = true;
            itemPintoBoyPrefab.allowDroppingAheadOfPlayer = true;
            itemPintoBoyPrefab.rotationOffset = new Vector3(0, 0, 0);
            itemPintoBoyPrefab.positionOffset = new Vector3(0, 0, 0);
            itemPintoBoyPrefab.restingRotation = new Vector3(-30, 0, 0);
            itemPintoBoyPrefab.verticalOffset = -0.1f;

            //Battery
            itemPintoBoyPrefab.requiresBattery = true;
            itemPintoBoyPrefab.batteryUsage = 600;

            itemPintoBoyPrefab.syncInteractLRFunction = true;




            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljSpiderPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljSlimePrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljLootbugPrefab);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(screenPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemPintoBoyPrefab.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemLJCartridgePrefab.spawnPrefab);

            Items.RegisterScrap(itemPintoBoyPrefab, (int) config_PintoboyRarity.Value, Levels.LevelTypes.All);
            Items.RegisterScrap(itemLJCartridgePrefab, (int)config_PintoboyRarity.Value, Levels.LevelTypes.All);

            Debug.Log("Scrapitems: "+Items.scrapItems.Count + ": " + Items.scrapItems[0].modName + " rarity:"+ Items.scrapItems[0].rarity);
        }

        private void LoadBundle()
        {
            try
            {

                pintoBundle = AssetBundle.LoadFromMemory(Properties.Resources.pintobund);
                if (pintoBundle == null) throw new Exception("Failed to load Pinto Bundle!");


                // PintoBoy prefabs
                itemPintoBoyPrefab = pintoBundle.LoadAsset<Item>($"{basePath}/pintoboy.asset");
                if (itemPintoBoyPrefab == null) throw new Exception("Failed to load Pinto Item!");
                PintoBoy pintoBoy = itemPintoBoyPrefab.spawnPrefab.AddComponent<PintoBoy>();
                if (pintoBoy == null) throw new Exception("Failed to load Pinto Boy!");
                pintoBoy.itemProperties = itemPintoBoyPrefab;

                matOffScreen = pintoBundle.LoadAsset<Material>($"{devicePath}/off screen.mat");
                if (matOffScreen == null) throw new Exception("Failed to load off screen material!");

                matOnScreen = pintoBundle.LoadAsset<Material>($"{devicePath}/Screen Mat.mat");
                if (matOnScreen == null) throw new Exception("Failed to load Screen Mat material!");


                Debug.Log("milestone 1");
                // Lethal Jumpany prefabs
                // Cartridge
                itemLJCartridgePrefab = pintoBundle.LoadAsset<Item>($"{ljBasePath}/lethaljumpany.asset");
                if (itemLJCartridgePrefab == null) throw new Exception("Failed to load LethalJumpany Item!");
                LJCartridge ljCart = itemLJCartridgePrefab.spawnPrefab.AddComponent<LJCartridge>();
                ljCart.itemProperties = itemLJCartridgePrefab;
                

                Debug.Log("milestone 2");

                //gameLethalJumpanyPrefab = pintoBundle.LoadAsset<GameObject>($"{ljBasePath}/game.prefab").AddComponent<LethalJumpany>();
                //if (gameLethalJumpanyPrefab == null) throw new Exception($"Failed to load gameLethalJumpanyPrefab at {ljBasePath}/2d.prefab");
                //ljCart.gamePrefab = gameLethalJumpanyPrefab;

                Debug.Log("milestone 3");
                // Enemies
                GameObject spider = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/spider/spider.prefab");
                if (spider == null) throw new Exception("Failed to load Spider Prefab Object!");
                spider.AddComponent<LJEnemy>();
                ljSpiderPrefab = spider;
                if (ljSpiderPrefab == null) throw new Exception("Failed to load Spider Prefab!");

                GameObject slime = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/slime/slime.prefab");
                if (slime == null) throw new Exception("Failed to load Slime Prefab Object!");
                slime.AddComponent<LJEnemy>();
                ljSlimePrefab = slime;
                if (ljSlimePrefab == null) throw new Exception("Failed to load Slime Prefab!");

                GameObject lootbug = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/loot bug/loot bug.prefab");
                if (lootbug == null) throw new Exception("Failed to load Lootbug Prefab Object!");
                lootbug.AddComponent<LJEnemy>();
                ljLootbugPrefab = lootbug;
                if (ljLootbugPrefab == null) throw new Exception("Failed to load Lootbug Prefab!");

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static AudioClip GetAudioClip(string path)
        {
            AudioClip clip = pintoBundle.LoadAsset<AudioClip>(path + ".wav");
            if (clip == null)
            {
                clip = pintoBundle.LoadAsset<AudioClip>(path + ".mp3");
            }
            if (clip == null)
            {
                throw new Exception($"Failed to load Audio Clip {path}");
            }

            return clip;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
        public static void PintoBoyAddedToShip(GrabbableObject scrapObject)
        {
            if (scrapObject is PintoBoy)
            {
                PintoBoy pinto = (PintoBoy)scrapObject;
                pinto.MakeScreenNOTSpawnable();
                Debug.Log("PintoBoy added to ship and screen not spawned");
            }
        }
    }
}
