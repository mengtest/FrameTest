# -*- coding: utf8 -*-'
import zlib,sys,os;
def findFile(fn,basedir):
    p = basedir +"/"+fn[0]+"/"+fn;
    return os.path.exists(p);
def checkStream(pf):
    cfgf = open(pf+".conf");
    cfg = {};
    for l in cfgf.readlines():
        strs = l.strip().split("=");
        if len(strs) == 2:
            cfg[strs[0]] = strs[1];
    basedir = cfg["APP_PATH"]+"/main/Assets/StreamingAssets/";
    with open(basedir+"ver_data.dat","rb") as f:
        verdata = zlib.decompress(f.read());
        vers = verdata.split("|");
        for v in vers:
            fields = v.split(",");
            if len(fields)==5:
                fname = fields[0];
                if not findFile(fname,basedir):
                    print "failed to find data ",fname," in local";
    with open(basedir+"ver_res.dat","rb") as f:
        verdata = zlib.decompress(f.read());
        vers = verdata.split("|");
        for v in vers:
            fields = v.split(",");
            if len(fields)==5:
                fname = fields[0];
                if not findFile(fname,basedir):
                    print "failed to find res ",fname," in local";
                    
if __name__ == "__main__":
    pf="android";
    
    if len(sys.argv)>1:
        pf=sys.argv[1];
    checkStream(pf);    