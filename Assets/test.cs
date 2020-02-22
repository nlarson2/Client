using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmashDomeNetwork;

public class test : MonoBehaviour
{

    bool firstRun;
    Message testMsg;
    MoveMsg testMove;
    string testString;
    // Start is called before the first frame update
    void Start()
    {
        firstRun = true;
        testMove = new MoveMsg(1);
        testMove.x = 1; testMove.y = 2; testMove.z = 3;
        testString = JsonUtility.ToJson(testMove);
        testMsg = JsonUtility.FromJson<MoveMsg>(testString);

    }

    // Update is called once per frame
    void Update()
    {
        if(firstRun)
        {
            if ((MsgType)testMsg.msgType == MsgType.MOVE)
            {
                MoveMsg msg = JsonUtility.FromJson<MoveMsg>(testString);
                Debug.Log(msg);
                Debug.Log(msg.x);
                Debug.Log(msg.y);
                Debug.Log(msg.z);
            }
            firstRun = false;
        }
        
    }
}
