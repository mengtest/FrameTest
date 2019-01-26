# -*- coding: utf8 -*-'
import os,md5,shutil,struct;
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
    
    return m.hexdigest(),len(data),m.digest();
def md5str(val):
    m = md5.new();
    m.update(val);  
    return m.hexdigest(),m.digest();
 

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
        convertCmd = "luajit32.exe -b {0} {1}";
    elif platform.system() == "Darwin":
        convertCmd = "./luajit -b {0} {1}";
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
            
            if destf in oldfiles:
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                if info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    shutil.copy(fn,destf);
                    changefiles.append(destf);
                    if f.find("shader")>=0:
                        showLog("orig:",fn,f,"dest:",destf,dest,root[len(src):]); 
            else:
                shutil.copy(fn,destf);
                addfiles.append(destf);
                #if fn.find("shader")>=0:
                #    showLog("copy orig:",fn,f,"dest:",destf,dest,root[len(src):]); 
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
def sortResFile(a,b):
    if a[0] >b[0]:
        return 1;
    if a[0]<b[0]:
        return -1;
    return 0;
def updateFilterFile(resdict,pf,subtype,inclueFiles):
    print "updateFilterFile",subtype;
 
    basedir = pf+"/ResFilters/";
    if not os.path.exists(basedir):
        os.makedirs(basedir);
    for d in os.listdir(basedir):
        destfn = basedir+d+"/"+subtype+".csv";
        
        if os.path.exists(destfn) :            
            tdict = {};
            for k in resdict:
                tdict[k] = resdict[k];
            print "tdict len:",len(tdict);    
            lns=[];    
            with open(destfn) as f:
                for l in f.readlines():
                    vals = l.strip().split(",");
                    if len(vals)>1:
                        '''
                        if subtype == "data":
                             
                            if vals[1] == "1":
                                inclueFiles[vals[0]] = 1;
                               # print "add inlcude file:",vals[0];
                        '''
                        if vals[0] in tdict:
                            vals[2] = str(tdict[vals[0]][1]);
                            if len(vals)<4:
                                vals.append(str(tdict[vals[0]][2]));
                            else:    
                                vals[3] = str(tdict[vals[0]][2]);
                            lns.append(vals);
                            del tdict[vals[0]];
            print "not in ",destfn,",len:",len(tdict);
            for k in tdict:
                #print "k:",k,tdict[k];
                lns.append([tdict[k][0],"2",str(tdict[k][1]),tdict[k][2]]);
            lns.sort(sortResFile);
            print "lns:",lns[0];
            with open(destfn,"w") as f:
                for l in lns:
                    f.write(",".join(l)+"\n");
def readManifest(fn,prefix):
    resDep={};
    with open(fn) as f:
        curKey=None;
        curDep=None;
        prevPrefix=None;
        for l in f.readlines():
            if l.startswith("    Info_"):
                if curKey != None:
                    resDep[curKey] = curDep;
                curKey = None;
            elif l.startswith("      Name:"):
                curKey = prefix+"/"+l.strip().split(":")[1].strip();
                prevPrefix = "Name";
            elif l.startswith("      Dependencies:"):
                curDep=[];
            elif l.startswith("        Dependency_"):
                prevPrefix = "Dependency";
                curDep.append(prefix+"/"+l.strip().split(":")[1].strip());
            elif prevPrefix == "Name" and l.startswith("       "):
                
                curKey =curKey+ l.rstrip()[len("       "):];
                 
            elif prevPrefix == "Dependency" and l.startswith("         "):
                 
                curDep[len(curDep)-1] = curDep[len(curDep)-1] +l.rstrip()[len("         "):];
    return resDep;   
def addDep(includeFiles,resDep,k):
    
    if k in resDep:
        
        deps = resDep[k];
        for d in deps:
            if d in includeFiles:
                continue;
            includeFiles[d] = 1;
            addDep(includeFiles,resDep,d);       
def copyCompressDb(src,dest):
    #print "copyCompressDb:",src,dest;
    with open(src,"rb") as f:
        data = f.read();
        with open(dest+"_z","wb") as f2:
            cdata = zlib.compress(data);
            f2.write(cdata);
            return len(cdata);
def takeFirst(elem):
    return elem[0];            
def encyptFiles(src,dest,subtype,a,pf,projdir,force):
    showLog("begin encrypt files from:",src,"to",dest,subtype);
    oldfiles = set();
    changefiles=[];
    delfiles=[];
    addfiles = [];
    infos={};
    includeFiles={};
    resdict={};
    includeFn = projdir+u"/Assets/ResFilters/必须包含的资源.csv";
    if os.path.exists(includeFn):
        print "exists:",includeFn;
        with open(includeFn) as f:
            for l in f.readlines():
                vals = l.strip().split(",");
                #print "vals:",vals;
                if vals[1] == "1":
                    includeFiles[vals[0]] = 1;
    else:
        with open("include.txt") as f:
            for l in f.readlines():
                includeFiles[l.strip()] = 1;
    mtype = subtype;
    if mtype == "data":
        mtype = "ui";
    depsInfo = readManifest(projdir+"/AssetBundles/"+pf+"/"+mtype+"/"+mtype+".manifest",mtype);
    showLog("before adddep include len:",len(includeFiles));
    keys = includeFiles.keys();
    for k in keys:
        addDep(includeFiles,depsInfo,includeFiles[k]);
    showLog("after adddep include len:",len(includeFiles));    
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
            md5name,md5namebin = md5str(abname);
            md5val,flen,md5valbin = md5file(fn);
            if md5name.find("ef0498de7a224b3ef8eaf416dfc55af7")>=0:
                print fn,md5val
                #exit(1)
            #print "fn:",fn;
            if not abname.startswith("data/") and  not abname.startswith("data2/") and  not abname.startswith("lua/"):
                resdict[abname] =(abname, flen,md5name ); 
            infos[md5name] = [abname,md5val,str(flen),md5namebin,md5valbin]; 
            destd = os.path.join(dest,md5name[0]);
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = os.path.join(destd,md5name);
            #showLog("orig:",fn,"dest:",destf,dest,root[len(src):]); 
            if destf in oldfiles:
                
                info1 = os.stat(fn);
                info2 = os.stat(destf);
                if fn.find("MapConfig.db")>0:
                    showLog("db file:",fn,destf,info1.st_mtime,info2.st_mtime,info1.st_size,info2.st_size);
                if  force or  info1.st_mtime>info2.st_mtime or info1.st_size != info2.st_size:
                    shutil.copy(fn,destf);
                    
                    if abname.startswith("data2/") or abname.startswith("lua/"):
                        nsize = copyCompressDb(fn,destf);
                        infos[md5name][2] = str(nsize);
                        changefiles.append(destf+"_z");
                        #print "copy:",destf;
                    changefiles.append(destf);
            else:
                shutil.copy(fn,destf);
                
                if abname.startswith("data2/") or abname.startswith("lua/"):
                    nsize = copyCompressDb(fn,destf);
                    infos[md5name][2] = str(nsize);
                    changefiles.append(destf+"_z");
                addfiles.append(destf);
            
            if destf in oldfiles:
                oldfiles.remove(destf);  
            if (destf+"_z") in oldfiles:
                oldfiles.remove(destf+"_z");      
    updateFilterFile(resdict,pf,subtype,includeFiles);
    f= open(os.path.join(dest,"../info_"+subtype+".csv"),"w");
    verstr="";
    verbinstr="";
    keys = infos.keys();
    keys.sort();
    for k in keys:
        val = infos[k];
        verbinstr += val[3] +val[4]+ struct.pack("<I",int(val[2])) 
        verstr +=k+","+val[1]+","+val[2];
        tp="5";
        if val[0].startswith("data/"):
            if val[0].endswith("data.db"):
                tp ="1";
            else:
                tp = "6";
        elif val[0].startswith("data2/"):
            if val[0].endswith("data.cdb"):
                tp ="8";
            else:
                tp = "9"; 
            #print "data:",val[0];   
         
        elif val[0].startswith("ui/"):
            if val[0] in includeFiles:
                tp = "3";
            else:    
                tp ="7";
        elif val[0].startswith("lua/"):
            tp ="2";
        elif val[0]=="res/res":
            tp = "4";
        else:
            if val[0] in includeFiles:
                #print "include res file:",val[0];
                tp ="4";
            else:
                tp = "5";
        verbinstr +=  struct.pack("<B",int(tp))        
        verstr +=","+tp;
        verstr +="|";
        f.write(",".join(val[0:3])+","+k+","+tp+"\n");
    verstr = verstr[:len(verstr)-1];
    with open(os.path.join(dest,"ver_"+subtype+".dat"),"wb") as verf:
        verf.write(zlib.compress(verstr));
    with open(os.path.join(dest,"ver_"+subtype+"2.dat"),"wb") as verf:
        verf.write(verbinstr);
          
    with open(os.path.join(dest,"../ver_"+subtype+".txt"),"w") as verf:
        verf.write(verstr); 
    f.close();
    datestr = datetime.now().strftime('%Y%m%d%H%M%S');
    with open(os.path.join(dest,"ver_"+subtype+".info"),"w") as verf2:
        verf2.write(md5file(os.path.join(dest,"ver_"+subtype+".dat"))[0]+","+datestr);
    with open(os.path.join(dest,"ver_"+subtype+"2.info"),"w") as verf2:
        verf2.write(md5file(os.path.join(dest,"ver_"+subtype+"2.dat"))[0]+","+datestr);
              
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
'''
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
'''
def copyNewData(src,dest,dataDir):
    showLog("begin copy new data",dataDir);
    basedir = os.getcwd();
    changefiles=[];
    addfiles=[];    
    client.update(dataDir ); 
    destdir = os.path.join(basedir,dest,"orig","data","data2");
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    for root, dirs, files in os.walk(dataDir):
        for f in files:
            if not f.endswith(".cdb") and f != "essword.dat":
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
def readSceneStub(fn):
    startStub = False;
    items = [];
    with open(fn) as f:
        for l in f.readlines():
            if l.startswith("  stubs:"):
                startStub = True;
            if startStub:
                if l.startswith("  - prefab:"):
                    item = l[len("  - prefab:"):].strip();
                    items.append(item);
                elif not l.startswith("  "):
                    startStub = False;
    print "items len:",len(items);
    return items;
def createDynamicSceneDep(subdir,destdir,assetModelDir):
    scenedir = assetModelDir+"../Assets/ART/Scene/";
    info = "";
    for f in os.listdir(scenedir):
         
        if f.endswith("_d.unity"):
            items = readSceneStub(os.path.join(scenedir,f));    
            info = items;
    pass;
    
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
        if val[0].startswith("data2/"):
            tp ="9";    
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
''' 
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
    if "UPLOAD" in cfg and  cfg["UPLOAD"] == "false":
        return;
     
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
def copyRes(pf,destpf,copyres,copyui,copylua,force):
    showLog("begin copyres,copyui:",copyui,",copyres:",copyres,",copylua:",copylua,",force:",force);
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
    changefiles=[];
    addfiles=[];
    datafiles=[];
     
    if copyres or force:
        changefiles,_,addfiles=copyModelAsset(pf,destpf,cfg["modeldir"]);
        resfiles = changefiles+addfiles;
        print "resfiles cnt:",len(resfiles),len(addfiles),len(changefiles);
    if copyui or force:
        changefiles,_,addfiles =copyuiAsset(pf,destpf,cfg["uidir"]);
        datafiles = changefiles+addfiles;
    if copylua or force:
        changefiles,_,addfiles =copyLuaAsset(pf,destpf,cfg["luadir"],cfg["luaConvertDir"]);
        datafiles = datafiles + changefiles+addfiles;
    #changefiles,addfiles,_=copyData(pf,destpf,cfg["datadir"]);
    datafiles = datafiles + changefiles+addfiles;
    changefiles,addfiles,_=copyNewData(pf,destpf,cfg["datadir"]);
    datafiles = datafiles + changefiles+addfiles;
    
    basedir = os.getcwd();
    showLog("resfiles cnt;",len(resfiles),"datafile cnt:",len(datafiles),force);
    if len(resfiles)>0 or force:
        changefiles,addfiles,delfiles = encyptFiles(os.path.join(basedir,pf,"orig","res" ),os.path.join(basedir,pf,"release","res" ),"res",resfiles,pf,cfg["RES_PATH"],force);
        #if len(changefiles)+len(addfiles)>100  or force:
        uploadAll(cfg,pf,"res",changefiles+addfiles);
        #else:    
        #    uploadChangeFiles(cfg,pf,"res",changefiles+addfiles);
     
    if len(datafiles)>0 or force:
        changefiles,addfiles,delfiles = encyptFiles(os.path.join(basedir,pf,"orig","data" ),os.path.join(basedir,pf,"release","data" ),"data",datafiles,pf,cfg["APP_PATH"]+"/main/",force);
        #if len(changefiles)+len(addfiles)>100  or force:
        uploadAll(cfg,pf,"data",changefiles+addfiles);
        #else:    
        #    uploadChangeFiles(cfg,pf,"data",changefiles+addfiles);
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
    copyRes(pf,destpf,False,True,True,False);

