using System;
using System.IO;
namespace SmashDomeNetwork
{
    public enum Datatype
    {
        _integer = 1,
        _float = 2,
        _char = 3
    }

    public class Cerealize
    {
        byte[] header;
        byte[] body;
        byte[] data;
        byte[] msgBytes;
        Int16 int2;
        Int32 int4;
        Int64 int8;
        float f8;
        String msg;

        public Cerealize()
        {
            //MakeHeader(t,seq);
        }

        public byte[] CerializeMSG(byte t, Int32 seq, Object obj)
        {
            MakeHeader(t, seq);
            Message(obj);
            FinalBytes();
            return getMSG();
        }

        public void IntByte(Int16 num)
        {
            byte[] bytes = BitConverter.GetBytes(num);
        }

        public void IntByte(Int32 num)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            Combine(body, bytes);
        }

        public void IntByte(Int64 num)
        {
            byte[] bytes = BitConverter.GetBytes(num);
        }

        public void FByte(float num)
        {
            num = (Int32)(num * 1000);
            byte[] bytes = BitConverter.GetBytes(num);
            Combine(body, bytes);
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third, byte[] fourth)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length + fourth.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length + third.Length,
                              fourth.Length);
            return ret;
        }

        private void MakeHeader(byte t, Int32 seq)
        {
            byte[] type = new byte[1];
            type[0] = t;

            this.header = Combine(type, BitConverter.GetBytes(seq));
        }

        private void FinalBytes()
        {
            Int64 msgSize = header.Length + body.Length + 8;
            msgBytes = BitConverter.GetBytes(msgSize);
            data = Combine(msgBytes, header, body);
        }


        //get header info and sends to correct parse function with msg parameters
        public Message ReadMessage(byte[] recMSG)
        {
            Message MSG = new Message();

            int bytesRead = 14; int bits = 4;
            byte[] size = new byte[8];
            byte[] header = new byte[5];

            Stream s = new MemoryStream(recMSG);
            s.Position = 0;

            int sz = s.Read(size, 0, 7);

            int bsize = System.BitConverter.ToInt32(size, 0) - 13;

            //Header data: messagetype|
            int hdr = s.Read(header, 8, 13);
            
            //message type
            byte[] MSGtype = { header[0],header[1],header[2],header[3] };

            //msgType defines what function to send data to
            switch (BitConverter.ToInt32(MSGtype))
            {
                case 1: MSG = parse(s,bytesRead,bsize,4,_integer);
                    break;
                case 2: MSG = parse(s,bytesRead,bsize,4,_float);
                    break;
                case 3: MSG = parse(s,bytesRead,bsize,1,_char);
                    break;
                default: Debug.Log("ReadMessage: message type error.\n");
            }

            return MSG;
        }

        //integer parse function for types using the integer values
        public Message parse(Stream s, int bytesRead, int bsize, int bits, int datatype)
        {
            int count = 0;
            byte[] data = new byte[bits];
            byte[][] bytes = new byte[bsize][];
            do {
                int dta = s.Read(data, bytesRead, bits);
                bytesRead += dta;
                bytes[count][0] = data[0];
                bytes[count][1] = data[1];
                bytes[count][2] = data[2];
                bytes[count][3] = data[3];
                count += 1;
            } while (bytesRead == bsize);

            switch (datatype)
            {
                case 1:
                case 2:
                    Int32[] i= ProcessMessageInt(data.Length, bytes);
                    msg.from = i[0];
                    break;
                case 3:
                    Int32[] i = ProcessMessageInt(bits, bytes);
                    msg.from = i[0];
                    float[] f = ProcessMessageFloat(data.Length, bytes, bits);
                    msg.pos = new Vector3(f[0], f[1], f[2]);
                    msg.playerRotation = new Quaternion(f[3], f[4], f[5], f[6]);
                    msg.cameraRotation = new Quaternion(f[7], f[8], f[9], f[10]);
                    break;
                default: Debug.Log("Message parse: datatype error.\n");
            }
            return MSG;
        }

        private Int32[] ProcessMessageInt(Int64 size, byte[][] data)
        {
            Int32[] variables = new Int32[size];
            for (int i = 0; i < size; i++)
            {
                byte[] integ = new byte[4];
                for (int j = 0; j < 4; j++)
                    integ[j] = data[i][j];
                variables[i] = BitConverter.ToInt32(integ, 0);
            }
            return variables;
        }

        private float[] ProcessMessageFloat(Int64 size, byte[][] data, int start)
        {
            float[] variables = new float[size];
            for (int i = start-1; i < size; i++)
            {
                byte[] flo = new byte[4];
                for (int j = 0; j < 4; j++)
                    flo[j] = data[i][j];
                variables[i] = (float)BitConverter.ToInt32(flo, 0) / 100;
            }
            return variables;
        }

        //
        private void Message(Object obj)
        {
            switch (obj.msgType)
            {
                case 1:
                    IntByte(obj.from);
                    break;
                case 2:
                    IntByte(obj.from);
                    break;
                case 3:
                    //from|pos(xyz)|lhand(xyz)|rhand(xyz)|quat(wxyz)
                    IntByte(obj.from);
                    //Vector3
                    byte [] pos = Combine(IntByte(obj.position.x), IntByte(obj.position.y), IntByte(obj.position.z));
                    //byte [] lhand = Combine(IntByte(obj.lhandPos.x), IntByte(obj.lhandPos.y), IntByte(obj.lhandPos.z));
                    //byte [] rhand = Combine(IntByte(obj.rhandPos.x), IntByte(obj.rhandPos.y), IntByte(obj.rhandPos.z));

                    //Quaternion
                    byte[] rot = Combine(IntByte(obj.rotation.w), IntByte(obj.rotation.x), IntByte(obj.rotation.y), IntByte(obj.rotation.z));
                    //byte[] lhandRot = Combine(IntByte(obj.lhandRot.w), IntByte(obj.lhandRot.x), IntByte(obj.lhandRot.y), IntByte(obj.lhandRot.z));
                    //byte[] rhandRot = Combine(IntByte(obj.rhandRot.w), IntByte(obj.rhandRot.x), IntByte(obj.rhandRot.y), IntByte(obj.rhandRot.z));
                    byte[] cameraRotation = Combine(IntByte(obj.cameraRotation.w), IntByte(obj.cameraRotation.x), IntByte(obj.cameraRotation.y), IntByte(obj.cameraRotation.z));
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                default: Debug.Log("Message: message type error.\n");
            }
        }
        
        public byte[] getMSG()
        {
            return data;
        }
    }
}
//Header
//  First 8 bytes - number of bytes in message including these 8 bytes
//  Second 1 byte - message type (0-255 is available)
//  third 4 bytes - number from msg sequence, each type is different
//Body
//  fourth body.Length - the payload, 4 byte variables only for now
//Footer (optional)

//Body format
    //login/logout
        //from
    //move
        //