# -*- coding: utf8 -*-'
from paramiko import SSHClient
from scp import SCPClient
import paramiko,sys,io
if __name__ == "__main__":
    s=sys.argv[1];
   
    if len(sys.argv)>2:
        destpf = sys.argv[2];
    cfgf = io.open("servers.ini", encoding="utf-8");
    cfg = {};
    for l in cfgf.readlines():
        
        strs = l.strip().split("#")[0];
        strs = strs.strip().split("=");
        print strs;
        if len(strs) == 2:
            cfg[strs[0]] = strs[1].split("|");
    print cfg;        
    server = cfg[s];
    if not s in cfg:
        print u"找不到服务器".encode("gbk"),s
        exit(1);
    
        
    ssh = SSHClient()
    ssh.load_system_host_keys()

    ssh.set_missing_host_key_policy(paramiko.WarningPolicy())
    ssh.connect(server[0],username=server[1],password=server[2])
    stdin, stdout, stderr = ssh.exec_command("cd  "+server[3]+" ; svn up bin/config ; ./safe_restartgame;echo success", get_pty=True);
    print stdout.readlines();
 
    ssh.close();
        