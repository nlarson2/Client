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
    public float fireRate = 0.2f; // Was 0.5f, seemed too slow.
    public float throwRate = 0.5f;
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

                shootmsg.position = Camera.main.transform.position + Camera.main.transform.forward / 2;
                shootmsg.rotation = Camera.main.transform.rotation;
                shootmsg.direction = Camera.main.transform.forward;

                netManager.SendMsg(shootmsg.GetBytes());
                curtime = 0;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousedown = false;
        }

        //IF E key pressed -> Throw Grenade
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("G is pressed");
            grenadeThrown = true;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            Debug.Log("G is releasd");
            grenadeThrown = false;
        }

        

        if (grenadeThrown && nadeTime > fireRate)
        {
            ShootMsg shootmsg = new ShootMsg(netManager.id);

            shootmsg.position = Camera.main.transform.position + Camera.main.transform.forward / 2;
            shootmsg.rotation = Camera.main.transform.rotation;
            shootmsg.direction = Camera.main.transform.forward;

            netManager.SendMsg(shootmsg.GetBytes());
            nadeTime = 0;       // Reset nade throw timer
        }


    }
}
