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

namespace PintoMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    public class Pinto_ModBase : BaseUnityPlugin
    {
        public const string MODGUID = "Pinta.PintoBoy";
        public const string MODNAME = "PintoBoy";
        public const string MODVERSION = "1.0.2";

        private readonly Harmony harmony = new Harmony(MODGUID);

        public ManualLogSource logger;

        public static ConfigEntry<float>
            config_PintoboyRarity;

        public static readonly Lazy<Pinto_ModBase> Instance = new Lazy<Pinto_ModBase>(() => new Pinto_ModBase());
        public static GameObject pintoPrefab;
        //public static GameObject screenPrefab;
        public static Item pintoGrab;
        public static GameObject spiderPrefab;
        public static GameObject slimePrefab;
        public static GameObject lootbugPrefab;

        public static Material matOffScreen;
        public static Material matOnScreen;

        public static AssetBundle pintoBundle;

        public int currentId = 0;

        static string audioPath = "assets/pintoboy/audio/";

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

            pintoGrab.canBeGrabbedBeforeGameStart = true;
            pintoGrab.isScrap = true;
            pintoGrab.canBeInspected = true;
            pintoGrab.allowDroppingAheadOfPlayer = true;
            pintoGrab.rotationOffset = new Vector3(0, 0, 0);
            pintoGrab.positionOffset = new Vector3(0, 0, 0);
            pintoGrab.restingRotation = new Vector3(-30, 0, 0);
            pintoGrab.verticalOffset = -0.1f;

            //Battery
            pintoGrab.requiresBattery = true;
            pintoGrab.batteryUsage = 600;

            pintoGrab.syncInteractLRFunction = true;




            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(spiderPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(slimePrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(lootbugPrefab);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(screenPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(pintoGrab.spawnPrefab);

            Items.RegisterScrap(pintoGrab, (int) config_PintoboyRarity.Value, Levels.LevelTypes.All);

            Debug.Log("Scrapitems: "+Items.scrapItems.Count + ": " + Items.scrapItems[0].modName + " rarity:"+ Items.scrapItems[0].rarity);
        }

        private void LoadBundle()
        {
            try
            {
                pintoBundle = AssetBundle.LoadFromMemory(Properties.Resources.pintobund);
                if (pintoBundle == null) throw new Exception("Failed to load Pinto Bundle!");

                //string[] assetNames = pintoBundle.GetAllAssetNames();
                //Debug.Log("Asset Names: \n" + string.Join("\n", assetNames));

                pintoPrefab = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/pintoboy.prefab");
                if (pintoPrefab == null) throw new Exception("Failed to load Pinto Prefab!");

                pintoGrab = pintoBundle.LoadAsset<Item>("assets/pintoboy/pintoboy.asset");
                if (pintoGrab == null) throw new Exception("Failed to load Pinto Item!");

                PintoBoy pintoBoy = pintoGrab.spawnPrefab.AddComponent<PintoBoy>();
                if (pintoBoy == null) throw new Exception("Failed to load Pinto Boy!");

                pintoBoy.itemProperties = pintoGrab;

                //screenPrefab = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/2d cam.prefab");
                //if (screenPrefab == null) throw new Exception("Failed to load Screen for Pinto!");
                ////screenPrefab.AddComponent<NetworkObject>();

                GameObject spider = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/2d/spider/spider.prefab");
                if (spider == null) throw new Exception("Failed to load Spider Prefab Object!");
                spider.AddComponent<JumpanyEnemy>();
                spiderPrefab = spider;

                if (spiderPrefab == null) throw new Exception("Failed to load Spider Prefab!");

                GameObject slime = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/2d/slime/slime.prefab");
                if (slime == null) throw new Exception("Failed to load Slime Prefab Object!");
                slime.AddComponent<JumpanyEnemy>();
                slimePrefab = slime;
                if (slimePrefab == null) throw new Exception("Failed to load Slime Prefab!");

                GameObject lootbug = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/2d/loot bug/loot bug.prefab");
                if (lootbug == null) throw new Exception("Failed to load Lootbug Prefab Object!");
                lootbug.AddComponent<JumpanyEnemy>();
                lootbugPrefab = lootbug;
                if (lootbugPrefab == null) throw new Exception("Failed to load Lootbug Prefab!");

                matOffScreen = pintoBundle.LoadAsset<Material>("assets/pintoboy/off screen.mat");
                if (matOffScreen == null) throw new Exception("Failed to load off screen material!");

                matOnScreen = pintoBundle.LoadAsset<Material>("assets/pintoboy/Screen Mat.mat");
                if (matOnScreen == null) throw new Exception("Failed to load Screen Mat material!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static AudioClip GetAudioClip(string path)
        {
            AudioClip clip = pintoBundle.LoadAsset<AudioClip>(audioPath + path + ".wav");
            if (clip == null)
            {
                clip = pintoBundle.LoadAsset<AudioClip>(audioPath + path + ".mp3");
            }
            if (clip == null)
            {
                throw new Exception($"Failed to load Audio Clip {path}. Full Path: {audioPath + path}");
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
