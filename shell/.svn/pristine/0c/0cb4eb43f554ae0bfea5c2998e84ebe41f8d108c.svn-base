# -*- coding: utf8 -*-'
import os,sys,json,zipfile,os.path,shutil;
import urllib2
#import jenkins;
import pysvn;
import subprocess;
from datetime import datetime;
from paramiko import SSHClient
from scp import SCPClient
import paramiko
from shutil import *
from time import sleep;
import sys;
from copyres2 import copyRes;
from copytostreamingasset_min2 import copyStreamingAsset,copyStreamingAssetToDir;
from encdll import encodeDll;
import zipfile;
import io;
def conflict_resolver( conflict_description ):
    print "conflict file:",conflict_description["kind"],conflict_description
    return pysvn.wc_conflict_choice.theirs_full, None, False
client = pysvn.Client();
client.callback_conflict_resolver = conflict_resolver
def checkAppSvn(d,pf,resid):
    
    print "begin to update:",d;
    client.update(d);
    
    cfg = readIni(basedir+"/"+pf+"_apprev.log");
    startv = int(cfg.get("rev") or 0);
    if startv<=0:
        return True,True,True,True;
    info = client.info(d+"/main");
    if info.revision.number == startv:
        return False,False,False,False;
    print "startv:",startv;
    appchange=False;
    reschange = False;
    luachange = False;
    filterchange = True;
    for msg in client.log(d+"/main",revision_end=pysvn.Revision(pysvn.opt_revision_kind.number,startv+1),discover_changed_paths=True,include_merged_revisions=True):
        showLog(msg.revision);
        for change in msg.changed_paths:
            #if change.path.find(".meta")>0:
            #    continue;
            showLog(change.path.decode("utf8"));
            if change.path.find("/UINew/")>0:
                reschange = True;
            elif change.path.find("/Assets/lua/")>0:
                luachange = True;
            elif change.path.find("/Assets/data/")>0 or change.path.find("/Assets/data2/")>0:
                continue;
            elif change.path.find("/Assets/ResFilters/")>0:
                if change.path.endswith("/Assets/ResFilters/"+resid+".csv"):
                    filterchange = True;
            else:
                appchange = True;    
            
                   
    showLog(u"资源变化:",reschange,u"包变化:",appchange,u"lua变化:",luachange);
 
    return reschange,appchange,luachange,filterchange;
    '''
    revlog=open(outdir+"/revlog.txt","w");
    
    '''
def checkResSvn(d,pf,resid):
    
    showLog("begin to update:",d);
    client.update(d);
    
    cfg = readIni(basedir+"/"+pf+"_resrev.log");
    startv = int(cfg.get("rev") or 0);
    if startv<=0:
        return True,True;
    info = client.info(d);
    if info.revision.number == startv:
        return False,False;
    showLog("start_res_v:",startv);
    res_change = False;
    filterchange = True;
    for msg in client.log(d+"/Assets",revision_end=pysvn.Revision(pysvn.opt_revision_kind.number,startv+1),discover_changed_paths=True,include_merged_revisions=True):
        showLog(msg.revision);
        #print msg.changed_paths;
        for change in msg.changed_paths:
            #if change.path.find(".meta")>0:
            #    continue;
            showLog(change.path.decode("utf-8"));
            #if change.path.find("/art/")>0:
            #    res_change = True;
            #if change.path.find("/Assets/")>0:
            #    res_change = True;
            if change.path.find("/Assets/ResFilters")>0:
                if change.path.endswith("/Assets/ResFilters/"+resid+".csv"):
                    filterchange = True;
            else:
                res_change = True;
    showLog("res_changes:",res_change,"filterchange:",filterchange);
    return res_change,filterchange;
def writeAppSvnInfo(d):
    cfg = readIni(basedir+"/"+pf+"_apprev.log");
    
    info = client.info(d+"/main");
    cfg["prev"] = cfg.get("rev") or 0;
    cfg["rev"]=info.revision.number;
    writeIni(cfg,basedir+"/"+pf+"_apprev.log");

def writeResSvnInfo(d):
    cfg = readIni(basedir+"/"+pf+"_resrev.log");
    
    info = client.info(d);
    cfg["prev"] = cfg.get("rev") or 0;
    cfg["rev"]=info.revision.number;
    writeIni(cfg,basedir+"/"+pf+"_resrev.log");
 
def showLog(*Arg ):
    print u" ".join([ type(k)==unicode and k or str(k) for k in Arg]),datetime.now()  ;
   
    sys.stdout.flush(); 
 
def deployRes(cfg,d,resid,force):
    showLog("begin deploy res:");
     
    client.update(d);
    showLog("update res svn end,begin check change ");
    res_change,filterchange = checkResSvn(d,pf,resid);
    showLog("res_changes:"+str(res_change)+",force:"+str(force)+",filterchange:"+str(filterchange));
      
    if res_change or force:
        showLog("res_is_changes:",res_change);
        
        reschangeabname =  deployResAbName(cfg,cfg["RES_PATH"]);
        waitAllCmd();
        logpath = os.path.join(logsdir,pf+"_res.log");
        executeCmd(['"'+cfg["UNITY_PATH"]+'"',"-batchmode -noUpm",'-projectPath',d,"-executeMethod","BuildTool.BuildAssetBundles","-quit"," -logFile "+logpath],logpath);
        #if returnCode != 0:
        #    print "excute project build error:",returnCode;
        #    exit(returnCode);
    '''
    if filterchange:
        showLog("begin updatefilter");
        updateFilter(d+"/Assets/ResFilters/"+resid+".csv",d+"/AssetBundles/"+pf+"/res/res.manifest",pf+"/ResFilters/"+resid+"/res.csv","res");
    '''    
    return res_change or force,filterchange;
def deployResAbName(cfg,d):
    client.update(d);
    showLog("begin run unity res proj");
    logpath = os.path.join(logsdir,pf+"_resab.log"); 
    #sleep(2)
    executeCmd(['"'+cfg["UNITY_PATH"]+'"',"-batchmode -noUpm",'-projectPath',d,"-executeMethod","BuildTool.UpdateAllAssetBundle","-quit"," -logFile "+logpath],logpath);
        
waitCmds=[];
def executeCmd(args,checkArgs,shell=True):
    global waitCmds;
    showLog("execute cmd:"+" ".join(args) );
    process = subprocess.Popen(" ".join(args),shell=True  );
    waitCmds.append((process," ".join(args),checkArgs));
    #out, err = process.communicate()
    #for line in process.stdout: 
    #    print line;
    #print out,err;
    #returnCode = process.returncode

    #print "execute cmd:",returnCode;
    
    #if returnCode != 0:
    #    print "excute cmd error:",",".join(args);
    #    exit(returnCode);
def waitAllCmd():
    global waitCmds;
    showLog("begin waitAllCmd,cnt:"+str(len(waitCmds)));
    for cmd in waitCmds:
        showLog("begin wait:"+cmd[1]);
        cmd[0].wait();
        showLog("run cmd end returncode:",cmd[0].returncode);
        if cmd[0].returncode != 0:
            showLog("execute cmd error code:"+str(cmd[0].returncode)+",cmd:"+cmd[1]+",logfile:"+str(cmd[2]));
            exit(cmd[0].returncode);
        elif cmd[2] != None:
            checkUnityLog(cmd[2]);
    waitCmds=[];        
def checkUnityLog(logPath):
    success=True;
    nochange = False;
    
    #if not os.path.exists(logPath):
        #os.mknod(logPath);
    showLog("begin check log:"+logPath);    
    f = open(logPath);
    for l in f.readlines():
        if (l.find(": error")>0 or l.find(" failed:")>0 or l.find("Exception:")>0) and l.find("Task failed:")<0:
            success=False;
            print l;
        if l.find("No AssetBundle needs to be rebuilt for this build.")>=0:
            nochange = True;
    if not success:
        showLog("do unity build failed:");
        exit(-1);
    return nochange;
    
def doJenkins(cfg,jobname):
    server = jenkins.Jenkins(cfg["JENKINS_URL"], username=cfg["JENKINS_USERNAME"], password=cfg["JENKINS_PASSWORD"])
    next_build_number = server.get_job_info(jobname)['nextBuildNumber']
    print server.build_job(jobname);

    sleep(10)
    build_info = None;
    while True:
        build_info = server.get_build_info(jobname, next_build_number)
        if build_info["building"] == True:
            print "waiting jenkins ",cfg["JENKINS_URL"],jobname,next_build_number," building...";
            sleep(5);

        else:
            break;
    if build_info["result"] == "SUCCESS":
        print "do jekins job:",cfg["JENKINS_URL"],jobname,next_build_number," success";
    else:
        print "do jekins job:",cfg["JENKINS_URL"],jobname,next_build_number," failed";
        print server.get_build_console_output(jobname,next_build_number);
        exit(-1);
    
def deployUI(cfg,d,pf):
    logpath = os.path.join(logsdir,pf+"_ui.log");
    executeCmd(['"'+cfg["UNITY_PATH"]+'"','-projectPath',d+"main/","-executeMethod","CreateAssetBundle.UpdateAndBuildAssetBundleFromPython","-batchmode -noUpm -nographics","-quit"," -logFile "+logpath],logpath);
    #checkUnityResult(cfg);
    
def uploadPkg(cfg,fpath,fn):
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])

    with SCPClient(ssh.get_transport()) as scp:
        print scp.put(fpath,cfg["UPLOAD_DEST_PATH"]+"/"+fn);
def zipAndUploadPkg(cfg,p,fn):
    p = p.replace("\\","/");
    if not p.endswith("/"):
        p = p+"/";
    f = zipfile.ZipFile(p+"../tmp.zip",'w',zipfile.ZIP_DEFLATED);
    for dirpath, dirnames, filenames in os.walk(p): 

        for filename in filenames: 
            subf = os.path.join(dirpath,filename).replace("\\","/");
            
            arcname = subf[len(p):];
            #print subf,p,arcname;
            f.write(subf,arcname) 

    f.close()
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])
    showLog("begin upload pkgs from:",p+"../tmp.zip","to",cfg["UPLOAD_DEST_PATH"]+"/"+fn+".zip");
    with SCPClient(ssh.get_transport()) as scp:
        print scp.put(p+"../tmp.zip",cfg["UPLOAD_DEST_PATH"]+"/"+fn+".zip");    
   
def deployPkg(cfg,pf,channel,resid):
    if pf != "android":
        copyStreamingAsset(pf, resid);
    print "begin deployPkg";
    pfcmd={"android":"ProjectBuild.BuildForAndroidProject",
    "Windows":"ProjectBuild.BuildForPC",
    "iOS":"ProjectBuild.BuildForIPhone"}[pf];
    pkgcfgf = cfg["APP_PATH"]+"/main/Assets/config/"+pf+"/"+channel+"/config.txt";
    print pkgcfgf,os.path.exists(pkgcfgf);
    cfgf = open(pkgcfgf);
    cfgdata =  cfgf.read();
    pkgcfg = json.loads(cfgdata);
    if pf == "android":
        outAndroidPath = cfg["APP_PATH"]+"/main/AndroidPlugins/"+channel+"/Android";
        inAndroidPath =  cfg["APP_PATH"]+"/main/Assets/Plugins/Android";
        shutil.rmtree(inAndroidPath);
        shutil.copytree(outAndroidPath,inAndroidPath );
        #resPath1 = cfg["APP_PATH"]+"/main/Assets/config/"+pf+"/"+channel+"/icon/Default.png";
        #resPath2 = cfg["APP_PATH"]+"/main/Assets/UI";
        #shutil.copy(resPath1,resPath2);
    '''
    texturedir = cfg["APP_PATH"]+"/main/Assets/StreamingAssets/texture";
    if os.path.exists(texturedir):
        shutil.rmtree(texturedir);
    shutil.copytree(cfg["APP_PATH"]+"/main/Assets/config/"+pf+"/"+channel+"/texture",texturedir,False);
    resdir = cfg["APP_PATH"]+"/main/Assets/config/"+pf+"/"+channel+"/res/Resource";
    for f in os.listdir(resdir):
        if not f.endswith(".meta"):
            shutil.copy(resdir+"/"+f,cfg["APP_PATH"]+"/main/Assets/Resources/"+f);
    '''
    pkgname = pkgcfg["pkgname"].replace("{tm}",datetime.today().strftime("%Y%m%d%H%M")).replace("{ver}",pkgcfg["bundleVersion"]);
    pkgname = pkgname.replace(".apk","");
    logpath = os.path.join(logsdir,pf+"_app.log");

    executeCmd(['"'+cfg["UNITY_PATH"]+'"',"-batchmode -noUpm -nographics", '-projectPath',cfg["APP_PATH"]+"/main","-executeMethod",pfcmd,"channel-"+channel,"subchannel-"+subchannel,"name="+pkgname, "-quit"," -logFile "+logpath],logpath);
    waitAllCmd();

    #checkUnityResult(cfg);
    pkgpath = cfg["APP_PATH"]+"/main/build/"+pkgname;
    pkgConfig1 = cfg["APP_PATH"]+"main/Assets/config/"+pf+"/"+channel+"/res/Resource/config.txt";
    pkgConfig2 = cfg["APP_PATH"]+"main/Assets/Resources/config.txt";
    print pkgConfig1;
    print pkgConfig2;
    shutil.copyfile(pkgConfig1,pkgConfig2);

    if pf=="iOS":
        print "begin iOS end";
        executeCmd(
            ['"' + cfg["UNITY_PATH"] + '"', "-batchmode -noUpm -nographics", '-projectPath', cfg["APP_PATH"] + "/main",
             "-executeMethod", "XCodePostProcess.xcodepostmk" ,"name="+pkgname, "-quit",
             " -logFile " + logpath], logpath);
        waitAllCmd();
        return;
    if pf == "Windows":
        zipAndUploadPkg(cfg, pkgpath,pkgname);   
    elif pf == "android":
        doAndroidGradle(cfg,pkgpath,pkgname,pf, resid);
        #doAndroidRepackage(cfg,pkgpath+".apk",pkgname+".apk");
    else:    
        if not os.path.exists(pkgpath):
            raise Exception("do unity package failed:"+pkgpath);
            
        uploadPkg(cfg,cfg["APP_PATH"]+"/main/build/",pkgname);
def doAndroidRepackage(cfg,pkgpath,fn):
    tmmpath = cfg["APP_PATH"]+"/main/build/temp";
    logpath = cfg["UNITY_LOG_PATH"]+"/repkg.log";
    executeCmd(['java',' -jar ',sys.path[0]+"/ShakaApktool.jar","d",pkgpath,"-f","-o temp"],None);
    waitAllCmd();
    txt=""; 
    with open("temp/apktool.yml") as f:
        txt = f.read();
        prefix = "targetSdkVersion: ";
        p1 = txt.find(prefix);
        p1 = txt.find("'",p1);
        p2 = txt.find("'",p1+ 1);
        txt = txt[:p1+1]+"22"+txt[p2:];
    with open("temp/apktool.yml","w") as f:    
        f.write(txt);
    with open("temp/AndroidManifest.xml") as f:
        txt = f.read();
        prefix = "platformBuildVersionCode";
        p1 = txt.find(prefix);
        p1 = txt.find('"',p1);
        p2 = txt.find('"',p1+ 1);
        txt = txt[:p1+1]+"22"+txt[p2:];
    with open("temp/AndroidManifest.xml","w") as f:    
        f.write(txt);  
    dllpath = "temp/assets/bin/Data/Managed/Assembly-CSharp.dll"; 
    if not os.path.exists("android/release/app"):
        os.mkdir("android/release/app");
    encodeDll("android",dllpath,dllpath,"android/release/app_temp/Assembly-CSharp.dll");    
    shutil.copyfile(cfg["APP_PATH"]+"/main/mono/libmono.so","temp/lib/armeabi-v7a/libmono.so");
    executeCmd(['java',' -jar ',sys.path[0]+"/ShakaApktool.jar","b","temp" ],None);
    waitAllCmd();    
    executeCmd(['"'+cfg["JDK_PATH"]+"/bin/jarsigner"+'"','-keystore jlcq.keystore ',"-storepass hl123456","-signedjar ",pkgpath[:pkgpath.rfind(".")]+"_new.apk","temp/dist/"+fn,"jlcq.keystore" ],None);
    waitAllCmd(); 
    uploadPkg(cfg,pkgpath[:pkgpath.rfind(".")]+"_new.apk",fn); 
    uploadDll();
def uploadDll():
     
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])

    with SCPClient(ssh.get_transport()) as scp:
        print scp.put("android/release/app_temp/Assembly-CSharp.dll",cfg["UPLOAD_RES_PATH"]+"/android/release/app/Assembly-CSharp.dll");
        print scp.put("android/release/app_temp/app.info",cfg["UPLOAD_RES_PATH"]+"/android/release/app/app.info");
    ssh.close();    
 
def copyStreamToApk(cfg,fn,pf, resid):
    destdir = fn[:fn.find(".")];
    
    print "unzip dest",destdir;
   
    if os.path.exists(destdir):
        shutil.rmtree(destdir);
    zf = zipfile.ZipFile(fn);
    infos = zf.infolist();
    origfiles={};
    for inf in infos:
        origfiles[inf.filename] = inf.compress_type;
        print inf.filename;
    zf.extractall(destdir);
    streamdir = destdir+"\\assets\\";
    copyStreamingAssetToDir(pf,resid,streamdir);
    fnnew = destdir+"_new.apk";
    newzf = zipfile.ZipFile(fnnew,"w",zipfile.ZIP_STORED);
    for dirpath, dirnames, filenames in os.walk(destdir):
        fpath = dirpath.replace(destdir,'')
        fpath = fpath and fpath + os.sep or ''
        for filename in filenames:
            wfn = fpath+filename;
            wfn = wfn.replace("\\","/").lstrip("/"); 
            if wfn.endswith(".so"):
                print "wfn:",wfn;
            if wfn in origfiles :
                print "orig:",wfn;
                newzf.write(os.path.join(dirpath, filename),fpath+filename,origfiles[wfn])
            else:
                newzf.write(os.path.join(dirpath, filename),fpath+filename,zipfile.ZIP_STORED)
    newzf.close();
    executeCmd(['"'+cfg["JDK_PATH"]+"/bin/jarsigner"+'"','-keystore jlcq.keystore ',"-storepass hl123456","-signedjar ",destdir+"_signed.apk",fnnew,"jlcq.keystore" ],None);
    waitAllCmd(); 
def doAndroidGradle(cfg,pkgpath,fn,pf, resid):
    srcdir = cfg["APP_PATH"]+"/main/AndroidPlugins/"+channel+"/Gradle";
    dirs = os.listdir(pkgpath);
    if len(dirs)!= 1:
        raise Exception(u"打包失败,找不到目录:"+pkgpath);
        exit(1);
    destdir =os.path.join(pkgpath,dirs[0]);
    showLog(u"destdir:",destdir);
    for dirpath, dirnames, filenames in os.walk(srcdir):
        for filename in filenames: 
            subf = os.path.join(dirpath,filename);
            destd = os.path.join(destdir,dirpath[len(srcdir):]);
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = os.path.join(destdir,subf[len(srcdir)+1:]);
            shutil.copyfile(subf,destf);
            showLog("copy grale file from:",subf,destf,subf,srcdir,destdir);
    pwd = os.getcwd();
    os.chdir(destdir.encode("gbk"));    
    executeCmd(['"'+cfg["GRADLE_PATH"]+'"',"assembleRelease"],None);
    waitAllCmd();
    os.chdir(pwd);
    
    pkgpath = os.path.join(destdir,u"build\\outputs\\apk");
    pkgname = None;
    for f in os.listdir(pkgpath):
        if f.find("-release.apk")>0:
            pkgname = f;
            break;
    if pkgname == None:
        raise Exception(u"打包失败,找不到apk,目录:"+pkgpath);
       
    uploadPkg(cfg,os.path.join(pkgpath,pkgname),fn+".apk");
def checkUnityResult(cfg):
    success=True;
    nochange = False;
    logPath = cfg["UNITY_LOG_PATH"]+"/Editor.log";
    #if not os.path.exists(logPath):
        #os.mknod(logPath);
    f = open(cfg["UNITY_LOG_PATH"]+"/Editor.log","a+");
    for l in f.readlines():
        if (l.find(": error")>0 or l.find(" failed:")>0 or l.find("Exception:")>0) and l.find("Task failed:")<0:
            success=False;
            print l;
        if l.find("No AssetBundle needs to be rebuilt for this build.")>=0:
            nochange = True;
    if not success:
        print "do unity build failed:";
        exit(-1);
    return nochange;


def deployApp(cfg,d,pf,channel,subchannel,resid,forceapp):
    reschange,appchange,luachange,filterchange = checkAppSvn(d,pf,resid);
    print "deployApp:",forceapp
    
    if reschange or forceapp  :
        deployUI(cfg,d,pf);
    '''
    if filterchange:
        updateFilter(d+"/main/Assets/ResFilters/"+resid+".csv",d+"/main/AssetBundles/"+pf+"/ui/ui.manifest",pf+"/ResFilters/"+resid+"/data.csv","ui");
    '''
    pkgchange = appchange or forceapp or filterchange;
        
    
    return reschange,luachange,pkgchange;    
  
def doDeployAll(cfg,pf,channel,subchannel,resid,force):
    reschange,filterchange =  deployRes(cfg,cfg["RES_PATH"],resid,force);
    showLog("deploy res end,do deploy app");    
    uichange,luachange,pkgchange = deployApp(cfg,cfg["APP_PATH"],pf,channel,subchannel,resid,force);
    showLog("uichange:"+str(uichange));
    sys.stdout.flush();
    waitAllCmd();
    
    showLog("begin copy res");
    copyRes(pf,pf,reschange,uichange,luachange,False);        
    showLog("end deploy res,begin jenkins job");
    if pkgchange or filterchange  :       
        deployPkg(cfg,pf,channel,resid);     
    sys.stdout.flush();
    writeAppSvnInfo(cfg["APP_PATH"]);
    writeResSvnInfo(cfg["RES_PATH"]);
def doDeployRes(cfg,pf,channel,subchannel,resid,force):
    reschange =  deployRes(cfg,cfg["RES_PATH"],resid,force);
    writeResSvnInfo(cfg["RES_PATH"]); 
def doDeployApp(cfg,pf,channel,subchannel,resid,force):
    uichange,luachange,packagechage = deployApp(cfg,cfg["APP_PATH"],pf,channel,subchannel,resid,force);
    
    waitAllCmd();
    if   uichange or luachange or force:
        showLog("begin copy res");
        copyRes(pf,pf,False,uichange,luachange,False);        
     
          
    writeAppSvnInfo(cfg["APP_PATH"]);
 
    
def doDeploy(cfg,pf,channel,subchannel,target,resid,force):
    print "begin to deplay:",pf,channel,subchannel,target,force;
    sys.stdout.flush();
    #reschangeabname =  deployResAbName(cfg,cfg["RES_PATH"]);
    #showLog("set abname end,do deploy res");
    if target == "all":
        doDeployAll(cfg,pf,channel,subchannel,resid,force);
    elif target == "res":
        doDeployRes(cfg,pf,channel,subchannel,resid,force);
        copyRes(pf,pf,True,False,False,False);
    elif target == "app":
        doDeployApp(cfg,pf,channel,subchannel,resid,force);
        copyRes(pf,pf,False,True,True,False);
    elif target == "pkg":
        #deployPkg(cfg,pf,channel,resid);
        copyStreamToApk(cfg,"F:\workspace\\app_android_online\\main\\build\\jlcqonline_201809141542\\test\\build\\outputs\\apk\\test-release.apk","android","1");
    elif target == "copy":
        copyRes(pf,pf,True,True,True,True);
    elif target=="repkg":
        pkgname = cfg["APP_PATH"]+"/main/build/"+resid
        doAndroidRepackage(cfg,pkgname,resid);
    
def get_login( realm, username, may_save ):
    print realm,username;
    global cfg;
    id = realm.split(" ")[1];
    print "id=",id;
    return (True,cfg["name_"+id],cfg["password_"+id],True);  
def writeIni(cfg,fn):
    cfgf = open(fn,"w");
    print cfg;
    for k in cfg:
        cfgf.write(k+"="+str(cfg[k])+"\n");
    cfgf.close();    
def readIni(fn):
    if not os.path.exists(fn):
        print "ini file not exists:",fn;
        return {};
    print "read ini :",fn;    
    cfgf = io.open(fn, encoding="utf-8");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
     
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    return cfg;     
if __name__ == "__main__":
    reload(sys)
    sys.setdefaultencoding("gbk");
    print sys.getdefaultencoding()
    if len(sys.argv)<2:
        print u"参数个数小于2";
        exit(-1);
    
     
    pf = sys.argv[1];
    channel = sys.argv[2];
    subchannel="";
    target = "all";
    resid = "1";
    '''
    if len(sys.argv)>3:
        subchannel = sys.argv[3];
    else:
        subchannel="";
    '''    
    force = False;
    if len(sys.argv)>3:
        target = sys.argv[3];
    if len(sys.argv)>4:
        resid = sys.argv[4];    
    if len(sys.argv)>5:
        force = sys.argv[5] == "true";
     
    basedir = os.getcwd()  

    logsdir = os.path.join(basedir,"logs");

    if not os.path.exists(logsdir):
        os.mkdir(logsdir);
    print "basedir:",basedir,"logdir:",logsdir;
    cfg = readIni(basedir+"/"+pf+".conf");
    print(cfg["datadir"]);

    client.callback_get_login = get_login;
     
    doDeploy(cfg,pf,channel,subchannel,target,resid,force);
#doDeployApp(cfg,pf,channel,subchannel,forceapp);
#doJenkins(cfg,"res_android");
#zipAndUploadPkg("E:/swcard/app_pc/main/build/swcard_1_0_0_20151214170122","test");