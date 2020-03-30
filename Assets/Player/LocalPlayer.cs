/*Script meant to be placed on local player
 *and send messages to the server based on movement*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;


public class LocalPlayer : MonoBehaviour
{

    NetworkManager networkManager;

    public float speed = 100.0f;
    public float time = 0;

    /*All position required to check if playesr has moved*/
    public Vector3 PrevPosition;
    public Vector3 PrevLHandPos;
    public Vector3 PrevRHandPos;
    public Quaternion PrevRotate;
    public Quaternion PrevLHandRot;
    public Quaternion PrevRHandRot;

    public new Transform camera;
    public GameObject lHand;
    public GameObject rHand;



    
    void Update()
    {
        //only run the update if the network manager has been established
        if(networkManager == null)
        {
            networkManager = NetworkManager.Instance;
            return;
        }
        //track time to only send messages at certain intervals
        time += Time.deltaTime;

        //only send message if a change has occured //idk if this actually works
        if (time > 0.1 && (
                PrevPosition != transform.position ||
                PrevRotate.y != transform.rotation.y ||
                PrevRotate.x != transform.rotation.x)
            )
        {
            MoveMsg movementMsg = new MoveMsg(networkManager.id);
            movementMsg.pos = transform.position;
            movementMsg.playerRotation = transform.rotation;
            movementMsg.cameraRotation = camera.transform.rotation;
            
            networkManager.SendMsg(movementMsg.GetBytes());
            time = 0;
        }

        //update previous information
        PrevPosition = transform.position;
        PrevRotate.x = camera.rotation.x;
        PrevRotate.y = transform.rotation.y;
    }

}
