using System;
using System.Collections.Generic;
using System.Text;

namespace PintoMod.Assets.Scripts.LethalJumpany
{
    public class LJCartridge : PintoBoyCartridge
    {
        void Awake()
        {
            game = Pinto_ModBase.gameLethalJumpanyPrefab;
            game.cartridge = this;
        }
    }
}
