using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


namespace PintoMod.Assets.Scripts.FacilityDash
{


    public class FD_BunkerSpider : FD_Enemy
    {
        void Awake()
        {
            Initialize();
            EnemyName = "Spider";
            Health = 5;
            AttackSpeeed = 4;

            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "spider sound");
        }

        void Update()
        {
            base.GameUpdate();

        }

    }
}
