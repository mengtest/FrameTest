# -*- coding: utf8 -*-'
#小包资源管理工具
import os,md5,shutil;
import zlib,sys,pysvn;
import platform;


def md5str(val):
    m = md5.new();
    m.update(val);  
    return m.hexdigest();

def convert():

    pass;
def stat();
    pass;
 

if len(sys.argv)>1:
    if sys.argv[1] == "convert":
        convert();
    elif sys.argv[1] == "stat":
        stat();
     
        


