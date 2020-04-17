﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;

public class Shoot : MonoBehaviour
{

    NetworkManager netManager;
    public GameObject start;
    public GameObject direction;
    public Transform gun;
    public bool hasGravity = true;
    public float fireRate = 0.2f; // Was 0.5f, seemed too slow.
    float curtime = 0.0f;
    bool mousedown = false;
    // Update is called once per frame
    void Update()
    {
        if(netManager == null)
        {
            netManager = NetworkManager.Instance;
            return;
        }
        curtime += Time.deltaTime;
        if(Input.GetMouseButtonDown(0))
        {
            mousedown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousedown = false;
        }
        if (mousedown && curtime > fireRate)
        {
            /*Vector3 dir = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            GameObject bull = Instantiate(bullet, transform.localPosition + transform.forward, transform.rotation);
            Rigidbody rig = bull.GetComponent<Rigidbody>();
            rig.useGravity = false;
            //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
            //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
            rig.AddForce(cam.forward);*/
            ShootMsg shootmsg = new ShootMsg(netManager.id);

            //shootmsg.position = start.transform.position;
            //shootmsg.direction = direction.transform.position-shootmsg.position;
            //shootmsg.rotation = gun.transform.rotation;


            shootmsg.position = Camera.main.transform.position + Camera.main.transform.forward / 2;
            shootmsg.rotation = Camera.main.transform.rotation;
            shootmsg.direction = Camera.main.transform.forward;
            //shootmsg.direction = transform.rotation * Vector3.forward;
 
            netManager.SendMsg(shootmsg.GetBytes());
            curtime = 0;
        }

    }
}
