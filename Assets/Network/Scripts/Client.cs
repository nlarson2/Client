
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


namespace SmashDomeNetwork
{
    public class Client
    {

        TcpClient cli;
        NetworkStream stream;

        public Cerealize cc = new Cerealize();

        //queue to store messages in order
        public Queue<byte[]> msgQueue;


        public Client()
        {
            msgQueue = new Queue<byte[]>();
        }

        public void Connect()
        {
            int count = 0;
            cli = new TcpClient();
            try
            {
                cli.Connect("47.6.148.20", 44444);
                //cli.Connect("localhost", 50000);
            }
            catch (Exception e)
            {
                Debug.Log("FAILED TO CONNECT");
                Application.Quit(0);
                count++;
            }
            Debug.Log("HERE");
            if (cli.Connected)
            {
                try
                {
                    stream = cli.GetStream();
                    Console.WriteLine("Connected\n");
                    //start receiving messages
                    Thread t = new Thread(RecvMsg);
                    t.Start();
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
            else
            {
                Console.WriteLine("Not Connected");
            }
        }

        //function that can be used to send messages to the server
        public void SendMsg(byte[] msg)
        {
            //Debug.Log(msg);
            try
            {
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("MESSAGE SENT");

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void RecvMsg()
        {
            byte[] buffer;//hold the bytes
                //int byteC;//byte count
            int buffSize = cli.ReceiveBufferSize;//how many bytes can be held
                                                 //String message;
            //NetworkStream stream = cli.stream;
            byte[] message;
            Boolean hasMSG = false;
            try
            {
                while (true)
                {
                    //used to determine level of json msg
                        //Some json msgs will have multiple sets of brackets
                        
                        //message = string.Empty;
                        while (true)
                        {
                            
                            buffer = new byte[4];

                            int index = 0; int res = 0;
                            while (index != 4)
                            {
                                res = stream.ReadByte();
                                if (res >= 0)
                                {
                                    buffer[index++] = (byte)(char)res;
                                }
                            }
                            Int32 msgSize = cc.ByteInt32(buffer);
                            if (msgSize < 9)
                                break;
                            Debug.Log("Read message size.");

                            
                            message = new byte[msgSize]; 
                            Array.Copy(buffer, 0, message, 0, 4);

                           
                            index = 4; res = 0;
                            while (index != msgSize && msgSize > 0)
                            {
                                res = stream.ReadByte();
                                if (res >= 0)
                                {
                                    message[index++] = (byte)(char)res;
                                }
                                //stream.Read(message, 4, msgSize);
                            }
                            //message = buffer;
                            msgQueue.Enqueue(message);
                            break;
                        }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception Handled " + e.StackTrace);
            }
        }


        public void Close()
        {
            cli.Close();
        }

    }
}

