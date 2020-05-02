using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;

public class Shoot : MonoBehaviour
{

    NetworkManager netManager;
    public GameObject start;
    public GameObject direction;
    public Transform gun;
    public GameObject grenade;
    public bool hasGravity = true;
    float fireRate = 0.25f; // Was 0.5f, seemed too slow.
    float throwRate = 5.0f;
    float curtime = 0.0f;
    float nadeTime = 0.0f;
    bool mousedown = false;
    bool grenadeThrown = false;
    // Update is called once per frame
    void Update()
    {

        if(netManager == null)
        {
            netManager = NetworkManager.Instance;
            return;
        }
        curtime += Time.deltaTime;
        nadeTime += Time.deltaTime;

        // IF left mouse click pressed -> shoot 
        if (Input.GetMouseButtonDown(0))
        {
            if (curtime > fireRate)
            {

                ShootMsg shootmsg = new ShootMsg(netManager.id);

                shootmsg.shootType = 0; // Defualt is 0 for bullets
                shootmsg.position = Camera.main.transform.position + Camera.main.transform.forward / 2;
                shootmsg.rotation = Camera.main.transform.rotation;
                shootmsg.direction = Camera.main.transform.forward;

                netManager.SendMsg(shootmsg.GetBytes());
                curtime = 0;
            }
        }
       

        //IF E key pressed -> Throw Grenade
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (nadeTime > throwRate)
            {
                ShootMsg shootmsg = new ShootMsg(netManager.id);

                shootmsg.shootType = 1; // This value is for rockets
                shootmsg.position = Camera.main.transform.position + Camera.main.transform.forward / 2;
                shootmsg.rotation = Camera.main.transform.rotation;
                shootmsg.direction = Camera.main.transform.forward;

                netManager.SendMsg(shootmsg.GetBytes());
                nadeTime = 0;       // Reset nade throw timer
            }
        }

    }
}
