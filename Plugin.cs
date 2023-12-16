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
using LC_API;
using System.Reflection;
using LethalLib.Modules;
using Object = UnityEngine.Object;
using HarmonyLib.Tools;
using UnityEngine.Yoga;
using BepInEx.Configuration;
using PintoMod.Assets.Scripts;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

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
        public static Item pintoGrab;

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

            ConfigSetup();
            LoadBundle();

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
            //
            
            config_PushCooldown = Config.Bind("Push Cooldown", "Value", 0.025f, "How long until the player can push again");
            config_PushForce = Config.Bind("Push Force", "Value", 12.5f, "How strong the player pushes.");
            config_PushRange = Config.Bind("Push Range", "Value", 3.0f, "The distance the player is able to push.");
            config_PushCost = Config.Bind("Push Cost", "Value", 0.08f, "The energy cost of each push.");
        }

        private void LoadBundle()
        {

            AssetBundle pintoBundle = AssetBundle.LoadFromMemory(Properties.Resources.pintobund);
            if (pintoBundle == null) throw new Exception("Failed to load Bundle!");

            string[] assetNames = pintoBundle.GetAllAssetNames();
            Debug.Log("Asset Names: \n" + string.Join("\n", assetNames));

            pintoPrefab = pintoBundle.LoadAsset<GameObject>("assets/pintoboy/pintoboy.prefab");
            if (pintoPrefab == null) throw new Exception("Failed to load Pinto Prefab!");

            pintoGrab = pintoBundle.LoadAsset<Item>("assets/pintoboy/pintoboy.asset");
            if (pintoGrab == null) throw new Exception("Failed to load Pinto Item!");

            PintoBoy pintoBoy = pintoGrab.spawnPrefab.AddComponent<PintoBoy>();

            BoomboxItem boombox = new BoomboxItem();
            //boombox.musicAudios

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(pintoGrab.spawnPrefab);
            Items.RegisterScrap(pintoGrab, 100, Levels.LevelTypes.All);


            pintoBoy.spiderPrefab = pintoBundle.LoadAsset<JumpanyEnemy>("assets/pintoboy/2d/spider/spider.prefab");
            pintoBoy.slimePrefab = pintoBundle.LoadAsset<JumpanyEnemy>("assets/pintoboy/2d/slime/slime.prefab");
            pintoBoy.lootbugPrefab = pintoBundle.LoadAsset<JumpanyEnemy>("assets/pintoboy/2d/lootbug/lootbug.prefab");
            pintoBoy.spiderPrefab.gameObject.AddComponent<JumpanyEnemy>();
            pintoBoy.slimePrefab.gameObject.AddComponent<JumpanyEnemy>();
            pintoBoy.lootbugPrefab.gameObject.AddComponent<JumpanyEnemy>();

        }
    }
}
