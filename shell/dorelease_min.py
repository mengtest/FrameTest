# -*- coding: utf8 -*-'
import os,md5,shutil;
import zlib,sys,pysvn;
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
 

def copyuiAsset(subdir,assetDir):
    uiAssetDir = assetDir+subdir+"/ui/";
    destdir = subdir+"/orig/data/ui/";
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
            #print "copy", fn," to",destf;
 

def copyModelAsset(subdir,assetModelDir):
    uiAssetDir = assetModelDir+subdir+"/res/";
    destdir = subdir+"/orig/res/res/";
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
            #print "copy", fn," to",destf;
def copyData(subdir,datadir):
    client.update(datadir);
    if not os.path.exists(subdir+"/orig/data/data/"):
        os.makedirs(subdir+"/orig/data/data/");
    shutil.copy(datadir+"/data.db",subdir+"/orig/data/data/data.db");
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
    print "addfiles",addfiles;
    print "delfiles",delfiles;
    print "changefiles",changefiles;
    if len(addfiles)>0:
        client.add(addfiles);
    if len(delfiles)>0:
        client.remove(delfiles);
    print "begin to commit:",len(d),d;
    client.checkin(d,"commit");  
def get_login(*args, **kwargs):
    return (True,"cj","cj123",False);
    
def updateSub(subdir,subtype,minres):
    resDict=None;
    resfn=subdir+"/"+subtype+".txt";
    if minres and os.path.exists(resfn):
        resDict={};
        resf = open(resfn,"r");
        for l in resf.readlines():
            if l != None and  l.strip() != "":
                print "l:",subtype+"/"+l.strip();
                resDict[subtype+"/"+l.strip()] = 1;
        resf.close();
         
    infos={};
    destdir = subdir+"/release/"+subtype+"/";
    if not os.path.exists(destdir):
        os.makedirs(destdir);
    oldfiles = set();
    for root, dirs, files in os.walk(destdir):
        for f in files:
            
            fn = os.path.join(root,f);
            if   fn.find(".svn")<0:
                oldfiles.add(fn);
    origdir = subdir+"/orig/"+subtype;
    for root, dirs, files in os.walk(origdir):
        
        for f in files:
            
            fn = os.path.join(root,f);
            if fn.find(".svn")>=0:
                continue;
            if fn.find("data.db")>0:
                print "fn:",fn;   
            
            key = fn[len(origdir):];
            key = key.replace("\\","/");
            if key[0] == "/":
                key = key[1:];
            md5key = md5str(key);
            dir1=md5key[:1];

            
            todir = destdir+dir1;
            if not os.path.exists(todir):
                os.mkdir(todir);
            destfn = todir+"/"+md5key;
            if key.startswith("data/") or key.startswith("lua/"):
                destf = open(destfn,"wb");
                srcf = open(fn,"rb");
                destf.write(zlib.compress(srcf.read()));
                destf.close();
                srcf.close();
            else:
                shutil.copy(fn,destfn);
            if destfn in oldfiles:
                oldfiles.remove(destfn);

            md5val,flen = md5file(destfn);
            infos[md5key] = (key,md5val,str(flen));
            #print key,md5val,flen;
    for f in oldfiles:
        os.remove(f);
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
            
            if minres and resDict != None and not val[0] in resDict:
                tp = "5";
            else:
                tp ="4";
        verstr +=","+tp;
        verstr +="|";
        f.write(",".join(val)+","+k+","+tp+"\n");
    verstr = verstr[:len(verstr)-1];
    verf.write(zlib.compress(verstr));
    verf.close;
    f.close();
    f= open(subdir+"/info_"+subtype+".txt","w");
    f.write(verstr);
    f.close();
    verf = open(destdir+"ver_"+subtype+".info","w");
    verf.write(md5file(destdir+"ver_"+subtype+".dat")[0]);
    verf.close();
def updateAll(subdir,minres):
    updateSub(subdir,"res",minres);
    updateSub(subdir,"data",minres);
 
    
pf=sys.argv[1];
minres = False;
if len(sys.argv)>1:
    minres = sys.argv[2]=="true"
print pf,minres;
cfgf = open(pf+".conf");
cfg = {};
for l in cfgf.readlines():
    strs = l.strip().split("=");
    if len(strs) == 2:
        cfg[strs[0]] = strs[1];
client.callback_get_login = get_login;
#copyData(pf,cfg["datadir"]);
updateAll(pf,minres);
#commitAll(pf);
