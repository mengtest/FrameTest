
using ProtoCmd;
using System.IO;
 
    public class ByteCmd:BaseCmd
    {
         

        public static uint ByteCmdTimestamp = 0xffffffff;
        public uint cmdVal { get; set; }
        public uint paramVal { get; set; }
        uint _timestamp = ByteCmdTimestamp;
        public uint timestampVal { get { return _timestamp; } set { _timestamp = value; } }
        
        public byte[] data;

        public LuaByteBuffer ReadBuffer()
        {

            return new LuaByteBuffer(data);
        }
       

        public void WriteBuffer(LuaByteBuffer strBuffer)
        {

            data = strBuffer.buffer;
            //MemoryStream stream = new MemoryStream();
            //stream.Write(strBuffer.buffer, 0, strBuffer.buffer.Length);
            //data = stream.ToArray(); 

        }
    }
 
