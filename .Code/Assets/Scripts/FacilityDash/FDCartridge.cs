using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FDCartridge : PintoBoyCartridge
    {
        public NetworkVariable<float> highScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> distanceTraveled = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> screenId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void CartridgeAwake()
        {
            base.CartridgeAwake();
            Debug.Log($"{this.name}: About to find Game transform");
            Transform gameTr = transform.Find("Game");
            if(gameTr == null)
            {
                return;
            }

            Debug.Log($"{this.name}: About to add component to game transform");
            transform.Find("Game").gameObject.AddComponent<FacilityDash>();
            Debug.Log($"{this.name}: About to get game component");
            game = transform.Find("Game").gameObject.GetComponent<FacilityDash>();

            game.transform.parent = null;
            game.cartridge = this;
        }
    }
}
