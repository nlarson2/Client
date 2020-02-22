
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



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
        cli = new TcpClient();
        //cli.Connect("97.97.128.93", 55555);
        cli.Connect("smashdome3d.hopto.org", 55555);
        //cli.Connect("54.193.55.141", 55555);

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

    public void SendMsg(byte[] send)
    {
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
        //byte[] buffer;//hold the bytes
        //int byteC;//byte count
        int buffSize = cli.ReceiveBufferSize;//how many bytes can be held
        string json;
        try
        {
            while (true)
            {
                json = string.Empty;
                while (true)
                {
                    int inByte = stream.ReadByte();
                    json += (char)inByte;
                    if (inByte == '}')
                        break;
                }
                msgQueue.Enqueue(json);
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

