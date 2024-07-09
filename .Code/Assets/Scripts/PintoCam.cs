using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.UIElements.Layout.LayoutDataStore;

namespace PintoMod.Assets.Scripts
{
    public class PintoCam : MonoBehaviour
    {
        int timeToCheck = 30;
        float timer = 2f;
        Collider col;

        // Start is called before the first frame update
        void Awake()
        {
            CheckColliders();
        }


        void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                CheckColliders();
                timer = timeToCheck;
            }
        }

        void CheckColliders()
        {
            Debug.Log("PintoBoy Cam: checking if any PintoBoy Cams intersect with this one");
            Collider[] colliders = Physics.OverlapBox(transform.position, Vector3.one * 10);
            if (col == null)
            {
                col = GetComponent<Collider>();
            }
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != col)
                {
                    CheckCollider(colliders[i]);
                }
            }
        }

        void CheckCollider(Collider collision)
        {
            Debug.Log("PintoBoy Cam: moving to stop intersecting");
            if (collision.transform.position.x > transform.position.x)
            {
                Debug.Log("PintoBoy Cam: moving "+ (Vector3.right * (-50 - UnityEngine.Random.value)));
                transform.position = transform.position + (Vector3.right * (-50 - UnityEngine.Random.value));
                CheckColliders();
            }
            else
            {
                Debug.Log("PintoBoy Cam: moving " + (Vector3.right * (50 + UnityEngine.Random.value)));
                transform.position = transform.position + (Vector3.right * (50 + UnityEngine.Random.value));
                CheckColliders();
            }
        }
    }
}
