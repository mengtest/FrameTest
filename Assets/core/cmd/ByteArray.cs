using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace sw.util
{
    public class ByteArray
    {
        private byte[] _buffer;
       
        public int position;
        public int end{get;set;}
        private int capacity;
        //public bool isBigendian;
  
        public ByteArray(bool isRecv=false)
        {
            if (isRecv)
            {
                _buffer = new byte[131072];//128k
                capacity = 131072;

            }
            else
            {
                _buffer = new byte[65536];//64k
                capacity = 65536;
            }
            position = 0;
            end = 0;
        }
        public ByteArray(byte[] buff)
        {
            _buffer = buff;
            capacity = buff.Length;
            position = 0;
            end = capacity;
        }
        public int recv(Stream stream)
        {
            int len = stream.Read(_buffer, end, remains());
            end += len;
            return len;
        }
        public int size()
        {
            return end - position;
        }
        public byte[] getBuffer()
        {
            return _buffer;
        }
        public void reset()
        {
            position = 0;
            end = 0;
        }
        public void rearrange()
        {
            
            for (int i = position; i < end; i++)
                _buffer[i - position] = _buffer[i];
            end = end - position;
            position = 0;
        }
        public int remains()
        {
            return capacity - end;
        }
        public static UInt16 ReverseBytes(UInt16 value)
        {
          return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        // reverse byte order (32-bit)
        public static UInt32 ReverseBytes(UInt32 value)
        {
          return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                 (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        //public static Int16 ReverseBytes(Int16 value)
        //{
        //    byte[] bts= BitConverter.GetBytes(value);
        //    bts.Reverse();
        //    return BitConverter.ToInt16(bts ,0);
             
        //}
        //public static UInt64 ReverseBytes(UInt64 value)
        //{
        //    byte[] bts = BitConverter.GetBytes(value);
        //    bts.Reverse();
        //    return BitConverter.ToUInt64(bts, 0);

        //}
        // reverse byte order (32-bit)
        //public static Int32 ReverseBytes(Int32 value)
        //{
        //    byte[] bts = BitConverter.GetBytes(value);
        //    bts = bts.Reverse().ToArray();
        //    return BitConverter.ToInt32(bts, 0);
        //}


        public uint readUnsignedInt()
        {
            if (position + 4 > end)
                return 0;
            uint result = BitConverter.ToUInt32(_buffer, position);
            position += 4;
            //if (isBigendian)
            //    return ReverseBytes(result);
            return result;
        }
        public string readUTFBytes(int len)
        {
            if (position + len > end)
                return "";
            string result = Encoding.UTF8.GetString(_buffer, position, len);
            int i = 0;
            for (i = 0; i < result.Length;i++ )
            {
                if (result[i] == '\0')
                    break;
            }
            if(i<result.Length)
                result = result.Substring(0,i);
            position += len;
            return result;
        }
        public byte readUnsignedByte()
        {
            if (position + 1 > end)
                return 0;
            byte result = _buffer[position];
            position += 1;
            return result;
        }
        public byte readByte()
        {
            if (position + 1 > end)
                return 0;
            byte result = _buffer[position];
            position += 1;
            return result;
        }
        public float readFloat()
        {
            if (position + 4 > end)
                return 0;
            float result = BitConverter.ToSingle(_buffer, position);
            position += 4;
            return result;
        }
        public void write(uint val)
        {
            write(BitConverter.GetBytes(val));
        }
        public void write(int val)
        {
            write(BitConverter.GetBytes(val));
        }
        public void write(short val)
        {
            write(BitConverter.GetBytes(val));
        }
        public void write(ushort val)
        {
            write(BitConverter.GetBytes(val));
        }
        public void write(ulong val)
        {
            write(BitConverter.GetBytes(val));
        }
        public void write(byte val)
        {
            if (end + 1 > _buffer.Length)
                return;
            _buffer[end++] = val;
            
        }
        public void write(byte[] bts)
        {
            if (end + bts.Length > _buffer.Length)
                return;
            for (int i = 0; i < bts.Length; i++)
                _buffer[end++] = bts[i];
           
        }
        
        public void writeString(String str, int len)
        {
            if (str == null)
                str = "";
            byte[] bts = Encoding.UTF8.GetBytes(str);
            if (end + len > _buffer.Length)
                return;
            for (int i = 0; i < bts.Length; i++)
                _buffer[end+i] = bts[i];
            if (bts.Length < len)
                Array.Clear(_buffer, end + bts.Length, len - bts.Length);
            

            end += len;
        }
        
        public ushort readUnsignedShort()
        {
            if (position + 2 > end)
                return 0;
            ushort result = BitConverter.ToUInt16(_buffer, position);
            position += 2;
            //if (isBigendian)
            //    return ReverseBytes(result);
            return result;
        }
       
        public short readShort()
        {
            if (position + 2 > end)
                return 0;
            short result = BitConverter.ToInt16(_buffer, position);
            position += 2;
            //if (isBigendian)
            //    return ReverseBytes(result);
            return result;
        }
       
        public int readInt()
        {
            if (position + 4 > end)
                return 0;
            int result = BitConverter.ToInt32(_buffer, position);
            position += 4;
            //if (isBigendian)
            //    return ReverseBytes(result);
            return result;
        }
        
        public ulong readUnsignedLong()
        {
            if (position + 8 > end)
                return 0;
            ulong result = BitConverter.ToUInt64(_buffer, position);
            position += 8;
            //if (isBigendian)
            //    return ReverseBytes(result);
            return result;
        }
        public string readUTF()
        {
            ushort sz = readUnsignedShort();
            if (sz == 0)
                return "";
            return readUTFBytes(sz);

        }
        public void writeUTF(string data)
        {

        }
    }
}
