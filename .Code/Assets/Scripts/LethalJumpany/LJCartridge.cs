using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PintoMod.Assets.Scripts.LethalJumpany
{
    public class LJCartridge : PintoBoyCartridge
    {
        void Awake()
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

            game = Pinto_ModBase.gameLethalJumpanyPrefab;
            game.cartridge = this;
        }
    }
}
