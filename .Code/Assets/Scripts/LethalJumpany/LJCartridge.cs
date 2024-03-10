using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PintoMod.Assets.Scripts.LethalJumpany
{
    public class LJCartridge : PintoBoyCartridge
    {
        public NetworkVariable<float> highScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> currentScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> lives = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> screenId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public override void CartridgeAwake()
        {

            scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();

            grabbable = true;
            parentObject = this.transform;

            grabbableToEnemies = true;

            scanNodeProperties.maxRange = 100;
            scanNodeProperties.minRange = 1;
            scanNodeProperties.requiresLineOfSight = true;
            scanNodeProperties.creatureScanID = -1;
            scanNodeProperties.nodeType = 2;

            startFallingPosition = new Vector3(0, 0, 0);
            targetFloorPosition = new Vector3(0, 0, 0);
            EnableItemMeshes(true);

            Debug.Log("LJCartrdige: About to find Game transform");
            Transform gameTr = transform.Find("Game");
            if(gameTr == null)
            {
                Debug.Log("LJCartrdige: Game transform was null");
                return;
            }

            Debug.Log("LJCartrdige: About to add component to game transform");

            transform.Find("Game").gameObject.AddComponent<LethalJumpany>();
            Debug.Log("LJCartrdige: About to get game component");

            game = transform.Find("Game").gameObject.GetComponent<LethalJumpany>();

            game.transform.parent = null;
            game.cartridge = this;
        }
    }
}
