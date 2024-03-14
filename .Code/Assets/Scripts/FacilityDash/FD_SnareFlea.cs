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
        float jumpTimerMin = 5;
        float jumpTimerMax = 10;
        float jumpTimer = 0;
        float jumpTime = 0;

        bool attacking = false;

        public bool latched = false;

        AudioClip acAngered;

        void Awake()
        {
            Initialize();
            jumpTimer = Random.Range(jumpTimerMin, jumpTimerMax);
            EnemyName = "Snare Flea";
            Health = 3;
            AttackSpeeed = 8;
            animator = GetComponent<Animator>();

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