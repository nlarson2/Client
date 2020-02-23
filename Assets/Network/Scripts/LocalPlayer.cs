using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;


public class LocalPlayer : MonoBehaviour
{

    NetworkManager networkManager = NetworkManager.Instance;

    public float speed = 100.0f;
    public float time = 0;
    public Vector3 PrevPosition;
    public Vector3 PrevLHandPos;
    public Vector3 PrevRHandPos;
    public Quaternion PrevRotate;
    public Quaternion PrevLHandRot;
    public Quaternion PrevRHandRot;

    public Transform camera;
    public GameObject lHand;
    public GameObject rHand;



    // Update is called once per frame
    void Update()
    {
        if(networkManager == null)
        {
            networkManager = NetworkManager.Instance;
            time = 0;
            return;
        }
        time += Time.deltaTime;

        if (time > 0.5 && (
                PrevPosition != transform.position ||
                PrevRotate.y != transform.rotation.y ||
                PrevRotate.x != transform.rotation.x)
            )
        {
            MoveMsg movementMsg = new MoveMsg(networkManager.id);
            Vector3 pos = transform.position;
            movementMsg.x = pos.x;
            movementMsg.y = pos.y;
            movementMsg.z = pos.z;
            movementMsg.xr = camera.rotation.x;
            movementMsg.yr = transform.rotation.y;
            movementMsg.zr = transform.rotation.z;
            movementMsg.wr = transform.rotation.w;
            
            networkManager.SendMsg(movementMsg);
            time = 0;
        }

        PrevPosition = transform.position;
        PrevRotate.x = camera.rotation.x;
        PrevRotate.y = transform.rotation.y;
    }
}
