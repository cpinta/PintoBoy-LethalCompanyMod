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
        public PintoBoy pintoBoy;

        void Awake()
        {
            targetFloorPosition = transform.position;
            startFallingPosition = transform.position;
            
            CartridgeAwake();
        }

        public virtual void CartridgeAwake()
        {

        }

        virtual protected void LateUpdate()
        {
            base.LateUpdate();

            if(pintoBoy != null)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                //Debug.Log("Setting rotation");
            }
        }

        public void InsertedIntoPintoBoy(PintoBoy pintoBoy, Transform gameRoot)
        {
            this.pintoBoy = pintoBoy;

            game.InsertedIntoPintoBoy(pintoBoy, gameRoot, this);
        }
    }
}
