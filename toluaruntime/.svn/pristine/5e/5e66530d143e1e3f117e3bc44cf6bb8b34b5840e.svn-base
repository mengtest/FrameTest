#!/bin/bash
# 64 Bit Version
mkdir -p window/x86_64

cd luajit-2.1
mingw32-make clean

mingw32-make BUILDMODE=static CC="gcc -m64 -O2" XCFLAGS=-DLUAJIT_ENABLE_GC64 #XCFLAGS=-DSHOW_LOG
cp src/libluajit.a ../window/x86_64/libluajit.a
mingw32-make clean

cd ..

gcc -m64 -g -std=gnu99 -shared \
 tolua.c \
 int64.c \
 uint64.c \
 pb.c \
 pbc-lua.c \
 pdb/pdb-lua.c \
 lsqlite3.c \
 lpeg.c \
 struct.c \
 cjson/strbuf.c \
 cjson/lua_cjson.c \
 cjson/fpconv.c \
 lsqlite3/shell.c \
 lsqlite3/sqlite3.c \
 luasocket/auxiliar.c \
 luasocket/buffer.c \
 luasocket/except.c \
 luasocket/inet.c \
 luasocket/io.c \
 luasocket/luasocket.c \
 luasocket/mime.c \
 luasocket/options.c \
 luasocket/select.c \
 luasocket/tcp.c \
 luasocket/timeout.c \
 luasocket/udp.c \
 luasocket/wsocket.c \
 pbc/alloc.c \
 pbc/array.c \
 pbc/bootstrap.c \
 pbc/context.c \
 pbc/decode.c \
 pbc/map.c \
 pbc/pattern.c \
 pbc/proto.c \
 pbc/register.c \
 pbc/rmessage.c \
 pbc/stringpool.c \
 pbc/varint.c \
 pbc/wmessage.c \
 pdb/pdb.c \
 pdb/checkmem.c \
 pdb/hashmap.c \
 crypto/aes.c \
 crypto/crc_32.c \
 crypto/md5.c \
 -o Plugins/x86_64/tolua.dll \
 -I./ \
 -Iluajit-2.1/src \
 -Icjson \
 -Iluasocket \
 -Ipbc \
 -Ipdb \
 -Ilsqlite3 \
 -lws2_32 \
 -Wl,--whole-archive window/x86_64/libluajit.a -Wl,--no-whole-archive -static-libgcc -static-libstdc++ \
 -Wl,-Map,a.map -DCHECK_MEM #-DSHOW_LOG

 #cp Plugins/x86_64/tolua.dll /e/workspace/unity/x3-frontend/main/Assets/Plugins/x86_64/
 ls -l Plugins/x86_64/tolua.dll