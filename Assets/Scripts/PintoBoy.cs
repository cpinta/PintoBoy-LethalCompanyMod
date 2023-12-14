using PintoMod;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PintoBoy : RemoteProp
{
    GameObject player;
    GameObject screen;
    ScanNodeProperties scanNodeProperties;
    Vector3 playerStartPos;
    float fallSpeed = 0.1f;
    float jumpHeight = 5f;

    // Start is called before the first frame update
    void Awake()
    {
        player = transform.Find("PintoScreen/PintoPlayer").gameObject;
        screen = transform.Find("PintoScreen").gameObject;
        scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();

        useCooldown = 0.5f;

        playerStartPos = player.transform.localPosition;
        Enable(false);

        grabbable = true;
        parentObject = this.transform;
        itemProperties = Pinto_ModBase.pintoGrab;

        customGrabTooltip = "Pinto grab tooltup custom";
        grabbableToEnemies = true;
        remoteAudio = this.GetComponent<AudioSource>();


        scanNodeProperties.maxRange = 100;
        scanNodeProperties.minRange = 0;
        scanNodeProperties.headerText = "PintoBoy";
        scanNodeProperties.subText = "PintoBoy Subtext";
        scanNodeProperties.creatureScanID = -1;
        scanNodeProperties.nodeType = 1;
    }



    // Update is called once per frame
    void Update()
    {
        if (playerStartPos.y < player.transform.localPosition.y)
        {
            player.transform.localPosition -= new Vector3(0, fallSpeed, 0);
        }


        if (playerStartPos.y > player.transform.localPosition.y)
        {
            player.transform.localPosition = playerStartPos;
        }
    }

    public void Jump()
    {
        player.transform.localPosition += new Vector3(0, jumpHeight, 0);
    }

    private void Enable(bool enable, bool inHand = true)
    {
        screen.SetActive(enable);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        Jump();

        Debug.Log("Pinto Button pressed");
    }

    public override void UseUpBatteries()
    {
        base.UseUpBatteries();
        Enable(false, false);
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        return 0;
    }

    public bool IsAnythingNull()
    {
        if (player == null)
        {
            Pinto_ModBase.Instance.Value.logger.LogInfo("Player is null");
        }
        if (screen == null)
        {
            Pinto_ModBase.Instance.Value.logger.LogInfo("Screen is null");
        }
        if (playerStartPos == null)
        {
            Pinto_ModBase.Instance.Value.logger.LogInfo("PlayerStartPos is null");
        }

        return false;
    }
}
