using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_Thumper : FD_Enemy
    {
        void Awake()
        {
            Initialize();
            EnemyName = "Thumper";
            Health = 4;
            AttackSpeeed = 3;

            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/thumper yell");
        }

        void Update()
        {
            base.GameUpdate();
        }
    }
}