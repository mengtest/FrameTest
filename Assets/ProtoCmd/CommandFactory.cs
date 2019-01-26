namespace ProtoCmd
{
	using System.Collections.Generic;
	using System;
	public class CommandFactory 
	{
		 private static Dictionary<int,Type> _cmdMap=new Dictionary<int,Type>() ;
		static CommandFactory(){
			_cmdMap[16642]=typeof(ProtoCmd.stClientLogUserCmd);
			_cmdMap[294]=typeof(ProtoCmd.stLnMobileLoginUserCmd);
			_cmdMap[295]=typeof(ProtoCmd.stMkjhAndrMobileLoginUserCmd);
			_cmdMap[292]=typeof(ProtoCmd.stMkjhex1MobileLoginUserCmd);
			_cmdMap[293]=typeof(ProtoCmd.stCs9377MobileLoginUserCmd);
			_cmdMap[1285]=typeof(ProtoCmd.stRemoveMapObjectMapScreenUserCmd);
			_cmdMap[1284]=typeof(ProtoCmd.stAddMapObjectMapScreenUserCmd);
			_cmdMap[1286]=typeof(ProtoCmd.stRemoveMapNpcMapScreenUserCmd);
			_cmdMap[1281]=typeof(ProtoCmd.stRemoveUserMapScreenUserCmd);
			_cmdMap[1289]=typeof(ProtoCmd.stMapUserDataMapScreenUserCmd);
			_cmdMap[1288]=typeof(ProtoCmd.stNPCHPMapScreenUserCmd);
			_cmdMap[514]=typeof(ProtoCmd.stRequestUserGameTimeTimerUserCmd);
			_cmdMap[4385]=typeof(ProtoCmd.stMobileChargeBeginReturnUserCmd);
			_cmdMap[4384]=typeof(ProtoCmd.stMobileChargeBeginUserCmd);
			_cmdMap[290]=typeof(ProtoCmd.stBnMobileLoginUserCmd);
			_cmdMap[6145]=typeof(ProtoCmd.stUserInfoUserCmd);
			_cmdMap[4628]=typeof(ProtoCmd.stPKModeUserCmd);
			_cmdMap[769]=typeof(ProtoCmd.stMainUserDataUserCmd);
			_cmdMap[1545]=typeof(ProtoCmd.stUseSkillFlashUserCmd);
			_cmdMap[1541]=typeof(ProtoCmd.stNpcMoveMoveUserCmd);
			_cmdMap[258]=typeof(ProtoCmd.stUserRequestLoginCmd);
			_cmdMap[259]=typeof(ProtoCmd.stServerReturnLoginFailedCmd);
			_cmdMap[4637]=typeof(ProtoCmd.stMagicFailUserCmd);
			_cmdMap[4638]=typeof(ProtoCmd.stMagicPkStatusUserCmd);
			_cmdMap[257]=typeof(ProtoCmd.stUserVerifyVerCmd);
			_cmdMap[1014]=typeof(ProtoCmd.stMsdkTokensUserCmd);
			_cmdMap[4619]=typeof(ProtoCmd.stObjectHpMpPopUserCmd);
			_cmdMap[4613]=typeof(ProtoCmd.stAttackMagicUserCmd);
			_cmdMap[4615]=typeof(ProtoCmd.stBackOffMagicUserCmd);
			_cmdMap[4614]=typeof(ProtoCmd.stRTMagicUserCmd);
			_cmdMap[4617]=typeof(ProtoCmd.stNpcDeathUserCmd);
			_cmdMap[4616]=typeof(ProtoCmd.stObtainExpUserCmd);
			_cmdMap[16641]=typeof(ProtoCmd.stBiForwordBeginUserCmd);
			_cmdMap[513]=typeof(ProtoCmd.stGameTimeTimerUserCmd);
			_cmdMap[515]=typeof(ProtoCmd.stUserGameTimeTimerUserCmd);
			_cmdMap[788]=typeof(ProtoCmd.stChangeFaceUserCmd);
			_cmdMap[1032]=typeof(ProtoCmd.stPickUpItemPropertyUserCmd);
			_cmdMap[1030]=typeof(ProtoCmd.stRefCountObjectPropertyUserCmd);
			_cmdMap[1031]=typeof(ProtoCmd.stUseObjectPropertyUserCmd);
			_cmdMap[6410]=typeof(ProtoCmd.stRelationFriendApplyDelUserCmd);
			_cmdMap[1025]=typeof(ProtoCmd.stAddObjectPropertyUserCmd);
			_cmdMap[1026]=typeof(ProtoCmd.stRemoveObjectPropertyUserCmd);
			_cmdMap[1020]=typeof(ProtoCmd.stPkStatusDataUserCmd);
			_cmdMap[1023]=typeof(ProtoCmd.stEndOfInitDataDataUserCmd);
			_cmdMap[3585]=typeof(ProtoCmd.stChannelChatUserCmd);
			_cmdMap[852]=typeof(ProtoCmd.stTempObjectUserCmd);
			_cmdMap[1054]=typeof(ProtoCmd.stSystemSettingsUserCmd);
			_cmdMap[1058]=typeof(ProtoCmd.stAddObjectListPropertyUserCmd);
			_cmdMap[1059]=typeof(ProtoCmd.stAddEquipListPropertyUserCmd);
			_cmdMap[1304]=typeof(ProtoCmd.stEnterDupUserCmd);
			_cmdMap[1300]=typeof(ProtoCmd.stAddMapNpcAndPosMapScreenStateUserCmd);
			_cmdMap[1301]=typeof(ProtoCmd.stBatchRemoveNpcMapScreenUserCmd);
			_cmdMap[1302]=typeof(ProtoCmd.stBatchRemoveUserMapScreenUserCmd);
			_cmdMap[1303]=typeof(ProtoCmd.stDupInfoUserCmd);
			_cmdMap[1063]=typeof(ProtoCmd.stSetUserSkillCdPropertyUserCmd);
			_cmdMap[1074]=typeof(ProtoCmd.stMoneyChangePropertyUserCmd);
			_cmdMap[863]=typeof(ProtoCmd.stChangeNameUserCmd);
			_cmdMap[771]=typeof(ProtoCmd.stMapScreenSizeDataUserCmd);
			_cmdMap[770]=typeof(ProtoCmd.stSetHPAndMPDataUserCmd);
			_cmdMap[778]=typeof(ProtoCmd.stJingjieLevelUpResultUserCmd);
			_cmdMap[1316]=typeof(ProtoCmd.stDupDataUserCmd);
			_cmdMap[1314]=typeof(ProtoCmd.stUseWayPointMapScreenUserCmd);
			_cmdMap[1313]=typeof(ProtoCmd.stUpdateWayPointMapScreenUserCmd);
			_cmdMap[1312]=typeof(ProtoCmd.stMapWayPointDataMapScreenUserCmd);
			_cmdMap[1311]=typeof(ProtoCmd.stBatchRemoveMapObjectMapScreenUserCmd);
			_cmdMap[1539]=typeof(ProtoCmd.stUserInstantJumpMoveUserCmd);
			_cmdMap[1538]=typeof(ProtoCmd.stUserMoveMoveUserCmd);
			_cmdMap[1076]=typeof(ProtoCmd.stFuryChangePropertyUserCmd);
			_cmdMap[1070]=typeof(ProtoCmd.stSelectReturnStatesPropertyUserCmd);
			_cmdMap[1071]=typeof(ProtoCmd.stSelectRemoveStatesPropertyUserCmd);
			_cmdMap[1065]=typeof(ProtoCmd.stRemoveUserSkillPropertyUserCmd);
			_cmdMap[1064]=typeof(ProtoCmd.stAddUserSkillPropertyUserCmd);
			_cmdMap[1067]=typeof(ProtoCmd.stSelectPropertyUserCmd);
			_cmdMap[291]=typeof(ProtoCmd.stBnMobileLoginRetUserCmd);
			_cmdMap[288]=typeof(ProtoCmd.stMkMobileLoginUserCmd);
			_cmdMap[1298]=typeof(ProtoCmd.stAddUserMapScreenUserCmd);
			_cmdMap[289]=typeof(ProtoCmd.stMkjhMobileLoginUserCmd);
			_cmdMap[286]=typeof(ProtoCmd.stMobileLoginUserCmd);
			_cmdMap[262]=typeof(ProtoCmd.stGateReturnLoginFailedCmd);
			_cmdMap[261]=typeof(ProtoCmd.stPasswdLogonUserCmd);
			_cmdMap[260]=typeof(ProtoCmd.stServerReturnLoginSuccessCmd);
			_cmdMap[267]=typeof(ProtoCmd.stKickOutUserCmd);
			_cmdMap[1293]=typeof(ProtoCmd.stClearObjectOwnerMapScreenUserCmd);
			_cmdMap[1295]=typeof(ProtoCmd.stClearStateMapScreenUserCmd);
			_cmdMap[12552]=typeof(ProtoCmd.stUnrideHorseCmd);
			_cmdMap[1299]=typeof(ProtoCmd.stAddUserAndPosMapScreenStateUserCmd);
			_cmdMap[1292]=typeof(ProtoCmd.stRTSelectedHpMpPropertyUserCmd);
			_cmdMap[1290]=typeof(ProtoCmd.stMapNpcDataMapScreenUserCmd);
			_cmdMap[1291]=typeof(ProtoCmd.stMapPetDataMapScreenUserCmd);
			_cmdMap[1297]=typeof(ProtoCmd.stAddMapPetMapScreenUserCmd);
			_cmdMap[1294]=typeof(ProtoCmd.stSetStateMapScreenUserCmd);
			_cmdMap[12551]=typeof(ProtoCmd.stRideHorseCmd);
		}
		public static Type getType(uint byCmd,uint byParam)
		{
			int key = (int)(byCmd*256+byParam);
			if(_cmdMap.ContainsKey(key)) return  _cmdMap[key] ;
			return null;
		}
	}

}