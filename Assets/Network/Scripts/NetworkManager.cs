
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
            public int personType;
        };

        public int id = 0;
        public int playerType = 1;

        Client client;

        public GameObject localPlayer;
        private LocalPlayer localPlayerScript;
        Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        Queue<int> addPlayerQ = new Queue<int>();
        Queue<int> destroyQ = new Queue<int>();
        Queue<StructureChangeMsg> addStructQ = new Queue<StructureChangeMsg>();
        Dictionary<int, GameObject> structures = new Dictionary<int, GameObject>();
        Queue<ShootMsg> bulletQ = new Queue<ShootMsg>();
        Queue<RespawnMsg> respawnQ = new Queue<RespawnMsg>();

        Dictionary<int, Snapshot> netobjects = new Dictionary<int, Snapshot>();
        Queue<NetObjectMsg> netInstantiate = new Queue<NetObjectMsg>();


        public GameObject playerPrefab;
        //public GameObject PCPlayer;
        public GameObject VRPlayer;
        public GameObject StructurePrefab;
        public GameObject bulletPrefab;
        public GameObject netCubePrefab;
        public Material mat1, mat2, mat3;
        Control controller;

        public GameObject Basic;
        public GameObject BrandonH;
        public GameObject BrandonB;
        public GameObject DOMINANT;
        
        float sendDelay = 0.03f;
        float curTime = 0;
        bool resetScene = false;

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
            controller = GameObject.Find("Controller").GetComponent<Control>();
            //localPlayerScript = localPlayer.GetComponent<LocalPlayer>();
            print("TEST");
            Instance = this;
            msgThread = new Thread(ReceiveMessages);
            msgThread.Start();
        }
       
        private void Update()
        {
            if (resetScene)
            {
                ResetGame();
                resetScene = false;
            }
            while (addPlayerQ.Count > 0)
            {
                int playerID = addPlayerQ.Dequeue();
                PlayerData player = players[playerID];
                if (player.id != this.id)
                {
                    if (player.playerType == 2)
                    {
                        //Debug.Log(string.Format("PersonType: {0}", player.personType));
                        //Debug.Log("INSTANTIATING VRPLAYER");
                        player.obj = Instantiate(VRPlayer, GetComponentInChildren<Transform>());
                    }
                    else
                    {
                        GameObject playerObj = Basic;
                        switch (player.personType)
                        {
                            case 1:
                                //Debug.Log("CHANGED TO BRANDON H");
                                playerObj = BrandonH;
                                break;
                            case 2:
                                playerObj = BrandonB;
                                break;
                            case 3:
                                playerObj = DOMINANT;
                                break;
                            default:
                                //Debug.Log("DEFAULTING");
                                break;
                        }
                 
                        player.obj = Instantiate(playerObj, GetComponentInChildren<Transform>());
                    }

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
                ////Debug.Log(destroyPlayer);
                Destroy(this.players[destroyPlayer].obj);
                players.Remove(destroyPlayer);
            }

            while (addStructQ.Count > 0)
            {
                StructureChangeMsg structMsg = addStructQ.Dequeue();
                GameObject obj;
                if (structures.ContainsKey(structMsg.from))
                {
                    ////Debug.Log("ALREADY EXISTS");
                    obj = structures[structMsg.from];
                }
                else {
                    obj = Instantiate(StructurePrefab, structMsg.pos, Quaternion.identity);
                    structures.Add(structMsg.from, obj);
                    //Material material = obj.GetComponent<Material>();
                    //Debug.Log(string.Format("TextureType: {0}", structMsg.textureType));
                    switch(structMsg.textureType)
                    {
                        case 0:
                            obj.GetComponent<MeshRenderer>().material = mat1;
                            break;
                        case 1:
                            obj.GetComponent<MeshRenderer>().material = mat2;
                            break;
                        case 2:
                            obj.GetComponent<MeshRenderer>().material = mat3;
                            break;
                    }

                }
                Mesh mesh = new Mesh();
                obj.GetComponent<MeshFilter>().mesh = mesh;
                mesh.Clear();
                mesh.vertices = structMsg.Vertices;
                mesh.triangles = structMsg.Triangles;
                Vector2[] uvs = new Vector2[mesh.vertices.Length];
                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
                    uvs[i].Scale(obj.transform.localScale);
                }
                mesh.uv = uvs;
                mesh.RecalculateNormals();
                MeshCollider collider = obj.GetComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                
                
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

            while(netInstantiate.Count > 0)
            {
                NetObjectMsg objs = netInstantiate.Dequeue();
                //does not exist, add new
                //Snapshot snap = new Snapshot();
                Debug.Log("HERE AT THE STUPID SPOT");
                GameObject netCube = Instantiate(netCubePrefab, objs.positions, objs.rotation);
                switch (objs.textureType)
                {
                    case 0:
                        netCube.GetComponent<MeshRenderer>().material = mat1;
                        break;
                    case 1:
                        netCube.GetComponent<MeshRenderer>().material = mat2;
                        break;
                    case 2:
                        netCube.GetComponent<MeshRenderer>().material = mat3;
                        break;
                }
                Snapshot snap = netCube.GetComponent<Snapshot>();
                snap.objID = objs.objID;
                snap.scale = objs.localScale;
                snap.pos = objs.positions;
                snap.rot = objs.rotation;

                //netobjects.Add(snap.objID, snap.GetObject());
                //Debug.Log(snap.objID);
                netobjects.Add(snap.objID, snap);

                
            }
            while(respawnQ.Count > 0)
            {
                RespawnMsg msg = respawnQ.Dequeue();
                if (localPlayer != null){
                    LocalPlayer lp = localPlayer.GetComponent<LocalPlayer>();
                    if (lp.lastRespawn > 3.0f)
                    {
                        localPlayer.SetActive(false);

                        localPlayer.transform.position = msg.pos;
                        //localPlayer.GetComponent<LocalPlayer>().respawn = true;
                        //localPlayer.GetComponent<LP>().respawnPos = ###;
                        localPlayer.SetActive(true);
                        //Debug.Log("Moving player to Respawn Position..");
                        lp.lastRespawn = 0.0f;
                    }
                }
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

            try
            {


                /*Message msg;
                msg = JsonUtility.FromJson<Message>(json);*/
                int msgType = Message.BytesToInt(Message.GetSegment(4, 4, bytes));
                ////Debug.Log(json);
                //translate the messages type and call appropriate fucntion
                switch ((MsgType)msgType)
                {
                    case MsgType.LOGIN:
                        ////Debug.Log("login");
                        LoginMsg login = new LoginMsg(bytes);
                        id = login.from;
                        login.playerType = controller.playerType;
                        login.personType = controller.personType;
                        //Debug.Log(string.Format("PERSON: {0}", id));
                        Login(login.GetBytes());
                        break;
                    case MsgType.LOGOUT:
                        LogoutMsg logout = new LogoutMsg(bytes);
                        ////Debug.Log("RemovePlayer");
                        RemovePlayer(logout);
                        break;
                    case MsgType.MOVE:
                        //MoveMsg move = new MoveMsg(bytes);
                        ////Debug.Log("Move");
                        Move(bytes);
                        break;
                    case MsgType.MOVEVR:
                        //MoveMsg move = new MoveMsg(bytes);
                        ////Debug.Log("MoveVR");
                        MoveVR(bytes);
                        break;
                    case MsgType.SHOOT:
                        Shoot(bytes);
                        break;
                    case MsgType.ADDPLAYER:
                        //Debug.Log("AddPlayer");
                        AddPlayer(bytes);
                        break;
                    case MsgType.SNAPSHOT:
                        ProcessSnapshot(bytes);
                        //Debug.Log("SnapShot");
                        break;
                    case MsgType.NETOBJECT:
                        //Debug.Log("NETOBJECT");
                        NetObject(bytes);
                        break;
                    case MsgType.STRUCTURE:
                        AddStructure(bytes);
                        //Debug.Log("Structure");
                        break;
                    case MsgType.RESPAWN:
                        Respawn(bytes);
                        //Debug.Log("RESPAWN");
                        break;
                    case MsgType.RESET:
                        resetScene = true;
                        break;
                }
            }
            catch (Exception e)
            {
                //Debug.Log("something wrong with a message");
            }
        }

        public void Login(byte[] msg)
        {
            /*LoginMsg outgoing = new LoginMsg(id);
            outgoing.from = id;*/
            //Debug.Log("LOGIN SENT");
            client.SendMsg(msg);
        }

        public void Move(byte[] move)
        {

            MoveMsg msg = new MoveMsg(move);
            try
            {
                ////Debug.Log(json);
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
                ////Debug.Log(json);
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
                player.personType = msg.personType;
                //Debug.Log(string.Format("Player:  {0}   Person:{1}", player.playerType, player.personType));
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
        public void ProcessSnapshot(byte[] _snapshot)
        {
            SnapshotMsg snapshot = new SnapshotMsg(_snapshot);
            //Debug.Log(string.Format("Snapshot: {0}", snapshot.objID.Count));
            ////Debug.Log(snapshot.userId.Count);
            for (int i = 0; i < snapshot.objID.Count; i++)
            {
                try
                {
                    //Debug.Log(string.Format("Pos: {0}", snapshot.positions[i]));
                    int id = snapshot.objID[i];
                    Snapshot cube = netobjects[id];

                    cube.pos = snapshot.positions[i];
                    cube.rot = snapshot.rotation[i];
                    //player.playerControl.linear_speed = snapshot.linear_speed[i];
                    //player.playerControl.angular_speed = snapshot.angular_speed[i];
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

        public void Respawn(byte[] respawnBytes)
        {
            RespawnMsg respawnMsg = new RespawnMsg(respawnBytes);
            respawnQ.Enqueue(respawnMsg);
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

        private void NetObject(byte[] msg)
        {
            NetObjectMsg objs = new NetObjectMsg(msg);
            ////Debug.Log(string.Format("ObjID.Count: {0}", objs.objID.Count));
            netInstantiate.Enqueue(objs);
            
        }
        void ResetGame()
        {
            GameObject[] cubes;
            cubes = GameObject.FindGameObjectsWithTag("Net Cube");
            Debug.Log("Reset");
            Debug.Log("HERE");
            Debug.Log("ERROR");
            Debug.Log($"cubes: {cubes.Length}");
            netInstantiate = new Queue<NetObjectMsg>();
            netobjects = new Dictionary<int, Snapshot>();
            foreach (GameObject cube in cubes) 
            {
                GameObject.Destroy(cube);
            }
        }
    }
}