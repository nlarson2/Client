
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
            public int playerType;
        };

        public int id = 0;
        public int playerType = 1;

        Client client;

        public GameObject localPlayer;
        Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        Queue<int> addPlayerQ = new Queue<int>();
        Queue<int> destroyQ = new Queue<int>();
        Queue<StructureChangeMsg> addStructQ = new Queue<StructureChangeMsg>();
        Dictionary<int, GameObject> structures = new Dictionary<int, GameObject>();
        Queue<ShootMsg> bulletQ = new Queue<ShootMsg>();

        public GameObject playerPrefab;
        public GameObject PCPlayer;
        public GameObject VRPlayer;
        public GameObject StructurePrefab;
        public GameObject bulletPrefab;

        float sendDelay = 0.03f;
        float curTime = 0;


        Thread msgThread;


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
                    if(playerType == 2)
                        player.obj = Instantiate(VRPlayer, GetComponentInChildren<Transform>());
                    else
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
                GameObject obj;
                if (structures.ContainsKey(structMsg.from))
                {
                    Debug.Log("ALREADY EXISTS");
                    obj = structures[structMsg.from];
                }
                else {
                    obj = Instantiate(StructurePrefab, structMsg.pos, Quaternion.identity);
                    structures.Add(structMsg.from, obj);
                }
                Mesh mesh = new Mesh();
                obj.GetComponent<MeshFilter>().mesh = mesh;
                mesh.Clear();
                mesh.vertices = structMsg.Vertices;
                mesh.triangles = structMsg.Triangles;
                
                
            }

            while(bulletQ.Count > 0)
            {
                ShootMsg shoot = bulletQ.Dequeue();
                //GameObject bull = Instantiate(bulletPrefab, shoot.position, transform.rotation);
                //Rigidbody rig = bull.GetComponent<Rigidbody>();
                //rig.useGravity = false;
                //rig.AddForce(Physics.gravity * (rig.mass * rig.mass));
                //rig.AddForce((transform.forward + transform.up / 4) * 2.0f);
                //rig.AddForce(shoot.direction);
            }
        }

        public void ReceiveMessages()
        {
            byte[] newMsg;
            while (true)
            {
                newMsg = null;
                while (client.msgQueue.Count > 0)
                {
                    newMsg = client.msgQueue.Dequeue();
                    TranslateMsg(newMsg);
                }
                SpinWait.SpinUntil(() => client.msgQueue.Count > 0);
            }
        }

        public void TranslateMsg(byte[] bytes)
        {
            
            /*Message msg;
            msg = JsonUtility.FromJson<Message>(json);*/
            int msgType = Message.BytesToInt(Message.GetSegment(4, 4, bytes));
            //Debug.Log(json);
            //translate the messages type and call appropriate fucntion
            switch ((MsgType)msgType)
            {
                case MsgType.LOGIN:
                    Debug.Log("login");
                    LoginMsg login = new LoginMsg(bytes);
                    id = login.from;
                    login.playerType = this.playerType;
                    Login(bytes);
                    break;
                case MsgType.LOGOUT:
                    LogoutMsg logout = new LogoutMsg(bytes);
                    Debug.Log("RemovePlayer");
                    RemovePlayer(logout);
                    break;
                case MsgType.MOVE:
                    //MoveMsg move = new MoveMsg(bytes);
                    Debug.Log("Move");
                    Move(bytes);
                    break;
                case MsgType.MOVEVR:
                    //MoveMsg move = new MoveMsg(bytes);
                    Debug.Log("MoveVR");
                    MoveVR(bytes);
                    break;
                case MsgType.SHOOT:
                    Shoot(bytes);
                    break;
                case MsgType.ADDPLAYER:
                    Debug.Log("AddPlayer");
                    AddPlayer(bytes);
                    break;/*
                case MsgType.SNAPSHOT:
                    ProcessSnapshot(bytes);
                    Debug.Log("SnapShot");
                    break;*/
                case MsgType.STRUCTURE:
                    AddStructure(bytes);
                    Debug.Log("Structure");
                    break;

            }
        }

        public void Login(byte[] msg)
        { 
            /*LoginMsg outgoing = new LoginMsg(id);
            outgoing.from = id;*/
            client.SendMsg(msg);
        }

        public void Move(byte[] move)
        {
            
            MoveMsg msg = new MoveMsg(move);
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

        public void MoveVR(byte[] move)
        {

            MoveVRMsg msg = new MoveVRMsg(move);
            try
            {
                //Debug.Log(json);
                Player player = players[msg.from].playerControl;
                player.position = msg.pos;
                player.rotation = msg.playerRotation;
                player.cameraRotation = msg.cameraRotation;
                player.lHandPos = msg.lHandPosition;
                player.rHandPos = msg.rHandPosition;
                player.lHandRot = msg.lHandRotation;
                player.rHandRot = msg.rHandRotation;
            }
            catch (Exception e)
            {
                return;
            }
        }

        public void AddPlayer(byte[] addPlayer)
        {
            AddPlayerMsg msg = new AddPlayerMsg(addPlayer);
            if (msg.from != this.id)
            {
                PlayerData player = new PlayerData();
                player.id = msg.from;
                player.playerType = msg.playerType;
                //player.type = msg.msgType;
                addPlayerQ.Enqueue(player.id);
                players.Add(player.id, player);
            }
        }
        public void Shoot(byte[] shoot)
        {
            //ShootMsg shoot = JsonUtility.FromJson<ShootMsg>(msg);
            ShootMsg msg = new ShootMsg(shoot);
            bulletQ.Enqueue(msg);

        }
        public void ProcessSnapshot(string json)
        {
            SnapshotMsg snapshot = JsonUtility.FromJson<SnapshotMsg>(json);
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

        public void AddStructure(byte[] structMsg)
        {
            //StructureChangeMsg structMsg = JsonUtility.FromJson<StructureChangeMsg>(json);
            StructureChangeMsg msg = new StructureChangeMsg(structMsg);
            addStructQ.Enqueue(msg);
        }

        public void RemovePlayer(Message msg)
        {
            destroyQ.Enqueue(msg.from);
        }


        //convert message to string
        private byte[] MsgToBytes(Message obj)
        {
            string s = JsonUtility.ToJson(obj);
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        public void SendMsg(byte[] msg)
        {
            
            client.SendMsg(msg);

        }

        //send a close msg when the program terminates
        private void OnApplicationQuit()
        {
            LogoutMsg logOut = new LogoutMsg(this.id);
            client.SendMsg(logOut.GetBytes());
            client.Close();
        }

    }
}