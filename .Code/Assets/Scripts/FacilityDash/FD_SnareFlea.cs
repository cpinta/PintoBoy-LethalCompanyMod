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
    public class FD_SnareFlea : FD_Enemy
    {
        bool attacking = false;

        public bool latched = false;

        public AudioClip acAngered;

        void Awake()
        {
            Initialize();
            animator = GetComponent<Animator>();
        }


        public override void Initialize()
        {
            base.Initialize();

            EnemyName = "Snare Flea";
            Health = 3;
            AttackSpeeed = 8;
            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/Snare Flea walk");
            acAngered = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/Snare Flea on head");
        }

        void LateUpdate()
        {
            //attacking is set so that LateUpdate doesnt override the attack animations transform
            //GameUpdate is in LateUpdate() so that it overrides the animator overriding the transform during the Idle clip
            if (!attacking)
            {
                base.GameUpdate();
                transform.localPosition = Vector3.zero;
            }
        }

        public override void Attack(bool isAttackBlockedByHiding)
        {
            if (latched) return;
            attacking = true;
            animator.SetBool(strAttackString, true);
        }

        public void Latch()
        {
            latched = true;
            game.Latch();
            game.PlaySound(acAngered);
        }
    }
}