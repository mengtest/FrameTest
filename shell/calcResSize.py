# -*- coding: utf8 -*-'
import sys,os;
def readManifest(fn,prefix):
    print "read manifest:",fn
    resDep={};
    reverseDep={};
    with open(fn) as f:
        curKey=None;
        curDep=None;
        prevPrefix=None;
        for l in f.readlines():
            if l.startswith("    Info_"):
                if curKey != None:
                    resDep[curKey] = curDep;
                    for d in curDep:
                        if not d in reverseDep:
                            reverseDep[d] = [];
                        reverseDep[d].append(curKey);    
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
             
        if curKey != None:
            resDep[curKey] = curDep;
            for d in curDep:
                if not d in reverseDep:
                    reverseDep[d] = [];
                reverseDep[d].append(curKey);                
              
    return resDep,reverseDep;   
def addDep(includeFiles,resDep,k):
    
    if k in resDep:
        
        deps = resDep[k];
        for d in deps:
            if d == "res/shader/sceneshaders.unity3d":
                continue;
            includeFiles[d] = 1;
            addDep(includeFiles,resDep,d);  
    else:
        print "file not found:",k;
resDep=None;
reverseDep = None;  
def checkResSize(pf,fn,allDict):
    
    
         
    global resDep, reverseDep
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    if resDep == None:        
        resDep,reverseDep = readManifest(cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/res.manifest","res");  
    prepath =  cfg["RES_PATH"]+"/AssetBundles/"+pf+"/";
    if os.path.exists(prepath+ fn[:-8]+"_d.unity3d"):
        print "exists:",fn;
        return 0;
    #if fn.find("scene_xsc_merge")>0:
    #    print "exists:", prepath+fn[:-8]+"_d.unity3d"
    #    exit(1);
    files = {};
    files[fn] = 1;
    addDep(files,resDep,fn);
    stubs=[];
    if fn.endswith("_d.unity3d"):
        scenefn = cfg["RES_PATH"]+"/Assets/Art"+fn[fn.find("/"):fn.rfind(".")]+".unity";
         
        
        print "scenffn:",scenefn;
        with open(scenefn) as f:
            bStart = False;
            abname="";
            for l in f.readlines():
                if not bStart:
                    if l.strip() == "stubs:":
                        bStart = True;
                    else:
                        continue;
                else:
                    if l.startswith("---"):
                        bStart = False;
                        
                    elif l.startswith("  - prefab:"):
                        abname = l[l.find(":")+1:].strip().lower();
                        
                    elif l.startswith("    pos:"):
                        stubs.append("res/"+abname);
                    else:
                        abname = abname+l.rstrip()[len("    ")+1:];
                        #print "abname:",abname;
    for stub in stubs:
        files[stub] = 1;
        addDep(files,resDep,stub);
    print "files len:",len(files),"stub:",len(stubs);
    destfn = fn[fn.rfind("/")+1:].split(".")[0];
    sz =0;
    #cmp = lambda x,y:return 
    names = files.keys();
    names.sort();
    if not os.path.exists("result"):
        os.mkdir("result");
    with open("result/"+destfn+".csv","w") as fp:
        for f in names:
            refcnt = 0;
            rdep=[];
            if allDict != None:
                allDict[f] = 1;
            if f in reverseDep:
                refcnt = len(reverseDep[f]);
                rdep=reverseDep[f];
            rf = prepath+f;
            info = os.stat(rf);
            sz = sz+info.st_size;
            fp.write(f+","+str(info.st_size)+","+str(refcnt)+","+"|".join(rdep)+"\n");
            
    print fn,"size:",sz/1000000.0;  
    return sz;
def checkAllResSize(pf):
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    scenebase = cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/scene/";
    prelen = len(cfg["RES_PATH"]+"/AssetBundles/"+pf+"/");
    filedata=[];
    allDict={};
    for fn in os.listdir(scenebase):
         
        if fn.endswith(".unity3d"):
            
            sz = checkResSize(pf,scenebase[prelen:]+fn,allDict);  
            if sz>0:
                filedata.append([fn,str(sz/1024.0/1024.0)]);
    with open("result/total.csv","w") as f:
        for d in filedata:
            f.write(",".join(d)+"\n");
    sz = 0;      
    prepath =  cfg["RES_PATH"]+"/AssetBundles/"+pf+"/";    
    for f in allDict:
         
        rf = prepath+f;
        info = os.stat(rf);
        sz = sz+info.st_size;
    basedir = cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/exmodel/";  
    prelen = len(cfg["RES_PATH"]+"/AssetBundles/"+pf)+1;
    notused={};
    for root,dirs,files in os.walk(basedir):
        for f in files:
            fn = os.path.join(root,f).replace("\\","/");
            fn = fn[prelen:];
            if fn.endswith(".unity3d"):
                if not fn in allDict:
                    notused[fn] = 1;
    with open("result/notused.txt","w") as f:
        for d in notused:
            f.write(d+"\n");            
    print "total size:",sz;   
if __name__ == "__main__":
    pf="android";
    resid = "1";
    if len(sys.argv)>1:
        pf=sys.argv[1];
    if sys.argv[2] == "all":
        checkAllResSize(pf );    
    else:
        checkResSize(pf,sys.argv[2]);    