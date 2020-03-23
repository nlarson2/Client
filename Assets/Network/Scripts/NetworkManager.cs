
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
            //string newMsg;
            byte[] newMsg;
            while (true)
            {
                //newMsg = string.Empty;
                while (client.msgQueue.Count > 0)
                {
                    newMsg = client.msgQueue.Dequeue();
                    TranslateMsg(newMsg);
                }
                SpinWait.SpinUntil(() => client.msgQueue.Count > 0);
            }
        }

        //public void TranslateMsg(string msg)
        public void TranslateMsg(byte[] MSG)
        {

            Message msg;

            //msg = JsonUtility.FromJson<Message>(json);
            byte[] json = MSG; //to clear Unity errors

            msg = cc.DeserializeMSG(MSG);

            //Debug.Log(json);
            //translate the messages type and call appropriate fucntion
            switch ((MsgType)msg.msgType)
            {
                case MsgType.LOGIN:
                    Debug.Log("login");
                    id = msg.from;
                    Login(id);
                    break;
                case MsgType.LOGOUT:
                    Debug.Log("RemovePlayer");
                    RemovePlayer(msg);
                    break;
                case MsgType.MOVE:
                    Debug.Log("Move");
                    Move(json);
                    break;
                case MsgType.SHOOT:
                    Shoot(json); //?
                    break;
                case MsgType.ADDPLAYER:
                    Debug.Log("AddPlayer");
                    AddPlayer(msg);
                    break;
                case MsgType.SNAPSHOT:
                    ProcessSnapshot(json); //?
                    Debug.Log("SnapShot");
                    break;
                case MsgType.STRUCTURE:
                    AddStructure(json); //?
                    Debug.Log("Structure");
                    break;

            }
        }

        public void Login(int id)
        { 
            Message outgoing = new LoginMsg(id);
            outgoing.from = id;
            client.SendMsg(cc.SerializeMSG(outgoing));
        }


        public void Move(byte[] json)
        {

            //MoveMsg msg = JsonUtility.FromJson<MoveMsg>(json);
            MoveMsg msg = cc.DeserializeMMSG(json);

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
        public void Shoot(byte[] msg)
        {
            //ShootMsg shoot = JsonUtility.FromJson<ShootMsg>(msg);
            ShootMsg shoot = cc.DeserializeSMSG(msg);

            bulletQ.Enqueue(shoot);

        }
        public void ProcessSnapshot(byte[] msg)
        {
            // SnapshotMsg snapshot = JsonUtility.FromJson<SnapshotMsg>(json);
            SnapshotMsg snapshot = cc.DeserializeSsMSG(msg);
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

        public void AddStructure(byte[] json)
        {
            //StructureChangeMsg structMsg = JsonUtility.FromJson<StructureChangeMsg>(json);
            StructureChangeMsg structMsg = cc.DeserializeSCMSG(json);

            addStructQ.Enqueue(structMsg);
        }

        public void RemovePlayer(Message msg)
        {
            destroyQ.Enqueue(msg.from);
        }


        //convert message to string
        private byte[] MsgToBytes(Message obj)
        {
            //string s = JsonUtility.ToJson(obj);
            return cc.SerializeMSG(obj);
        }

        public void SendMsg(Message msg)
        {
            //string json = JsonUtility.ToJson(msg);
            byte[] json = cc.SerializeMSG(msg);
            client.SendMsg(json);

        }

        //send a close msg when the program terminates
        private void OnApplicationQuit()
        {
            LogoutMsg logOut = new LogoutMsg(this.id);
            //client.SendMsg(JsonUtility.ToJson(logOut));
            client.SendMsg(cc.SerializeMSG(logOut));

            client.Close();
        }

    }
}