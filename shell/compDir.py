# -*- coding: utf8 -*-'
import threading  
import time,thread,md5
import sqlite3,os,sys
from ftplib import FTP 
mylock = thread.allocate_lock()  #Allocate a lock
def md5str(val):
    m = md5.new();
    m.update(val);  
    return m.hexdigest();
def md5file(fn):
    f = open(fn,"rb");
    m = md5.new();
    data = f.read();
    m.update(data);
    f.close();
    
    return m.hexdigest(),len(data);
src = sys.argv[1];
dest = sys.argv[2];
print "begin compare:",src,dest;    
origFiles = {};
changeFiles = {};
editexe = "C:\\Program Files (x86)\\Notepad++\\notepad++.exe";
for root,dirs,files in os.walk(src):
    for f in files:
        fn = os.path.join(root,f);
        fn = fn.replace("\\","/");
        md5k = md5file(fn);
        rfn = fn[len(src):];
        if rfn.startswith("/"):
            rfn = rfn[1:];
        origFiles[rfn] = md5k;
for root,dirs,files in os.walk(dest):
    for f in files:
        fn = os.path.join(root,f);
        fn = fn.replace("\\","/");
        md5k = md5file(fn);
        rfn = fn[len(dest):];
        if rfn.startswith("/"):
            rfn = rfn[1:];
        if rfn in origFiles:
            if origFiles[rfn] == md5k:
                continue;
            else:
                changeFiles[rfn] = (1,md5k,origFiles[rfn]);
        else:
            changeFiles[rfn] = (2,md5k,"");
dataIds = {};            
with open(dest+"/info_data.csv") as f:
    for l in f.readlines():
        vals = l.strip().split(",");
        dataIds[vals[3].strip()] = vals[0].strip();
resIds = {};            
with open(dest+"/info_res.csv") as f:
    for l in f.readlines():
        vals = l.strip().split(",");
        resIds[vals[3].strip()] = vals[0].strip();     
with open("check.txt","w") as f:
    for item in changeFiles:
        id = item[item.rfind("/")+1:];
        if item.startswith("data/"):
            if id in dataIds:
                f.write(item+","+dataIds[id]+"\n");
            else:
                f.write(item+ "\n");
        else:
            if id in resIds:
                f.write(item+","+resIds[id]+"\n");
            else:
                f.write(item+ "\n");
                
os.system("\""+editexe+"\" check.txt");   