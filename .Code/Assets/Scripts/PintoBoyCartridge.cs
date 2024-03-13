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
                //Debug.Log("Setting rotation");
                transform.localRotation = Quaternion.Euler(0, 0, 270);
            }
        }

        public void InsertedIntoPintoBoy(PintoBoy pintoBoy, Transform gameRoot)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, 0, 90);
            itemProperties.restingRotation = new Vector3(0, 0, 90);
            this.pintoBoy = pintoBoy;


            game.InsertedIntoPintoBoy(pintoBoy, gameRoot, this);
        }
    }
}
