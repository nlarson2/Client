
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
        int count = 0;
        cli = new TcpClient();
        try
        {
            cli.Connect("smashdome2.hopto.org", 44444);
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
                int brackets = 0;
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

