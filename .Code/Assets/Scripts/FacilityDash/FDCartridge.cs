using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FDCartridge : PintoBoyCartridge
    {

        public override void CartridgeAwake()
        {
            grabbable = false;
            parentObject = this.transform;

            grabbableToEnemies = false;

            startFallingPosition = new Vector3(0, 0, 0);
            targetFloorPosition = new Vector3(0, 0, 0);
            EnableItemMeshes(true);

            Debug.Log("FDCartrdige: About to find Game transform");
            Transform gameTr = transform.Find("Game");
            if(gameTr == null)
            {
                return;
            }

            Debug.Log("FDCartrdige: About to add component to game transform");

            transform.Find("Game").gameObject.AddComponent<FacilityDash>();
            Debug.Log("FDCartrdige: About to get game component");

            game = transform.Find("Game").gameObject.GetComponent<FacilityDash>();

            game.transform.parent = null;
            game.cartridge = this;
        }
    }
}
