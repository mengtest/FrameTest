#ifdef __cplusplus
extern "C" {
#endif
#include "lua.h"
#include "lualib.h"
#include "lauxlib.h"

#include "pdb.h"
#include "../tolua.h"
#ifdef __cplusplus
}
#endif
#include "lib_log.h"
#include "pbc.h"
#include <time.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#if defined(__APPLE__)
    #include <malloc/malloc.h>
#else
    #include <malloc.h>
#endif
#ifdef CHECK_MEM
#include "checkmem.h"

#endif

#ifdef _MSC_VER
#define alloca _alloca
#endif
#if LUA_VERSION_NUM == 501

#define lua_rawlen lua_objlen
#endif
extern int doCheck(lua_State *L);
void push_value(lua_State *L, int type, const char * type_name, union pbc_value *v);
extern void new_array(lua_State *L, int id, const char *key);
int
pb_decode(struct pbc_env * env,lua_State *L,const char *type,struct pbc_slice *slice);

static const char *pdb_meta      = ":pdb";
typedef struct pdb pdb;
/* information about database */
struct pdb {
    /* associated lua state */
    lua_State *L;
    /* sqlite database handle */
    spdb *db;

  
};

static pdb *newdb (lua_State *L) {
    pdb *db = (pdb*)lua_newuserdata(L, sizeof(pdb));
    db->L = L;
    db->db = NULL;  /* database handle is currently `closed' */
    

    luaL_getmetatable(L, pdb_meta);
    lua_setmetatable(L, -2);        /* set metatable */

    // /* to keep track of 'open' virtual machines */
    // lua_pushlightuserdata(L, db);
    // lua_newtable(L);
    // lua_rawset(L, LUA_REGISTRYINDEX);

    return db;
}
static int cleanupdb(lua_State *L, pdb *db) {
     

    /* remove entry in lua registry table */
    lua_pushlightuserdata(L, db);
    lua_pushnil(L);
    lua_rawset(L, LUA_REGISTRYINDEX);

    
    /* close database */
    if(db->db ==NULL)
    {
        return 0;
    }
    int result = protodb_close(db->db);
    db->db = NULL;

    
    return result;
}

static int pdb_do_open(lua_State *L, const char *filename) {
    pdb *db = newdb(L); /* create and leave in stack */
    int ret = protodb_open(filename, &db->db);
    if (ret == PDB_OK) {
        struct pbc_env *env = protodb_getEnv(db->db);
        lua_pushlightuserdata(L,env); 
        LOG(L,"open db success:%s,env:%x",filename,env);
        /* database handle already in the stack - return it */
        return 2;
    }
    LOG(L,"open db failed:%s,ret:%d",filename,ret);
    /* failed to open database */
    lua_pushnil(L);                             /* push nil */
    lua_pushinteger(L, ret);
     
    /* clean things up */
    //cleanupdb(L, db);
   
    /* return */
    return 2;
}

static int pdb_open(lua_State *L ) {
    const char *filename = luaL_checkstring(L, 1);
    return pdb_do_open(L, filename );
}

static pdb *pdb_getdb(lua_State *L, int index) {
    pdb *db = (pdb*)luaL_checkudata(L, index, pdb_meta);
    if (db == NULL) luaL_typerror(L, index, "protobuff database");
    return db;
}
 
static int fillTable(lua_State *L,read_buff *buff,head_item *items,int headItemLen,const char *tablename)
{
    lua_newtable(L);
    int idx = 1;
    buff->pos = 0;
    //TRY_LOG("begin read all  buff pos:%d,len:%d,tablename:%s", buff->pos,buff->len,tablename);
    while(buff->pos<buff->len)
    {
        uint32_t len =  readBuffUint32(buff);
        // TRY_LOG("begin read len:%d, tablename:%s,pos:%d,headItemLen:%d", len,tablename, buff->pos,headItemLen);  
        if(len <=4)
            continue;
        len -=4;
        if(buff->len<buff->pos+len)
            break;
        lua_createtable(L,headItemLen,0);
       
        for(int i=0;i<headItemLen;i++)
        {
            switch(items[i].tp)
            {
                case FIELD_TYPE_INTEGER:
                {
                   int32_t v =  readBuffInt32(buff);
                   if(v== 0)
                        continue;
                    lua_pushinteger(L,v);
                }
                  
                    
                break;
                case FIELD_TYPE_INT64:
                {
                    int64_t v = readBuffInt64(buff);
                   if(v== 0)
                        continue;
                    lua_pushnumber(L, (lua_Number)v);
                }
                    
                    
                    break;
                case FIELD_TYPE_STRING:
                {
                    const char* v = readBuffString(buff);
                    if(v == NULL || strlen(v) == 0)
                        continue;
                    lua_pushstring(L,v);
                }
                    
                    break;
                case FIELD_TYPE_FLOAT:
                {
                    float v = readBuffFloat(buff);
                    if(v == 0)
                        continue;
                     lua_pushnumber(L,v);
                }
                   
                    break;
                default:
                    {
                         
                         
                        return 0;
                        
                    }
                    break;
            }
            lua_rawseti(L,-2,i+1);
        }
    
        /*
        struct pbc_slice slice;
        slice.buffer = buff->buff+buff->pos;
        slice.len = len;
        buff->pos += len; 
    
        struct pbc_rmessage * m = pbc_rmessage_new(env, tablename, &slice);
        
        if (m==NULL)
            break;
            */
        // lua_pushinteger(L,idx);
        // lua_pushlightuserdata(L,m);
        lua_rawseti(L,-2,idx);
        // lua_settable(L,-3);
        idx++;
    }
    return 1;
    //TRY_LOG("end fill table cnt:%d,tablename:%s",idx-1,tablename);
}
 
/*
** =======================================================
** Database Methods
** =======================================================
*/
static int db_getTableMeta(lua_State *L)
{
    CHECK_VALID(0);
    int top = lua_gettop(L);
    if(top<2)
        luaL_error(L, "args number less than 2" );
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    const char *tablename = luaL_checkstring(L, 2);
    // TRY_LOG("get tablemeta:%s",tablename);
    head_item *item=NULL;
    int headItemLen=0;
    int ret = protodb_getTableHeadItem(db->db,tablename,&item,&headItemLen);
    if(ret != PDB_OK)
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
    // TRY_LOG("gethead len:%d,item:%x",headItemLen,item);
    lua_createtable(L,0,headItemLen);
    for(int i=0;i<headItemLen;i++)    
    {
        lua_pushstring(L,item[i].field);
        lua_pushinteger(L,(item[i].tp<<10)+ i+1);
        lua_rawset(L,-3);
    }
    return 1;
}
static int db_isopen(lua_State *L) {
    CHECK_VALID(0);
    pdb *db = pdb_getdb(L, 1);
    lua_pushboolean(L, db->db != NULL ? 1 : 0);
    return 1;
}
static int getAllParams(lua_State *L,param_pair **params,int startPos)
{
    int top = lua_gettop(L);
    int cnt = top-startPos+1;
    if(cnt<=0)
    {
        //TRY_LOG("getAllParams cnt is zero:%d",cnt );
        *params = NULL;
        return 0;
    }
    *params = (param_pair*)calloc(cnt,sizeof(param_pair));
    //TRY_LOG("getAllParams cnt:%d,startPos:%d,top:%d",cnt,startPos,top);
    for(int i=0;i<cnt;i++)
    {
        int tp = lua_type(L,startPos+i);
        switch(tp)
        {
            case LUA_TNUMBER:
            {
         
                (*params)[i].ival = luaL_checkinteger(L,startPos+i);
                (*params)[i].valtp = ID_INTEGER;
                // TRY_LOG("getAllParams param ival:%d",(*params)[i].ival);
            }
            break;
            case LUA_TSTRING:
            {
                (*params)[i].strval = luaL_checkstring(L,startPos+i); 
                (*params)[i].valtp = ID_STRING;
                // TRY_LOG("getAllParams param strval:%d",(*params)[i].strval);
            }
            break;
            default:
            {
                free((void*)*params);
                luaL_error(L, "args format error tp:%s,index:%d",lua_typename(L,lua_type(L,startPos+i)),i );
            }
        }
        
        

    }
    return cnt;
}
static void freeParam(param_pair * params,int len)
{
    if(params == NULL)
        return;
    // for(int i=0;i<len;i++)
    // {
    //     if(params[i].valtp == ID_STRING_NEW && params[i].strval != NULL)
    //     {
    //         free(params[i].strval);
    //     }
    // }
    free(params);
}
static int db_getAllById(lua_State *L) {
    CHECK_VALID(0);
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    const char *tablename = luaL_checkstring(L, 2);
    int top = lua_gettop(L);
    if(top<3)
        luaL_error(L, "args number less than 3" );
    param_pair * params;    
    int cnt = getAllParams(L,&params,3);
    if(cnt == 0)
    {
        lua_pushnil(L);
        lua_pushinteger(L,PDB_INSUFFICIENT_ID);
        return 2;
    }
    //TRY_LOG("db_getAllById param len:%d",cnt);
    read_buff buff;
    struct pbc_env *env=NULL;
    //if(params[0].valtp == ID_STRING)
        // TRY_LOG("getAllById table:%s, param1 tp:%d,strval:%x",tablename,params[0].valtp,params[0].strval);
    head_item *item;
    int headItemLen;    
    int ret =  protodb_getAllByIds(db->db,tablename,params,cnt,&buff,&item,&headItemLen);
    freeParam(params,cnt);
    if(ret != PDB_OK)
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
 
    //TRY_LOG("find ret buff len:%d",buff.len);
    if(1 != fillTable(L,&buff,item,headItemLen,tablename))
    {
        freeBuff(&buff);
        lua_pushnil(L);
        lua_pushinteger(L,PDB_FORMAT_ERROR);
        return 2;
    }
    freeBuff(&buff);
    return 1;
}
/*
static int _getById(lua_State *L,pdb *db,const char* tablename,void *val,int valtp)
{
    void *buff;
    int len;
    struct pbc_env *env;
    int c1 = clock();
    int ret = PDB_ID_TYPE_ERROR;
    switch(valtp)
    {
        case ID_INTEGER:
            ret = protodb_getByIntId(db->db, tablename, *(int*)val, &buff, &len, &env);
            break;
        case ID_STRING:
            ret = protodb_getById(db->db, tablename, *(const char**)val, &buff, &len, &env);
            break;
    }
    
    if (ret != PDB_OK)
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
    int c2 = clock();
    double cost = (double)(c2 - c1) / CLOCKS_PER_SEC;
    TRY_LOG("time cost 1 is: %f secs", cost);

    struct pbc_slice slice;
    slice.buffer = buff;
    slice.len = len;
    int c3 = clock();
    //TRY_LOG("getbyid ret:%d,env:%x",ret,env);
    ret = pb_decode(env, L, tablename, &slice, 4);
    if(ret<0)
    {
        lua_pushnil(L);
        lua_pushinteger(L,PDB_PROTO_ERROR);
        return 2;
    }
    int c4 = clock();
    double cost2 = (double)(c4 - c3) / CLOCKS_PER_SEC;
    TRY_LOG("time cost 2 is: %f secs", cost2);

    return 1;
}
*/
static int db_getLen(lua_State *L) {
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    const char *tablename = luaL_checkstring(L, 2);
    int len;
    int ret = protodb_getLen(db->db,tablename,&len);
    if(ret != PDB_OK)
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
    lua_pushinteger(L,len);
    return 1;
}

static int db_getById(lua_State *L) {
    CHECK_VALID(0);
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    const char *tablename = luaL_checkstring(L, 2);
    int fullLoad = luaL_checkinteger(L,3);
    int paramStart = 4;
    if(fullLoad)
    {
        luaL_checktype(L, 4 , LUA_TTABLE);
        paramStart = 5;
    }    
    param_pair * params=NULL;    

    int cnt = getAllParams(L,&params,paramStart);
    if(cnt == 0 || params == NULL)
    {
        lua_pushnil(L);
        lua_pushinteger(L,PDB_INSUFFICIENT_ID);
        return 2;
    }

    read_buff buff;
    struct pbc_env *env;
    //if(params[0].valtp == ID_STRING)
        // TRY_LOG("getByIds table:%s, param1 tp:%d,strval:%x,tp:%s,isnumber:%d",tablename,params[0].valtp,params[0].strval,lua_typename(L,lua_type(L,4)),lua_isnumber(L,4));
    head_item *items;
    int headItemLen;
    int ret = protodb_getByIds(db->db,tablename,params,cnt,&buff,&items,&headItemLen);
    freeParam(params,cnt);
    if(ret != PDB_OK)
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
    // TRY_LOG("getById headItemLen:%d",headItemLen);
    // int b1 = lua_gc(L, LUA_GCCOUNTB, 0);
    lua_createtable(L,headItemLen,0);
    // int b3 = lua_gc(L, LUA_GCCOUNTB, 0);
    buff.pos = 4;
    for(int i=0;i<headItemLen;i++)
    {
        // int b4 = lua_gc(L, LUA_GCCOUNTB, 0);
        int clen = 0;
        switch(items[i].tp)
        {
            case FIELD_TYPE_INTEGER:
                lua_pushinteger(L,readBuffInt32(&buff));
                
            break;
            case FIELD_TYPE_INT64:
                lua_pushnumber(L,(lua_Number)readBuffInt64(&buff)); 
                
                break;
            case FIELD_TYPE_STRING:
                {
                    const char *str = readBuffString(&buff);
                    // clen = strlen(str);
                    lua_pushstring(L,str);
                }
                break;
            case FIELD_TYPE_FLOAT:
                lua_pushnumber(L,readBuffFloat(&buff));
                break;
            default:
                {
                    freeBuff(&buff);
                     
                    lua_pushnil(L);
                    lua_pushinteger(L,PDB_FORMAT_ERROR);
                    return 2;
                     
                }
                break;
        }
        // int b5 = lua_gc(L, LUA_GCCOUNTB, 0);
        lua_rawseti(L,-2,i+1);
        // int b6 = lua_gc(L, LUA_GCCOUNTB, 0);
        // TRY_LOG("getbyid lua mem sub:%d,%d,tp:%d,clen:%d",(b5 - b4),(b6 - b5),items[i].tp,clen);
    }
    // int b2 = lua_gc(L, LUA_GCCOUNTB, 0);
    // TRY_LOG("getbyid lua mem:%d,%d,buff len:%d,headItemLen:%d",(b3- b1),(b2-b3),buff.len,headItemLen);
    freeBuff(&buff);
    return 1;
    /*
    struct pbc_slice slice;
    slice.buffer = buff.buff+4;//skip length head
    slice.len = buff.len-4;
    if(fullLoad)
    {
        lua_pushvalue(L, 4);
        lua_newtable(L);
        
        int n = pb_decode(env,L, tablename, &slice);
        freeBuff(&buff);
        if(n<0)
        {
            lua_pushnil(L);
            lua_pushinteger(L,PDB_PROTO_ERROR);
            return 2;
        }
        return 1;
    }
    else
    {
       
        clock_t c3 = clock();
        //TRY_LOG("getbyid ret:%d,env:%x",ret,env);

        struct pbc_rmessage *m = pbc_rmessage_new(env, tablename, &slice);
        freeBuff(&buff);
        if (m == NULL)
        {
            lua_pushnil(L);
            lua_pushinteger(L, PDB_PROTO_ERROR);
            return 2;
        }
        lua_pushlightuserdata(L, m);
        return 1;
    }
     */
}
/*
int _getById2(lua_State *L,pdb *db,const char *tablename,void *val,int valtp){
    void *buff;
    int len;
    struct pbc_env *env;
    clock_t c1 = clock();
    int ret = -1;
    switch(valtp)
    {
        case ID_INTEGER:
            ret = protodb_getByIntId(db->db, tablename, *(int*)val, &buff, &len, &env);
            break;
        case ID_STRING:
            ret = protodb_getById(db->db, tablename, *(const char **)val, &buff, &len, &env);
    }
    
    if (ret != PDB_OK)
    {
        TRY_LOG("failed to find by id:%d,table:%x", val, tablename);
        lua_pushnil(L);
        lua_pushinteger(L, ret);
        return 2;
    }
    clock_t c2 = clock();

    struct pbc_slice slice;
    slice.buffer = buff;
    slice.len = len;
    clock_t c3 = clock();
    //TRY_LOG("getbyid ret:%d,env:%x",ret,env);

    struct pbc_rmessage *m = pbc_rmessage_new(env, tablename, &slice);
    free(buff);
    if (m == NULL)
    {
        lua_pushnil(L);
        lua_pushinteger(L, PDB_PROTO_ERROR);
        return 2;
    }
    lua_pushlightuserdata(L, m);
    clock_t c4 = clock();

    //TRY_LOG("time cost  is c2-c1=%f,c3-c2=%f,c4-c3=%f secs",(c2-c1)*1.0f/CLOCKS_PER_SEC,(c3-c2)*1.0f/CLOCKS_PER_SEC,(c4-c3)*1.0f/CLOCKS_PER_SEC  );

    return 1;
}

static int db_getById2(lua_State *L) {
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    const char *tablename = luaL_checkstring(L, 2);
    //TRY_LOG("getbyid:%s,isnumber:%d,isstring:%d",tablename,lua_isnumber(L,3),lua_isstring(L,3));
    if(lua_isnumber(L,3)){
        int val =  luaL_checkinteger(L,3);
       return _getById2(L,db,tablename,&val,ID_INTEGER);
        
    }
    else if (lua_isstring(L,3)) {
        const char *val = luaL_checkstring(L,3);
       return _getById2(L,db,tablename,&val,ID_STRING);
    }
    else
    {
        lua_pushnil(L);
        lua_pushinteger(L,PDB_FORMAT_ERROR);
        return 2;
    }
   
}
*/
static int db_getAll(lua_State *L) {
    CHECK_VALID(0);
    pdb *db = pdb_getdb(L, 1);
    if(db->db == NULL)
        luaL_error(L, "db  is nil" );
    clock_t t1 = clock();    
    const char *tablename = luaL_checkstring(L, 2);
    read_buff buff;
    struct pbc_env *env=NULL;
    head_item *item;
    int headItemLen;
    int ret = protodb_getAll(db->db,tablename,&buff,&item,&headItemLen);
    if(PDB_OK != ret )
    {
        lua_pushnil(L);
        lua_pushinteger(L,ret);
        return 2;
    }
    clock_t t2 = clock();    
    if(1 != fillTable(L,&buff,item,headItemLen,tablename))
    {
        freeBuff(&buff);
        lua_pushnil(L);
        lua_pushinteger(L,PDB_FORMAT_ERROR);
        return 2;
    }
 
    clock_t t3 = clock();    
    freeBuff(&buff);
    //TRY_LOG("readall tm,t2-t1:%f,t3-t2:%f,env:%x",(t2-t1)*1.0f/CLOCKS_PER_SEC,(t3-t2)*1.0f/CLOCKS_PER_SEC,env);
    //TRY_LOG("read all idx:%d,error:%s",idx,pbc_error(env));
    return 1;
}
static int db_close(lua_State *L){
    pdb *db = (pdb*)luaL_checkudata(L, 1, pdb_meta);
    if(db == NULL)
        return 0;    
    cleanupdb(L,db);
    return 0;
}

/*
	lightuserdata env
 */
static int
_last_error(lua_State *L) {
	pdb *db = pdb_getdb(L, 1);
	const char * err = protodb_getLastError(db->db);
	lua_pushstring(L,err);
	return 1;
}
static int
_check_error(lua_State *L) {
    
	doCheck(L);
       
	return 0;
}
/* ======================================================= */

static const luaL_Reg dblib[] = {
    {"isopen",              db_isopen               },
    {"getById",              db_getById               },
    // {"getById2",              db_getById2               },
    {"getAllById",              db_getAllById               },
    {"getLen",              db_getLen               },
   
    
    {"getAll",              db_getAll               },
    {"_last_error", _last_error }, 
    {"_error_info", _check_error }, 
    {"getTableMeta",db_getTableMeta},
    {"close",            db_close            },
    {NULL, NULL}
};
static void create_meta(lua_State *L, const char *name, const luaL_Reg *lib) {
    luaL_newmetatable(L, name);
    lua_pushstring(L, "__index");
    lua_pushvalue(L, -2);               /* push metatable */
    lua_rawset(L, -3);                  /* metatable.__index = metatable */

    /* register metatable functions */
    luaL_openlib(L, NULL, lib, 0);

    /* remove metatable from stack */
    lua_pop(L, 1);
}
static int pdb_meminfo(lua_State *L)
{
    #ifdef CHECK_MEM
        lua_pushinteger(L,mc_count());
    #else
        lua_pushinteger(L,0);
    #endif    
    return 1;
}
#ifdef __cplusplus
extern "C" {
#endif
LUA_API 
int
luaopen_pdb(lua_State *L) {
    create_meta(L, pdb_meta, dblib);
	luaL_Reg reg[] = {
		{"open",            pdb_open            },
        {"meminfo",            pdb_meminfo            },
        
        {NULL,NULL},
	};

 
	luaL_register(L,"protodb.c",reg);

	return 1;
}

#ifdef __cplusplus
}
#endif        
