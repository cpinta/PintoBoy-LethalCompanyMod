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

    }
}
