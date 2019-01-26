# -*- coding: utf8 -*-'
import os,md5,shutil;
import zlib,sys,pysvn;
import platform;
client = pysvn.Client();
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
 

def copyuiAsset(subdir,assetDir,todir):
    uiAssetDir = assetDir+subdir+"/ui/";
    destdir = todir+"/orig/data/ui/";
    print "Source: ", uiAssetDir; 
    print "Dest: ", destdir;
    
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
def copySoundAsset(subdir,assetDir,todir):
    uiAssetDir = assetDir;
    destdir = todir+"/orig/data/sound/";
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
def copyLuaAsset(subdir,assetDir,luaConvertDir,todir):
    print "systyem:",platform.system();
    strsys = platform.system();
    if strsys == "Windows":
        convertCmd = "luajit.exe -b {0} {1}";
    elif platform.system() == "Darwin":
        convertCmd = "./luac -o {1} {0}";
    retval = os.getcwd();
    print "retval:",retval;
    uiAssetDir = assetDir;
    destdir = todir+"/orig/data/lua/";
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    oldfiles = set();
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
            if False: #f.endswith(".lua"):
            
                cmd = convertCmd.format(fn,retval+"/"+destf);
                
                os.chdir(luaConvertDir);
                os.system(cmd);
                os.chdir(retval);
            else:
                shutil.copy(fn,destf);
            #shutil.copy(fn,destf);
            if destf in oldfiles:
                oldfiles.remove(destf);
            print "copy", fn," to",destf;
    for f in oldfiles:
        os.remove(f);
def copyModelAsset(subdir,assetModelDir,todir):
    
    uiAssetDir = assetModelDir+subdir+"/res/";
    destdir = todir+"/orig/res/res/";
    print "begin copyModelAsset",uiAssetDir,"to",destdir;
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
 #           print destd;
 #           if destd:
   #             continue;
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(uiAssetDir):];
            shutil.copy(fn,destf);
            print "copy", fn," to",destf;
            if destf in oldfiles:
                oldfiles.remove(destf);
    for f in oldfiles:
        os.remove(f);
            
def updateSub(subdir,subtype):
    infos={};
    destdir = subdir+"/release/"+subtype+"/";
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    origdir = subdir+"/orig/"+subtype;
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
def get_login(*args, **kwargs):
    global cfg;
    return (True,cfg["name"],cfg["password"],False);   

    
pf=sys.argv[1];
print pf;
cfgf = open(pf+".conf");
cfg = {};
for l in cfgf.readlines():
    strs = l.strip().split("=");
    if len(strs) == 2:
        cfg[strs[0]] = strs[1];
client.callback_get_login = get_login;
print cfg["modeldir"];
print cfg["uidir"];
pf=cfg["pf"];
topf=cfg["topf"];
print pf;
copyModelAsset(pf,cfg["modeldir"],topf);
copyuiAsset(pf,cfg["uidir"],topf);
copyLuaAsset(pf,cfg["luadir"],cfg["luaConvertDir"],topf);
copySoundAsset(pf,cfg["sounddir"],topf);
commitAll(topf);
