using System.Collections.Generic;

namespace sw.util
{
    public class CheckTools
    {
        //先暂时写到这里
        public static string ENHA_CuiLian = "woshicuilian";
        public static string ENHA_XiLianTiHuan = "woshixiliantihuan";
        public static string MAIL_MailPoint = "woshiyoujiannxiaodian";
        public static string ON_SCENE_MONSTER_INFO_BACK = "onSceneMonsterInfoBack";
        public static string ON_FENTIANDOUFA_SCORE_BACK = "ON_FENTIANDOUFA_SCORE_BACK";
        public static string FenTianDouFaScoreChange = "FenTianDouFaScoreChange";

        public static int Check_tabQiangHua = 0;
        //public static int Check_tabShengJi = 1;
        public static int Check_tabCuiLian = 2;
        //public static int Check_tabJiCheng = 3;
        public static int Check_tabXiLian = 4;
        public static int Check_tabShengPin = 5;
        //public static int Check_tabJinJie = 6;
        public static int Check_tabShuXing = 7;

        public static string FuncNameQiangHang = "qianghua";
        public static string FuncNameXiLian = "xilian";
        public static string FuncNameCuiLian = "cuilian";
        public static string FuncNameShengPin = "shengpin";

        public static int MaxQiangHuaLevel = 0;
        public static int MaxShengPinLevel = 12;

        public static IDataRepository datare;

        static CheckTools()
        {
            //EventDispatcher.AddEventListener(DupEventType.FENTIANZHILU_CHECK_STAR_AWARD, OnTanMuStarChange);
            //EventDispatcher.AddEventListener(DupEventType.FENTIANZHILU_STAR_REWARD_DATE, OnTanMuStarChange);
            //EventDispatcher.AddEventListener(DataEventType.MYSTERY_SUPERMAKET_UPDATA, CheckShopPoint);
            //EventDispatcher.AddEventListener(UIEventType.SUPERMARKET_BUY_ALL_REFRESH, CheckShopPoint);
        }

         
        //public object checkArySameItem(Dictionary<K, V> ary, string item, object SameItme, string subProperty = null)
        //{
        //    foreach(Dictionary<K,V> obj in ary)
        //    {
        //        if(obj!=null)
        //        {
        //            if(subProperty)
        //            {
        //                if(obj[subProperty].hasOwnProperty(item))
        //                {
        //                    if(obj[subProperty][item]==SameItme)
        //                    {
        //                        return obj[subProperty];
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if(obj.hasOwnProperty(item))
        //                {
        //                    if(obj[item]==SameItme)
        //                    {
        //                        return obj;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        /**
        *返回一个类中所有属性 
        * @param obj
        * @return 
        * 
        */
        /*
        public string printObjectType(Object obj)
        {
            if (obj != null)
            {
                Type tp = Type.forInstance(obj);
                string pfs = "";
                foreach (Field f in tp.fields)
                {
                    if (obj.hasOwnProperty(f.name))
                    {
                        pfs += "\n" + f.name + ":" + f.getValue(obj);
                        if (obj.hasOwnProperty(f.name) is Object)
                        {
                            pfs += "\n【" + f.name + "】" + printObjectType(obj[f.name]);
                        }
                    }
                }
            }
            return pfs;
        } */
        //public static bool FuncIsOpen(string funcName, PlayerData mainData)
        //{
        //    //return true;
        //    if (ConfigAsset2.Instance.GetById<FunctionOpenConfig>(funcName) != null && mainData.mapData.parentfield.level >= ConfigAsset2.Instance.GetById<FunctionOpenConfig>(funcName).openNeedLevel)
        //    {
        //        return true;
        //    }
        //    //MessageBox.ShowSubLableTips("人物" + ConfigAsset2.Instance.GetById<FunctionOpenConfig>(funcName).openNeedLevel + "级开启");
        //    return false;
        //}
        //public static bool CheckIsCanQiangHua(ItemInfo selecteInfo, bool isShowTips, EquipConfig equipInfoArr = null)
        //{
        //    PlayerData mainData = DataRepository.Instance.mainPlayerData;
        //    EquipConfig equipInfo = equipInfoArr;
        //    if (equipInfoArr == null)
        //    {
        //        equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id);
        //    }

        //    if (equipInfo.cuilianId == 0 || !FuncIsOpen(FuncNameQiangHang, mainData))
        //    {
        //        if (isShowTips)
        //            MessageBox.ShowSubLableTips("该装备不能强化！");
        //        return false;
        //    }
        //    //if (isNeedJudgeLevel)
        //    //{
        //    //    CuilianProp clp = ConfigAsset2.Instance.GetById<CuilianProp>(equipInfo.cuilianId,(int)selecteInfo.equip.cuiLianLevel);
        //    //    if (clp == null || mainData.mapData.parentfield.level < clp.needLevel)
        //    //        return false;
        //    //}
        //    return true;
        //}
        //public static bool CheckIsCanShengPin(ItemInfo selecteInfo, bool isShowTips)
        //{
        //    EquipConfig tempEquipCfg = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id);
        //    PlayerData mainData = DataRepository.Instance.mainPlayerData;
        //    EquipQualityUpConfig equipGradeCostInfo = ConfigAsset2.Instance.GetById<EquipQualityUpConfig>(tempEquipCfg.improveQualityObjId, (int)selecteInfo.equip.grade);
        //    if (selecteInfo.cfg.quality >= 4 && equipGradeCostInfo != null && FuncIsOpen(FuncNameShengPin, mainData))
        //        return true;
        //    else
        //    {
        //        if (isShowTips)
        //            MessageBox.ShowSubLableTips("20级以上金装开启此功能");
        //        return false;
        //    }
        //}
        //public static bool CheckIsCanXiLian(ItemInfo selecteInfo, bool isShowTips)
        //{
        //    PlayerData mainData = DataRepository.Instance.mainPlayerData;
        //    if (selecteInfo.cfg.quality >= 3 && FuncIsOpen(FuncNameXiLian, mainData))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        if (isShowTips)
        //            MessageBox.ShowSubLableTips("紫装开启此功能");
        //        return false;
        //    }
        //}
        //public static bool CheckIsCanCuiLian(ItemInfo selecteInfo, bool isShowTips)
        //{
        //    PlayerData mainData = DataRepository.Instance.mainPlayerData;
        //    EquipConfig equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id);
        //    if (equipInfo.objId == 0 || !FuncIsOpen(FuncNameCuiLian, mainData))
        //    {
        //        if (isShowTips)
        //            MessageBox.ShowSubLableTips("该装备不能淬炼");
        //        return false;
        //    }
        //    return true;
        //}
        //public static bool CheckQiangHuaIsMax(ItemInfo selecteInfo, bool isShowTips, EquipConfig equipInfoArr = null)
        //{
        //    EquipConfig equipInfo = equipInfoArr;
        //    if (equipInfo == null)
        //    {
        //        equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id);
        //    }
        //    if (equipInfo.cuilianId != 0)
        //    {
        //        if (selecteInfo.equip.cuiLianLevel >= GetMaxQiangHuaLevel())
        //            return true;
        //        return false;
        //    }
        //    if (isShowTips)
        //        MessageBox.ShowSubLableTips("该装备已强化到最高等级！");
        //    return true;
        //}
        //public static bool CheckCuiLianIsMax(ItemInfo selecteInfo, bool isShowTips)
        //{
        //    EquipConfig equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id);
        //    if (equipInfo.objId != 0)
        //    {
        //        if (selecteInfo.equip.enhanceLevel < Constants.ENHANCE_LEVEL)
        //        {
        //            return false;
        //        }
        //    }
        //    if (isShowTips)
        //        MessageBox.ShowSubLableTips("该装备已淬炼到最高等级！");
        //    return true;
        //}
        //public static bool CheckShengPinIsMax(ItemInfo selecteInfo, bool isShowTips)
        //{
        //    if (selecteInfo.equip.grade < 12)
        //    {
        //        return false;
        //    }
        //    if (isShowTips)
        //        MessageBox.ShowSubLableTips("已达到最高品阶！");
        //    return true;
        //}
        //public static bool SelecteEquipFilter(ItemInfo selecteInfo, int tab, bool isShowTips)
        //{
        //    if (tab == Check_tabQiangHua)
        //    {
        //        return CheckIsCanQiangHua(selecteInfo, isShowTips);
        //    }
        //    else if(tab == Check_tabShuXing)
        //    {
        //        return true;
        //    }
        //    //else if (tab == Check_tabShengJi) {
        //    //    if (dataRepo.HasHeadFeature("shengji"))
        //    //    {
        //    //        int upgradeID = ConfigAsset2.Instance.GetById<EquipConfig>(selecteInfo.cfg.id).upgradeid;
        //    //        if (upgradeID == 0)
        //    //        {
        //    //            MessageBox.ShowSubLableTips("该装备已满级！");
        //    //            return false;
        //    //        }
        //    //        return true;
        //    //    }
        //    //    else
        //    //    {
        //    //        //int taskLevel = ConfigAsset2.Instance.GetById<TaskConfig>(ConfigAsset.Instance.GetById<FunctionOpenConfig>("shengji").openTaskId).showLevel;
        //    //        //MessageBox.ShowSubLableTips(taskLevel + "级任务开启");
        //    //        return false;
        //    //    }
        //    //}
        //    else if (tab == Check_tabCuiLian)
        //    {
        //        return CheckIsCanCuiLian(selecteInfo, isShowTips);
        //    }
        //    //else if (tab == Check_tabJiCheng)
        //    //{
        //    //}
        //    else if (tab == Check_tabXiLian)
        //    {
        //        // 洗练
        //        return CheckIsCanXiLian(selecteInfo, isShowTips);
        //    }
        //    else if (tab == Check_tabShengPin)
        //    {
        //        // 升品
        //        return CheckIsCanShengPin(selecteInfo, isShowTips);
        //    }
        //    //else if (tab == Check_tabJinJie)
        //    //{
        //    //    // 进阶
        //    //    if (ConfigAsset2.Instance.GetById<EquipUpgrade>(selecteInfo.cfg.id) != null && selecteInfo.cfg.quality >= 4)
        //    //    {
        //    //        return true;
        //    //    }
        //    //    else
        //    //    {
        //    //        MessageBox.ShowSubLableTips("50级无暇品质以上金装开启此功能");
        //    //        return false;
        //    //    }
        //    //}
        //    else
        //    {
        //        object obj = LuaUtil.CallMethod("CheckTools", "CheckEquipIsCanOp", selecteInfo.obj.pos.pos, tab, isShowTips)[0];
        //        return (bool)obj;
        //    }
        //}
        //public static bool SelecteEquipIsMaxLevel(ItemInfo selecteInfo, int tab)
        //{
        //    if (tab == Check_tabQiangHua)
        //    {
        //        return CheckQiangHuaIsMax(selecteInfo, true);
        //    }
        //    else if (tab == Check_tabCuiLian)
        //    {
        //        return CheckCuiLianIsMax(selecteInfo, true);
        //    }
        //    else if (tab == Check_tabShengPin)
        //    {
        //        // 升品
        //        return CheckShengPinIsMax(selecteInfo, true);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        private static List<int> tmpList = new List<int>();
        //public static bool filterEquipt(ItemInfo equip, IDataRepository dataRepo)
        //{
        //    if (equip == null || equip.equip == null || equip.obj == null)
        //        return false;
        //    //强化1
        //    if (upDateEquipQiangHua(equip, dataRepo))
        //    {
        //        return true;
        //    }
        //    //淬炼2
        //    if (upDateEquipCuiLian(equip, dataRepo))
        //    {
        //        return true;
        //    }
        //    //洗练3
        //    //if (dataRepo.mainPlayerData.userData.level >= ConfigAsset2.Instance.GetById<FunctionOpenConfig>("xilian").openNeedLevel)
        //    //{
        //    //    if (equip.cfg.quality >= 3)
        //    //    {
        //    //        if (upDateEquipxiLian(equip, dataRepo))
        //    //        {
        //    //            trmpList.Add(3);
        //    //            return trmpList;
        //    //        }
        //    //    }
        //    //}
        //    //升品4
        //    if (upDateEquipShengPin(equip, dataRepo))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //判断 强化条件满不满足
        //public static bool upDateEquipQiangHua(ItemInfo equip, IDataRepository dataRepo)
        //{
        //    EquipConfig equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(equip.cfg.id);
        //    if (!CheckIsCanQiangHua(equip, false, equipInfo) || CheckQiangHuaIsMax(equip, false, equipInfo))
        //        return false;
        //    CuilianProp curP = ConfigAsset2.Instance.GetById<CuilianProp>(equipInfo.cuilianId, (int)equip.equip.cuiLianLevel);
        //    if (curP != null)
        //    {
        //        uint hasMoney = dataRepo.mainPlayerData.userData.moneys[int.Parse(curP.consumeMoneyType) - 1];
        //        if (hasMoney >= curP.consumeMoneyNum)
        //        {
        //            if (curP.useObjectid == 0 && dataRepo.mainPlayerData.mapData.parentfield.level >= curP.needLevel)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                if (dataRepo.getTotalItemNumByPos(curP.useObjectid) >= curP.useObjectnum && dataRepo.mainPlayerData.mapData.parentfield.level >= curP.needLevel)
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
        //判断 淬炼条件满不满足
        //public static bool upDateEquipCuiLian(ItemInfo equip, IDataRepository dataRepo)
        //{
        //    if (!CheckIsCanCuiLian(equip, false) || CheckCuiLianIsMax(equip, false))
        //        return false;
        //    int enhanceCostObjID = ConfigAsset2.Instance.GetById<EquipConfig>(equip.cfg.id).objId;
        //    EquipEnhanceCostObjConfig enhanceCostItem = ConfigAsset2.Instance.GetById<EquipEnhanceCostObjConfig>(enhanceCostObjID, (int)equip.equip.enhanceLevel);
        //    if (enhanceCostItem.objid != 0)
        //    {
        //        int totalNum = dataRepo.getTotalItemNumByPos(enhanceCostItem.objid);
        //        int neadNum = enhanceCostItem.objnum;
        //        if (totalNum >= neadNum)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //判断 洗练条件满不满足
        //public static bool upDateEquipxiLian(ItemInfo equip, IDataRepository dataRepo)
        //{
        //    if (dataRepo.mainPlayerData.userData.level <= 50)
        //    {
        //        if (!CheckIsCanXiLian(equip, false))
        //            return false;
        //        for (int k = 0; k < EquipConstValue.MAX_BONUS_ITEM_NUM; k++)
        //        {
        //            t_EquipBonusProp equipBonus = equip.equip.bonus[k];
        //            if (equipBonus.id != 0)
        //            {
        //                return false;
        //            }
        //        }
        //        string str01 = EquipBasicUtil.getFuncDesc("recastUseMoneyNum" + equip.cfg.quality);
        //        string str02 = EquipBasicUtil.getFuncDesc("recastUseMoneyType" + equip.cfg.quality);
        //        if (String.IsNullOrEmpty(str01) || str01.Equals("0"))
        //            return false;

        //        if (String.IsNullOrEmpty(str02) || str02.Equals("0"))
        //            return false;


        //        int costMoneyNum = int.Parse(str01);
        //        int moneyType = int.Parse(str02);
        //        MoneyConfig moneyConfig = ConfigAsset2.Instance.GetById<MoneyConfig>(moneyType);
        //        if (moneyConfig == null)
        //            return false;
        //        int haveMoney = (int)dataRepo.mainPlayerData.userData.moneys[moneyType - 1];
        //        if (haveMoney >= costMoneyNum)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //// 判断 升级装备的材料够不够
        //private static bool updateCost(ItemInfo item, IDataRepository dataRepo)
        //{
        //    //Dictionary<string, string> equipBasicDic = ConfigAsset2.Instance.equipBasicCfgDict;
        //    //selectedObj=new Array;
        //    //if (!ConfigAsset2.Instance.equipUpgradeCostTypeLevelCfgsDict.ContainsKey(item.cfg.needlevel))
        //    //{
        //    //    //到顶级了
        //    //    return false;
        //    //}

        //    //EquipUpgradeCostTypeLevelConfig equipUpgrade = ConfigAsset2.Instance.equipUpgradeCostTypeLevelCfgsDict[item.cfg.needlevel];
        //    List<EquipUpgradeCostObjConfig> equipUpgradeInfo = ConfigAsset2.Instance.getAll<EquipUpgradeCostObjConfig>(item.cfg.needlevel);
        //    int equipUpgradeInfoLen = equipUpgradeInfo.Count;
        //    int mark = -1;
        //    EquipUpgradeCostObjConfig costObjConfig = new EquipUpgradeCostObjConfig();
        //    for (int k = 0; k < equipUpgradeInfoLen; k++)
        //    {
        //        mark++;
        //        costObjConfig = equipUpgradeInfo[k];
        //        //string[] equipPos = costObjConfig.equippos.Split(',');
        //        //int equipPosLen = equipPos.Length;
        //        bool find = false;
        //        //for (int m = 0; m < equipPosLen; m++)
        //        //{
        //        //    if (item.cfg.kind + "" == equipPos[m])
        //        //    {
        //        //        find = true;
        //        //        break;
        //        //    }
        //        //}
        //        if (item.cfg.kind == costObjConfig.equippos)
        //            find = true;
        //        if (find)
        //            break;
        //    }
        //    if (mark == equipUpgradeInfoLen)
        //    {
        //        return false;
        //    }

        //    int slotNum = 0;
        //    bool b = false;
        //    if (item.cfg.quality == 0)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic0 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(0) != "")
        //        {
        //            qualityDic0 = ConfigTools.getQualityDict(costObjConfig.quality(0));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic0.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    else if (item.cfg.quality == 1)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic1 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(1) != "")
        //        {
        //            qualityDic1 = ConfigTools.getQualityDict(costObjConfig.quality(1));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic1.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    else if (item.cfg.quality == 2)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic2 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(2) != "")
        //        {
        //            qualityDic2 = ConfigTools.getQualityDict(costObjConfig.quality(2));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic2.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    else if (item.cfg.quality == 3)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic3 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(3) != "")
        //        {
        //            qualityDic3 = ConfigTools.getQualityDict(costObjConfig.quality(3));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic3.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    else if (item.cfg.quality == 4)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic4 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(4) != "")
        //        {
        //            qualityDic4 = ConfigTools.getQualityDict(costObjConfig.quality(4));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic4.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    else if (item.cfg.quality == 5)
        //    {
        //        Dictionary<int, EquipUpgradeCostInfo> qualityDic5 = new Dictionary<int, EquipUpgradeCostInfo>();
        //        if (costObjConfig.quality(5) != "")
        //        {
        //            qualityDic5 = ConfigTools.getQualityDict(costObjConfig.quality(5));
        //            foreach (EquipUpgradeCostInfo itemObj in qualityDic5.Values)
        //            {
        //                slotNum++;
        //                b = updateObj(itemObj, slotNum, dataRepo);
        //                if (b == false)
        //                    break;
        //            }
        //        }
        //    }
        //    return b;
        //}

        //private static bool updateObj(EquipUpgradeCostInfo obj, int i, IDataRepository dataRepo)
        //{
        //    int itemId = int.Parse(obj.id);
        //    if (itemId == 0)
        //        return false;
        //    int num = int.Parse(obj.value);
        //    int totalNum = dataRepo.getTotalItemNumByPos(itemId);

        //    if (totalNum < num)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //// 判断 进阶的材料够不够
        //private static bool upDateEquipDianJin(ItemInfo curEquip, IDataRepository dataRepo)
        //{
        //    bool b = false;
        //    if (curEquip != null)
        //    {
        //        if (ConfigAsset2.Instance.GetById<EquipUpgrade>(curEquip.cfg.id) != null)
        //        {
        //            if (ConfigAsset2.Instance.GetById<EquipUpgrade>(curEquip.cfg.id) != null)
        //            {
        //                EquipUpgrade upGradeCfg = ConfigAsset2.Instance.GetById<EquipUpgrade>(curEquip.cfg.id);
        //                string[] itemArr = upGradeCfg.needObj.Split(';');
        //                int mark = 0;
        //                foreach (string str in itemArr)
        //                {
        //                    string[] item = str.Split(',');
        //                    int itemId = int.Parse(item[0]);
        //                    int num = int.Parse(item[1]);
        //                    ItemConfig itemCfg = ConfigAsset2.Instance.GetById<ItemConfig>(itemId);
        //                    int totalNum = dataRepo.getTotalItemNumByPos(itemCfg.id);
        //                    if (totalNum < num)
        //                    {
        //                        b = false;
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        b = true;
        //                    }
        //                    mark++;
        //                }
        //            }
        //        }
        //    }
        //    return b;
        //}

        //// 判断升品材料够不够
        //public static bool upDateEquipShengPin(ItemInfo curEquip, IDataRepository dataRepo)
        //{
        //    bool b = false;
        //    if (CheckIsCanShengPin(curEquip, false) && !CheckShengPinIsMax(curEquip, false))
        //    {
        //        EquipConfig equipInfo = ConfigAsset2.Instance.GetById<EquipConfig>(curEquip.cfg.id);
        //        EquipQualityUpConfig equipGradeCostInfo = ConfigAsset2.Instance.GetById<EquipQualityUpConfig>(equipInfo.improveQualityObjId, (int)curEquip.equip.grade);
        //        ItemConfig itemCfg = ConfigAsset2.Instance.GetById<ItemConfig>(equipGradeCostInfo.objId);
        //        int totalNum = dataRepo.getTotalItemNumByPos(equipGradeCostInfo.objId);
        //        int num = equipGradeCostInfo.objNum;
        //        if (totalNum >= num)
        //        {
        //            b = true;
        //        }
        //        else
        //        {
        //            b = false;
        //        }
        //    }
        //    return b;
        //}
        ////检测背包红点
        //public static bool checkPackBgPoint(IDataRepository dataRepo, List<ItemInfo> infoList, int pageType = 0)
        //{
        //    if (infoList != null && infoList.Count != 0)
        //    {
        //        for (int i = 0; i < infoList.Count; i++)
        //        {
        //            if (pageType != 0 && pageType != infoList[i].cfg.pageType)
        //                continue;
        //            if (infoList[i].cfg.cat == ItemCat.ItemCat_Equip)
        //            {
        //                if (infoList[i].cfg.occupy == dataRepo.mainPlayerData.career.id || infoList[i].cfg.occupy == 0)
        //                {
        //                    bool isNeedShowWarning = CheckTools.isEquipCanUp(infoList[i], dataRepo);
        //                    if (isNeedShowWarning)
        //                    {
        //                        if (infoList[i].cfg.needlevel <= dataRepo.mainPlayerData.userData.level)
        //                        {
        //                            return true;
        //                        }
        //                    }
        //                }
        //            }
        //            else if (infoList[i].cfg.cat == 20 || infoList[i].cfg.cat == 23)
        //            {
        //                if (CheckTools.isEquipCanCompound(infoList[i], dataRepo))
        //                {
        //                    return true;
        //                }
        //            }
        //            ////是否为多合一的宝箱类合成
        //            //else if (infoList[i].cfg.cat == 9 && infoList[i].cfg.kind == 22)
        //            //{
        //            //    if (CheckTools.isOtherCanCompound(infoList[i], dataRepo))
        //            //    {
        //            //        return true;
        //            //    }
        //            //}
        //        }
        //    }
        //    return false;
        //}
        
        ////背包可提升装备过滤
        //public static bool isEquipCanUp(ItemInfo info, IDataRepository dataRepo)
        //{
        //    ItemInfo item = dataRepo.getItemByPos(OBJECTCELLTYPE.OBJECTCELLTYPE_EQUIP, ItemMap.Instance.type2equipt[info.cfg.kind]);
        //    //Transform tr = slot.transform.FindChild("equipUpWarning");
        //    bool isNeedShowWarning = false;
        //    if (item == null)
        //    {
        //        isNeedShowWarning = true;
        //    }
        //    else
        //    {
        //        if (info.cfg.quality > item.cfg.quality)
        //        {
        //            isNeedShowWarning = true;
        //        }
        //        else if (info.cfg.quality == item.cfg.quality)
        //        {
        //            if (info.cfg.zizhi > item.cfg.zizhi)
        //            {
        //                isNeedShowWarning = true;
        //            }
        //        }
        //    }
        //    return isNeedShowWarning;
        //}
        ////背包可合成装备碎片过滤
        //public static bool isEquipCanCompound(ItemInfo info, IDataRepository dataRepo)
        //{
        //    List<MergeObjConfig> _curItem = ConfigAsset2.Instance.getByField<MergeObjConfig>("mergeObjId1", info.cfg.id.ToString());
        //    if (_curItem.Count != 0)
        //    {
        //        int needComNum = _curItem[0].mergeObjNum(1) * _curItem[0].objNumPerTime;
        //        int num = dataRepo.getTotalItemNumByPos(info.cfg.id);
        //        if (num >= needComNum)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        ////背包可合成多合一物品过滤
        //public static bool isOtherCanCompound(ItemInfo info, IDataRepository dataRepo)
        //{
        //    //List<MergeIdxTypeConfig> idxCfgs = ConfigAsset2.Instance.getByField<MergeIdxTypeConfig>("id",5);
        //    //if (idxCfgs.Count == 0)
        //    //    return false;
        //    int searchId = 0;
        //    List<MergeObjConfig> mergeItems = null;
        //    mergeItems = ConfigAsset2.Instance.getByField<MergeObjConfig>("mergeObjId1", info.cfg.id);
        //    if (mergeItems.Count == 0)
        //    {
        //        mergeItems = ConfigAsset2.Instance.getByField<MergeObjConfig>("mergeObjId2", info.cfg.id);
        //        if (mergeItems.Count == 0)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            searchId = 1;
        //        }
        //    }
        //    else
        //    {
        //        searchId = 2;
        //    }
        //    if (dataRepo.getTotalItemNumByPos(mergeItems[0].mergeObjId(searchId)) >= mergeItems[0].mergeObjNum(searchId))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        ////英雄敬仰总检查
        //public static void isHaveHeroRankingPoint(stSendWorshipHeroDataCmd cmd)
        //{
        //    if (cmd.leftNum > 0 || CheckTools.isHaveHeroRankingReward(cmd) || CheckTools.isHaveHeroBeRankingRewarrd(cmd))
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.HERO, true);
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.HEROBANG, true);
        //    }
        //    else
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.HERO, false);
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.HEROBANG, false);
        //    }
        //}
        ////当前是否有可以领取的敬仰奖励
        //public static bool isHaveHeroRankingReward(stSendWorshipHeroDataCmd cmd)
        //{
        //    List<YXJYConfig> temp = ConfigAsset2.Instance.getByField<YXJYConfig>("AdmirationRewardType", 2);
        //    bool isHave = false;
        //    for (int y = 0; y < temp.Count; y++)
        //    {
        //        if (cmd.totalNum >= temp[y].Number)
        //        {
        //            BitArray bitArr = new BitArray(System.BitConverter.GetBytes((cmd.awardState)));
        //            if (!bitArr.Get(temp[y].id - 1))
        //            {
        //                isHave = true;
        //                break;
        //            }
        //        }
        //    }
        //    return isHave;
        //}
        //当前是否有可以领取的被敬仰奖励
        //public static bool isHaveHeroBeRankingRewarrd(stSendWorshipHeroDataCmd cmd)
        //{
        //    List<YXJYConfig> temp = ConfigAsset2.Instance.getByField<YXJYConfig>("AdmirationRewardType", 3);
        //    bool isHave = false;
        //    for (int y = 0; y < temp.Count; y++)
        //    {
        //        if (cmd.totaledNum >= temp[y].Number)
        //        {
        //            BitArray bitArr = new BitArray(System.BitConverter.GetBytes((cmd.awardState)));
        //            if (!bitArr.Get(temp[y].id - 1))
        //            {
        //                isHave = true;
        //                break;
        //            }
        //        }
        //    }
        //    return isHave;
        //}
        static uint officeTimerId = 0;
        static int officeGong = 0;
        ////当前是否可以升职和领取奖励s
        //static void OnCountryOfficeTime()
        //{
            
        //}
        
        //public static void BroadcostJuanXian(IDataRepository dataRepo)
        //{
        //    if (isCanCountryJuanXian(dataRepo))
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.LIELONG, true);
        //    }
        //    else
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.LIELONG, false);
        //    }
        //}
        //检测商城红点
        static uint shopTimeId;
        //public static void CheckShopPoint(params object[] arg)
        //{
        //    if (shopTimeId != 0)
        //    {
        //        TimerManager2.DelTimer(OnShopTimer);
        //    }
        //    shopTimeId = TimerManager2.AddTimer(2500, 100000, OnShopTimer);
        //}
        //static void OnShopTimer()
        //{
        //    TimerManager2.DelTimer(OnShopTimer);
        //    List<stShopItem> items = GetDataRepo().getSupermarket(enumShopBuyType.SHOPBUY_NORMAL);
        //    if (items == null || items.Count == 0)
        //        return;
        //    shopTimeId = 0;
        //    //int Star = GetDataRepo().GetTanMuAllStar();
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        int left = items[i].singleNumLimit > 0 ? (int)(items[i].singleNumLimit - items[i].selfNumBuy) : -1;
        //        List<SuperMarketBasicConfig> pages = ConfigAsset2.Instance.getByField<SuperMarketBasicConfig>("objId", (int)items[i].objid);
        //        if (pages == null || pages.Count == 0)
        //            return;
        //        int xian = 0;
        //        foreach (SuperMarketBasicConfig cfg in pages)
        //        {
        //            if (cfg.superMarketPageId == items[i].page)
        //            {
        //                xian = cfg.discontPrice;
        //            }
        //        }
        //        if (xian <= 0 && left > 0)
        //        {
        //            EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOP, true);
        //            return;
        //        }
        //    }
        //    if (GetIsHasTanMuStar())
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOP, true);
        //        return;
        //    }
        //    List<MysteriousShopBase> list = ConfigAsset2.Instance.getAll<MysteriousShopBase>();
        //    if (list.Count > 0)
        //    {
        //        for (int k = 0; k < list.Count; k++)
        //        {
        //            MysteriousShopBase temp = list[k];
        //            if (temp.od == 0)
        //                continue;
        //            if (temp.id == 4 && GetDataRepo().familyInfo == null) continue;
        //            stNewMysteryshopDataUserCmd mydata = GetDataRepo().GetMysteryshopData(temp.id);
        //            if (mydata == null)
        //                continue;
        //            for (int i = 0; i < mydata.data.Count; i++)
        //            {
        //                MysteriousShopObj shop = FastBuyUtil.searchInMysteriousmarket((int)mydata.mysteryshopId, (int)mydata.data[i].id);
        //                if (shop != null)
        //                {
        //                    if (shop.moneyNum == 0 && mydata.data[i].itemCount > 0)
        //                    {
        //                        EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOP, true);
        //                        return;
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOP, false);
        //}
        ////探墓星级变了 看看探墓商店有没有可以买的东西
        //static void OnTanMuStarChange(params object[] arg){
        //    stFentianStarInfoUserCmd cmd = (stFentianStarInfoUserCmd)arg[0];
        //    OnTanMuStarChange();
        //}
        //public static void OnTanMuStarChange()
        //{
        //    if (GetIsHasTanMuStar())
        //    {
        //        return;
        //    }
        //    int star = GetDataRepo().GetTanMuAllStar();
        //    int tanMuId = GetTanMuId(star);
        //    int TanMuCurStarId = PlayerPrefs.GetInt(GetDataRepo().mainPlayerData.displayName + "TanMuCurStarId", -1);
        //    if (TanMuCurStarId == -1 && tanMuId != -1)
        //    {
        //        if (tanMuId != -1)
        //        {
        //            PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + "TanMuHasPoint", 1);
        //            PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + "TanMuCurStarId", tanMuId);
        //        }
        //    }
        //    else
        //    {
        //        if (TanMuCurStarId != tanMuId)
        //        {
        //            PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + "TanMuHasPoint", 1);
        //            PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + "TanMuCurStarId", tanMuId);
        //        }
        //    }
        //    CheckShopPoint();
        //}
        //static int GetTanMuId(int star)
        //{
        //    int tanMuId = -1;
        //    //List<SuperMarketBasicConfig> sbc = ConfigAsset2.Instance.getByField<SuperMarketBasicConfig>("superMarketPageId", 7);
        //    //for (int i = 0; i < sbc.Count - 1; i++)
        //    //{
        //    //    if (sbc[i].needstarnumber <= star && sbc[i + 1].needstarnumber > star)
        //    //    {
        //    //        tanMuId = sbc[i].id;
        //    //    }
        //    //}
        //    //if (tanMuId == -1 && sbc[sbc.Count - 1].needstarnumber <= star)
        //    //{
        //    //    tanMuId = sbc[sbc.Count - 1].id;
        //    //}
        //    return tanMuId;
        //}
        //public static void SetIsHasTanMuStar()
        //{
        //    PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + "TanMuHasPoint", -1);
        //    CheckShopPoint();
        //}
        //public static bool GetIsHasTanMuStar()
        //{
        //    return PlayerPrefs.GetInt(GetDataRepo().mainPlayerData.displayName + "TanMuHasPoint", -1)==1?true:false;
        //}
        //检测我的国家是否正在宣战状态中 如果是则在主界面国战按钮上显示倒计时
        public static string SHOW_COUNTRYBTN_TIME = "show_countryBtn_time";//当有倒计时时广播到主界面显示时间
        static uint countryWarStateTime = 0;
        static uint countryWarStateTimeId = 0;
        //public static void CheckCountryWarState(uint sec)//stDragonGetWarInfoListRetUserCmd cmd, IDataRepository dataRepo)
        //{
        //    //if (countryWarStateTimeId == 0)
        //    //{
        //    //    for (int i = 0; i < cmd.warList.Count; i++)
        //    //    {
        //    //        if (cmd.warList[i].startTime > dataRepo.GetServerTimeSec())
        //    //        {
        //    //            if (cmd.warList[i].attackId == dataRepo.GetDragWarInfo().dragonId || cmd.warList[i].defenceId == dataRepo.GetDragWarInfo().dragonId)
        //    //            {
        //    //                DateTime dt = GetTime(cmd.warList[i].startTime.ToString());
        //    //                DateTime server = GetTime(dataRepo.GetServerTimeSec().ToString());
        //    //                TimeSpan ND = dt - server;
        //    //                countryWarStateTime = (uint)ND.TotalSeconds;
        //    //                countryWarStateTimeId = TimerManager2.AddTimer(0, 1000, OnCountryWarState);
        //    //                break;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    if (countryWarStateTimeId != 0)
        //    {
        //        TimerManager2.DelTimer(OnCountryWarState);
        //    }
        //    countryWarStateTime = sec;
        //    countryWarStateTimeId = TimerManager2.AddTimer(0, 1000, OnCountryWarState);
            
        //}
        //static void OnCountryWarState(){

        //    string result = TimerManager2.getTimeFmt(countryWarStateTime);
        //    EventDispatcher.DispatchEvent(CheckTools.SHOW_COUNTRYBTN_TIME, result);

        //    if (countryWarStateTime <= 0)
        //    {
        //        TimerManager2.DelTimer(OnCountryWarState);
        //        countryWarStateTimeId = 0;
        //        EventDispatcher.DispatchEvent(CheckTools.SHOW_COUNTRYBTN_TIME, "");
        //    }
        //    countryWarStateTime--;
        //}
        //static DateTime GetTime(string timeStamp)
        //{
        //    DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        //    long lTime = long.Parse(timeStamp + "0000000");
        //    TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        //}
        ////检测首冲的重置到几档次了 每档在主界面显示的按钮不同
        //public static void CheckShouChongDang(stFirstChargeGoldStatusUserCmd cmd,IDataRepository datare)
        //{
        //    int shouChongDang = GetChongZhiDang(cmd);
        //    List<FirstChargeBasic> allFirst = ConfigAsset2.Instance.getAll<FirstChargeBasic>();
        //    for (int i = 0; i < allFirst.Count; i++)
        //    {
        //        EventDispatcher.DispatchEvent(UIEventConst.MAINUI_REMOVE_RIGHTTOP_FEATURE, allFirst[i].buttonName);
        //    }
        //    if (shouChongDang != 0)
        //    {
        //        FirstChargeBasic fcb = ConfigAsset2.Instance.GetById<FirstChargeBasic>(shouChongDang);
        //        if (fcb != null && datare.HasHeadFeature("shouchongdali"))
        //        {
        //            EventDispatcher.DispatchEvent(UIEventConst.MAINUI_ADD_RIGHTTOP_FEATURE, fcb.buttonName);
        //            if (IsHasShouChongPoint(cmd))
        //            {
        //                if (fcb.kind == 1)
        //                {
        //                    EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOUCHONG, true);
        //                }
        //                else
        //                {
        //                    EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.KAIFUHAOLI, true);
        //                }
        //            }
        //            else
        //            {
        //                if (fcb.kind == 1)
        //                {
        //                    EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.SHOUCHONG, false);
        //                }
        //                else
        //                {
        //                    EventDispatcher.DispatchEvent(UIEventConst.SHOW_REDPOINT, UIEventConst.PointType.KAIFUHAOLI, false);
        //                }
        //            }
        //        }
        //    }
        //}
        //static bool IsHasShouChongPoint(stFirstChargeGoldStatusUserCmd cmd)
        //{
        //    if (cmd == null)
        //        return false;
        //    BitArray bitArr = new BitArray(System.BitConverter.GetBytes((cmd.status)));
        //    for (int i = 0; i < bitArr.Length - 1; i++)
        //    {
        //        if (bitArr.Get(i) && !bitArr.Get(i + 1))
        //        {
        //            return true;
        //        }
        //        i++;
        //    }
        //    return false;
        //}
        //public static int GetChongZhiDang(stFirstChargeGoldStatusUserCmd cmd)
        //{
        //    if (cmd == null)
        //        return 0;
        //    BitArray bitArr = new BitArray(System.BitConverter.GetBytes((cmd.status)));
        //    //bool[] bitRe = new bool[bitArr.Length];
        //    //for (int i = 0; i < bitArr.Length; i++)
        //    //{
        //    //    bitRe[i] = bitArr.Get(i);
        //    //}
        //    for (int i = 0; i < bitArr.Length-1; i++)
        //    {
        //        if (bitArr.Get(i) && !bitArr.Get(i + 1))
        //        {
        //            return i / 4 + 1;
        //        }
        //        i++;
        //    }
        //    for (int i = 0; i < bitArr.Length; i++)
        //    {
        //        if (!bitArr.Get(i))
        //        {
        //            return i / 4 + 1;
        //        }
        //        i++;
        //    }
        //    return 0;
        //}
        
        ////试炼小红点检测
        //static List<ActivityDailyTaskConfig> activityList;
        //public static void CheckShiLianPoint(IDataRepository dataRepo)
        //{
        //    if (activityList == null)
        //    {
        //        activityList = ConfigAsset2.Instance.getAll<ActivityDailyTaskConfig>();
        //    }
        //    List<ActivityDailyTaskConfig> opended = new List<ActivityDailyTaskConfig>();
        //    for (int i = 0; i < activityList.Count; i++)
        //    {
        //        ActivityDailyTaskConfig curActivity = activityList[i];
        //        if (curActivity.pageId == 0)
        //            continue;
        //        if (curActivity.pageId == 1)
        //        {
        //            //跳过特殊处理的 哎
        //            if (curActivity.id == 3019 || curActivity.id == 3020 || curActivity.id == 3017)
        //            {
        //                continue;
        //            }
        //            if (curActivity.functionOpen.Equals(""))
        //            {
        //                if (curActivity.openLevel <= dataRepo.mainPlayerData.userData.level)
        //                {
        //                    opended.Add(curActivity);
        //                }
        //            }
        //            else
        //            {
        //                if (dataRepo.HasHeadFeature(curActivity.functionOpen))
        //                {
        //                    opended.Add(curActivity);
        //                }
        //            }

        //        }
        //    }
        //    int isChecked = PlayerPrefs.GetInt(dataRepo.mainPlayerData.displayName + "shiLian_isTodayChecked",-1);
        //    if (isChecked == -1 || isChecked != System.DateTime.Now.Day)
        //    {
        //        PlayerPrefs.SetInt(dataRepo.mainPlayerData.displayName + "shiLian_isTodayChecked", System.DateTime.Now.Day);
        //        for (int i = 0; i < opended.Count; i++)
        //        {
        //            PlayerPrefs.SetInt(dataRepo.mainPlayerData.displayName + opended[i].id.ToString(), 0);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < opended.Count; i++)
        //        {
        //            int tempd = PlayerPrefs.GetInt(dataRepo.mainPlayerData.displayName + opended[i].id.ToString(), -2);
        //            if (tempd == -2)
        //            {
        //                PlayerPrefs.SetInt(dataRepo.mainPlayerData.displayName + opended[i].id.ToString(), 0);
        //            }
        //            if (opended[i].activityID != 0)
        //            { 
        //                if (tempd != -1)
        //                {
        //                    stActivityDegreeItemStstus dis = dataRepo.getActivityItem(opended[i].activityID);
        //                    if (dis != null)
        //                    {
        //                        if (dis.progress > 0)
        //                        {
        //                            PlayerPrefs.SetInt(dataRepo.mainPlayerData.displayName + opended[i].id.ToString(), -1);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    for (int i = 0; i < opended.Count; i++)
        //    {
        //        if (PlayerPrefs.GetInt(dataRepo.mainPlayerData.displayName + opended[i].id.ToString(), -1) == -1)
        //        {
        //            EventDispatcher.DispatchEvent(UIEventType.UI_DUP_MAP_BTN_HINT_EVENT,opended[i].id.ToString(),false);
        //        }
        //        else
        //        {
        //            EventDispatcher.DispatchEvent(UIEventType.UI_DUP_MAP_BTN_HINT_EVENT, opended[i].id.ToString(), true);
        //        }
        //    }
        //}
        ////点击一次小红点当天消失
        //public static void SetShiLianDataIsClicked(string key)
        //{
        //    PlayerPrefs.SetInt(GetDataRepo().mainPlayerData.displayName + key, -1);
        //    EventDispatcher.DispatchEvent(UIEventType.UI_DUP_MAP_BTN_HINT_EVENT, key, false);
        //}
        ////获取当前当天试炼信息
        //public static int GetShiLianPlayerPrefs(string key,int defult=-1)
        //{
        //    return PlayerPrefs.GetInt(GetDataRepo().mainPlayerData.displayName + key, defult);
        //}

        //public static IDataRepository GetDataRepo()
        //{
        //    if(datare == null){
        //        datare = DataRepository.Instance;
        //    }
        //    return datare;
        //}
        //检测装备是否需要继承
        //public static bool CheckIsNeedJiCheng(ItemInfo curInfo,ItemInfo theInfo)
        //{
        //    if (curInfo == null)
        //        return false;
        //    if (theInfo.cfg.needlevel > GetDataRepo().mainPlayerData.userData.level)
        //    {
        //        return false;
        //    }
        //    if (curInfo.cfg.quality > theInfo.cfg.quality)
        //        return false;
        //    if (curInfo.cfg.quality == theInfo.cfg.quality)
        //    {
        //        if (curInfo.cfg.zizhi > theInfo.cfg.zizhi)
        //            return false;
        //    }
        //    //bool isHasBounds = false;
        //    //for (int i = 0; i < _equip.equip.bonus.Count; i++)
        //    //{
        //    //    if (_equip.equip.bonus[i].id != 0)
        //    //    {
        //    //        isHasBounds = true;
        //    //        break;
        //    //    }
        //    //}
        //    if (curInfo.equip.enhanceLevel > theInfo.equip.enhanceLevel || curInfo.equip.cuiLianLevel > theInfo.equip.cuiLianLevel || curInfo.equip.grade > theInfo.equip.grade)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        bool isOtherNeed = (bool)LuaUtil.CallMethod("CheckTools", "CheckNeedJiCheng", curInfo.obj.pos.pos, theInfo.obj.pos.pos)[0];
        //        if (isOtherNeed)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}

