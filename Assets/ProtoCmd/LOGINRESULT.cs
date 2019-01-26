namespace ProtoCmd
{
	public class LOGINRESULT
	{
		public const int LOGIN_RETURN_UNKNOWN=0;
		public const int LOGIN_RETURN_VERSIONERROR=1;
		public const int LOGIN_RETURN_UUID=2;
		public const int LOGIN_RETURN_DB=3;
		public const int LOGIN_RETURN_PASSWORDERROR=4;
		public const int LOGIN_RETURN_CHANGEPASSWORD=5;
		public const int LOGIN_RETURN_IDINUSE=6;
		public const int LOGIN_RETURN_IDINCLOSE=7;
		public const int LOGIN_RETURN_GATEWAYNOTAVAILABLE=8;
		public const int LOGIN_RETURN_USERMAX=9;
		public const int LOGIN_RETURN_ACCOUNTEXIST=10;
		public const int LOGON_RETURN_ACCOUNTSUCCESS=11;
		public const int LOGIN_RETURN_CHARNAMEREPEAT=12;
		public const int LOGIN_RETURN_USERDATANOEXIST=13;
		public const int LOGIN_RETURN_USERNAMEREPEAT=14;
		public const int LOGIN_RETURN_TIMEOUT=15;
		public const int LOGIN_RETURN_PAYFAILED=16;
		public const int LOGIN_RETURN_JPEG_PASSPORT=17;
		public const int LOGIN_RETURN_LOCK=18;
		public const int LOGIN_RETURN_WAITACTIVE=19;
		public const int LOGIN_RETURN_INVALIDZONE=20;
		public const int LOGIN_RETURN_INVALIDNAME=21;
		public const int LOGIN_RETURN_NEEDRELOGON=22;
		public const int LOGIN_RETURN_PARSE_ERROR=23;
		public const int LOGIN_RETURN_VERIFY_ERROR=24;
		public const int LOGIN_RETURN_WAITING=25;
		public const int LOGIN_RETURN_WAIT_FULL=26;
		public const int LOGIN_RETURN_INVALIDNAME_INCLUDE_NUM=27;
		public const int LOGIN_RETURN_FORBID_IP=28;
		public const int LOGIN_RETURN_FORBID=33;
		public const int LOGIN_RETURN_FORBID_ACC=34;
		public const int LOGIN_RETURN_SIGN_ERROR=35;
		public const int LOGIN_RETURN_TOKEN_OVERDUE=36;
		public const int LOGIN_RETURN_SRV_INNER_ERROR=37;
		public const int LOGIN_RETURN_FORBID_IMEI=38;
	}
}
