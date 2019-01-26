#ifndef loglib_h
#define loglib_h
#include "lua.h"
#ifdef __ANDROID__
#include <android/log.h>
#define LOG_TAG  "C_TAG"
#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)
#else
#define LOGD(...)  
#endif
void LOG(lua_State *L,const char * fmt,...);
void SET_LOG_LUASTATE(lua_State* L);
void CLEAR_LOG_LUASTATE(lua_State* L);
void _TRY_LOG(const char * fmt,...); 
#ifdef SHOW_LOG
#define TRY_LOG _TRY_LOG 
#else
#define TRY_LOG(...) 
#endif
void RESET_PERF_COUNT(int cnt);
void ADD_PERF_TIME(int idx);
void LOG_PERF_TIME();
#endif