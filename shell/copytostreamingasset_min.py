# -*- coding: utf8 -*-'
import os,md5,shutil;
import zlib,sys,pysvn,struct;
import platform;
from datetime import datetime;

def md5str(val):
    m = md5.new();
    m.update(val);  
    return m.hexdigest();


def copyDir(srcdir,destdir):
    
    for root, dirs, files in os.walk(srcdir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
            destd = destdir+root[len(srcdir):];
            if not os.path.exists(destd):
                os.makedirs(destd);
            destf = destdir+fn[len(srcdir):];
            shutil.copy(fn,destf);
            #print "copy ",fn," to ",destf;

def copyModelAsset(subdir,destsubdir,assetModelDir,resdict,copyAll):
     
    print "read dict :",len(resdict);
    uiAssetDir = subdir+"/orig/res/res/";
    releaseDir=subdir+"/release/res/";
    destdir = assetModelDir;
    '''
    restypefile = "../"+subdir+"/restype.txt";
    tpf = open(restypefile);
    filetps={};
    print "open file: " + restypefile;
    for l in tpf.readlines():
        s = l.strip().split("\t");
        if len(s) != 2:
            continue;
        filetps[s[0].strip().replace("\\","/")]=s[1].strip();
        print s[0].strip().replace("\\","/");
    '''
    includeFiles ={};
    copyCnt = 0;
    copySz = 0;
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
           
            k = "res/"+fn[len(uiAssetDir):];
            k = k.replace("\\","/");
            
            if not copyAll and (  not k in resdict or resdict[k] != 1):
                continue;
            '''
            if not( k in filetps):
                print "file not in restype:",k;
                continue;
            if filetps[k] != "1":
                print "ignore file:",k;
                continue;

            if k.find("dg_fuben_05_2.unity3d")>=0:
                print k;
            '''
            md5k = md5str(k);
            fn=releaseDir+md5k[0]+"/"+md5k;
            includeFiles[md5k] = 1;
            destd =  destdir+md5k[0];
            if not os.path.exists(destd):
                os.makedirs(destd);
            
            
            destf = destd+"/"+md5k;
            #if md5k == "bed41778776424b7ebcbc1923ee2e573":
            #    print "file ", k, "copy ",fn,"to",destf,"md5k:",md5k;
            shutil.copy(fn,destf);
            info = os.stat(fn);
            copyCnt = copyCnt +1;
            copySz = copySz + info.st_size;
    showLog("copy res file cnt:",copyCnt,",res file size:",copySz);  
    #print "include bed41778776424b7ebcbc1923ee2e573:",includeFiles["bed41778776424b7ebcbc1923ee2e573"];
    copyDat(releaseDir+"ver_res.dat",destdir+"ver_res.dat",includeFiles);
    copyNewDat(releaseDir+"ver_res2.dat",destdir+"ver_res2.dat",includeFiles);
    
    #shutil.copy(releaseDir+"ver_res.dat",destdir+"ver_res.dat");
    shutil.copy(releaseDir+"ver_res.info",destdir+"ver_res.info");
    shutil.copy(releaseDir+"ver_res2.info",destdir+"ver_res2.info");
def copyNewDat(src,dest,includeFiles):
    #print "includeFiles:",len(includeFiles);
    with open(src,"rb") as f:
        data = f.read(); 
        ndata="";
        pos = 0;        
        while pos<len(data):        
            fbytes = data[pos:pos+16];
            fn = "".join("{:02x}".format(ord(c)) for c in fbytes);
            if fn in includeFiles:
                #print "found in includeFiles:",fn,struct.unpack("<B",data[pos+36]),struct.unpack("<B",data[pos+36])[0]+128;
                ntp = struct.pack("<B",struct.unpack("<B",data[pos+36])[0]+128);
                ndata += data[pos:pos+36]+ ntp ;
            else:
                ndata += data[pos:pos+37] ;
            pos +=37;    
         
        with open(dest,"wb") as f2:
            f2.write(ndata);
def copyDat(src,dest,includeFiles):
    with open(src,"rb") as f:
        data = f.read();
        data = zlib.decompress(data);
        data = data.split("|");
        for i in range(len(data)):
            d = data[i].split(",");
            if d[0] in includeFiles:
                d.append("1");
                data[i] = ",".join(d);
        realdata = "|".join(data);
        testfn = src[:src.rfind("/")];
        testfn = testfn+"/.."+src[src.rfind("/"):src.rfind(".")]+"_stream.txt";
        showLog("write testver:",testfn);
        with open(testfn,"w") as f2:
            f2.write(realdata);
        with open(dest,"wb") as f2:
            f2.write(zlib.compress(realdata));
            
def copyuiAsset(subdir,destsubdir,assetDir,resdict,copyAll):
    uiAssetDir = subdir+"/orig/data/";
    releaseDir=subdir+"/release/data/";
    destdir = assetDir;
    includeFiles = {};
    copyCnt = 0;
    copySz = 0;
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
           
            k = fn[len(uiAssetDir):];
            k = k.replace("\\","/");
            #if k.startswith("data/") and k != "data/data.db":
                #print "ignore db file:",k;
            #    continue;
            if k.startswith("data/"):
                continue;
                #if k != "data/data.db" and k != "data/essword.dat":
                #    continue;
            elif k.startswith("data2/"):
                if k != "data2/data.cdb" and k != "data2/essword.dat":
                    continue; 
            elif k.startswith("ui/"):
                #if k == "ui/atlas/chongwu.unity3d":
                #    print "try copy:",k,k in resdict,resdict[k]
                if not copyAll and ( not k in resdict or resdict[k] != 1):
                    continue;
            md5k = md5str(k);
            fn=releaseDir+md5k[0]+"/"+md5k;

            destd =  destdir+md5k[0];
            if not os.path.exists(destd):
                os.makedirs(destd);
            
            destf = destd+"/"+md5k;
            #if k == "ui/atlas/chongwu.unity3d":
            #    print "file:",os.path.join(root,f)," copy ",fn,"to",destf;
            fdest = open(destf,"wb");
            fsrc = open(fn,"rb");
            includeFiles[md5k] = 1;
            if k.startswith("lua/"):
                #fdest.write(zlib.decompress(fsrc.read()));
                if   subdir == "android" and not k.endswith("hCmdClient.pb"):
                    fdest.write(zlib.compress(fsrc.read()));
                else:
                    fdest.write(fsrc.read());
            elif k=="data2/data.cdb":
                fdest.write(zlib.compress(fsrc.read()));
            else:
                fdest.write(fsrc.read());
            #fdest.write(fsrc.read());    
            fdest.close;
            fsrc.close();
            info = os.stat(destf);
            copyCnt = copyCnt +1;
            copySz = copySz + info.st_size;
            #print "copy ui file:",k;
    showLog("copy ui file cnt:",copyCnt,",ui file size:",copySz);        
    copyDat(releaseDir+"ver_data.dat",destdir+"ver_data.dat",includeFiles); 
    copyNewDat(releaseDir+"ver_data2.dat",destdir+"ver_data2.dat",includeFiles); 
    
    #shutil.copy(releaseDir+"ver_data.dat",destdir+"ver_data.dat");
    shutil.copy(releaseDir+"ver_data.info",destdir+"ver_data.info");
    shutil.copy(releaseDir+"ver_data2.info",destdir+"ver_data2.info");

def copydbAsset(subdir,destsubdir,assetDir):
    uiAssetDir = "../"+subdir+"/orig/db/";
    releaseDir="../"+subdir+"/release/db/";
    destdir = assetDir;
    insideFiles={};
    for root, dirs, files in os.walk(uiAssetDir):
        for f in files:
            if f.find(".manifest")>0 or f.find(".meta")>0:
                continue;
            fn = os.path.join(root,f);
           
            k = fn[len(uiAssetDir):];
            k = k.replace("\\","/");
            #if k.startswith("data/") and k != "data/data.db":
                #print "ignore db file:",k;
            #    continue;
            #if k.startswith("data/"):
            #    if k != "data/data.db" and k != "data/essword.dat":
            #        continue;
                    
            md5k = md5str(k);
            fn=releaseDir+md5k[0]+"/"+md5k;

            destd =  destdir+md5k[0];
            if not os.path.exists(destd):
                os.makedirs(destd);
            
            destf = destd+"/"+md5k;
            #print "file:",os.path.join(root,f)," copy ",fn,"to",destf;
            fdest = open(destf,"wb");
            fsrc = open(fn,"rb");
            if k.startswith("lua/"):
                fdest.write(zlib.decompress(fsrc.read()));
            else:
                fdest.write(fsrc.read());
                
            fdest.close;
            fsrc.close();
    
        
    shutil.copy(releaseDir+"ver_db.dat",destdir+"ver_db.dat");
    shutil.copy(releaseDir+"ver_db.info",destdir+"ver_db.info");	
def get_login(*args, **kwargs):
    return (True,"cj","cj123",False);       
def showLog(*Arg ):
    print u" ".join([ type(k)==unicode and k or str(k) for k in Arg]),datetime.now()  ;
   
    sys.stdout.flush();     
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
            includeFiles[d] = 1;
            addDep(includeFiles,resDep,d);
def addSceneDep(resBase,includeFiles):
    keys = includeFiles.keys();
    for k in keys:
        if k.startswith("res/scene/") and k.endswith("_d.unity3d"):
             
            sceneFn = k[k.rfind("/")+1:];
            sceneFn = sceneFn[0:sceneFn.rfind("_")];
            basedir = resBase+"/res/exmodel/art/scene/"+sceneFn+"/prefabs/";
             
            for f in os.listdir(basedir):
                 
                if f.endswith(".unity3d"):
                    fn = "res/exmodel/art/scene/"+sceneFn+"/prefabs/"+f;
                     
                    includeFiles[fn] = 1;
             
              
def updateFilter(filterfn,manifestfn,releasefilterfn,prefix):
    showLog("begin to update filter:", releasefilterfn);
    includeFiles = {};
    with open(filterfn) as f:
        for l in f.readlines():
            vals = l.strip().split(",");
            if len(vals)>1 and vals[1].strip() == "1":
                includeFiles[vals[0].strip().lower()] = 1;
    resDep = readManifest(manifestfn,prefix);
    keys = includeFiles.keys();
    print "includeFiles before len:",len(includeFiles)
    for k in keys:
        addDep(includeFiles,resDep,k);
      
    print "includeFiles after len:",len(includeFiles)
    releaseDict={};
    cnt = 0;
    if os.path.exists(releasefilterfn):
        with open(releasefilterfn) as f:
            
            for l in f.readlines():
                val = l.strip().split(",");
                if len(val) <2:
                    print "continue:",l
                    continue;
                k = val[0].strip();
                if k in includeFiles:
                    val[1] = "1";
                    del includeFiles[k];
                elif val[1] == "1":
                    val[1] = "2";
                releaseDict[k] = val;
                cnt = cnt+1;
    else:
        d = releasefilterfn[:releasefilterfn.rfind("/")];
        if not os.path.exists(d):
            os.makedirs(d);
    print "releaseDict   len:",len(releaseDict),cnt;        
    for k in includeFiles:
        releaseDict[k] = [k,"1","",""];
    keys = releaseDict.keys();
    keys.sort();
    with open(releasefilterfn,"w") as f:
        for k in keys:
            f.write(",".join(releaseDict[k])+"\n");
    showLog("end to update filter");        
    print filterfn;
def removeStreamingFiles(assetDir):
    if os.path.exists(assetDir):
        shutil.rmtree(assetDir);
    os.mkdir(assetDir);
def copyStreamingAsset(pf,resid):
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    if resid != "all":        
        updateFilter(cfg["APP_PATH"]+"/main/Assets/ResFilters/"+resid+".csv",cfg["APP_PATH"]+"/main/AssetBundles/"+pf+"/ui/ui.manifest",pf+"/ResFilters/"+resid+"/data.csv","ui");
        updateFilter(cfg["RES_PATH"]+"/Assets/ResFilters/"+resid+".csv",cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/res.manifest",pf+"/ResFilters/"+resid+"/res.csv","res");
        
    uiDict={};
    uiSz = 0;
    uiCnt = 0;
    if resid != "all": 
        with open(pf+"/ResFilters/"+resid+"/data.csv") as f:
            for l in f.readlines():
                vals = l.strip().split(",");
                if len(vals)>1:
                    if int(vals[1]) == 1:
                        uiDict[vals[0]] = int(vals[1]);
                    #if uiDict[vals[0]] == 1:
                        #uiSz = uiSz + int(vals[2]);
                        #uiCnt = uiCnt +1;
    resDict={};
    resSz = 0;
    resCnt = 0;
    if resid != "all": 
        with open(pf+"/ResFilters/"+resid+"/res.csv") as f:
            for l in f.readlines():
                vals = l.strip().split(",");
                if len(vals)>1:
                    if int(vals[1]) == 1:
                        resDict[vals[0]] = int(vals[1]);  
                    #if resDict[vals[0]] == 1:
                        #resSz = resSz + int(vals[2]);
                        #resCnt = resCnt +1; 
    addSceneDep(cfg["RES_PATH"]+"/AssetBundles/"+pf,resDict); 
    resDep = readManifest(cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/res.manifest","res");    
    keys = resDict.keys();
    for k in keys:
        addDep(resDict,resDep,k);    
    print "rescnt:",resCnt,",resSz:",resSz,",uicnt:",uiCnt,",uiSz:",uiSz;      
    removeStreamingFiles(cfg["STREAM_RESOURCE_TARGET_DIR"]);
    copyModelAsset(pf,pf,cfg["STREAM_RESOURCE_TARGET_DIR"],resDict,resid=="all");
    copyuiAsset(pf,pf,cfg["STREAM_RESOURCE_TARGET_DIR"],uiDict,resid=="all");
    #copydbAsset(pf,pf,cfg["STREAM_RESOURCE_TARGET_DIR"]);
def checkResSize(pf,fn):
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    resDep = readManifest(cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/res.manifest","res");  
    prepath =  cfg["RES_PATH"]+"/AssetBundles/"+pf+"/";
    files = {};
    files[fn] = 1;
    addDep(files,resDep,fn);
    print "files len:",len(files);
    
    sz =0;
    names = files.keys();
    names.sort();
    with open("test.csv","w") as fp:
        for f in names:
            rf = prepath+f;
            info = os.stat(rf);
            sz = sz+info.st_size;
            fp.write(f+","+str(info.st_size)+"\n");
            
    print "size:",sz;    
if __name__ == "__main__":
    pf="android";
    resid = "1";
    if len(sys.argv)>1:
        pf=sys.argv[1];
    if len(sys.argv)>2:
        resid=sys.argv[2];
    #checkResSize(pf,sys.argv[3]);    
    '''    
    client = pysvn.Client();
    client.callback_get_login = get_login;
    client.update(pf); 
    '''
    copyStreamingAsset(pf,resid);

