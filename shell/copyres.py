# -*- coding: utf8 -*-'
import os,md5,shutil;
import zlib,sys,pysvn;
import platform;
from datetime import datetime;
from paramiko import SSHClient
from scp import SCPClient
import paramiko
from multiprocessing import Pool, Queue,Process
import multiprocessing;
import time
import io;
import zipfile
def conflict_resolver( conflict_description ):
    print "conflict file:",conflict_description
    return pysvn.wc_conflict_choice.theirs_full, None, False
client = pysvn.Client();
client.callback_conflict_resolver = conflict_resolver

def zip_dir(dirname,zipfilename):
    filelist = []
    if os.path.isfile(dirname):
        filelist.append(dirname)
    else :
        for root, dirs, files in os.walk(dirname):
            for name in files:
                filelist.append(os.path.join(root, name))
    showLog("begin zip to:",zipfilename,",len:",len(filelist));     
    zf = zipfile.ZipFile(zipfilename, "w", zipfile.ZIP_STORED)
    for tar in filelist:
        arcname = tar[len(dirname):]
        #print arcname
        zf.write(tar,arcname)
    zf.close()
def md5file(fn):
    f = open(fn,"rb");
    m = md5.new();
    data = f.read();
    m.update(data);
    f.close();
    return m.hexdigest(),len(data);
def md5str(val):
    m = md5.new();
    m.update(val);  
    return m.hexdigest();
 

def copyuiAsset(subdir,destdir,assetDir):
    showLog("begin copyuiAsset");
    basedir = os.getcwd();
    uiAssetDir = os.path.join(assetDir,subdir,"ui");    
    destdir = os.path.join(basedir,destdir,"orig","data","ui");
     
    return copyFiles(uiAssetDir,destdir);
    '''
    oldfiles = set();
    for root, dirs, files in os.walk(destdir):
        for f in files:
            fn = os.path.join(root,f);
            oldfiles.add(fn);
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
            destd = destdir+root[len(uiAssetDir):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(uiAssetDir):];
            shutil.copy(fn,destf);
            if destf in oldfiles:
                oldfiles.remove(destf);
            if fn.find("mainviewlua")>=0:
                print "copy", fn," to",destf;
    for f in oldfiles:
        os.remove(f);
    '''    
def copySoundAsset(subdir,destdir,assetDir):
    uiAssetDir = assetDir;
    destdir = destdir+"/orig/data/sound/";
    oldfiles = set();
    for root, dirs, files in os.walk(destdir):
        for f in files:
            fn = os.path.join(root,f);
            oldfiles.add(fn);
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
            destd = destdir+root[len(uiAssetDir):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(uiAssetDir):];
            shutil.copy(fn,destf);
            if destf in oldfiles:
                oldfiles.remove(destf);
            
    for f in oldfiles:
        os.remove(f);
def copyLuaAsset(subdir,destdir,assetDir,luaConvertDir):
    showLog("begin copyLuaAsset");
    strsys = platform.system();
    if strsys == "Windows":
        convertCmd = "luajit.exe -b {0} {1}";
    elif platform.system() == "Darwin":
        convertCmd = "./luac -o {1} {0}";
    retval = os.getcwd()
    print "retval:",retval;
    uiAssetDir = assetDir;
    destdir = destdir+"/orig/data/lua/";
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    oldfiles = set();
    changefiles=[];
    delfiles=[];
    addfiles = [];
    for root, dirs, files in os.walk(destdir):
        for f in files:
            fn = os.path.join(root,f);
            oldfiles.add(fn);
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if  not f.endswith(".lua") and not f.endswith(".pb") :
                continue;
            fn = os.path.join(root,f);
            destd = destdir+root[len(uiAssetDir):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(uiAssetDir):];
            if f.endswith(".lua"):
                if subdir == "Windows":
                    shutil.copy(fn,destf);
                else:    
                    cmd = convertCmd.format(fn,retval+"/"+destf);
                    
                    os.chdir(luaConvertDir);
                    if 0 != os.system(cmd):
                        print "execute lua encode errur:",fn;
                        exit(-1);
                    os.chdir(retval);
            else:
                shutil.copy(fn,destf);
            changefiles.append(destf);    
            #shutil.copy(fn,destf);
            if destf in oldfiles:
                oldfiles.remove(destf);
            #print "copy", fn," to",destf;
    for f in oldfiles:
        os.remove(f);
    return changefiles,addfiles,delfiles;
def copyFiles(src,dest):
    showLog("begin copy files from:",src,"to",dest);
    oldfiles = set();
    changefiles=[];
    delfiles=[];
    addfiles = [];
    for root, dirs, files in os.walk(dest):
         
        for f in files:
            fn = os.path.join(root,f);
            oldfiles.add(fn);            
    for root, dirs, files in os.walk(src):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
            destd = dest+root[len(src):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = dest+fn[len(src):];
            #showLog("orig:",fn,"dest:",destf,dest,root[len(src):]); 
            if destf in oldfiles:
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                if info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    shutil.copy(fn,destf);
                    changefiles.append(destf);
            else:
                shutil.copy(fn,destf);
                addfiles.append(destf);
            
            if destf in oldfiles:
                oldfiles.remove(destf);  
    for f in oldfiles:
        os.remove(f);    
        delfiles.append(f);   
    if len(changefiles)>10:
        showLog("changefiles count:",len(changefiles),"files:\n","\n".join(changefiles[:10]),"\n...");
    elif len(changefiles)>0:
        showLog("changefiles:\n","\n".join(changefiles) );   
    if len(delfiles)>10:
        showLog("delfiles count:",len(delfiles),"files:\n","\n".join(delfiles[:10]),"\n...");
    elif len(delfiles)>0:
        showLog("delfiles:\n","\n".join(delfiles) );   
    if len(addfiles)>10:
        showLog("addfiles count:",len(addfiles),"files:\n","\n".join(addfiles[:10]),"\n...");
    elif len(delfiles)>0:
        showLog("addfiles:\n","\n".join(addfiles) );   
             
    return changefiles,delfiles,addfiles;   
def encyptFiles(src,dest,subtype,a):
    showLog("begin encrypt files from:",src,"to",dest);
    oldfiles = set();
    changefiles=[];
    delfiles=[];
    addfiles = [];
    infos={};
    for root, dirs, files in os.walk(dest):
         
        for f in files:
            fn = os.path.join(root,f);
            if fn.find("ver_")<0:
                oldfiles.add(fn);            
    for root, dirs, files in os.walk(src):
        for f in files:
            
            fn = os.path.join(root,f);
            abname = fn[len(src)+1:];
            abname = abname.replace("\\","/");
            md5name = md5str(abname);
            md5val,flen = md5file(fn);
            infos[md5name] = (abname,md5val,str(flen)); 
            destd = os.path.join(dest,md5name[0]);
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = os.path.join(destd,md5name);
            #showLog("orig:",fn,"dest:",destf,dest,root[len(src):]); 
            if destf in oldfiles:
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                #if fn.find("MapConfig.db")>0:
                #    showLog("db file:",fn,destf,info1.st_mtime,info2.st_mtime,info1.st_size,info2.st_size);
                if info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    shutil.copy(fn,destf);
                    changefiles.append(destf);
            else:
                shutil.copy(fn,destf);
                addfiles.append(destf);
            
            if destf in oldfiles:
                oldfiles.remove(destf);  
 
    f= open(os.path.join(dest,"../info_"+subtype+".csv"),"w");
    verstr="";
    for k in infos:
        val = infos[k];
        
        verstr +=k+","+val[1]+","+val[2];
        tp="4";
        if val[0].startswith("data/"):
            if val[0].endswith("data.db"):
                tp ="1";
            else:
                tp = "6";
        elif val[0].startswith("ui/"):
            tp ="3";
        elif val[0].startswith("lua/"):
            tp ="2";
        else:
            tp ="4";
        verstr +=","+tp;
        verstr +="|";
        f.write(",".join(val)+","+k+","+tp+"\n");
    verstr = verstr[:len(verstr)-2];
    with open(os.path.join(dest,"ver_"+subtype+".dat"),"wb") as verf:
        verf.write(zlib.compress(verstr));
    with open(os.path.join(dest,"../ver_"+subtype+".txt"),"w") as verf:
        verf.write(verstr); 
    f.close();
    datestr = datetime.now().strftime('%Y%m%d%H%M%S');
    with open(os.path.join(dest,"ver_"+subtype+".info"),"w") as verf2:
        verf2.write(md5file(os.path.join(dest,"ver_"+subtype+".dat"))[0]+","+datestr);
             
    showLog("write info file:",os.path.join(dest,"ver_"+subtype+".info"),",dat file:",os.path.join(dest,"ver_"+subtype+".dat"));
    for f in oldfiles:
        os.remove(f);    
        delfiles.append(f);   
    if len(changefiles)>10:
        showLog("changefiles count:",len(changefiles),"files:\n","\n".join(changefiles[:10]),"\n...");
    elif len(changefiles)>0:
        showLog("changefiles:\n","\n".join(changefiles) );   
    if len(delfiles)>10:
        showLog("delfiles count:",len(delfiles),"files:\n","\n".join(delfiles[:10]),"\n...");
    elif len(delfiles)>0:
        showLog("delfiles:\n","\n".join(delfiles) );   
    if len(addfiles)>10:
        showLog("addfiles count:",len(addfiles),"files:\n","\n".join(addfiles[:10]),"\n...");
    elif len(delfiles)>0:
        showLog("addfiles:\n","\n".join(addfiles) );   
    if len(changefiles)>0 or len(addfiles)>0 or len(delfiles)>0:
        changefiles.append(os.path.join(dest,"ver_"+subtype+".dat"));
        changefiles.append(os.path.join(dest,"ver_"+subtype+".info"));
        
    return changefiles,addfiles,delfiles ;  
def copyData(src,dest,dataDir):
    showLog("begin copy data");
    basedir = os.getcwd();
    changefiles=[];
    addfiles=[];    
    client.update(dataDir ); 
    destdir = os.path.join(basedir,dest,"orig","data","data");
    for root, dirs, files in os.walk(dataDir):
        for f in files:
            if not f.endswith(".db"):
                continue;
            fn = os.path.join(root,f);
            destd = dest+root[len(src):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf =os.path.join(destdir,f);
            
            if os.path.exists(destf):
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                if info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    showLog("copy db:",fn,destf);
                    shutil.copy(fn,destf);
                    changefiles.append(destf);
            else:
                shutil.copy(fn,destf);
                addfiles.append(destf);
            
            #if destf in oldfiles:
            #    oldfiles.remove(destf);   
    return changefiles,addfiles,None
def copyNewData(src,dest,dataDir):
    showLog("begin copy new data");
    basedir = os.getcwd();
    changefiles=[];
    addfiles=[];    
    client.update(dataDir ); 
    destdir = os.path.join(basedir,dest,"orig","data","data2");
    for root, dirs, files in os.walk(dataDir):
        for f in files:
            
            fn = os.path.join(root,f);
            destd = dest+root[len(src):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf =os.path.join(destdir,f);
            
            if os.path.exists(destf):
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                if info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    showLog("copy db:",fn,destf);
                    shutil.copy(fn,destf);
                    changefiles.append(destf);
            else:
                shutil.copy(fn,destf);
                addfiles.append(destf);
            
            #if destf in oldfiles:
            #    oldfiles.remove(destf);   
    return changefiles,addfiles,None    
def copyModelAsset(subdir,destdir,assetModelDir):
    showLog("begin copyModelAsset");
    basedir = os.getcwd();
    uiAssetDir = os.path.join(assetModelDir,subdir,"res");
    destdir = os.path.join(basedir,destdir,"orig","res","res");
    return copyFiles(uiAssetDir,destdir);
    
    '''
    oldfiles = {};
    for root, dirs, files in os.walk(destdir):
        for f in files:
            fn = os.path.join(root,f);
            oldfiles.add(fn);
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
            destd = destdir+root[len(uiAssetDir):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(uiAssetDir):];
            shutil.copy(fn,destf);
            if destf in oldfiles:
                oldfiles.remove(destf);
    for f in oldfiles:
        os.remove(f);
            #print "copy", fn," to",destf;
    '''        
def updateSub(subdir,destdir,subtype):
    infos={};
    destdir = subdir+"/release/"+subtype+"/";
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    origdir = destdir+"/orig/"+subtype;
    for root, dirs, files in os.walk(origdir):
        
        for f in files:
            fn = os.path.join(root,f);
            md5val,flen = md5file(fn);
            key = fn[len(origdir):];
            key = key.replace("\\","/");
            if key[0] == "/":
                key = key[1:];
            md5key = md5str(key);
            dir1=md5key[:1];

            infos[md5key] = (key,md5val,str(flen));
            todir = destdir+dir1;
            if not os.path.exists(todir):
                os.mkdir(todir);
            shutil.copy(fn,todir+"/"+md5key);
            #print key,md5val,flen;
    verf = open(destdir+"ver_"+subtype+".dat","wb");
    f= open(subdir+"/info_"+subtype+".csv","w");
    verstr="";
    for k in infos:
        val = infos[k];
        
        verstr +=k+","+val[1]+","+val[2];
        tp="4";
        if val[0].startswith("data/"):
            tp ="1";
        elif val[0].startswith("ui/"):
            tp ="3";
        elif val[0].startswith("lua/"):
            tp ="2";
        else:
            tp ="4";
        verstr +=","+tp;
        verstr +="|";
        f.write(",".join(val)+","+k+","+tp+"\n");
    verstr = verstr[:len(verstr)-2];
    verf.write(zlib.compress(verstr));
    verf.close;
    f.close();
    verf = open(destdir+"ver_"+subtype+".info","w");
    verf.write(md5file(destdir+"ver_"+subtype+".dat")[0]);
    verf.close();
def updateAll(subdir):
    updateSub(subdir,"res");
    updateSub(subdir,"data");
 
def checkValidFile(fn):
     
    return True;
def commitAll(d):
    print "begin commit:",d;
    changes = client.status(d);
    addfiles = [];
    delfiles = [];
    changefiles=[];
    for f in changes:
        if f.text_status == pysvn.wc_status_kind.unversioned:
            if os.path.isdir(f.path):
                addfiles.append(f.path);
                
                for root,dirs,files in os.walk(f.path):
                    for d in dirs:
                        changefiles.append(os.path.join(root,d));
                    for fn in files:
                        if(checkValidFile(fn)):
                            changefiles.append(os.path.join(root,fn));
                
            else:                
                if(checkValidFile(f.path)):
                    addfiles.append(f.path);
        elif f.text_status == pysvn.wc_status_kind.deleted or f.text_status == pysvn.wc_status_kind.missing:
            delfiles.append(f.path);
        elif f.text_status == pysvn.wc_status_kind.modified:
            changefiles.append(f.path);
        elif f.text_status == pysvn.wc_status_kind.added:
            changefiles.append(f.path);
        elif f.text_status == pysvn.wc_status_kind.conflicted:
            raise Exception("file confilicated:"+f.path); 
        elif  f.text_status == pysvn.wc_status_kind.normal:
            pass;
        else:
            print "unknown :",f.text_status,f.path;
    print "addfiles",addfiles;
    print "delfiles",delfiles;
    print "changefiles",changefiles;
    if len(addfiles)>0:
        client.add(addfiles);
    if len(delfiles)>0:
        print "begin to del files:",len(delfiles);
        client.remove(delfiles);
    print "begin to commit:",len(d),d;
    client.checkin(d,"commit"); 
def doUploadFunc(q,cfg,idx):
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])
    
    with SCPClient(ssh.get_transport()) as scp:
        while True:
            if q.empty():
                break;
            item = q.get(True,1);
            if item == None:
                break;
            #print "upload:", item[0];    
            scp.put(item[0],item[1]);
    showLog("upload end",idx);
def task_init(q):
    doUploadFunc.q = q    
def uploadChangeFiles(cfg,pf,subdir,files):
    basedir = os.getcwd();
    assetDir = os.path.join(basedir,pf,"release",subdir); 
    destDir = cfg["UPLOAD_RES_PATH"]+"/"+pf+"/release/"+subdir+"/";
    q = multiprocessing.Queue()
    for fn in files:
        destfn =destDir + fn[len(assetDir)+1:].replace("\\","/");
        q.put((fn,destfn));
    allp = []; 
    total = q.qsize();
    for i in range(5):        
        p = Process(target=doUploadFunc, args=(q,cfg,i))
        p.start();
        allp.append(p);
    lastTm = time.time();    
    while True:    
        allEnd = True;
        for p in allp:
            if p.is_alive():
                allEnd = False;
                break;
        if not allEnd:        
            time.sleep(1)
            if time.time() - lastTm>10:
                showLog("upload progress:",str((total - q.qsize()))+"/"+str(total));
                lastTm = time.time();
        else:
            break;    
    showLog("upload end");           
def uploadAll(cfg,pf,subdir,files):
    basedir = os.getcwd();
    t1 = time.time();
    assetDir = os.path.join(basedir,pf,"release",subdir); 
    destDir = cfg["UPLOAD_RES_PATH"]+"/"+pf+"/release/"+subdir+"/";
    tmpdir = os.path.join(basedir,"tmp");
    if not os.path.exists(tmpdir):
        os.mkdir(tmpdir);
    zipfn = os.path.join(tmpdir,"tmp.zip");
    zip_dir(assetDir,zipfn);
    t2 = time.time();
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])
    destfn =destDir + "tmp.zip";
    showLog("upload zip from:",zipfn,",to:",destfn);
    with SCPClient(ssh.get_transport()) as scp:
        scp.put(zipfn,destfn);
    t3 = time.time(); 
    showLog("execute cmd:","cd  "+destDir+" ; unzip -o tmp.zip ; rm -f tmp.zip");
    stdin, stdout, stderr = ssh.exec_command("cd  "+destDir+" ; unzip -oq tmp.zip ; rm -f tmp.zip;echo success", get_pty=True);
    showLog(stdout.readlines());
 
    ssh.close();
    t4 = time.time();
    showLog("upload tm:",(t2-t1),t3-t2,t4-t3);
    '''   
    #q = multiprocessing.Manager().Queue()
    q = multiprocessing.Queue()
    for root, dirs, files in os.walk(assetDir):
        for f in files:
            fn = os.path.join(root,f);
            destfn =destDir + fn[len(assetDir)+1:].replace("\\","/");
            q.put((fn,destfn));
    allp = []; 
    total = q.qsize();
    for i in range(5):        
        p = Process(target=doUploadFunc, args=(q,cfg,i))
        p.start();
        allp.append(p);
    lastTm = time.time();    
    while True:    
        allEnd = True;
        for p in allp:
            if p.is_alive():
                allEnd = False;
                break;
        if not allEnd:        
            time.sleep(1)
            if time.time() - lastTm>10:
                showLog("upload progress:",str((total - q.qsize()))+"/"+str(total));
                lastTm = time.time();
        else:
            break;
     
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(cfg["UPLOAD_HOST"],username=cfg["UPLOAD_USERNAME"],password=cfg["UPLOAD_PASSWORD"])

    with SCPClient(ssh.get_transport()) as scp:
        for root, dirs, files in os.walk(assetDir):
            for f in files:
                fn = os.path.join(root,f);
                destfn =destDir + fn[len(assetDir)+1:].replace("\\","/");
                #showLog("upload from:",fn,",to:",destfn);
                 
                scp.put(fn,destfn);
    '''            
    showLog("upload end");     
def get_login(*args, **kwargs):
    global cfg;
    return (True,cfg["name"],cfg["password"],False);   
def copyRes(pf,destpf,copyres,copyui,copylua):
    cfgf = io.open(destpf+".conf", encoding="utf-8");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
            
    #client.callback_get_login = get_login;        
    #client.update(pf); 
    resfiles=[];  
    datafiles=[];
    if copyres:
        changefiles,addfiles,_=copyModelAsset(pf,destpf,cfg["modeldir"]);
        resfiles = changefiles+addfiles;
    if copyui:
        changefiles,addfiles,_=copyuiAsset(pf,destpf,cfg["uidir"]);
        datafiles = changefiles+addfiles;
    if copylua:
        changefiles,addfiles,_=copyLuaAsset(pf,destpf,cfg["luadir"],cfg["luaConvertDir"]);
        datafiles = datafiles + changefiles+addfiles;
    changefiles,addfiles,_=copyData(pf,destpf,cfg["datadir"]);
    datafiles = datafiles + changefiles+addfiles;
    basedir = os.getcwd();
    if len(resfiles)>0:
        changefiles,addfiles,delfiles = encyptFiles(os.path.join(basedir,pf,"orig","res" ),os.path.join(basedir,pf,"release","res" ),"res",resfiles);
        if len(changefiles)+len(addfiles)>100:
            uploadAll(cfg,pf,"res",changefiles+addfiles);
        else:    
            uploadChangeFiles(cfg,pf,"res",changefiles+addfiles);
    if len(datafiles)>0:
        changefiles,addfiles,delfiles = encyptFiles(os.path.join(basedir,pf,"orig","data" ),os.path.join(basedir,pf,"release","data" ),"data",datafiles);
        if len(changefiles)+len(addfiles)>100:
            uploadAll(cfg,pf,"data",changefiles+addfiles);
        else:    
            uploadChangeFiles(cfg,pf,"data",changefiles+addfiles);
    showLog("copy res end");
    #updateAll(pf);
    #copySoundAsset(pf,destpf,cfg["sounddir"]);
    
    #commitAll(pf);
def showLog(*Arg ):
    print u" ".join([ type(k)==unicode and k or str(k) for k in Arg]),datetime.now()  ;
   
    sys.stdout.flush();     
if __name__ == "__main__":
    pf=sys.argv[1];
    destpf = pf;
    if len(sys.argv)>2:
        destpf = sys.argv[2];
    print pf,destpf;
    copyRes(pf,destpf);

