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
            AttackSpeeed = 6;

        }

        void Update()
        {
            base.GameUpdate();

        }

    }
}
