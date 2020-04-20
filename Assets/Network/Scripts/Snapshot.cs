using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmashDomeNetwork
{
    public class Snapshot : MonoBehaviour
    {

        NetworkManager net = NetworkManager.Instance;

        public float speed = 10.0f;

        public Vector3 linear_speed; //directional speed, move to this for autonomy
        public Quaternion angular_speed; //rotation speed

        public GameObject obj;
        public Vector3 scale;
        public Vector3 pos;
        public Quaternion rot;
        public int objID;

        // Start is called before the first frame update
        void Start()
        {
            this.obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //this.obj = gameObject.transform.gameObject; //later will need dimensions specs
            this.scale = gameObject.transform.localScale;
            this.pos = gameObject.transform.position;
            this.rot = gameObject.transform.rotation;

        }

        // Update is called once per frame
        void Update()
        {
            //if pos or rot change
            // snapshotUpdate();

            if (pos != transform.position)
            {
                if (Vector3.Distance(pos, transform.position) > 10.0f)
                    transform.position = pos;
                else
                    transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * speed);
            }
            if (rot != transform.rotation)
            {
                transform.eulerAngles = rot.eulerAngles;
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.1f);
            }
        }
    }
}