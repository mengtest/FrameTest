# -*- coding: utf8 -*-'
import urllib2
import sys;
cdnbase = "http://cdn.x3.mikeyouxi.com/x3/cdn27/appres/android/release/"
releasebase = "F:/workspace/release/channel/android/release/";
resbase = "data";

def checkFile(fn):
    response = urllib2.urlopen(cdnbase+fn);
    remotedata = response.read();
    localdata=None;
    with open(releasebase+fn,"rb") as f:
        localdata = f.read();
    if len(remotedata) != len(localdata):
        print u"文件长度不一致 fn:",fn," remote:",len(remotedata),",local:",len(localdata)
    else:
        for i in range(len(remotedata)):
            if remotedata[i] != localdata[i]:
                print u"文件内容不一致  :",fn;
                break;
def checkDefault():
    checkFile("res/ver_res.dat");
    checkFile("res/ver_res.info");
    checkFile("data/ver_data.dat");
    checkFile("data/ver_data.dat");
if len(sys.argv) >1:        
    fn=sys.argv[1];
    checkFile(fn);
else:
    checkDefault();