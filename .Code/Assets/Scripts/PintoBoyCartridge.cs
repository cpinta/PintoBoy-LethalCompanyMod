using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    public abstract class PintoBoyCartridge : GrabbableObject
    {
        public ScanNodeProperties scanNodeProperties;

        public PintoBoyGame gamePrefab;
        public PintoBoyGame game;

        void Awake()
        {
            CartridgeAwake();
        }

        public virtual void CartridgeAwake()
        {

        }

        public void InsertedIntoPintoBoy(PintoBoy pintoBoy, Transform gameRoot)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 270);

            game.InsertedIntoPintoBoy(pintoBoy, gameRoot, this);
        }
    }
}
