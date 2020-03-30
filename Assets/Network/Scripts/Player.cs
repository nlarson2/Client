﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmashDomeNetwork
{

    public class Player : MonoBehaviour
    {

        NetworkManager networkManager = NetworkManager.Instance;

        public float speed = 10.0f;

        public Vector3 position;
        public Vector3 lHandPos;
        public Vector3 rHandPos;
        public Quaternion rotation;
        public Quaternion lHandRot;
        public Quaternion rHandRot;
        public Quaternion cameraRotation;

        public GameObject lHand;
        public GameObject rHand;
        public GameObject body;



        // Update is called once per frame
        void Update()
        {

            if (position != transform.position)
            {
                //if the destination gets more than 10 away from the player, it snaps them to the correct postions
                if (Vector3.Distance(position, transform.position) > 10.0f)
                    transform.position = position;
                else
                    transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * speed);
            }
            if (rotation != transform.rotation)
            {
                //meant to gradually rotate player
                transform.eulerAngles = rotation.eulerAngles;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.1f);
            }
            if (cameraRotation != body.transform.rotation)
            {
                body.transform.eulerAngles = cameraRotation.eulerAngles;
                body.transform.rotation = Quaternion.Slerp(body.transform.rotation, cameraRotation, 0.1f);
            }

            if (lHand != null)
            {
                /*Left hand*/
                if (lHandPos != lHand.transform.localPosition)
                    lHand.transform.localPosition = lHandPos;
                if (lHandRot != lHand.transform.rotation)
                    lHand.transform.eulerAngles = lHandRot.eulerAngles;

                /*Right hand*/
                if (rHandPos != rHand.transform.localPosition)
                    rHand.transform.localPosition = rHandPos;
                if (rHandRot != rHand.transform.rotation)
                    rHand.transform.eulerAngles = rHandRot.eulerAngles;
            }
        }
    }
}