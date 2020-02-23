
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
        Queue<PlayerData> addPlayerQ = new Queue<PlayerData>();
        Queue<int> destroyQ = new Queue<int>();

        public GameObject playerPrefab;
        public GameObject PCPlayer;

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
                PlayerData player = addPlayerQ.Dequeue();

                if (player.id != this.id)
                {
                    player.obj = Instantiate(PCPlayer, GetComponentInChildren<Transform>());
                    player.playerControl = player.obj.GetComponent<Player>();
                    player.obj.name = "NetworkPlayer:" + player.id;
                    players.Add(player.id, player);
                }
                else
                {
                    localPlayer.name = "LocalNetworkPlayer:" + player.id;
                }

            }

            while (destroyQ.Count > 0)
            {
                int destroyPlayer = destroyQ.Dequeue();
                Destroy(this.players.ElementAt(destroyPlayer).Value.obj);
                players.Remove(destroyPlayer);
            }

        }

        public void ReceiveMessages()
        {
            string newMsg;
            while (true)
            {
                newMsg = string.Empty;
                while (client.msgQueue.Count > 0)
                {
                    newMsg = client.msgQueue.Dequeue();
                    TranslateMsg(newMsg);
                }
                SpinWait.SpinUntil(() => client.msgQueue.Count > 0);
            }
        }

        public void TranslateMsg(string json)
        {
            
            Message msg;
            msg = JsonUtility.FromJson<Message>(json);
            Debug.Log(json);
            //translate the messages
            switch ((MsgType)msg.msgType)
            {
                case MsgType.LOGIN:
                    Debug.Log("login");
                    id = msg.to;
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
                case MsgType.ADDPLAYER:
                    Debug.Log("AddPlayer");
                    AddPlayer(msg);
                    break;

            }
        }

        public void Login(int id)
        { 
            Message outgoing = new LoginMsg(id);
            outgoing.from = id;
            client.SendMsg(JsonUtility.ToJson(outgoing));
        }

        public void Move(string json)
        {
            MoveMsg msg = JsonUtility.FromJson<MoveMsg>(json);
            Player player = players.ElementAt(msg.from).Value.obj.GetComponent<Player>();
            player.position = new Vector3(msg.x, msg.y, msg.z);
            player.rotate = new Quaternion(msg.xr, msg.yr, msg.zr, 0);
            
        }

        public void AddPlayer(Message msg)
        {
            PlayerData player = new PlayerData();
            player.id = msg.from;
            //player.type = msg.msgType;sf  
            addPlayerQ.Enqueue(player);
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

        public void SendMsg(Message msg)
        {
            string json = JsonUtility.ToJson(msg);
            client.SendMsg(json);

        }

        //send a close msg when the program terminates
        private void OnApplicationQuit()
        {
            LogoutMsg logOut = new LogoutMsg(this.id);
            client.SendMsg(JsonUtility.ToJson(logOut));
            client.Close();
        }

    }
}