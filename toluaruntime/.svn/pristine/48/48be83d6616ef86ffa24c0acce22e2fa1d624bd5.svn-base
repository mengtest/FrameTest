LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := libluajit
LOCAL_SRC_FILES := libluajit.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_FORCE_STATIC_EXECUTABLE := true
LOCAL_MODULE := tolua
LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../luajit-2.1/src
LOCAL_C_INCLUDES += $(LOCAL_PATH)/../../
LOCAL_C_INCLUDES += $(LOCAL_PATH)/../../lsqlite3
LOCAL_C_INCLUDES += $(LOCAL_PATH)/../../pdb

LOCAL_CPPFLAGS := -O2
LOCAL_CFLAGS :=  -O2 -std=gnu99    -fvisibility=hidden #-DCHECK_MEM
LOCAL_SRC_FILES :=	../../tolua.c \
					../../int64.c \
					../../uint64.c \
					../../pb.c \
					../../lpeg.c \
					../../struct.c \
					../../pbc-lua.c \
					../../lsqlite3.c \
					../../cjson/strbuf.c \
					../../cjson/lua_cjson.c \
					../../cjson/fpconv.c \
					../../lsqlite3/shell.c \
					../../lsqlite3/sqlite3.c \
					../../luasocket/auxiliar.c \
 					../../luasocket/buffer.c \
 					../../luasocket/except.c \
 					../../luasocket/inet.c \
 					../../luasocket/io.c \
 					../../luasocket/luasocket.c \
 					../../luasocket/mime.c \
 					../../luasocket/options.c \
 					../../luasocket/select.c \
 					../../luasocket/tcp.c \
 					../../luasocket/timeout.c \
 					../../luasocket/udp.c \
 					../../luasocket/usocket.c \
 					../../pbc/alloc.c \
 					../../pbc/array.c \
 					../../pbc/bootstrap.c \
 					../../pbc/context.c \
 					../../pbc/decode.c \
 					../../pbc/map.c \
 					../../pbc/pattern.c \
 					../../pbc/proto.c \
 					../../pbc/register.c \
 					../../pbc/rmessage.c \
 					../../pbc/stringpool.c \
 					../../pbc/varint.c \
 					../../pbc/wmessage.c \
                    ../../crypto/aes.c \
                    ../../crypto/crc_32.c \
					../../crypto/md5.c \
					../../pdb/pdb-lua.c \
					../../pdb/pdb.c \
					../../pdb/checkmem.c \
 					../../pdb/hashmap.c \
                     
LOCAL_WHOLE_STATIC_LIBRARIES += libluajit
include $(BUILD_SHARED_LIBRARY)