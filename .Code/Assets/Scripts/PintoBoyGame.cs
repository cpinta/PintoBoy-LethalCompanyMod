using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    public abstract class PintoBoyGame : MonoBehaviour
    {
        public PintoBoy pintoBoy;
        public PintoBoyCartridge cartridge;
        public bool isActive;
        public bool isPaused;
        public bool isInGame;


        public void ButtonPress() { }

        public void GameUpdate() { }

        public void UnPause() { }
        public void Pause() { }

        public void TurnedOff() { }
        public void TurnedOn() { }

        public void IntializeObjects(Transform gameRoot) { }

        public void InsertedIntoPintoBoy(PintoBoy pintoBoy)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            this.pintoBoy = pintoBoy;
        }
    }
}
