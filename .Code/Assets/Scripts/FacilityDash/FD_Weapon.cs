using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FD_Weapon : MonoBehaviour
{
    Animator movementAnim;
    FD_Shovel shovel;
    Animator weaponAnim;
    string strDeadString = "Dead";
    public UnityEvent hit;
    bool isDead;
    float startYHeight = 0;
    float deadYHeight = -40;
    float deathFallSpeed = 4;
    public UnityEvent dead;

    // Start is called before the first frame update
    void Awake()
    {
        if(movementAnim == null)
        {
            movementAnim = GetComponent<Animator>();
        }
        startYHeight = transform.localPosition.y;

        shovel = transform.Find("Shovel").GetComponent<FD_Shovel>();
        shovel.hit.AddListener(Swing);
    }

    // Update is called once per frame
    void Update()
    {
        //if (isDead)
        //{
        //    if(transform.localPosition.y > deadYHeight)
        //    {
        //        transform.localPosition = Vector3.down * deathFallSpeed * Time.deltaTime;
        //    }
        //    else
        //    {
        //        isDead = false;
        //        dead.Invoke();
        //    }
        //}
    }

    public void Swing()
    {
        hit.Invoke();
    }

    public void TurnOff()
    {
        shovel.spriteRenderer.enabled = false;
    }

    public void TurnOn()
    {
        shovel.spriteRenderer.enabled = true;
    }

    public void Dead()
    {
        isDead = true;
        movementAnim.SetBool(strDeadString, isDead);
        dead.Invoke();
    }

    public void GameStarted()
    {
        isDead = false;
        transform.localPosition = new Vector3(0, startYHeight, 0);
        if(movementAnim == null)
        {
            movementAnim = GetComponent<Animator>();
        }
        movementAnim.SetBool(strDeadString, isDead);
    }
}
