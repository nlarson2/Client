
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using System.Linq;

namespace SmashDomeNetwork
{
    public class NetworkManager : MonoBehaviour
    {
        public struct PlayerData
        {
            public int id;
            public int type;
            public GameObject obj;
            public Player playerControl;
        };

        public int id = 0;
        public int playerType = 1;

        Client client;

        public GameObject localPlayer;
        Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        Queue<int> addPlayerQ = new Queue<int>();
        Queue<int> destroyQ = new Queue<int>();
        Queue<StructureChangeMsg> addStructQ = new Queue<StructureChangeMsg>();
        Queue<ShootMsg> bulletQ = new Queue<ShootMsg>();

        public GameObject playerPrefab;
        public GameObject PCPlayer;
        public GameObject StructurePrefab;
        public GameObject bulletPrefab;

        float sendDelay = 0.03f;
        float curTime = 0;


        Thread msgThread;

        public Cerealize cc = new Cerealize();


        //Set up to make NetworkManger a singleton
        private NetworkManager() { }
        private static NetworkManager instance = null;
        private static readonly object padlock = new object(); //lock down the Instance
        public static NetworkManager Instance
        {
            get
            {
                return instance;
            }
            set
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = value;
                        instance.client = new Client();
                        instance.client.Connect();
                    }
                }

            }

        }  

        private void Start()
        {
            print("TEST");
            Instance = this;
            msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
        }
       
        private void Update()
        {
            while (addPlayerQ.Count > 0)
            {
                int playerID = addPlayerQ.Dequeue();
                PlayerData player = players[playerID];
                if (player.id != this.id)
                {
                    player.obj = Instantiate(PCPlayer, GetComponentInChildren<Transform>());
                    player.playerControl = player.obj.GetComponent<Player>();
                    player.obj.name = "NetworkPlayer:" + player.id;
                    players.Remove(playerID);
                    players.Add(playerID, player);
                    
                }
                else
                {
                    localPlayer.name = "LocalNetworkPlayer:" + player.id;
                }

            }

            while (destroyQ.Count > 0)
            {
                int destroyPlayer = destroyQ.Dequeue();
                Debug.Log(destroyPlayer);
                Destroy(this.players[destroyPlayer].obj);
                players.Remove(destroyPlayer);
            }

            while (addStructQ.Count > 0)
            {
                StructureChangeMsg structMsg = addStructQ.Dequeue();
                GameObject obj = Instantiate(StructurePrefab, structMsg.pos, Quaternion.identity);
                Mesh mesh = new Mesh();
                obj.GetComponent<MeshFilter>().mesh = mesh;
                mesh.Clear();
                mesh.vertices = structMsg.vertices;
                mesh.triangles = structMsg.triangles;
                
            }

            while(bulletQ.Count > 0)
            {
                ShootMsg shoot = bulletQ.Dequeue();
                GameObject bull = Instantiate(bulletPrefab, shoot.position, transform.rotation);
                Rigidbody rig = bull.GetComponent<Rigidbody>();
                rig.useGravity = false;
                //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
                //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
                rig.AddForce(shoot.direction);
            }
        }

        public void ReceiveMessages()
        {
            byte[] newMsg;
            while (true)
            {
                
                while (client.msgQueue.Count > 0)
                {
                    newMsg = client.msgQueue.Dequeue();
                    TranslateMsg(newMsg);
                }
                SpinWait.SpinUntil(() => client.msgQueue.Count > 0);
            }
        }

        public void TranslateMsg(byte[] MSG)
        {

            /* Message msg;
             msg = cc.DeserializeMSG(MSG);*/
            int type = cc.ByteInt32(MSG[4]);
            
            //translate the messages type and call appropriate fucntion
            switch ((MsgType)type)
            {
                case MsgType.LOGIN:
                    LoginMsg msg = cc.DeserializeLiMSG(MSG);
                    Debug.Log("login");
                    id = msg.from;
                    Login(id);
                    break;
                case MsgType.LOGOUT:
                    LogoutMsg lmsg = cc.DeserializeLoMSG(MSG);
                    Debug.Log("RemovePlayer");
                    RemovePlayer(lmsg);
                    break;
                case MsgType.MOVE:
                    MoveMsg mmsg = cc.DeserializeMMSG(MSG);
                    Debug.Log("Move");
                    Move(MSG);
                    break;
                case MsgType.SHOOT:
                    ShootMsg smsg = cc.DeserializeSMSG(MSG);
                    Shoot(MSG);
                    break;
                case MsgType.ADDPLAYER:
                    AddPlayer amsg = cc.DeserializeAPMSG(MSG);
                    Debug.Log("AddPlayer");
                    AddPlayer(amsg);
                    break;
                case MsgType.SNAPSHOT:
                    SnapshotMsg ssmsg = cc.DeserializeSsMSG(MSG);
                    ProcessSnapshot(MSG);
                    Debug.Log("SnapShot");
                    break;
                case MsgType.STRUCTURE:
                    StructureChangeMsg tmsg = cc.DeserializeSCMSG(MSG);
                    AddStructure(MSG);
                    Debug.Log("Structure");
                    break;

            }
        }

        public void Login(int id)
        { 
            LoginMsg outgoing = new LoginMsg(id);
            outgoing.from = id;
            client.SendMsg(cc.SerializeMSG(outgoing));
        }

        public void Move(byte[] MSG)
        {
            
            MoveMsg msg = cc.DeserializeMMSG(MSG);
            try
            {
                //Debug.Log(json);
                Player player = players[msg.from].playerControl;
                player.position = msg.pos;
                player.rotation = msg.playerRotation;
                player.cameraRotation = msg.cameraRotation;
            }
            catch (Exception e)
            {
                return;
            }
        }

        public void AddPlayer(Message msg)
        {
            if (msg.from != this.id)
            {
                PlayerData player = new PlayerData();
                player.id = msg.from;
                //player.type = msg.msgType;
                addPlayerQ.Enqueue(player.id);
                players.Add(player.id, player);
            }
        }
        public void Shoot(byte[] MSG)
        {
            ShootMsg shoot = cc.DeserializeSMSG(MSG);
            bulletQ.Enqueue(shoot);

        }
        public void ProcessSnapshot(byte[] MSG)
        {
            SnapshotMsg snapshot = cc.DeserializeSsMSG(MSG);
            Debug.Log(snapshot.userId.Count);
            for(int i = 0; i < snapshot.userId.Count; i++)
            {
                try
                {
                    if (snapshot.userId[i] == this.id) continue;

                    int playerID = snapshot.userId[i];
                    Player player = players[playerID].obj.GetComponent<Player>();
                    player.position = snapshot.positions[i];
                    player.rotation = snapshot.rotation[i];
                    player.cameraRotation = snapshot.camRotation[i];
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        public void AddStructure(byte[] MSG)
        {
            StructureChangeMsg structMsg = cc.DeserializeSCMSG(MSG);
            addStructQ.Enqueue(structMsg);
        }

        public void RemovePlayer(Message msg)
        {
            destroyQ.Enqueue(msg.from);
        }


        //convert message to string
        private byte[] MsgToBytes(Message obj)
        {
            byte[] s = cc.SerializeMSG(obj);
            return s;
        }

        public void SendMsg(Message msg)
        {
            byte[] MSG = cc.SerializeMSG(msg);
            client.SendMsg(MSG);

        }

        //send a close msg when the program terminates
        private void OnApplicationQuit()
        {
            LogoutMsg logOut = new LogoutMsg(this.id);
            client.SendMsg(cc.SerializeMSG(logOut));
            client.Close();
        }

    }
}