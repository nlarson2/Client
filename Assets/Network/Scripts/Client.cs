
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using SmashDomeNetwork;


public class Client
{

    TcpClient cli;
    NetworkStream stream;

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
            cli.Connect("smashdome3d.hopto.org", 44444);
            //cli.Connect("localhost", 44444);
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
        try
        {
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("MESSAGE SENT");
            Debug.Log("MESSAGE SENT");

        }
        catch (SocketException e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }

    public void RecvMsg()
    {
        //byte[] buffer;//hold the bytes
        //int byteC;//byte count
        int buffSize = cli.ReceiveBufferSize;//how many bytes can be held
        string json;
        try
        {
            while (true)
            {
                //used to determine level of json msg
                //Some json msgs will have multiple sets of brackets
                /*int brackets = 0;
                json = string.Empty;
                while (true)
                {
                    int inByte = stream.ReadByte();
                    json += (char)inByte;
                    if (inByte == '{')
                        brackets++;
                    if (inByte == '}')
                        brackets--;
                    if(brackets == 0)
                        break;
                }
                msgQueue.Enqueue(json);*/
                /*byte[] sizeInBytes =
                    {
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte(),
                        (byte)stream.ReadByte()

                    };*/
                //Debug.Log("GOT SIZE");
                //int size = Message.BytesToInt(sizeInBytes);
                //Debug.Log(String.Format("SIZE: {0}", size));
                //if (size < 0) continue;
                //byte[] msg = new byte[size];
                List<byte> byteList = new List<byte>();
                int delimCount = 0;
                /*for (int i = 0; i < size; i++)
                {
                    msg[i] = (byte)stream.ReadByte();
                    Debug.Log("READING");
                }*/
                while (delimCount < 8)
                {
                    byte inByte = (byte)stream.ReadByte();
                    byteList.Add(inByte);
                    delimCount = (char)inByte == '\n' ? delimCount + 1 : 0;
                }

                msgQueue.Enqueue(byteList.ToArray());
                Debug.Log("ENQUEUED");
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

