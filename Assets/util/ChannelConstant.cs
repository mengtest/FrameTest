

namespace sw.util
{
    public class ChannelConstant
    {

#if UNITY_IPHONE
    #if I91
            public static string pf="I91";
    #elif Itb
            public static string pf="ITB";
    #elif Iitools
            public static string pf="IIT";
    #elif Ipp
		    public static string pf="IPP";
    #elif Ixy
		    public static string pf="IXY";
    #elif Ihm
		    public static string pf="IHM";
    #elif Iky
            public static string pf = "IKY";
    #elif Ii4
            public static string pf = "IIS";
    #elif Idj
            public static string pf = "IDL";
    #elif Ihl
            public static string pf = "IMD";
    #elif Ing
            public static string pf = "IMD";
#else
            public static string pf="default";
#endif
#else
#if ATX
        public static string pf = "ATX";
#elif AftHoolai||Atmfyyyb||Atmfyly
        public static string pf = "AMD";
#else
        public static string pf = "default";
#endif
#endif
#if UNITY_IPHONE
        public static string snid = "45";
#else
        public static string snid = "15";
#endif
        public static string  sourcePlatform;
        private static int _appPtid;

        public static int appPtid
        {
            get { 
                if(AppConfig.testPTID!=0)
                {
                    return AppConfig.testPTID;
                }
                else
                {
                    return ChannelConstant._appPtid; 
                }                
            }
            set { 
                ChannelConstant._appPtid = value; 
            }
        }
        public static int subChannel;

        public static string extraChannel = "";//额外定义的渠道信息

        public static string productName = "";
#if!UNITY_IOS
        public static string pfChannel="-1";//后台匹配的渠道值
#elif UNITY_IOS
       public static string pfChannel="IOS";//后台匹配的渠道值

#endif
        public static int pfptid = -1;//后台匹配的平台ID
        public static string biPf
        {
            get
            {
#if AftHoolai || Ihl ||Atmfyyyb||Atmfyly
                if (string.IsNullOrEmpty(extraChannel))
                    return ChannelConstant.pf + "-" + SDKFactory.getInterface.biChannnel;
                else
                    return ChannelConstant.pf + "-" + SDKFactory.getInterface.biChannnel + "_" + extraChannel;
#else
                if (string.IsNullOrEmpty(extraChannel))
                    return ChannelConstant.pf + "-" + SDKFactory.getInterface.subChannel;
                else
                    return ChannelConstant.pf + "-" + SDKFactory.getInterface.subChannel + "_" + extraChannel;
#endif
            }
        }
        //BI UID 渠道-渠道返回ID 用于后台查询ID
        public static string biUID()
        {
            if (SDKFactory.getInterface != null)
                return SDKFactory.getInterface.biUid.ToString();
            return "";
        }

#if Ihl
        public static int iospf = 1;
#endif
        public static string versionCheckBG = "";
    }
}
