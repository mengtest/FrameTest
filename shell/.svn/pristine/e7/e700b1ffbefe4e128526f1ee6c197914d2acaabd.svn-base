# -*- coding: utf8 -*-'
import sys,os;
from common import *;
 
def searchDep(depDict,root,childFn):
    for k in depDict[root]:
        if k == childFn:
            return True;
        else:
            if searchDep(depDict,k,childFn):
                print k,"\n";
                return True;
    return False;
def getDepChain(pf,fn,childFn):
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    resDep,reverseDep = readManifest(cfg["RES_PATH"]+"/AssetBundles/"+pf+"/res/res.manifest","res");  
    
    print "resDep:",len(resDep);
    searchDep(resDep,fn,childFn);
if __name__ == "__main__":
    pf="android";
    resid = "1";
    if len(sys.argv)>1:
        pf=sys.argv[1];
   
    getDepChain(pf,sys.argv[2],sys.argv[3]);       