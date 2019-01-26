# -*- coding: utf8 -*-'
import threading  
import time,thread,md5
import sqlite3,os
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
class Uploader(threading.Thread):
    def __init__(self,idx,cfgs, datas ):  
        threading.Thread.__init__(self) 
        self.datas = datas;
        self.idx = idx;
        self.thread_stop = False; 
    def run(self):
        ftp=FTP()
        ftp.connect(cfgs["ftp_ip"],cfgs["ftp_port"])
        ftp.login(cfgs["ftp_user"],cfgs["ftp_pass"])
        print ftp.getwelcome()
        
        ftp.cmd("/cdn")
        while not self.thread_stop:
            mylock.acquire();
            if len(self.datas) == 0:
                mylock.release();
                print "thread end";
                thread.exit_thread()  
            val = self.datas.pop();
            mylock.release();
            print "val:",val,",idx:",self.idx;
            time.sleep(0.1);
        pass;
    def stop(self):  
        self.thread_stop = True      
def doUpload(idx,datas):
    print "start :",idx;
    while True:
        try:
            mylock.aquire();
            if len(datas) == 0:
                mylock.release();
                print "thread end";
                thread.exit_thread()  
            val = datas.pop();
            mylock.release();
            print "val:",val,",idx:",idx;
            thread.sleep(0.1);
        except:
            print "error";
            thread.exit_thread()  
def startUpload(pf):
    cfgs={};
    with open("upload.conf") as f:
        for l in f.readlines():
            vals = l.strip().split("=");
            if len(vals) != 2:
                continue;
            cfgs[vals[0].strip()] = vals[1].strip();
    cdnpath = cfgs["cdn_path"];
    
    k = md5str(cdnpath);
    if not os.path.exists("db"):
        os.makedirs("db");
    dbfn = "db/"+k+".info";
    fileInfos = {};
    if os.path.exists(dbfn):    
        with open(dbfn) as f:
            for l in f.readlines()[1:]:
                vals = l.strip().split(",");
                if len(vals) == 2:
                    fileInfos[vals[0].strip()] = vals[1].strip();
    print "start search:" ;                
    toUpload = [];                
    for root,dirs,files in os.walk(pf+"/release"):
        for f in files:
            fn = os.path.join(root,f);
            fn  = fn.replace("\\","/");
            md5k = md5file(fn);
            if not fn in fileInfos or fileInfos[fn] != md5k:
                toUpload.append((fn,md5k));
                
    print "toUpload:",len(toUpload);
 
    t = Uploader(0,cfgs,toUpload); 
    t.start();
    '''
    for i in range(1000):
        datas.append(i);
        
    for i in range(10):    
        t = Uploader(i,datas); 
        t.start();
    '''    
if __name__== '__main__':  
    startUpload("android")
    time.sleep(3)