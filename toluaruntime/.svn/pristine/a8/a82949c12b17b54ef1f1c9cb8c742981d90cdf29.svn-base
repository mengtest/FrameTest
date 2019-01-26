#include "lib_log.h"
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include "lua.h"
#include "lj_strfmt.h"
#include "lauxlib.h"
void LOG(lua_State *L,const char * fmt,...)
{
    const char *msg;
    lua_getfield(L, LUA_GLOBALSINDEX, "logger");
    if (!lua_isfunction(L, -1))
    {
        luaL_error(L, LUA_QL("loader") " must be a function");
        return;
    }
    va_list vs;
    
    va_start(vs,fmt);
    msg = lj_strfmt_pushvf(L, fmt, vs);
      
    lua_call(L, 1, 0);  /* call it */
    va_end(vs);
}
static lua_State *curL=NULL;
//#define SHOW_LOG
//#ifdef SHOW_LOG

void _TRY_LOG(const char * fmt,...)
{

    if(curL == NULL)
        return;
    //const char *msg;
    lua_getfield(curL, LUA_GLOBALSINDEX, "logger");
    if (!lua_isfunction(curL, -1))
    {
        luaL_error(curL, LUA_QL("loader") " must be a function");
        return;
    }
    va_list vs;
    
    va_start(vs,fmt);
    lj_strfmt_pushvf(curL, fmt, vs);
      
    lua_call(curL, 1, 0);  /* call it */
    va_end(vs);
  
}
//#endif  
void SET_LOG_LUASTATE(lua_State* L){
    curL = L;
}
void CLEAR_LOG_LUASTATE(lua_State* L){
    if(L == curL)
        curL = NULL;
}
struct PERF_COUNT{
    clock_t t;
    int idx;
};
/*
static struct PERF_COUNT *_perfData=NULL;
static int _perfPos;
static int _perfCnt;
void RESET_PERF_COUNT(int cnt){
    if(_perfData != NULL)
        free(_perfData);
    _perfData = (struct PERF_COUNT *)calloc(cnt,sizeof(struct PERF_COUNT));
    _perfCnt = cnt;
    _perfPos = 0;
}
void ADD_PERF_TIME(int idx){

}
void LOG_PERF_TIME(){

}
*/