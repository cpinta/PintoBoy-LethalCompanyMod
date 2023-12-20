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
using UnityEngine.Yoga;
using BepInEx.Configuration;
using PintoMod.Assets.Scripts;

namespace PintoMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    public class Pinto_ModBase : BaseUnityPlugin
    {
        public const string MODGUID = "LCMOD.Pinto_Mod";
        public const string MODNAME = "Pinto_Mod";
        public const string MODVERSION = "1.0.0";

        private readonly Harmony harmony = new Harmony(MODGUID);

        public ManualLogSource logger;

        public static ConfigEntry<float>
            config_PushCooldown,
            config_PushForce,
            config_PushRange,
            config_PushCost;

        public static readonly Lazy<Pinto_ModBase> Instance = new Lazy<Pinto_ModBase>(() => new Pinto_ModBase());
        public static GameObject pintoPrefab;
        public static GameObject screenPrefab;
        public static Item pintoGrab;
        public static GameObject spiderPrefab;
        public static GameObject slimePrefab;
        public static GameObject lootbugPrefab;

        public static Material matOffScreen;
        public static Material matOnScreen;

        public static AssetBundle pintoBundle;

        static string audioPath = "assets/pintoboy/audio/";

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

            ConfigSetup();
            LoadBundle();
            SetVariables();

            harmony.PatchAll(typeof(Pinto_ModBase));
            harmony.PatchAll(typeof(PlayerControllerB_Patches));
            harmony.PatchAll(typeof(NetworkHandler));

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

            logger.LogInfo($"Pintoboy has initialized!");
        }

        private void ConfigSetup()
        {
            //config_PushCooldown = Config.Bind("Push Cooldown", "Value", 0.025f, "How long until the player can push again");
            //config_PushForce = Config.Bind("Push Force", "Value", 12.5f, "How strong the player pushes.");
            //config_PushRange = Config.Bind("Push Range", "Value", 3.0f, "The distance the player is able to push.");
            //config_PushCost = Config.Bind("Push Cost", "Value", 0.08f, "The energy cost of each push.");
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


            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(pintoGrab.spawnPrefab);
            Items.RegisterScrap(pintoGrab, 20, Levels.LevelTypes.All);
        }

        private void LoadBundle()
        {
            try
            {
                pintoBundle = AssetBundle.LoadFromMemory(Properties.Resources.pintobund);
                if (pintoBundle == null) throw new Exception("Failed to load Pinto Bundle!");

                string[] assetNames = pintoBundle.GetAllAssetNames();
                Debug.Log("Asset Names: \n" + string.Join("\n", assetNames));

                pintoPrefab = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/pintoboy.prefab");
                if (pintoPrefab == null) throw new Exception("Failed to load Pinto Prefab!");

                pintoGrab = pintoBundle.LoadAsset<Item>("assets/pintoboy/pintoboy.asset");
                if (pintoGrab == null) throw new Exception("Failed to load Pinto Item!");

                PintoBoy pintoBoy = pintoGrab.spawnPrefab.AddComponent<PintoBoy>();
                if (pintoBoy == null) throw new Exception("Failed to load Pinto Boy!");

                pintoBoy.itemProperties = pintoGrab;

                screenPrefab = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/2d cam.prefab");
                if (screenPrefab == null) throw new Exception("Failed to load Screen for Pinto!");

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

        public void InstantiateScreenWithUniqueRenderTexture(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation, int textureWidth, int textureHeight, int textureDepth)
        {
            // Step 1: Create a Render Texture
            RenderTexture uniqueRenderTexture = new RenderTexture(textureWidth, textureHeight, textureDepth);

            // Step 2: Instantiate the Prefab
            GameObject prefabInstance = Instantiate(prefab, spawnPosition, spawnRotation);

            // Step 3: Assign the Unique Render Texture to the Prefab Instance
            prefabInstance.GetComponent<Camera>().targetTexture = uniqueRenderTexture;

            // Step 4: Use Render Texture in Shader or Camera
            Material material = prefabInstance.GetComponent<Renderer>().material;
            material.SetTexture("_MainTex", uniqueRenderTexture);

            // Or if using a camera:
            // Camera camera = prefabInstance.GetComponent<Camera>();
            // camera.targetTexture = uniqueRenderTexture;
        }
    }
}
