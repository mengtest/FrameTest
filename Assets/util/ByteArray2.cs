namespace sw.util
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ByteArray2
    {
        private byte[] _buffer;
        private int capacity;
        public int position;

        public ByteArray2(bool isRecv = false)
        {
            if (isRecv)
            {
                this._buffer = new byte[0x20000];
                this.capacity = 0x20000;
            }
            else
            {
                this._buffer = new byte[0x10000];
                this.capacity = 0x10000;
            }
            this.position = 0;
            this.end = 0;
        }

        public ByteArray2(byte[] buff)
        {
            this._buffer = buff;
            this.capacity = buff.Length;
            this.position = 0;
            this.end = this.capacity;
        }

        public byte[] getBuffer()
        {
            return this._buffer;
        }

        public byte readByte()
        {
            if ((this.position + 1) > this.end)
            {
                return 0;
            }
            byte num = this._buffer[this.position];
            this.position++;
            return num;
        }

        public float readFloat()
        {
            if ((this.position + 4) > this.end)
            {
                return 0f;
            }
            float num = BitConverter.ToSingle(this._buffer, this.position);
            this.position += 4;
            return num;
        }

        public int readInt()
        {
            if ((this.position + 4) > this.end)
            {
                return 0;
            }
            int num = BitConverter.ToInt32(this._buffer, this.position);
            this.position += 4;
            return num;
        }

        public short readShort()
        {
            if ((this.position + 2) > this.end)
            {
                return 0;
            }
            short num = BitConverter.ToInt16(this._buffer, this.position);
            this.position += 2;
            return num;
        }

        public byte readUnsignedByte()
        {
            if ((this.position + 1) > this.end)
            {
                return 0;
            }
            byte num = this._buffer[this.position];
            this.position++;
            return num;
        }

        public uint readUnsignedInt()
        {
            if ((this.position + 4) > this.end)
            {
                return 0;
            }
            uint num = BitConverter.ToUInt32(this._buffer, this.position);
            this.position += 4;
            return num;
        }

        public ulong readUnsignedLong()
        {
            if ((this.position + 8) > this.end)
            {
                return 0L;
            }
            ulong num = BitConverter.ToUInt64(this._buffer, this.position);
            this.position += 8;
            return num;
        }

        public ushort readUnsignedShort()
        {
            if ((this.position + 2) > this.end)
            {
                return 0;
            }
            ushort num = BitConverter.ToUInt16(this._buffer, this.position);
            this.position += 2;
            return num;
        }

        public string readUTF()
        {
            ushort len = this.readUnsignedShort();
            if (len == 0)
            {
                return "";
            }
            return this.readUTFBytes(len);
        }

        public string readUTFBytes(int len)
        {
            if ((this.position + len) > this.end)
            {
                return "";
            }
            string str = Encoding.UTF8.GetString(this._buffer, this.position, len);
            int length = 0;
            length = 0;
            while (length < str.Length)
            {
                if (str[length] == '\0')
                {
                    break;
                }
                length++;
            }
            if (length < str.Length)
            {
                str = str.Substring(0, length);
            }
            this.position += len;
            return str;
        }

        public void rearrange()
        {
            for (int i = this.position; i < this.end; i++)
            {
                this._buffer[i - this.position] = this._buffer[i];
            }
            this.end -= this.position;
            this.position = 0;
        }

        public int recv(Stream stream)
        {
            int num = stream.Read(this._buffer, this.end, this.remains());
            this.end += num;
            return num;
        }

        public int remains()
        {
            return (this.capacity - this.end);
        }

        public void reset()
        {
            this.position = 0;
            this.end = 0;
        }

        public static ushort ReverseBytes(ushort value)
        {
            return (ushort)(((value & 0xff) << 8) | ((value & 0xff00) >> 8));
        }

        public static uint ReverseBytes(uint value)
        {
            return (uint)(((((value & 0xff) << 0x18) | ((value & 0xff00) << 8)) | ((value & 0xff0000) >> 8)) | ((value & -16777216) >> 0x18));
        }

        public int size()
        {
            return (this.end - this.position);
        }

        public void write(byte val)
        {
            if ((this.end + 1) <= this._buffer.Length)
            {
                int num;
                this.end = (num = this.end) + 1;
                this._buffer[num] = val;
            }
        }

        public void write(short val)
        {
            this.write(BitConverter.GetBytes(val));
        }

        public void write(int val)
        {
            this.write(BitConverter.GetBytes(val));
        }

        public void write(ushort val)
        {
            this.write(BitConverter.GetBytes(val));
        }

        public void write(uint val)
        {
            this.write(BitConverter.GetBytes(val));
        }

        public void write(ulong val)
        {
            this.write(BitConverter.GetBytes(val));
        }

        public void write(byte[] bts)
        {
            if ((this.end + bts.Length) <= this._buffer.Length)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    int num2;
                    this.end = (num2 = this.end) + 1;
                    this._buffer[num2] = bts[i];
                }
            }
        }

        public void writeString(string str, int len)
        {
            if (str == null)
            {
                str = "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            if ((this.end + len) <= this._buffer.Length)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    this._buffer[this.end + i] = bytes[i];
                }
                if (bytes.Length < len)
                {
                    Array.Clear(this._buffer, this.end + bytes.Length, len - bytes.Length);
                }
                this.end += len;
            }
        }

        public void writeUTF(string data)
        {
        }

        public int end { get; set; }
    }
}
