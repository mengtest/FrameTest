# -*- coding: utf8 -*-'
import urllib2,os,sys,zlib
import zipfile
from datetime import datetime;

allCdnbase ={"zh": "http://cdn.x3.mikeyouxi.com/x3/","gt":"http://jlcq.tw.cdn.mecheast.com/cdn/"}
alleditor={"ios":"/Applications/Sublime Text 2.app/Contents/SharedSupport/bin/subl","android":"C:\\Program Files (x86)\\Notepad++\\notepad++.exe"}
 
def readRemoteVer(url):
    print "get urlxx:",url;
    response = urllib2.urlopen(url);    
    remotedata = response.read(); 
    verdata =  zlib.decompress(remotedata);
    verarr =  verdata.split("|");
    verdic = {};
    for ar in verarr:
        arrs = ar.split(",");
        verdic[arrs[0]] = arrs;
    return verdic; 
def readLocalVer(fn):
    print "get local fn:",fn;
    with open(fn,"rb") as f:
        data = f.read();     
        verdata =  zlib.decompress(data);
        verarr =  verdata.split("|");
        verdic = {};
        for ar in verarr:
            arrs = ar.split(",");
            verdic[arrs[0]] = arrs;
        return verdic;     
def readRemoteCsv(url):
    response = urllib2.urlopen(url);
    infodata = response.read();   
    with open("temp.txt","w") as f:
        f.write(infodata);
    print "remote csv:",url;    
    rows = infodata.split("\n");
    rowdict = {};
    for r in rows:
        if r.strip() != "":
            rarr = r.strip().split(",");
            if rarr[3] == '18009b2f14d5745596eb9a37dc684ece':
                print "remote:",r
            rowdict[rarr[3]] = rarr;
    return rowdict;    
def readLocalCsv(fn):
    with open(fn) as f:    
        infodata = f.read();   
        rows = infodata.split("\n");
        rowdict = {};
        for r in rows:
            if r.strip() != "":
                rarr = r.strip().split(",");
                rowdict[rarr[3]] = rarr;
        return rowdict;    
def checkUpdateSize(zone,pf,cdn1,cdn2,sub):
    print "checkUpdateSize",pf
    cdnbase = allCdnbase[zone];
    verData1 = cdnbase+cdn1+"/appres/"+pf+"/release/"+sub+"/ver_"+sub+".dat";
    verData2 = cdnbase+cdn2+"/appres/"+pf+"/release/"+sub+"/ver_"+sub+".dat";
    verDataDic1 = readRemoteVer(verData1);
    verDataDic2 = readRemoteVer(verData2);
    needUpdate = {};
    forceUpdate = {};
    needUpdateSz = 0;
    forceUpdateSz = 0;
    for k in verDataDic2:
        if (not k in verDataDic1) or (  k in verDataDic1 and verDataDic1[k][1] != verDataDic2[k][1] ) :
            
            #print "data:",verDataDic2[k]
            tp = int(verDataDic2[k][3]);
            print "tp:",tp;
            needUpdateSz = needUpdateSz + int(verDataDic2[k][2]);
            if tp == 2 or tp == 3 or tp == 4 or tp == 9:
                forceUpdate[k] = verDataDic2[k];
                forceUpdateSz = forceUpdateSz + int(verDataDic2[k][2]);
            else:
                needUpdate[k] = verDataDic2[k];
    csvurl =  cdnbase+cdn2+"/appres/"+pf+"/release/info_"+sub+".csv";
    resdict = readRemoteCsv(csvurl);
    return (needUpdateSz,forceUpdateSz,needUpdate,forceUpdate,resdict)
def checkLocalUpdateSize(zone,cdn1,pf,sub):
    cdnbase = allCdnbase[zone];
    verData1 = cdnbase+cdn1+"/appres/"+pf+"/release/"+sub+"/ver_"+sub+".dat";
    verData2 = pf+"/release/"+sub+"/ver_"+sub+".dat";
    verDataDic1 = readRemoteVer(verData1);
    verDataDic2 = readLocalVer(verData2);
    needUpdate = {};
    forceUpdate = {};
    needUpdateSz = 0;
    forceUpdateSz = 0;
    for k in verDataDic2:
        if (not k in verDataDic1) or (  k in verDataDic1 and verDataDic1[k][1] != verDataDic2[k][1] ) :
            
            tp = int(verDataDic2[k][3]);
            needUpdateSz = needUpdateSz + int(verDataDic2[k][2]);
            if tp == 2 or tp == 3 or tp == 4 or tp == 9:
                forceUpdate[k] = verDataDic2[k];
                forceUpdateSz = forceUpdateSz + int(verDataDic2[k][2]);
            else:
                needUpdate[k] = verDataDic2[k];
    csvurl =  pf+"/release/info_"+sub+".csv";
    resdict = readLocalCsv(csvurl);
    return (needUpdateSz,forceUpdateSz,needUpdate,forceUpdate,resdict)  
def checkAll(zone,pf,cdn1,cdn2):
    res = checkUpdateSize(zone,pf,cdn1,cdn2,"res");
    data = checkUpdateSize(zone,pf,cdn1,cdn2,"data");
    writeLog(res,pf,data);
def writeLog(res,pf,data):    
    logfn = "check.txt";
    with open(logfn,"w") as f:
        f.write("hot update total:"+str(res[1]+data[1])+"\n");
        f.write("\tres:"+str(res[1])+"\n");
        f.write("\tdata:"+str(data[1])+"\n");
        f.write("change total:"+str(res[0]+data[0])+"\n");
        f.write("\tres:"+str(res[0])+"\n");
        f.write("\tdata:"+str(data[0])+"\n");
        f.write("hot update data:\n");
        for k in data[3]:
            if not k in data[4]:
                print "file not in data[4]:",k,data[3][k]
            f.write("\tid:"+k+",name:"+data[4][k][0]+",size:"+data[3][k][2]+",tp:"+data[3][k][3]+"\n");
        f.write("hot update res:\n");
        for k in res[3]:
            f.write("\tid:"+k+",name:"+res[4][k][0]+",size:"+res[3][k][2]+",tp:"+res[3][k][3]+"\n");
        f.write("change data:\n");
        for k in data[2]:
            #print "data:",k,data[2][k]
            f.write("\tid:"+k+",name:"+data[4][k][0]+",size:"+data[2][k][2]+",tp:"+data[2][k][3]+"\n");
        f.write("change res:\n");
        for k in res[2]:
            f.write("\tid:"+k+",name:"+res[4][k][0]+",size:"+res[2][k][2]+",tp:"+res[2][k][3]+"\n");
             
    print "\""+alleditor[pf]+"\" check.txt";    
    os.system("\""+alleditor[pf]+"\" check.txt");

#差异和版本文件打包成release.zip
def ziprelease(res,data,pf):

    basedir = os.getcwd();
    releasePath=basedir+"/"+pf + "/release/";
    releasePathData =basedir+"/"+pf + "/release/data/";
    releasePathRes = basedir+"/"+pf + "/release/res/";
    tempzip=basedir+"/"+pf +"/"+pf+"_temp.zip";
    releasefiles=[];

    releasefiles.append(releasePathData+"ver_data.dat");
    releasefiles.append(releasePathData + "ver_data.info");
    releasefiles.append(releasePathRes + "ver_res.dat");
    releasefiles.append(releasePathRes + "ver_res.info");

    releasefiles.append(releasePath + "info_data.csv");
    releasefiles.append(releasePath + "info_res.csv");

    for k in data[3]:
        releasefiles.append(releasePathData+k[0]+"/"+k);
        if os.path.exists(releasePathData+k[0]+"/"+k+"_z"):
            releasefiles.append(releasePathData+k[0]+"/"+k+"_z");
    for k in res[3]:
        releasefiles.append(releasePathRes + k[0]+"/"+k)
    for k in data[2]:
        releasefiles.append(releasePathData + k[0]+"/"+k);
        if os.path.exists(releasePathData+k[0]+"/"+k+"_z"):
            releasefiles.append(releasePathData+k[0]+"/"+k+"_z");
    for k in res[2]:
        releasefiles.append(releasePathRes + k[0]+"/"+k);

    dirname=basedir+"/"+pf + "/";
    zip_dir(releasefiles,tempzip,dirname);

def zip_dir(filelist,zipfilename,dirname):

    showLog("begin zip to:",zipfilename,",len:",len(filelist));
    zf = zipfile.ZipFile(zipfilename, "w", zipfile.ZIP_STORED)
    for tar in filelist:
        arcname = tar[len(dirname):]
        #print arcname
        zf.write(tar,arcname)
    zf.close()

def showLog(*Arg ):
    print u" ".join([ type(k)==unicode and k or str(k) for k in Arg]),datetime.now()  ;

def checkLocal(zone,pf,cdn1):
    res = checkLocalUpdateSize(zone,cdn1,pf,"res");
    data = checkLocalUpdateSize(zone,cdn1,pf,"data");    
    writeLog(res,pf,data);
    ziprelease(res,data,pf);

if __name__ == "__main__":
    zone = "zh";
    
    if len(sys.argv)>4:
        checkAll(sys.argv[1],sys.argv[2],sys.argv[3],sys.argv[4]);
    else:
        checkLocal(sys.argv[1],sys.argv[2],sys.argv[3]);