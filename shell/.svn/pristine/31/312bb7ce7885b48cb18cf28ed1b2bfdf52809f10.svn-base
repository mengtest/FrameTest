# -*- coding: utf8 -*-'
import urllib2,os,sys,zlib
pf = "android";
data = {};
with open(pf+"/release/info_res.csv") as f:
    for l in f.readlines():
        vars = l.strip().split(",");
        data[vars[3]] = vars[0];
with open(sys.argv[1]) as f:
    for l in f.readlines():
        p1 = l.rfind("/");
        p1 = l[:p1-1].rfind("/");
        p2 = l.find(" ",p1);
        f = l[p1+1:p2];
        print "f:",f;
        if f in data:
            print data[f];