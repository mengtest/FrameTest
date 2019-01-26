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