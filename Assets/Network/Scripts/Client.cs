
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

        //queue to store messages in order
        public Queue<string> msgQueue;


        public Client()
        {
            msgQueue = new Queue<string>();
        }

        public void Connect()
        {
            int count = 0;
            cli = new TcpClient();
            try
            {
                cli.Connect("107.77.227.10", 44444);
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
        public void SendMsg(string msg)
        {
            byte[] send = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            //Debug.Log(msg);
            try
            {
                stream.Write(send, 0, send.Length);
                Console.WriteLine("MESSAGE SENT");

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void RecvMsg()
        {
            Byte[] buffer;//hold the bytes
            //int byteC;//byte count
            int buffSize = cli.ReceiveBufferSize;//how many bytes can be held
            String message;
            try
            {
                while (true)
                {
                    //used to determine level of json msg
                    //Some json msgs will have multiple sets of brackets
                    int brackets = 0;
                    message = string.Empty;
                    while (true)
                    {
                        buffer = new Byte[256];
                        int byteReceived = stream.ReadByte();

                        if ((char)byteReceived == '|')
                        {
                            int sz = stream.Read(buffer, 0, 7);

                            long size = BitConverter.ToInt64(buffer,0);
                            for (int i = 0; i < size - 8; i++)
                            {
                                byteReceived = stream.ReadByte();
                                message += (char)byteReceived;
                            }

                            break;
                        }

                    }
                    msgQueue.Enqueue(message);
                    Debug.Log(message);
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