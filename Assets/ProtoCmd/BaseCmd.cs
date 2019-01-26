 

namespace ProtoCmd
{
 
    public interface BaseCmd
    {
        uint paramVal { get; set; }
        uint cmdVal { get; set; }
        uint timestampVal { get; set; }
    }
}
