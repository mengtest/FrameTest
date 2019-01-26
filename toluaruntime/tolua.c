/*
The MIT License (MIT)

Copyright (c) 2015-2016 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#include <string.h>
#if !defined __APPLE__
#include <malloc.h>
#endif
#include <stdbool.h>
#include <math.h>
#include <stdint.h>
#include <stdlib.h>

#include "lua.h"
#include "lib_log.h"
#include "lualib.h"
#include "lauxlib.h"
#include "tolua.h"
#include "crypto/aes.h"
#include "crypto/crc.h"
#include "crypto/md5.h"
 
#ifdef _WIN32
#include <windows.h>
#else
#include <time.h>
#include <sys/time.h>
#endif
#include "lib_log.h"
 
 
int toluaflags = FLAG_INDEX_ERROR;
static int tag = 0;  
static int gettag = 0;
static int settag = 0;
static int vptr = 1;

/*---------------------------tolua extend functions--------------------------------*/
LUALIB_API void* tolua_tag()
{
	return &tag;
}

LUALIB_API void tolua_newudata(lua_State *L, int val)
{
    //lua_reset(L);
    //lua_set(L);
    // CHECK_VALID_VOID
	int* pointer = (int*)lua_newuserdata(L, sizeof(int));    
    lua_pushvalue(L, TOLUA_NOPEER);            
    lua_setfenv(L, -2);                        
	*pointer = val;
}

LUALIB_API int tolua_rawnetobj(lua_State *L, int index)
{
    // CHECK_VALID(-1)
    int* udata = (int*)lua_touserdata(L, index);
    
    if (udata != NULL) 
    {
        return *udata;
    }
    else if (lua_istable(L, index))
    {
        lua_pushvalue(L, index);        
        lua_pushlightuserdata(L, &vptr);
        lua_rawget(L, -2);                

        if (lua_isuserdata(L, -1))
        {
            lua_replace(L, index);
            udata = (int*)lua_touserdata(L, index);

            if (udata != NULL) 
            {
                return *udata;
            }
        }    
        else
        {
            lua_pop(L, 1);
        }               
    }    

	return -1;
}

LUALIB_API char* tolua_tocbuffer(const char *csBuffer, int sz)
{
     
	char* buffer = (char*)malloc(sz+1);
	memcpy(buffer, csBuffer, sz);	
	buffer[sz] = '\0';
	return buffer;
}

LUALIB_API void tolua_freebuffer(void* buffer)
{
  free(buffer);
}

LUALIB_API void tolua_getvec2(lua_State *L, int pos, float* x, float* y)
{
	lua_getref(L, LUA_RIDX_UNPACKVEC2);
	lua_pushvalue(L, pos);
	lua_call(L, 1, 2);
	*x = (float)lua_tonumber(L, -2);
	*y = (float)lua_tonumber(L, -1);
	lua_pop(L, 2);
}

LUALIB_API void tolua_getvec3(lua_State *L, int pos, float* x, float* y, float* z)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_UNPACKVEC3);
	lua_pushvalue(L, pos);
	lua_call(L, 1, 3);
    
    *x = (float)lua_tonumber(L, -3);
    *y = (float)lua_tonumber(L, -2);
    *z = (float)lua_tonumber(L, -1);
    lua_pop(L, 3);
}

LUALIB_API void tolua_getvec4(lua_State *L, int pos, float* x, float* y, float* z, float* w)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_UNPACKVEC4);
	lua_pushvalue(L, pos);
	lua_call(L, 1, 4);
    
	*x = (float)lua_tonumber(L, -4);
	*y = (float)lua_tonumber(L, -3);
	*z = (float)lua_tonumber(L, -2);
	*w = (float)lua_tonumber(L, -1);
	lua_pop(L, 4);
}

LUALIB_API void tolua_getquat(lua_State *L, int pos, float* x, float* y, float* z, float* w)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_UNPACKQUAT);
	lua_pushvalue(L, pos);
	lua_call(L, 1, 4);
    
	*x = (float)lua_tonumber(L, -4);
	*y = (float)lua_tonumber(L, -3);
	*z = (float)lua_tonumber(L, -2);
	*w = (float)lua_tonumber(L, -1);
	lua_pop(L, 4);
}

LUALIB_API void tolua_getclr(lua_State *L, int pos, float* r, float* g, float* b, float* a)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_UNPACKCLR);
	lua_pushvalue(L, pos);
	lua_call(L, 1, 4);
    
	*r = (float)lua_tonumber(L, -4);
	*g = (float)lua_tonumber(L, -3);
	*b = (float)lua_tonumber(L, -2);
	*a = (float)lua_tonumber(L, -1);
	lua_pop(L, 4);
}

LUALIB_API int tolua_getlayermask(lua_State *L, int pos)
{
    if (lua_isnumber(L, pos))
    {
        return (int)lua_tointeger(L, pos);
    }

    lua_getref(L, LUA_RIDX_UNPACKLAYERMASK);
    lua_pushvalue(L, pos);
    lua_call(L, 1, 1);
    int mask = (int)lua_tointeger(L, -1);
    lua_pop(L, 1);
    return mask;
}

LUALIB_API void tolua_pushvec2(lua_State *L, float x, float y)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_PACKVEC2);
    
	lua_pushnumber(L, x);
	lua_pushnumber(L, y);
	lua_call(L, 2, 1);
}

LUALIB_API void tolua_pushvec3(lua_State *L, float x, float y, float z)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_PACKVEC3);
	lua_pushnumber(L, x);
	lua_pushnumber(L, y);
    
	lua_pushnumber(L, z);
	lua_call(L, 3, 1);
}

LUALIB_API void tolua_pushvec4(lua_State *L, float x, float y, float z, float w)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_PACKVEC4);
	lua_pushnumber(L, x);
	lua_pushnumber(L, y);
    
	lua_pushnumber(L, z);
	lua_pushnumber(L, w);
	lua_call(L, 4, 1);
}

LUALIB_API void tolua_pushquat(lua_State *L, float x, float y, float z, float w)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_PACKQUAT);
	lua_pushnumber(L, x);
	lua_pushnumber(L, y);
    
	lua_pushnumber(L, z);
	lua_pushnumber(L, w);
	lua_call(L, 4, 1);
}

LUALIB_API void tolua_pushclr(lua_State *L, float r, float g, float b, float a)
{
    // CHECK_VALID_VOID
	lua_getref(L, LUA_RIDX_PACKCLR);
	lua_pushnumber(L, r);
	lua_pushnumber(L, g);
    
	lua_pushnumber(L, b);
	lua_pushnumber(L, a);
	lua_call(L, 4, 1);
}

LUALIB_API void tolua_pushlayermask(lua_State *L, int mask)
{
    // CHECK_VALID_VOID
    lua_getref(L, LUA_RIDX_PACKLAYERMASK);
    lua_pushnumber(L, mask);
    
    lua_call(L, 1, 1);
}


LUA_API const char* tolua_tolstring(lua_State *L, int index, int* len) 
{
    size_t sz;
    const char *ret = lua_tolstring(L, index, &sz);
    *len = (int)sz;
    return ret;
}

LUA_API void tolua_pushlstring(lua_State *L, const char *s, int l)
{
    lua_pushlstring(L, s, (size_t)l);
}

LUA_API void* tolua_newuserdata(lua_State *L, int sz)
{
    return lua_newuserdata(L, (size_t)sz);    
}

LUA_API int tolua_objlen(lua_State *L, int idx)
{
    size_t len = lua_objlen(L, idx);
    return (int)len;
}

LUA_API bool tolua_toboolean(lua_State *L, int idx) 
{
    int value = lua_toboolean(L, idx);
    return value == 0 ? false : true;
}

LUA_API int32_t tolua_tointeger(lua_State *L, int idx) 
{
    return (int32_t)lua_tointeger(L, idx);
}

LUALIB_API int tolua_loadbuffer(lua_State *L, const char *buff, int sz, const char *name)
{
    return luaL_loadbuffer(L, buff, (size_t)sz, name);
}

static int _lua_getfield(lua_State *L)
{
    const char *name = lua_tostring(L, 2);    
    lua_getfield(L, 1, name);    
    return 1;
}
        
LUA_API int tolua_getfield(lua_State *L, int idx, const char *field)
{
    idx = abs_index(L, idx);    
    lua_pushcfunction(L, _lua_getfield);
    lua_pushvalue(L, idx);
    lua_pushstring(L, field);
    return lua_pcall(L, 2, 1, 0);
}

static int _lua_setfield(lua_State *L)
{
    const char *name = lua_tostring(L, 2);
    lua_setfield(L, 1, name);
    return 0;
}

LUA_API int tolua_setfield(lua_State *L, int idx, const char *key)        
{
    int top = lua_gettop(L);
    idx = abs_index(L, idx);
    lua_pushcfunction(L, _lua_setfield);
    lua_pushvalue(L, idx);
    lua_pushstring(L, key);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    return lua_pcall(L, 3, -1, 0);
}

static int _lua_gettable(lua_State *L)
{    
    lua_gettable(L, 1);    
    return 1;
}

LUA_API int tolua_gettable(lua_State *L, int idx)
{
    int top = lua_gettop(L);
    idx = abs_index(L, idx);
    lua_pushcfunction(L, _lua_gettable);
    lua_pushvalue(L, idx);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    return lua_pcall(L, 2, -1, 0);
}

static int _lua_settable(lua_State *L)
{
    lua_settable(L, 1);
    return 0;
}

LUA_API int tolua_settable(lua_State *L, int idx)
{
    int top = lua_gettop(L);
    idx = abs_index(L, idx);
    lua_pushcfunction(L, _lua_settable);
    lua_pushvalue(L, idx);
    lua_pushvalue(L, top - 1);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    lua_remove(L, top - 1);
    return lua_pcall(L, 3, -1, 0);
}

static int tolua_closure(lua_State *L)
{
    lua_CFunction fn = (lua_CFunction)lua_tocfunction(L, lua_upvalueindex(2));
    int r = fn(L);    
    
    if (lua_toboolean(L, lua_upvalueindex(1)))
    {
        lua_pushboolean(L, 0);
        lua_replace(L, lua_upvalueindex(1));
        return lua_error(L);
    }
    
    return r;
}

//hack for luac, 避免luac error破坏包裹c#函数的异常块(luajit采用的是类似c++异常)
LUA_API int tolua_pushcfunction(lua_State *L, lua_CFunction fn)
{        
    lua_pushboolean(L, 0);
    lua_pushcfunction(L, fn);
    lua_pushcclosure(L, tolua_closure, 2);
    return 0;
}

static int tolua_pusherror(lua_State *L, const char *fmt, ...)
{
    va_list argp;
    va_start(argp, fmt);
    luaL_where(L, 1);
    lua_pushvfstring(L, fmt, argp);
    va_end(argp);
    lua_concat(L, 2);
    return 1;
}

LUALIB_API int tolua_argerror(lua_State *L, int narg, const char *extramsg)
{
    lua_Debug ar;
    
    if (!lua_getstack(L, 0, &ar))  /* no stack frame? */
    {
        return tolua_pusherror(L, "bad argument #%d (%s)", narg, extramsg);        
    }

    lua_getinfo(L, "n", &ar);

    if (strcmp(ar.namewhat, "method") == 0) 
    {
        narg--;  /* do not count `self' */

        if (narg == 0)  /* error is in the self argument itself? */
        {
            return tolua_pusherror(L, "calling " LUA_QS " on bad self (%s)", ar.name, extramsg);
        }
    }

    if (ar.name == NULL)
    {
        ar.name = "?";
    }    

    return tolua_pusherror(L, "bad argument #%d to " LUA_QS " (%s)", narg, ar.name, extramsg);
}

LUALIB_API int tolua_error(lua_State *L, const char *msg)
{
    lua_pushboolean(L, 1);
    lua_replace(L, lua_upvalueindex(1));
    lua_pushstring(L, msg);
    return 1;
}

LUALIB_API int tolua_getn(lua_State *L, int i)
{
    return tolua_objlen(L, i);
}

LUALIB_API int tolua_strlen(const char *str)
{
    if (str == NULL)
    {
        return 0;
    }

    int len = (int)strlen(str);
    return len;
}

static bool _preload(lua_State *L)
{    
    lua_settop(L, 2); 
    lua_getmetatable(L, 1);
    lua_pushstring(L, ".name");             //stack: t key mt ".name"
    lua_rawget(L, -2);                      //stack: t key mt space

    if (!lua_isnil(L, -1))                  //stack: t key mt space
    {                      
        lua_getref(L, LUA_RIDX_PRELOAD);    //stack: t key mt space preload
        lua_pushvalue(L, -2);               //stack: t key mt space preload space
        lua_pushstring(L, ".");             //stack: t key mt space preload space.
        lua_pushvalue(L, 2);                //stack: t key mt space preload space.key
        lua_concat(L, 3);                   //stack: t key mt space preload key1
        lua_pushvalue(L, -1);               //stack: t key mt space preload key1 key1
        lua_rawget(L, -3);                  //stack: t key mt space preload key1 value

        if (!lua_isnil(L, -1)) 
        {      
            lua_pop(L, 1);                      //stack: t key mt space preload key1
            lua_getref(L, LUA_RIDX_REQUIRE);    //stack: t key mt space preload key1 require
            lua_pushvalue(L, -2);
            lua_call(L, 1, 1);                         
            return true;
        }        
    }

    lua_settop(L, 2); 
    return false;
}

static int class_index_event(lua_State *L)
{
	int t = lua_type(L, 1);

    if (t == LUA_TUSERDATA)
    {    	
        lua_getfenv(L,1);

        if (!lua_rawequal(L, -1, TOLUA_NOPEER))     // stack: t k env
        {
            while (lua_istable(L, -1))                       // stack: t k v mt 
            {      
                lua_pushvalue(L, 2); 
                lua_rawget(L, -2);
            
                if (!lua_isnil(L, -1))
                {                    
                    return 1;
                }

                lua_pop(L, 1);
                lua_pushlightuserdata(L, &gettag);          
                lua_rawget(L, -2);                      //stack: obj key env tget
            
                if (lua_istable(L, -1))
                {                    
                    lua_pushvalue(L, 2);                //stack: obj key env tget key
                    lua_rawget(L, -2);                  //stack: obj key env tget func 

                    if (lua_isfunction(L, -1))
                    {                        
                        lua_pushvalue(L, 1);
                        lua_call(L, 1, 1);
                        return 1;
                    }    

                    lua_pop(L, 1);                 
                }

                lua_pop(L, 1); 

                if (lua_getmetatable(L, -1) == 0)               // stack: t k v mt mt
                {
                    lua_pushnil(L);
                }

                lua_remove(L, -2);                              // stack: t k v mt
            }        
        };

        lua_settop(L,2);                                        				
    	lua_pushvalue(L, 1);						// stack: obj key obj	

    	while (lua_getmetatable(L, -1) != 0)
    	{        	
        	lua_remove(L, -2);						// stack: obj key mt

			if (lua_isnumber(L,2))                 	// check if key is a numeric value
			{		    
		    	lua_pushstring(L,".geti");
		    	lua_rawget(L,-2);                   // stack: obj key mt func

		    	if (lua_isfunction(L,-1))
		    	{
		        	lua_pushvalue(L,1);
		        	lua_pushvalue(L,2);
		        	lua_call(L,2,1);
		        	return 1;
		    	}
			}
			else
        	{
        		lua_pushvalue(L, 2);			    // stack: obj key mt key
        		lua_rawget(L, -2);					// stack: obj key mt value        

        		if (!lua_isnil(L, -1))
        		{
        	    	return 1;
        		}
                
                lua_pop(L, 1);
				lua_pushlightuserdata(L, &gettag);        	
        		lua_rawget(L, -2);					//stack: obj key mt tget

        		if (lua_istable(L, -1))
        		{
        	    	lua_pushvalue(L, 2);			//stack: obj key mt tget key
        	    	lua_rawget(L, -2);           	//stack: obj key mt tget value 

        	    	if (lua_isfunction(L, -1))
        	    	{
        	        	lua_pushvalue(L, 1);
        	        	lua_call(L, 1, 1);
        	        	return 1;
        	    	}                    
        		}
    		}

            lua_settop(L, 3);
        }

        lua_settop(L, 2);
        int *udata = (int*)lua_touserdata(L, 1);

        if (*udata == LUA_NULL_USERDATA)
        {
            return luaL_error(L, "attemp to index %s on a nil value", lua_tostring(L, 2));   
        }
        
        if (toluaflags & FLAG_INDEX_ERROR)
        {
            return luaL_error(L, "field or property %s does not exist", lua_tostring(L, 2));
        }        
    }
    else if(t == LUA_TTABLE)
    {
    	lua_pushvalue(L,1);                          //stack: obj key obj

		while (lua_getmetatable(L, -1) != 0)         //stack: obj key obj mt
    	{
        	lua_remove(L, -2);						// stack: obj key mt

        	lua_pushvalue(L, 2);			    	// stack: obj key mt key
        	lua_rawget(L, -2);						// stack: obj key mt value        

        	if (!lua_isnil(L, -1))
        	{
        		if (lua_isfunction(L, -1))			//cache static function
        		{
        			lua_pushvalue(L, 2);           // stack: obj key mt value key
        			lua_pushvalue(L, -2);          // stack: obj key mt value key value
        			lua_rawset(L, 1);
        		}

        	    return 1;
        	}        	
        	
            lua_pop(L, 1);        
			lua_pushlightuserdata(L, &gettag);        	
        	lua_rawget(L, -2);						//stack: obj key mt tget

        	if (lua_istable(L, -1))
        	{
        	    lua_pushvalue(L, 2);				//stack: obj key mt tget key
        	    lua_rawget(L, -2);           		//stack: obj key mt tget value 

        	    if (lua_isfunction(L, -1))
        	    {        	        	
                    lua_pushvalue(L, 1);
        	        lua_call(L, 1, 1);
        	        return 1;
        	    }
        	}
    		
        	lua_settop(L, 3);
    	}        
        
        if (_preload(L))
        {
            return 1;
        }          
        
        if (toluaflags & FLAG_INDEX_ERROR)
        {
            return luaL_error(L, "field or property %s does not exist", lua_tostring(L, 2));               
        }      
    }

    lua_pushnil(L);
    return 1;
}

static void storeatubox (lua_State *L, int lo)
{
    lua_getfenv(L, lo);                         // stack: t, k, v, _env

    if (lua_rawequal(L, -1, TOLUA_NOPEER)) 
    {
        lua_pop(L, 1);
        return;
        //lua_newtable(L);                        // stack: t, k, v, t
        //lua_pushvalue(L, -1);                   // stack: t, k, v, t, t        
        //lua_setfenv(L, lo);                     // stack: t, k, v, t
    };

    lua_insert(L, -3);
    lua_settable(L, -3);
    lua_pop(L, 1);
}

static int class_newindex_event(lua_State *L)
{
	int t = lua_type(L, 1);

	if (t == LUA_TUSERDATA)
    {
        bool useEnv = false;
        lua_getfenv(L, 1);

        if (!lua_rawequal(L, -1, TOLUA_NOPEER)) 
        {             
            useEnv = true;

            while (lua_istable(L, -1))                       // stack: t k v mt 
            {       
                lua_pushvalue(L, 2);                        // stack: t k v mt k
                lua_rawget(L, -2);                          // stack: t k v mt value        

                if (!lua_isnil(L, -1))
                {                    
                    lua_pop(L, 1);
                    lua_insert(L, -3);
                    lua_rawset(L, -3);
                    return 0;
                }

                lua_pop(L, 1);                
                lua_pushlightuserdata(L, &settag);              // stack: t k v mt tset
                lua_rawget(L, -2);                   

                if (lua_istable(L, -1)) 
                {
                    lua_pushvalue(L, 2);                         // stack: t k v mt tset k
                    lua_rawget(L, -2);                    

                    if (lua_isfunction(L, -1)) 
                    {                          
                        lua_pushvalue(L, 1); 
                        lua_pushvalue(L, 3); 
                        lua_call(L, 2, 0);
                        return 0;
                    }

                    lua_pop(L, 1);                              // stack: t k v mt tset 
                }  

                lua_pop(L, 1);                                  // stack: t k v mt

                if (lua_getmetatable(L, -1) == 0)               // stack: t k v mt mt
                {
                    lua_pushnil(L);
                }

                lua_remove(L, -2);                              // stack: t k v mt
            }              
        }

        lua_settop(L, 3);
    	lua_getmetatable(L,1);

    	while (lua_istable(L, -1))                			// stack: t k v mt
    	{        	
    		if (lua_isnumber(L, 2))
    		{
				lua_pushstring(L,".seti");
                lua_rawget(L,-2);                      		// stack: obj key mt func

                if (lua_isfunction(L,-1))
                {
                    lua_pushvalue(L,1);
                    lua_pushvalue(L,2);
                    lua_pushvalue(L,3);
                    lua_call(L,3,0);                    
                }

                return 0;
    		}
    		else
        	{
        		lua_pushlightuserdata(L, &settag);
        		lua_rawget(L, -2);                      	// stack: t k v mt tset

        		if (lua_istable(L, -1))
        		{
            		lua_pushvalue(L, 2);
            		lua_rawget(L, -2);                     	// stack: t k v mt tset func

            		if (lua_isfunction(L, -1))
            		{
                		lua_pushvalue(L, 1);
                		lua_pushvalue(L, 3);
                		lua_call(L, 2, 0);
                		return 0;
            		}

            		lua_pop(L, 1);                          // stack: t k v mt tset 
        		}

        		lua_pop(L, 1);                              // stack: t k v mt 

        		if (lua_getmetatable(L, -1) == 0)           // stack: t k v mt mt
        		{
            		lua_pushnil(L);
        		}

        		lua_remove(L, -2);                          // stack: t k v mt 
        	}
    	}

        lua_settop(L, 3);                                       // stack: t k v
        int* udata = (int*)lua_touserdata(L, 1);

        if (*udata == LUA_NULL_USERDATA)
        {
            return luaL_error(L, "attemp to index %s on a nil value", lua_tostring(L, 2));   
        }        

        if (useEnv) 
        {
            lua_getfenv(L, 1);                         // stack: t, k, v, _env
            lua_insert(L, -3);
            lua_settable(L, -3);
            lua_pop(L, 1);
            return 0;
        }                       
	}
	else if (t == LUA_TTABLE)
	{
		lua_getmetatable(L, 1);								// stack: t k v mt 

		while (lua_istable(L, -1))                			// stack: t k v mt 
    	{  		
			lua_pushlightuserdata(L, &settag);				// stack: t k v mt tset
        	lua_rawget(L, -2);       

        	if (lua_istable(L,-1)) 
        	{
            	lua_pushvalue(L,2);  						// stack: t k v mt tset k
            	lua_rawget(L,-2);

            	if (lua_isfunction(L,-1)) 
            	{  
                	lua_pushvalue(L,1); 
                	lua_pushvalue(L,3); 
                	lua_call(L,2,0);
                	return 0;
            	}

            	lua_pop(L, 1);                          	// stack: t k v mt tset 
        	}  

			lua_pop(L, 1);                              	// stack: t k v mt

        	if (lua_getmetatable(L, -1) == 0)           	// stack: t k v mt mt
        	{
            	lua_pushnil(L);
        	}

        	lua_remove(L, -2);                          	// stack: t k v mt
        }      
	}

    lua_settop(L, 3); 
    return luaL_error(L, "field or property %s does not exist", lua_tostring(L, 2));      
}

static int enum_index_event (lua_State *L)
{
	lua_getmetatable(L, 1);									//stack: t, k, mt

	if (lua_istable(L, -1))
	{
		lua_pushvalue(L, 2);								//stack: t, k, mt, k
		lua_rawget(L, -2);									//stack: t, k, mt, v

		if (!lua_isnil(L, -1))
		{		
			return 1;
		}

		lua_pop(L, 1);										//stack: t, k, mt
		lua_pushlightuserdata(L, &gettag);		
		lua_rawget(L, -2); 									//stack: t, k, mt, tget

		if (lua_istable(L,-1)) 
		{
            lua_pushvalue(L,2);  							//stack: t k mt tget k
            lua_rawget(L,-2);								//stack: t k mt tget v

           	if (lua_isfunction(L,-1)) 
			{  
				lua_call(L, 0, 1);					
				lua_pushvalue(L,2); 				
				lua_pushvalue(L,-2); 				
				lua_rawset(L, 3);		
				return 1;			
			}

			lua_pop(L, 1);
        }			
	}

	lua_settop(L, 2);
	lua_pushnil(L);
	return 1;	
}

static int enum_newindex_event(lua_State *L)
{	
	luaL_error(L, "the left-hand side of an assignment must be a variable, a property or an indexer");
    return 1;
}

static int static_index_event(lua_State *L)
{    
    lua_pushvalue(L, 2);                    //stack: t key key
    lua_rawget(L, 1);                       //stack: t key value        

    if (!lua_isnil(L, -1))
    {
        return 1;
    }

    lua_pop(L, 1);            
    lua_pushlightuserdata(L, &gettag);      //stack: t key tag    
    lua_rawget(L, 1);                       //stack: t key tget

    if (lua_istable(L, -1))
    {
        lua_pushvalue(L, 2);                //stack: obj key tget key
        lua_rawget(L, -2);                  //stack: obj key tget func 

        if (lua_isfunction(L, -1))
        {                       
            lua_call(L, 0, 1);
            return 1;
        }        
    }
    
    lua_settop(L, 2);

    if (_preload(L))
    {
        return 1;
    }
    
    if (toluaflags & FLAG_INDEX_ERROR)
    {
        luaL_error(L, "field or property %s does not exist", lua_tostring(L, 2));    
    }

    return 1;
}

static int static_newindex_event(lua_State *L)
{
    lua_pushlightuserdata(L, &settag);              //stack: t k v tag
    lua_rawget(L, 1);                               //stack: t k v tset

    if (lua_istable(L,-1)) 
    {
        lua_pushvalue(L, 2);                         //stack: t k v tset k
        lua_rawget(L, -2);                           //stack: t k v tset func

        if (lua_isfunction(L, -1))
        {    
            lua_pushvalue(L,1); 
            lua_pushvalue(L,3); 
            lua_call(L,2,0);
            return 0;
        }
    }

    lua_settop(L, 3); 
    luaL_error(L, "field or property %s does not exist", lua_tostring(L, 2));
    return 1;
}

static int vptr_index_event(lua_State *L)
{    
    lua_pushlightuserdata(L, &vptr);
    lua_rawget(L, 1);                                   // stack: t key u
    lua_replace(L, 1);                                  // stack: u key
    lua_pushvalue(L, 1);                                // stack: u key u

    while (lua_getmetatable(L, -1) != 0)
    {
        lua_remove(L, -2);                              // stack: u key mt
        lua_pushvalue(L, 2);                            // stack: u key mt key
        lua_rawget(L, -2);                              // stack: u key mt value        

        if (!lua_isnil(L, -1))
        {
            return 1;
        }
        
        lua_pop(L, 1);        
        lua_pushlightuserdata(L, &gettag);          
        lua_rawget(L, -2);                              //stack: u key mt tget

        if (lua_istable(L, -1))
        {
            lua_pushvalue(L, 2);                        //stack: obj key mt tget key
            lua_rawget(L, -2);                          //stack: obj key mt tget value 

            if (lua_isfunction(L, -1))
            {
                lua_pushvalue(L, 1);
                lua_call(L, 1, 1);
                return 1;
            }
        }

        lua_settop(L, 3);
    }

    lua_settop(L, 2);
    lua_pushnil(L);
    return 1;
}

static int vptr_newindex_event(lua_State *L)
{    
    lua_pushlightuserdata(L, &vptr);
    lua_rawget(L, 1);                                   // stack: t key v u        
    lua_getmetatable(L, -1);

    while (lua_istable(L, -1))                          // stack: u k v mt
    {           
        lua_pushlightuserdata(L, &settag);
        lua_rawget(L, -2);                              // stack: u k v mt tset

        if (lua_istable(L, -1))
        {
            lua_pushvalue(L, 2);
            lua_rawget(L, -2);                          // stack: u k v mt tset func

            if (lua_isfunction(L, -1))
            {
                lua_pushvalue(L, 4);
                lua_pushvalue(L, 3);
                lua_call(L, 2, 0);
                return 0;
            }

            lua_pop(L, 1);                              // stack: t k v mt tset 
        }

        lua_pop(L, 1);                                  // stack: t k v mt 

        if (lua_getmetatable(L, -1) == 0)               // stack: t k v mt mt
        {
            lua_pushnil(L);
        }

        lua_remove(L, -2);                              // stack: t k v mt             
    }

    lua_settop(L, 3);    
    return 1;
}

LUALIB_API bool tolua_isvptrtable(lua_State *L, int index)
{    
    lua_pushlightuserdata(L, &vptr);
    lua_rawget(L, index);
    bool flag = lua_isnil(L, -1) ? false : true;
    lua_pop(L, 1);
    return flag;
}

LUALIB_API void tolua_setindex(lua_State *L)
{
	lua_pushstring(L, "__index");
	lua_pushcfunction(L, class_index_event);
	lua_rawset(L, -3);
}

LUALIB_API void tolua_setnewindex(lua_State *L)
{
	lua_pushstring(L, "__newindex");
	lua_pushcfunction(L, class_newindex_event);
	lua_rawset(L, -3);
}

LUALIB_API bool tolua_pushudata(lua_State *L, int index)
{
	lua_getref(L, LUA_RIDX_UBOX);			// stack: ubox
	lua_rawgeti(L, -1, index); 				// stack: ubox, obj

	if (!lua_isnil(L, -1))
	{
		lua_remove(L, -2); 					// stack: obj
		return true;
	}

	lua_pop(L, 2);
	return false;
}

LUALIB_API void tolua_pushnewudata(lua_State *L, int metaRef, int index)
{
	lua_getref(L, LUA_RIDX_UBOX);
	tolua_newudata(L, index);
	lua_getref(L, metaRef);
	lua_setmetatable(L, -2);
	lua_pushvalue(L, -1);
	lua_rawseti(L, -3, index);
	lua_remove(L, -2);	
}
void printTable(lua_State *L,int pos)
{
    if(lua_isnil(L,pos))
        {
            TRY_LOG("printtable is nil pos:%d",pos);
            return;
        }
    lua_pushnil(L);
    if(pos<0)
        pos--; 
    while (lua_next(L, pos)) {
        TRY_LOG("printTable key:%s,value:%d",lua_tostring(L,-2),lua_type(L,-1));
        /* 此时栈上 -1 处为 value, -2 处为 key */
        lua_pop(L, 1);
    }
    lua_pop(L, 1);

}
static int module_index_event(lua_State *L)
{    
    
    lua_pushvalue(L, 2);                    //stack: t key key
    lua_rawget(L, 1);                       //stack: t key value            
    //TRY_LOG("module_index_event key:%s",lua_tostring(L,-2));
    if (!lua_isnil(L, -1))
    {
        return 1;
    }

    lua_pop(L, 1);                          //stack: t key 
    lua_pushstring(L, ".name");             //stack: t key ".name"
    lua_rawget(L, 1);        
    
    if (!lua_isnil(L, -1))                  //stack: t key space
    {               
       lua_getref(L, LUA_RIDX_PRELOAD);    //stack: t key space preload

        // lua_pushnil(L);
        // while (lua_next(L, -2)) {
        //     TRY_LOG("module_index_event key:%s,value:%d",lua_tostring(L,-2),lua_type(L,-1));
        //     /* 此时栈上 -1 处为 value, -2 处为 key */
        //     lua_pop(L, 1);
        // }
        // lua_pop(L, 1);
        lua_pushvalue(L, -2);
        lua_pushstring(L, ".");
        lua_pushvalue(L, 2);
        lua_concat(L, 3);                   //stack: t key space preload key
        lua_pushvalue(L, -1);               //stack: t key space preload key1 key1
        lua_rawget(L, -3);                  //stack: t key space preload key1 value        
        //TRY_LOG("module_index_event key:%s,value:%d,key1:%s,value:%d",lua_tostring(L,2),lua_type(L,1),lua_tostring(L,-2),lua_type(L,-1));        
        
        if (!lua_isnil(L, -1)) 
        {      
            lua_pop(L, 1);                      //stack: t key space preload key1
            lua_getref(L, LUA_RIDX_REQUIRE);
            lua_pushvalue(L, -2);
            lua_call(L, 1, 1);                    
        }
        else
        {
            lua_pushnil(L);                            
        }
    }
    
    return 1;
}

typedef struct stringbuffer 
{        
  const char *buffer;
  size_t len;
} stringbuffer;

static stringbuffer sb;

void initmodulebuffer()
{
    sb.len = 0;
    sb.buffer = NULL;
}

void pushmodule(lua_State *L, const char *str)
{    
    luaL_Buffer b;
    luaL_buffinit(L, &b);

    if (sb.len > 0)
    {
        luaL_addlstring(&b, sb.buffer, sb.len);
        luaL_addchar(&b, '.');
    }

    luaL_addstring(&b, str);
    luaL_pushresult(&b);    
    sb.buffer = lua_tolstring(L, -1, &sb.len);    
}

LUALIB_API bool tolua_beginmodule(lua_State *L, const char *name)
{
    if (name != NULL)
    {                
        lua_pushstring(L, name);			//stack key
        lua_rawget(L, -2);					//stack value

        if (lua_isnil(L, -1))  
        {
            lua_pop(L, 1);
            lua_newtable(L);				//stack table

            lua_pushstring(L, "__index");
            lua_pushcfunction(L, module_index_event);
            lua_rawset(L, -3);

            lua_pushstring(L, name);        //stack table name         
            lua_pushstring(L, ".name");     //stack table name ".name"            
            pushmodule(L, name);            //stack table name ".name" module            
            lua_rawset(L, -4);              //stack table name            
            lua_pushvalue(L, -2);			//stack table name table
            lua_rawset(L, -4);   			//stack table

            lua_pushvalue(L, -1);
            lua_setmetatable(L, -2);
            return true;
        }
        else if (lua_istable(L, -1))
        {
            if (lua_getmetatable(L, -1) == 0)
            {
                lua_pushstring(L, "__index");
                lua_pushcfunction(L, module_index_event);
                lua_rawset(L, -3);

                lua_pushstring(L, name);        //stack table name         
                lua_pushstring(L, ".name");     //stack table name ".name"            
                pushmodule(L, name);            //stack table name ".name" module            
                lua_rawset(L, -4);              //stack table name            
                lua_pushvalue(L, -2);           //stack table name table
                lua_rawset(L, -4);              //stack table

                lua_pushvalue(L, -1);
                lua_setmetatable(L, -2);                    
            }
            else
            {
                lua_pushstring(L, ".name");
                lua_gettable(L, -3);      
                sb.buffer = lua_tolstring(L, -1, &sb.len);                    
                lua_pop(L, 2);
            }

            return true;
        }

        return false;
    }
    else
    {                
        lua_pushvalue(L, LUA_GLOBALSINDEX);
        return true;
    }                
}

LUALIB_API void tolua_endmodule(lua_State *L)
{
    lua_pop(L, 1);
    int len = (int)sb.len;

    while(len-- >= 0)
    {
        if (sb.buffer[len] == '.')
        {
            sb.len = len;
            return;
        }
    }

    sb.len = 0;
}

static int class_new_event(lua_State *L)
{         
    if (!lua_istable(L, 1))
    {
        return luaL_typerror(L, 1, "table");        
    }

    int count = lua_gettop(L); 
    lua_pushvalue(L,1);  

    if (lua_getmetatable(L,-1))
    {   
        lua_remove(L,-2);                      
        lua_pushstring(L, "New");               
        lua_rawget(L,-2);    

        if (lua_isfunction(L,-1))
        {            
            for (int i = 2; i <= count; i++)
            {
                lua_pushvalue(L, i);                    
            }

            lua_call(L, count - 1, 1);
            return 1;
        }

        lua_settop(L,3);
    }    

    return luaL_error(L,"attempt to perform ctor operation failed");    
}

static void _pushfullname(lua_State *L, int pos)
{
    if (sb.len > 0)
    {
        lua_pushlstring(L, sb.buffer, sb.len);
        lua_pushstring(L, ".");
        lua_pushvalue(L,  pos < 0 ? pos - 2 : pos + 2);
        lua_concat(L, 3);
    }
    else
    {
        lua_pushvalue(L, pos);
    }
}

static void _addtoloaded(lua_State *L)
{
    lua_getref(L, LUA_RIDX_LOADED);
    _pushfullname(L, -3);
    lua_pushvalue(L, -3);
    lua_rawset(L, -3);
    lua_pop(L, 1);
}

LUALIB_API int tolua_beginclass(lua_State *L, const char *name, int baseType, int ref)
{
    int reference = ref;
    lua_pushstring(L, name);                
    lua_newtable(L);      
    _addtoloaded(L);

    if (ref == LUA_REFNIL)        
    {
        lua_newtable(L);
        lua_pushvalue(L, -1);
        reference = luaL_ref(L, LUA_REGISTRYINDEX); 
    }
    else
    {
        lua_getref(L, reference);    
    }

    if (baseType != 0)
    {
        lua_getref(L, baseType);        
        lua_setmetatable(L, -2);
    }
           
    lua_pushlightuserdata(L, &tag);
    lua_pushnumber(L, 1);
    lua_rawset(L, -3);

    lua_pushstring(L, ".name");
    _pushfullname(L, -4);
    lua_rawset(L, -3);

    lua_pushstring(L, ".ref");
    lua_pushinteger(L, reference);
    lua_rawset(L, -3);

    lua_pushstring(L, "__call");
    lua_pushcfunction(L, class_new_event);
    lua_rawset(L, -3);

    tolua_setindex(L);
    tolua_setnewindex(L); 
    return reference;
}


LUALIB_API void tolua_endclass(lua_State *L)
{
	lua_setmetatable(L, -2);
    lua_rawset(L, -3);            
}

/*void settablename(lua_State *L, const char *label, const char *name)
{
    lua_pushstring(L, "name");
    char cname[128];
    int l1 = strlen(label);
    int l2 = strlen(name)    ;
    strncat(cname, label, 128 - l1);
    strncat(cname, name, 128 - l1 - l2);
    lua_pushstring(L, cname);
    lua_rawset(L, -3);
}*/

LUALIB_API int tolua_beginenum(lua_State *L, const char *name)
{
	lua_pushstring(L, name);                               
    lua_newtable(L);                                       
    _addtoloaded(L);
    lua_newtable(L);
    lua_pushvalue(L, -1);
    int reference = luaL_ref(L, LUA_REGISTRYINDEX);            
    lua_pushlightuserdata(L, &tag);
    lua_pushnumber(L, 1);
    lua_rawset(L, -3);

    lua_pushstring(L, ".name");
    _pushfullname(L, -4);  
    lua_rawset(L, -3);

	lua_pushstring(L, "__index");
	lua_pushcfunction(L, enum_index_event);
	lua_rawset(L, -3);

	lua_pushstring(L, "__newindex");
	lua_pushcfunction(L, enum_newindex_event);
	lua_rawset(L, -3);	

	return reference;
}

LUALIB_API void tolua_endenum(lua_State *L)
{
	lua_setmetatable(L, -2);
    lua_rawset(L, -3);    
}

LUALIB_API void tolua_beginstaticclass(lua_State *L, const char *name)
{    
    lua_pushstring(L, name);  
    lua_newtable(L);
    _addtoloaded(L);
    lua_pushvalue(L, -1);

    lua_pushlightuserdata(L, &tag);
    lua_pushnumber(L, 1);
    lua_rawset(L, -3);

    lua_pushstring(L, ".name");
    _pushfullname(L, -4);
    lua_rawset(L, -3);

    lua_pushstring(L, "__index");
    lua_pushcfunction(L, static_index_event);
    lua_rawset(L, -3);

    lua_pushstring(L, "__newindex");
    lua_pushcfunction(L, static_newindex_event);
    lua_rawset(L, -3);      
}

LUALIB_API void tolua_endstaticclass(lua_State *L)
{
    lua_setmetatable(L, -2);
    lua_rawset(L, -3);    
}

LUALIB_API void tolua_constant(lua_State *L, const char *name, double value)
{
    lua_pushstring(L, name);
    lua_pushnumber(L, value);
    lua_rawset(L,-3);
}

LUALIB_API void tolua_function(lua_State *L, const char *name, lua_CFunction fn)
{
  	lua_pushstring(L, name);
    tolua_pushcfunction(L, fn);
  	lua_rawset(L, -3);

    /*lua_pushstring(L, name);
    lua_pushcfunction(L, fn);
    lua_rawset(L, -3);*/
}

LUALIB_API void tolua_variable(lua_State *L, const char *name, lua_CFunction get, lua_CFunction set)
{                
    lua_pushlightuserdata(L, &gettag);
    lua_rawget(L, -2);

    if (!lua_istable(L, -1))
    {
        /* create .get table, leaving it at the top */
        lua_pop(L, 1);
        lua_newtable(L);        
        lua_pushlightuserdata(L, &gettag);
        lua_pushvalue(L, -2);
        lua_rawset(L, -4);
    }

    lua_pushstring(L, name);
    //lua_pushcfunction(L, get);
    tolua_pushcfunction(L, get);
    lua_rawset(L, -3);                  /* store variable */
    lua_pop(L, 1);                      /* pop .get table */

    /* set func */
    if (set != NULL)
    {        
        lua_pushlightuserdata(L, &settag);
        lua_rawget(L, -2);

        if (!lua_istable(L, -1))
        {
            /* create .set table, leaving it at the top */
            lua_pop(L, 1);
            lua_newtable(L);            
            lua_pushlightuserdata(L, &settag);
            lua_pushvalue(L, -2);
            lua_rawset(L, -4);
        }

        lua_pushstring(L, name);
        //lua_pushcfunction(L, set);
        tolua_pushcfunction(L, set);
        lua_rawset(L, -3);                  /* store variable */
        lua_pop(L, 1);                      /* pop .set table */
    }
}

LUALIB_API int toluaL_ref(lua_State *L)
{
	int stackPos = abs_index(L, -1);	
	lua_getref(L, LUA_RIDX_FIXEDMAP);
	lua_pushvalue(L, stackPos);
	lua_rawget(L, -2);

	if (!lua_isnil(L, -1))
	{
		int ref = (int)lua_tointeger(L, -1);
		lua_pop(L, 3);
		return ref;
	}
	else
	{
		lua_pushvalue(L, stackPos);
		int ref = luaL_ref(L, LUA_REGISTRYINDEX);
		lua_pushvalue(L, stackPos);
		lua_pushinteger(L, ref);
		lua_rawset(L, -4);
		lua_pop(L, 3);
		return ref;
	}
}

LUALIB_API void toluaL_unref(lua_State *L, int reference)
{
	lua_getref(L, LUA_RIDX_FIXEDMAP);
	lua_getref(L, reference);
	lua_pushnil(L);
	lua_rawset(L, -3);
	luaL_unref(L, LUA_REGISTRYINDEX, reference);
	lua_pop(L, 1);
}

LUA_API lua_State* tolua_getmainstate(lua_State *L1)
{
	lua_rawgeti(L1, LUA_REGISTRYINDEX, LUA_RIDX_MAINTHREAD);
	lua_State *L = lua_tothread(L1, -1);
	lua_pop(L1, 1);
	return L;
}

LUA_API int tolua_getvaluetype(lua_State *L, int stackPos)
{
	stackPos = abs_index(L, stackPos);
	lua_getref(L, LUA_RIDX_CHECKVALUE);
	lua_pushvalue(L, stackPos);
	lua_call(L, 1, 1);
	int ret = (int)lua_tonumber(L, -1);
	lua_pop(L, 1);
	return ret;
}

LUALIB_API bool tolua_createtable(lua_State *L, const char *path, int szhint)
{
	const char *e = NULL;
	lua_pushvalue(L, LUA_GLOBALSINDEX);						//stack _G

	do 
	{
	  e = strchr(path, '.');
	  if (e == NULL) e = path + strlen(path);
	  lua_pushlstring(L, path, e - path);					//stack _G key
	  lua_rawget(L, -2);									//stack _G value
	  int type = lua_type(L, -1);

	  if (type == LUA_TNIL) 
	  {  
	    lua_pop(L, 1); 										//stack _G
	    lua_createtable(L, 0, (*e == '.' ? 1 : szhint));	//stack _G table
	    lua_pushlstring(L, path, e - path);					//stack _G table name
	    lua_pushvalue(L, -2);								//stack _G table name table
	    lua_settable(L, -4);  								//stack _G table
	  }
	  else if (type != LUA_TTABLE) 
	  {  
	    lua_pop(L, 2);  
	    return false;  
	  }

	  lua_remove(L, -2);  									//stack table
	  path = e + 1;
	} while (*e == '.');

	return true;
}

LUALIB_API bool tolua_beginpremodule(lua_State *L, const char *path, int szhint)
{
    const char *e = NULL;
    const char *name = path;
    lua_pushvalue(L, LUA_GLOBALSINDEX);                     //stack _G

    do 
    {
        e = strchr(path, '.');
        if (e == NULL) e = path + strlen(path);
        lua_pushlstring(L, path, e - path);                   //stack t key
        lua_rawget(L, -2);                                    //stack t value
        int type = lua_type(L, -1);

        if (type == LUA_TNIL) 
        {
            lua_pop(L, 1);                                      //stack t
            lua_createtable(L, 0, (*e == '.' ? 1 : szhint));    //stack t table
            lua_pushlstring(L, path, e - path);                 //stack t table name
            lua_pushvalue(L, -2);                               //stack t table name table
            lua_settable(L, -4);                                //stack t table

            lua_pushstring(L, ".name");        
            pushmodule(L, name);
            lua_rawset(L, -3);               

            lua_pushstring(L, "__index");
            lua_pushcfunction(L, module_index_event);
            lua_rawset(L, -3);    
        }
        else if (type != LUA_TTABLE) 
        {  
            lua_pop(L, 1);  
            return false;  
        }

        lua_remove(L, -2);                                    //stack table
        path = e + 1;
    } while (*e == '.');

    lua_pushstring(L, ".name");
    lua_gettable(L, -2);      
    sb.buffer = lua_tolstring(L, -1, &sb.len);    
    lua_pop(L, 1);
    return true;
}

LUALIB_API bool tolua_endpremodule(lua_State *L, int ref)
{    
    
    int t1 = lua_gettop(L);
   
    lua_getref(L, ref);
    //TRY_LOG("tolua_endpremodule ref:%d,pos:%d,t1:%d,isnil:%d",ref,lua_gettop(L),t1,lua_isnil(L,-1));
    lua_pushstring(L, ".name");
    lua_rawget(L, -2);                

    if (!tolua_createtable(L, lua_tostring(L, -1), 0))
    {
        lua_pushnil(L);
    }
    
    sb.len = 0;
    return true;
}

LUALIB_API bool tolua_addpreload(lua_State *L, const char *path)
{    
    const char *e = NULL;
    const char *name = path;
    int top = lua_gettop(L);
    lua_pushvalue(L, LUA_GLOBALSINDEX);                         //stack _G

    do 
    {
        e = strchr(path, '.');
        if (e == NULL) e = path + strlen(path);
        lua_pushlstring(L, path, e - path);                     //stack t key
        lua_rawget(L, -2);                                      //stack t value
        int type = lua_type(L, -1);

        if (type == LUA_TNIL) 
        {
            lua_pop(L, 1);                                      //stack t
            lua_createtable(L, 0, 0);                           //stack t table
            lua_pushlstring(L, path, e - path);                 //stack t table name
            lua_pushvalue(L, -2);                               //stack t table name table
            lua_settable(L, -4);                                //stack t table

            lua_pushstring(L, ".name");        
            lua_pushstring(L, name);
            lua_rawset(L, -3);               

            lua_pushstring(L, "__index");
            lua_pushcfunction(L, module_index_event);
            lua_rawset(L, -3);    
        }
        else if (type != LUA_TTABLE) 
        {  
            lua_settop(L, top);
            return false;  
        }

        lua_remove(L, -2);                                      //stack table
        path = e + 1;
    } while (*e == '.');

    lua_settop(L, top);
    return true;
}

LUALIB_API int tolua_getclassref(lua_State *L, int pos)
{    
    //TRY_LOG("tolua_getclassref top1:%d,pos:%d",lua_gettop(L),pos);
    int top = lua_gettop(L);
    lua_getmetatable(L, pos);           //mt
    
    lua_pushstring(L, ".ref");          //mt .ref
    lua_rawget(L, -2);                  //mt ref
    int ref = lua_tointeger(L, -1);   
    // printTable(L,top+1);    
    // TRY_LOG("tolua_getclassref top2:%d,pos:%d,ref:%d",lua_gettop(L),pos,ref);  
    return ref;
}

LUALIB_API bool tolua_pushluatable(lua_State *L, const char *path)
{
	const char *e = NULL;
	lua_pushvalue(L, LUA_GLOBALSINDEX);	

	do 
	{
	  e = strchr(path, '.');
	  if (e == NULL) e = path + strlen(path);
	  lua_pushlstring(L, path, e - path);
	  lua_rawget(L, -2);

	  if (!lua_istable(L, -1))
	  {  
	    lua_pop(L, 2); 
	    return false;
	  }

	  lua_remove(L, -2);  
	  path = e + 1;
	} while (*e == '.');

	return true;
}

LUALIB_API const char* tolua_typename(lua_State *L, int lo)
{
    int tag = lua_type(L,lo);
    
    if (tag == LUA_TNONE)
    {
		lua_pushstring(L,"[no object]");
    }
    else if (tag != LUA_TUSERDATA && tag != LUA_TTABLE)
    {
        lua_pushstring(L, lua_typename(L,tag));
    }
    else if (tag == LUA_TUSERDATA)
    {
        if (!lua_getmetatable(L,lo))
        {
            lua_pushstring(L, lua_typename(L,tag));
        }
        else
        {
            lua_pushstring(L, ".name");
            lua_rawget(L, -2);

            if (!lua_isstring(L,-1))
            {
                lua_pop(L,1);
                lua_pushstring(L,"[undefined]");
            }
        }
    }
    else  //is table
    {
        lua_pushvalue(L,lo);        

        if (!lua_getmetatable(L,lo))
        {
            lua_pop(L,1);
            lua_pushstring(L,"table");
        }
        else
        {
            lua_pushstring(L, ".name");
            lua_rawget(L, -2);

            lua_pushstring(L,"class ");
            lua_insert(L,-2);
            lua_concat(L,2);
        }
    }

    return lua_tostring(L, -1);
}

LUALIB_API int tolua_getmetatableref(lua_State *L, int pos)
{
	int ref = LUA_REFNIL;	

    if (lua_getmetatable(L, pos) != 0)
    {
        lua_pushstring(L, ".ref");
        lua_rawget(L, -2);	

        if (lua_isnumber(L, -1))
        {
            ref = (int)lua_tointeger(L, -1);
        }

        lua_pop(L, 2);
    }

    return ref;
}

static lua_State* getthread(lua_State *L, int *arg) 
{
    if (lua_isthread(L, 1)) 
    {
        *arg = 1;
        return lua_tothread(L, 1);
    }
    else 
    {
        *arg = 0;
        return L;  
    }
}

static int traceback(lua_State *L) 
{
    int arg;
    lua_State *L1 = getthread(L, &arg);
    const char *msg = lua_tostring(L, arg + 1);

    if (msg == NULL && !lua_isnoneornil(L, arg + 1))  
    {
        lua_pushvalue(L, arg + 1);  
    }
    else 
    {
        if (NULL != strstr(msg, "stack traceback:"))
        {
            lua_pushvalue(L, arg + 1);
            return 1;
        }

        int level = (int)luaL_optinteger(L, arg + 2, (L == L1) ? 1 : 0);
#ifdef LUAJIT_VERSION            
        luaL_traceback(L, L1, msg, level);
#else        
        lua_getref(L, LUA_RIDX_TRACEBACK);
        lua_pushthread(L1);
        lua_pushvalue(L, arg + 1);
        lua_pushnumber(L, level + 1);
        lua_call(L, 3, 1);   
#endif            
    }     

    return 1;
}

LUALIB_API int tolua_beginpcall(lua_State *L, int reference)
{	
    lua_getref(L, LUA_RIDX_CUSTOMTRACEBACK);
	int top = lua_gettop(L);
	lua_getref(L, reference);
	return top;
}

LUALIB_API void tolua_pushtraceback(lua_State *L)
{
	lua_getref(L, LUA_RIDX_CUSTOMTRACEBACK);
}

/*static const int sentinel_ = 0;
#define sentinel ((void *)&sentinel_)

static int _require(lua_State *L)
{
    const char *name = luaL_checkstring(L, 1);    
    lua_settop(L, 1);  
    const char *key = luaL_gsub(L, name, "/", ".");            
    lua_getfield(L, LUA_REGISTRYINDEX, "_LOADED");
    lua_getfield(L, 3, key);

    if (lua_toboolean(L, -1)) 
    {  
        if (lua_touserdata(L, -1) == sentinel)  
        {
            luaL_error(L, "loop or previous error loading module " LUA_QS, name);
        }

        return 1;  //package is already loaded
    }

    // else must load it; iterate over available loaders
    lua_getfield(L, LUA_ENVIRONINDEX, "loaders");

    if (!lua_istable(L, -1))
    {
        luaL_error(L, LUA_QL("package.loaders") " must be a table");
    }

    lua_pushliteral(L, "");  //error message accumulator

    for(int i = 1; ; i++) 
    {
        lua_rawgeti(L, -2, i);  // get a loader

        if (lua_isnil(L, -1))
        {
            luaL_error(L, "module " LUA_QS " not found:%s", name, lua_tostring(L, -2));
        }

        lua_pushstring(L, name);
        lua_call(L, 1, 1);  

        if (lua_isfunction(L, -1))  // did it find module?
        {
            break;  // module loaded successfully
        }
        else if (lua_isstring(L, -1))  // loader returned error message?
        {
            lua_concat(L, 2);  // accumulate it
        }
        else
        {
            lua_pop(L, 1);            
        }    
    }

    lua_pushlightuserdata(L, sentinel);
    lua_setfield(L, 3, key);   // _LOADED[name] = sentinel
    lua_pushstring(L, name);    // pass name as argument to module
    lua_call(L, 1, 1);          // run loaded module

    if (!lua_isnil(L, -1))  // non-nil return? 
    {
        lua_setfield(L, 3, key);  // _LOADED[name] = returned value
    }

    lua_getfield(L, 3, key);

    if (lua_touserdata(L, -1) == sentinel) 
    {   // module did not set a value? 
        lua_pushboolean(L, 1);          // use true as result
        lua_pushvalue(L, -1);           // extra copy to be returned
        lua_setfield(L, 3, key);        // _LOADED[name] = true 
    }

    return 1;
}

void tolua_openrequire(lua_State *L)
{    
    lua_getglobal(L, "require");
    lua_pushcfunction(L, _require);
    lua_getfenv(L, -2);
    lua_setfenv(L, -2);
    lua_setglobal(L, "require");
    lua_pop(L, 1);
}*/

LUALIB_API int tolua_require(lua_State *L, const char *fileName)
{
    //LOG(L,"begin require:%s",fileName);
    int top = lua_gettop(L);
    lua_getref(L, LUA_RIDX_CUSTOMTRACEBACK);
    lua_getref(L, LUA_RIDX_REQUIRE);
    lua_pushstring(L, fileName);        
    int ret = lua_pcall(L, 1, -1, top + 1);
    lua_remove(L, top + 1);    
    return ret;
}


/*LUALIB_API bool tolua_checkluaslot(lua_State *L, int stackPos, int *func, int *table)
{
    lua_pushstring(L, "__call");      
    lua_rawget(L, stackPos);

    if (lua_isfunction(L, -1))
    {
        *func = toluaL_ref(L);        
    }
    else
    {
        lua_pop(L, 1);
        return false;
    }

    lua_pushvalue(L, stackPos);
    *table = toluaL_ref(L);    
    return true;
}*/

/*static int do_operator (lua_State *L, const char *op)
{
    if (lua_isuserdata(L,1))
    {        
        lua_pushvalue(L,1);  

        while (lua_getmetatable(L,-1))
        {   
            lua_remove(L,-2);                      
            lua_pushstring(L,op);               
            lua_rawget(L,-2);    

            if (lua_isfunction(L,-1))
            {
                lua_pushvalue(L,1);
                lua_pushvalue(L,2);
                lua_call(L,2,1);
                return 1;
            }

            lua_settop(L,3);
        }
    }

    luaL_error(L,"attempt to perform operation %s failed", op);
    return 0;
}

static int class_add_event (lua_State *L)
{
    return do_operator(L,"op_Addition");
}

static int class_sub_event (lua_State *L)
{
    return do_operator(L,"op_Subtraction");
}

static int class_mul_event (lua_State *L)
{
    return do_operator(L,"op_Multiply");
}

static int class_div_event (lua_State *L)
{
    return do_operator(L,"op_Division");
}

static int class_equals_event (lua_State *L)
{
    return do_operator(L,"op_Equality");
}*/


#ifdef _WIN32
double tolua_timegettime()
{
	FILETIME ft;
	double t;
	GetSystemTimeAsFileTime(&ft);
	/* Windows file time (time since January 1, 1601 (UTC)) */
	t = ft.dwLowDateTime / 1.0e7 + ft.dwHighDateTime*(4294967296.0 / 1.0e7);
	/* convert to Unix Epoch time (time since January 1, 1970 (UTC)) */
	return (t - 11644473600.0);
}
#else
double tolua_timegettime()
{
	struct timeval v;
	gettimeofday(&v, (struct timezone *) NULL);
	/* Unix Epoch time (time since January 1, 1970 (UTC)) */
	return v.tv_sec + v.tv_usec / 1.0e6;
}
#endif

static int tolua_gettime(lua_State *L)
{
	lua_pushnumber(L, tolua_timegettime());
	return 1;
}

static int tolua_bnd_setpeer(lua_State *L) 
{
    // stack: userdata, table
    if (!lua_isuserdata(L, -2)) 
    {
        return luaL_error(L, "Invalid argument #1 to setpeer: userdata expected.");        
    }

    if (lua_isnil(L, 2)) 
    {
        lua_pop(L, 1);
        lua_pushvalue(L, TOLUA_NOPEER);
        lua_setfenv(L, -2);
    }        
    else
    {
        lua_pushvalue(L, 2);                //stack: u p p
        lua_setfenv(L, -3);                 //stack: u p
        lua_newtable(L);                    //stack: u p vt        
        
        lua_pushlightuserdata(L, &vptr);    
        lua_pushvalue(L, 1);
        lua_rawset(L, -3);
        //lua_pushvalue(L, 1);
        //lua_rawseti(L, -2, 1);

        lua_getref(L, LUA_RIDX_VPTR);       //stack: u p vt mt
        lua_setmetatable(L, -2);            //stack: u p vt

        lua_pushstring(L, "base");          //stack: u p vt "base"
        lua_pushvalue(L, -2);               //stack: u p vt "base" vt
        lua_rawset(L, 2);                   //stack: u p vt    
        lua_pop(L, 1);
    }

    return 0;
};

static int tolua_bnd_getpeer(lua_State *L) 
{    
    lua_getfenv(L, -1);

    if (lua_rawequal(L, -1, TOLUA_NOPEER)) 
    {
        lua_pop(L, 1);
        lua_pushnil(L);
    };

    return 1;
};

static int tolua_bnd_getfunction(lua_State *L)
{
    lua_pushvalue(L, 1);                            // stack: obj key obj

    while (lua_getmetatable(L, -1) != 0)            // stack: obj key mt
    {            
        lua_remove(L, -2);                          // stack: obj key mt

        lua_pushvalue(L, 2);                        // stack: obj key mt key
        lua_rawget(L, -2);                          // stack: obj key mt value        

        if (!lua_isfunction(L, -1))
        {
            return 1;
        }
        else
        {
            lua_pop(L, 1);
        }
        
        lua_pushlightuserdata(L, &gettag);          
        lua_rawget(L, -2);                          //stack: obj key mt tget

        if (lua_istable(L, -1))
        {
            lua_pushvalue(L, 2);                    //stack: obj key mt tget key
            lua_rawget(L, -2);                      //stack: obj key mt tget value 

            if (lua_isfunction(L, -1))
            {                                           
                return 1;
            }
        }
            
        lua_settop(L, 3);                           //stack: obj key mt
    }

    lua_settop(L, 2);
    lua_pushnil(L);
    return 1;
}

static int tolua_bnd_type (lua_State *L)
{
    tolua_typename(L,lua_gettop(L));
    return 1;
}

static int tolua_initgettable(lua_State *L)
{
    if (!lua_istable(L, 1))
    {        
        return luaL_typerror(L, 1, "table");         
    }

    lua_newtable(L);
    lua_pushlightuserdata(L, &gettag);
    lua_pushvalue(L, -2);    
    lua_rawset(L, 1);
    return 1;
}

static int tolua_initsettable(lua_State *L)
{
    if (!lua_istable(L, 1))
    {        
        return luaL_typerror(L, 1, "table");         
    }

    lua_newtable(L);
    lua_pushlightuserdata(L, &settag);    
    lua_pushvalue(L, -2);    
    lua_rawset(L, 1);
    return 1;
}

void tolua_openvptr(lua_State *L)
{
    lua_newtable(L);        

    lua_pushstring(L, "__index");
    lua_pushcfunction(L, vptr_index_event);
    lua_rawset(L, -3);  

    lua_pushstring(L, "__newindex");
    lua_pushcfunction(L, vptr_newindex_event);
    lua_rawset(L, -3);  

    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_VPTR); 
}

static const struct luaL_Reg tolua_funcs[] = 
{
	{ "gettime", tolua_gettime },
	{ "typename", tolua_bnd_type },
	{ "setpeer", tolua_bnd_setpeer},
	{ "getpeer", tolua_bnd_getpeer},
    { "getfunction", tolua_bnd_getfunction},
    { "initset", tolua_initsettable},
    { "initget", tolua_initgettable},
    { "int64", tolua_newint64},        
    { "uint64", tolua_newuint64},
    { "traceback", traceback},
	{ NULL, NULL }
};

void tolua_setluabaseridx(lua_State *L)
{    
	for (int i = 1; i <= 64; i++)
	{
		lua_pushinteger(L, i);
		lua_rawseti(L, LUA_REGISTRYINDEX, i);
	}

    //同lua5.1.5之后版本放入mainstate和_G
	lua_pushthread(L);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_MAINTHREAD);

	lua_pushvalue(L, LUA_GLOBALSINDEX);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS);

    //cache require函数
    lua_getglobal(L, "require");
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_REQUIRE);      
}

void tolua_openpreload(lua_State *L)
{
    lua_getglobal(L, "package");
    lua_pushstring(L, "preload");
    lua_rawget(L, -2);
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PRELOAD);    
    lua_pushstring(L, "loaded");
    lua_rawget(L, -2);
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_LOADED);    
    lua_pop(L, 1);
}

void tolua_opentraceback(lua_State *L)
{
    lua_getglobal(L, "debug");
    lua_pushstring(L, "traceback");
    lua_rawget(L, -2);
    lua_pushvalue(L, -1);
    lua_setfield(L, LUA_GLOBALSINDEX, "traceback");            
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_TRACEBACK);
    lua_pop(L, 1);    

    lua_pushcfunction(L, traceback);
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_CUSTOMTRACEBACK);    
}

void tolua_openubox(lua_State *L)
{
	lua_newtable(L);
	lua_newtable(L);            
	lua_pushstring(L, "__mode");
	lua_pushstring(L, "v");
	lua_rawset(L, -3);
	lua_setmetatable(L, -2);            
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UBOX);	
}

void tolua_openfixedmap(lua_State *L)
{
	lua_newtable(L);
	//lua_newtable(L);
	//lua_pushstring(L, "__mode");
	//lua_pushstring(L, "k");
	//lua_rawset(L, -3);
	//lua_setmetatable(L, -2);    	
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_FIXEDMAP);		
}

//对于下列读取lua 特定文件需要判空报错
void tolua_openvaluetype(lua_State *L)
{
	lua_getglobal(L, "GetLuaValueType");
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_CHECKVALUE);	
}

void tolua_openluavec3(lua_State *L)
{    
	lua_getglobal(L, "Vector3");

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "Vector3 does not exist or not be loaded");
        return;
    }

	lua_pushstring(L, "New");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKVEC3);	
	lua_pushstring(L, "Get");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKVEC3);	
	lua_pop(L, 1);
}

void tolua_openluavec2(lua_State *L)
{    
	lua_getglobal(L, "Vector2");

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "Vector2 does not exist or not be loaded");
        return;
    }

	lua_pushstring(L, "New");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKVEC2);	
	lua_pushstring(L, "Get");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKVEC2);	
	lua_pop(L, 1);
}

void tolua_openluavec4(lua_State *L)
{    
	lua_getglobal(L, "Vector4");

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "Vector4 does not exist or not be loaded");
        return;
    }

	lua_pushstring(L, "New");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKVEC4);	
	lua_pushstring(L, "Get");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKVEC4);	
	lua_pop(L, 1);
}

void tolua_openluaclr(lua_State *L)
{    
	lua_getglobal(L, "Color");

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "Color does not exist or not be loaded");
        return;
    }

	lua_pushstring(L, "New");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKCLR);	
	lua_pushstring(L, "Get");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKCLR);	
	lua_pop(L, 1);
}

void tolua_openluaquat(lua_State *L)
{    
	lua_getglobal(L, "Quaternion");

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "Quaternion does not exist or not be loaded");
        return;
    }

	lua_pushstring(L, "New");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKQUAT);	
	lua_pushstring(L, "Get");
	lua_rawget(L, -2);
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKQUAT);		
	lua_pop(L, 1);
}

void tolua_openlualayermask(lua_State *L)
{    
    lua_getglobal(L, "LayerMask");   

    if (!lua_istable(L, 1))
    {        
        luaL_error(L, "LayerMask does not exist or not be loaded");
        return;
    }

    lua_pushstring(L, "New");
    lua_rawget(L, -2);
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_PACKLAYERMASK);   
    lua_pushstring(L, "Get");
    lua_rawget(L, -2);
    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UNPACKLAYERMASK); 
    lua_pop(L, 1);
}

void tolua_openupdate(lua_State *L)
{
    lua_getglobal(L, "Update");

    if (!lua_isfunction(L, 1))
    {
        luaL_error(L, "Update function does not exist or not be loaded");
        return;
    }

    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_UPDATE);    
    lua_getglobal(L, "LateUpdate");

    if (!lua_isfunction(L, 1))
    {
        luaL_error(L, "LateUpdate function does not exist or not be loaded");
        return;
    }

    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_LATEUPDATE);  
    lua_getglobal(L, "FixedUpdate");

    if (!lua_isfunction(L, 1))
    {
        luaL_error(L, "FixedUpdate function does not exist or not be loaded");
        return;
    }

    lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_FIXEDUPDATE);      
}


static int _openlualibs(lua_State *L)
{
    tolua_openvaluetype(L);
    tolua_openluavec3(L);
    tolua_openluavec2(L);
    tolua_openluavec4(L);
    tolua_openluaclr(L);
    tolua_openluaquat(L);   
    tolua_openlualayermask(L);
    tolua_openupdate(L);
    return 0;
}

LUALIB_API int tolua_openlualibs(lua_State *L)
{    
    lua_pushcfunction(L, _openlualibs);
    return lua_pcall(L, 0, -1, 0);    
}

static int mathf_ispoweroftwo(lua_State *L)
{
    lua_Integer mask = luaL_checkinteger(L, 1);
    bool flag = (mask & (mask-1)) == 0 ? true : false;
    lua_pushboolean(L, flag);
    return 1;
}

lua_Integer NextPowerOfTwo(lua_Integer v)
{
    v -= 1;
    v |= v >> 16;
    v |= v >> 8;
    v |= v >> 4;
    v |= v >> 2;
    v |= v >> 1;
    return v + 1;
}

static int mathf_nextpoweroftwo(lua_State *L)
{
    lua_Integer v = luaL_checkinteger(L, 1);
    v = NextPowerOfTwo(v);
    lua_pushnumber(L, v);
    return 1;
}

static int mathf_closestpoweroftwo(lua_State *L)
{
    lua_Integer v = luaL_checkinteger(L, 1);
    lua_Integer nextPower = NextPowerOfTwo (v);
    lua_Integer prevPower = nextPower >> 1;    

    if (v - prevPower < nextPower - v)
    {
        lua_pushnumber(L, prevPower);        
    }
    else
    {    
        lua_pushnumber(L, nextPower);
    }

    return 1;
}

static int mathf_gammatolinearspace(lua_State *L)
{
    lua_Number value = luaL_checknumber(L, 1);

    if (value <= 0.04045f)
    {
        value /= 12.92f;
    }
    else if (value < 1.0f)
    {    
        value = pow((value + 0.055f)/1.055f, 2.4f);
    }
    else
    {
        value = pow(value, 2.4f);
    }

    lua_pushnumber(L, value);
    return 1;    
}

static int mathf_lineartogammaspace (lua_State *L)
{
    lua_Number value = luaL_checknumber(L, 1);

    if (value <= 0.0f)
    {    
        value = 0;
    }
    else if (value <= 0.0031308f)
    {
        value *= 12.92f;
    }
    else if (value <= 1.0f)
    {
        value = 1.055f * powf(value, 0.41666f) - 0.055f;
    }
    else
    {
        value = powf(value, 0.41666f);
    }

    lua_pushnumber(L, value);
    return 1;
}

static int mathf_normalize(lua_State *L)
{
    float x = (float)lua_tonumber(L, 1);
    float y = (float)lua_tonumber(L, 2);
    float z = (float)lua_tonumber(L, 3);

    float len = sqrt(x * x + y * y + z * z);
    
    if (len == 1)
    {
        lua_pushnumber(L, x);
        lua_pushnumber(L, y);
        lua_pushnumber(L, z);
    }
    else if (len > 1e-5)
    {
        lua_pushnumber(L, x);
        lua_pushnumber(L, y);
        lua_pushnumber(L, z);
    }
    else    
    {
        lua_pushnumber(L, 0);
        lua_pushnumber(L, 0);
        lua_pushnumber(L, 0);
    }

    return 3;
}

static const struct luaL_Reg tolua_mathf[] = 
{
    { "NextPowerOfTwo", mathf_nextpoweroftwo },
    { "ClosestPowerOfTwo", mathf_closestpoweroftwo },
    { "IsPowerOfTwo", mathf_ispoweroftwo},
    { "GammaToLinearSpace", mathf_gammatolinearspace},
    { "LinearToGammaSpace", mathf_lineartogammaspace},
    { "Normalize", mathf_normalize},
    { NULL, NULL }
};

LUALIB_API void tolua_openlibs(lua_State *L)
{   

    initmodulebuffer();
    luaL_openlibs(L);   
    int top = lua_gettop(L);    

    tolua_setluabaseridx(L);    
    tolua_opentraceback(L);
    tolua_openpreload(L);
    tolua_openubox(L);
    tolua_openfixedmap(L);    
    tolua_openint64(L);
    tolua_openuint64(L);
    tolua_openvptr(L);    
    //tolua_openrequire(L);

    luaL_register(L, "Mathf", tolua_mathf);     
    luaL_register(L, "tolua", tolua_funcs);    

    lua_getglobal(L, "tolua");

    lua_pushstring(L, "gettag");
    lua_pushlightuserdata(L, &gettag);
    lua_rawset(L, -3);

    lua_pushstring(L, "settag");
    lua_pushlightuserdata(L, &settag);
    lua_rawset(L, -3);

    lua_pushstring(L, "version");
    lua_pushstring(L, "1.0.7");
    lua_rawset(L, -3);

    lua_settop(L,top);
}

LUALIB_API void tolua_setflag(int bit, bool flag)
{
    if (flag)
    {
        toluaflags |= bit;    
    }
    else
    {
        toluaflags &= ~bit;    
    }
}

LUALIB_API bool tolua_getflag(int bit)
{
    return toluaflags & bit ? true : false;
}

static luaL_Buffer lua_buffer[3];
static int _bufferIndex = 0;

LUALIB_API luaL_Buffer* tolua_buffinit(lua_State *L)
{
    luaL_Buffer* buffer = &lua_buffer[_bufferIndex & 3];
    luaL_buffinit(L, buffer);
    ++_bufferIndex;
    return buffer;
}

LUALIB_API void tolua_addlstring(luaL_Buffer *b, const char *s, int l)
{
    luaL_addlstring(b, s, (size_t)l);
}

LUALIB_API void tolua_addstring(luaL_Buffer *b, const char *s)
{
    luaL_addstring(b, s);
}

LUALIB_API void tolua_pushresult(luaL_Buffer *b)
{
    luaL_pushresult(b);
}

LUALIB_API void tolua_addchar(luaL_Buffer *b, char c)
{
    luaL_addchar(b, c);
}

LUALIB_API int tolua_update(lua_State *L, float deltaTime, float unscaledTime)
{
    int top = tolua_beginpcall(L, LUA_RIDX_UPDATE);
    lua_pushnumber(L, deltaTime);
    lua_pushnumber(L, unscaledTime);
    return lua_pcall(L, 2, -1, top);
}

LUALIB_API int tolua_lateupdate(lua_State *L)
{
    int top = tolua_beginpcall(L, LUA_RIDX_LATEUPDATE);
    return lua_pcall(L, 0, -1, top);
}

LUALIB_API int tolua_fixedupdate(lua_State *L, float fixedTime)
{
    int top = tolua_beginpcall(L, LUA_RIDX_FIXEDUPDATE);
    lua_pushnumber(L, fixedTime);
    return lua_pcall(L, 1, -1, top);
}

static int index_op_this(lua_State *L)
{
    lua_pushvalue(L, 2);                                //stack: t, k, k
    lua_rawget(L, -2);                                  //stack: t, k, v

    if (!lua_isnil(L, -1))
    {       
        lua_pushlightuserdata(L, &vptr);               // stack: t, k, v, vptr
        lua_rawget(L, 1);                              // stack: t, k, v, u
        lua_replace(L, 1);                             // stack: u, k, v
    }    

    return 1;
}

static int newindex_op_this(lua_State *L)
{
    lua_pushvalue(L, 2);                                //stack: t, k, v, k
    lua_rawget(L, 1);                                   //stack: t, k, v, f

    if (!lua_isnil(L, -1))
    {       
        lua_pushlightuserdata(L, &vptr);               // stack: t, k, v, f, vptr
        lua_rawget(L, 1);                              // stack: t, k, v, f, u
        lua_replace(L, 1);                             // stack: u, k, v, f
    }    

    return 1;
}

LUALIB_API void tolua_regthis(lua_State *L, lua_CFunction get, lua_CFunction set)
{
    lua_newtable(L);                        //u t
    
    lua_pushlightuserdata(L, &vptr);    
    lua_pushvalue(L, -3);
    lua_rawset(L, -3);

    if (get != NULL)
    {
        lua_pushstring(L, "get");    
        tolua_pushcfunction(L, get);
        lua_rawset(L, -3);                  
    }

    if (set != NULL)
    {
        lua_pushstring(L, "set");    
        tolua_pushcfunction(L, set);
        lua_rawset(L, -3);          
    }    

    lua_pushstring(L, "__index");
    lua_pushcfunction(L, index_op_this);
    lua_rawset(L, -3);  

    lua_pushstring(L, "__newindex");
    lua_pushcfunction(L, newindex_op_this);
    lua_rawset(L, -3);          
}

LUALIB_API int tolua_where (lua_State *L, int level) 
{
    lua_Debug ar;

    if (lua_getstack(L, level, &ar)) 
    {  
        lua_getinfo(L, "Sl", &ar);  

        if (ar.currentline > 0) 
        {              
            lua_pushstring(L, ar.source);
            return ar.currentline;
        }
    }

    lua_pushliteral(L, "");
    return -1;
}
static char *bin2hex(const char* data,int len)
{
    char *re = (char *)malloc(len*2+1);
    for(int i=0;i<len;i++)
    {
        TRY_LOG("result: i:%d,val:%x",i,(uint8_t)data[i]);
        uint8_t val = ((uint8_t)data[i])>>4;
        if(val<10)
            re[i*2]= val+'0';
        else
            re[i*2]= (val-10)+'a';
        uint8_t val2 = data[i] &0xf ;
        if(val2<10)
            re[i*2+1]= val2+'0';
        else
            re[i*2+1]= (val2-10)+'a';         
    }
    re[len*2] = 0;
    return re;
}
static uint8_t *hex2bin(const char* data,int len)
{
    uint8_t *re = (uint8_t *)malloc(len/2);
    int i=0,j=0;
    for(i=0;i<len/2;i++)
    {
        re[i] = 0; 
        for(j=0;j<2;j++)
        {
            int k = i*2+j;
            if(data[k]>='0' && data[k] <='9')
                re[i] =(re[i]<<4) +(data[k] - '0');
            else if(data[k]>='a' && data[k] <='f')
                re[i] = (re[i]<<4) +(data[k] - 'a'+10); 
            else if(data[k]>='A' && data[k] <='F')
                re[i] = (re[i]<<4) +(data[k] - 'A'+10);     
        }
       
       
    }
    return re;
}
// LUA_API int tolua_checkstring(int id,const char *info,const char *expect  ) 
// {
//     const char* key = "0KD9F18z5YLcsIeEdbdVW0ruFANG78v13A8G8maLgUp01j6cNG00kYem0ojgiyUO";
//     int len1 = strlen(info);
//     int len2 = strlen(key);
//     int totalLen = len1+len2;

//     char *newbuff = (char *)malloc(totalLen+1);
//     memcpy(newbuff,info,strlen(info));
//     memcpy(newbuff+strlen(info),key,strlen(key));
//     newbuff[totalLen] = 0;
//     char code[17];
//     md5(newbuff, totalLen, code);
//     code[16] = 0;
//     char *hex = bin2hex(code,16);
//     TRY_LOG("result:%s,orig:%s,expect:%s",hex,newbuff,expect);
     
   
//     free(newbuff);
//     free(hex);
    
// }
LUA_API char * tolua_getstring(const char *in, int len,int *reallen ) 
{
    
    uint8_t key[] = {0x9e,0x86,0xd6,0x2a,0xda,0xae,0x3e,0xe2,0x1,0x1d,0xf3,0x47,0x18,0xbd,0x8a,0x5e};
    for(int i=0;i<16;i++)
        key[i] ^=0x11;
     
    struct AES_ctx ctx;
    len -=4;
    char * out = (char*) malloc(len*sizeof(char));
    
    memcpy(out,in,len);
    
    AES_init_ctx(&ctx, key);
    
    int total = len;
    while(len>=16)
    {
        AES_ECB_decrypt(&ctx, out+(total - len));
        len -=16;
    }
    int success = 1;
    uint32_t crc = crc32buf(out,total);
    for(int i=0;i<4;i++)
    {
        if((uint8_t)in[total+i] != (uint8_t)crc)
        {
            success = 0;
            break;
        }
        crc >>=8;
    }
    //return out;
    
    if(success == 1)    
    {
        *reallen = 0;
        //while(*reallen<total && out[*reallen++] != 0) ;
        
        while((*reallen)<total && out[(*reallen)] != 0) 
            (*reallen)++;
        return out;
    }
    else
    {
        //sprintf(out,"check error,crc:%x,total:%d,val:%x,%x,%x,%x",crc,total,(BYTE)in[total],(BYTE)in[total+1],(BYTE)in[total+2],(BYTE)in[total+3]);
        free(out);
        return NULL;
    }
}
LUA_API void tolua_releasestring(char *in) 
{
    if(in != NULL)
        free(in);
}

static FILE *OpenFileWithPath(const char *path)
{
    const char *fileMode = "rb";
    return fopen (path, fileMode);
}

static char *ReadStringFromFile(const char *pathName, int *size)
{
    FILE *file = OpenFileWithPath (pathName);
    if (file == NULL)
    {
        return 0;
    }
    fseek (file, 0, SEEK_END);
    int length = ftell(file);
    fseek (file, 0, SEEK_SET);
    if (length < 0)
    {
        fclose (file);
        return 0;
    }
    *size = length;
    char *outData =(char*) malloc (length);
    int readLength = fread (outData, 1, length, file);
    fclose(file);
    if (readLength != length)
    {
        free (outData);
        return 0;
    }
    return outData;
}
long timeDelta(struct timeval t1,struct timeval t2)
{
	return 1000000 *(t2.tv_sec - t1.tv_sec)+t2.tv_usec - t1.tv_usec;
}
int getRealData(char *data,int len)
{
	struct timeval t1,t2,t3;
	gettimeofday(&t1, NULL);
	int i=0;
	TRY_LOG("check magic head:%x,%x",(uint8_t)*data,(uint8_t)*(data+1));
	//magic number 0x4d5a for normal DLL
	if((uint8_t)*data == 0x4d   &&  (uint8_t)*(data+1) == 0x5a )
		return 0; 
    uint8_t key[] = {0x9e,0x86,0xd6,0x2a,0xda,0xae,0x3e,0xe2,0x1,0x1d,0xf3,0x47,0x18,0xbd,0x8a,0x5e};
    for(i=0;i<16;i++)
    {
		key[i] ^= ((uint8_t)data[(len - i/4-1)]>>(i%4));
		TRY_LOG("key %d:%x,%x",i,key[i],(uint8_t)data[(len - i/4-1)]);
	}    
    //uint8_t iv[]  = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
    struct AES_ctx ctx;
    len -=4;
    
    
    AES_init_ctx(&ctx, key);
    
    int total = len;
	int smallBlock = 256;
	int encCnt = smallBlock/16;
	int largeBlock = smallBlock*10;
    int cnt = 0;
    while(len>=smallBlock)
    {
		for(i=0;i<encCnt;i++)
        {
            AES_ECB_decrypt(&ctx, data+(total - len)+16*i);  
            char* tmp = data+(total - len)+16*i;  
            if(i==0 && cnt == 1)
            {
                TRY_LOG("dec addr:%x, result:%x%x%x%x",(total - len)+16*i,(uint8_t)tmp[0],(uint8_t)tmp[1],(uint8_t)tmp[2],(uint8_t)tmp[3]);
            }
        }	
        cnt++;
        len -=largeBlock;
        //TRY_LOG("dec:%d",(total - len));
    }
    FILE* out =  fopen ("e:/test.dll", "wb");
    fwrite(data,1,total,out);
    fclose(out);
	gettimeofday(&t2, NULL);
    int success = 0;
    uint32_t crc = crc32buf(data,total);
	gettimeofday(&t3, NULL);
	TRY_LOG("crc:%x,total:%d,expect:%x%x%x%x,time:%d,%d",crc,total,(uint8_t)data[total],(uint8_t)data[total+1],(uint8_t)data[total+2],(uint8_t)data[total+3],timeDelta(t1,t2),timeDelta(t2,t3));
    for(i=0;i<4;i++)
    {
        if((uint8_t)data[total+3-i] != (uint8_t)crc)
        {
            success = 1;
            break;
        }
        crc >>=8;
    }
	return success;
}
extern  __attribute__((visibility ("default"))) void testDll(const char *fn) 
{
    TRY_LOG("testDll %s",fn);
     
    int datasize = 0;
    char *bytes = ReadStringFromFile (fn, &datasize);
		 
    TRY_LOG("try load file:%s,datasize:%d",fn,datasize);	
    if( getRealData(bytes,datasize) != 0)
    {
        TRY_LOG("getRealData failed" );
       
    }	
    else
        TRY_LOG("getRealData success" );    
}

 
uint8_t _DATA_ID[] = {0x5,0x9b,0x5,0x15,0x15,0xff};//data
uint8_t _SKEY_ID[] = {0x2,0x94,0xff};//k
uint8_t _TP_ID[] = {0x3,0x8b,0x4,0xff};//tp
uint8_t _F_ID[] = {0x2,0x99,0xff};//f
uint8_t _FUNC_ID[] = {0x05,0x99,0x13,0x1b,0x0d,0xff};//func
uint8_t _DISPATCH_ID[] = {0x10,0xba,0x33,0x13,0x0b,0x1a,0x30,0x2d,0x1a,0x03,0x11,0x15,0x17,0x0b,0x0d,0x17,0xff};//EventDispatcher
int _check2(lua_State *L)//_G root dataval _k v
{
    lua_pushsafestring(L, _TP_ID);//_G root dataval _k v tp
    lua_rawget(L, -2);//_G root dataval _k v tpval
    if(lua_isnil(L,-1))
        return -1;
    int tp = lua_tointeger(L,-1);
    if(tp <=0)
        return -1;
    //TRY_LOG("doCheck tp:%d",tp);
    //lua_pop(1);//t

    //lua_pushvalue(L, LUA_GLOBALSINDEX);//t tp _G
    
    lua_pushsafestring(L, _SKEY_ID);//_G root dataval _k v tpval k
    lua_rawget(L, -6);//_G root dataval _k v tpval root.k
    if(lua_isnil(L,-1))
        return tp;
    lua_pushvalue(L,-2);//_G root dataval _k v tpval root.k tpval 
    lua_rawget(L, -2);//_G root dataval _k v tpval root.k root.k.tpval 
    if(lua_isnil(L,-1))
        return tp;
    size_t klen;
    
    const char* kstr = lua_tolstring(L,-1,&klen);
    //TRY_LOG("doCheck kstr:%s",kstr);
    lua_pop(L,2);//_G root dataval _k v tpval
    lua_pushsafestring(L, _F_ID);//_G root dataval _k v tpval f
    lua_rawget(L, -6);//_G root dataval _k v tpval root.f
    if(lua_isnil(L,-1))
    {
        //TRY_LOG("doCheck exit 1,_F_ID:%s",(char *)(_F_ID+1));
        return tp;
    }    
    lua_pushvalue(L,-2);//_G root dataval _k v tpval root.f tpval
    lua_rawget(L, -2);//_G root dataval _k v tpval root.f root.f.tpval
    if(lua_isnil(L,-1))
    {
        //TRY_LOG("doCheck exit 2");
        return tp;
    }   
    const char* fields = lua_tostring(L,-1);
    TRY_LOG("doCheck fields:%s",fields);
    lua_pop(L,3);//_G root dataval _k v
    lua_pushsafestring(L, _DATA_ID);//_G root dataval _k v data
    lua_rawget(L, -2);//_G root dataval _k v v.data

    if(!lua_istable(L,-1))
    {
        //TRY_LOG("doCheck exit 3,_DATA_ID:%s",(char *)(_DATA_ID+1));
        return tp;
    }   

    size_t lenField=0;
  
    uint8_t *bfields = hex2bin(fields,strlen(fields));
    //TRY_LOG("doCheck bfields %x%x%x%x",bfields[0],bfields[1],bfields[2],bfields[3]);
    const char* sfields = getSafeStr(bfields,&lenField);
    //TRY_LOG("doCheck sfields:%s",sfields);
    int start = 0;
    int i=0;
    while(i<lenField&& sfields[i] != '|') i++;
    if(i == lenField)
    {
        free(bfields);
        TRY_LOG("doCheck exit 4");
        return tp;
    }
    //TRY_LOG("doCheck key1 len:%d,i:%d,lenField:%d",i-start,i,lenField);
    lua_pushlstring(L,sfields,i-start);//_G root dataval _k v v.data k1
    lua_gettable(L, -2);//_G root dataval _k v v.data v.data.k1
    if(lua_isnil(L,-1))
    {
        TRY_LOG("doCheck exit  5 ");
        return tp;
    }  
 
   
    //TRY_LOG("doCheck md5Re before lenField:%d ",lenField  );
    size_t relen=0;
    const char* md5Re =  lua_tolstring(L,-1,&relen);
   // TRY_LOG("doCheck md5Re:%s,lenField:%d,relen:%d,lenField2:%d,addr1:%x,%x,%x",md5Re,lenField,relen,lenField2,&lenField,&lenField2,&relen);
    if(relen != 32)
    {
        TRY_LOG("doCheck exit  6 ");
        free(bfields);
        return tp;
    }
    lua_pop(L,1);//_G root dataval _k v v.data
    start = ++i;
    if(start>=lenField)
    {
        free(bfields);
        TRY_LOG("doCheck exit  7 start:%d,lenField:%d,klen:%d  ",start,lenField,klen );
        return tp;
    }
  
    luaL_Buffer b;
    luaL_buffinit(L, &b);
    stringbuffer dsb;
     
        

    while(true)
    {
        while(i<lenField && sfields[i] != '|') i++;
        if(start < i)
        {
            //TRY_LOG("doCheck get by key:%s,len:%d",sfields+start,(i-start));
            lua_pushlstring(L,sfields+start,(i-start));//_G root dataval _k v v.data k1
     
            lua_gettable(L, -2);//_G root dataval _k v v.data v.data.k1        
            if(lua_isnil(L,-1))
            {
                free(bfields);
                return tp;
            }
            size_t itemLen = 0;
            const char* item =  lua_tolstring(L,-1,&itemLen);
            //TRY_LOG("doCheck item:%s,fields:%s,start:%d",item,sfields,start);
            luaL_addlstring(&b, item, itemLen);
            lua_pop(L,1);//_G root dataval _k v v.data
        }    
        start = ++i;
        if(i >= lenField)
            break;
    }
    uint8_t *kfields = hex2bin(kstr,strlen(kstr));
    const char* kstrval = getSafeStr(kfields,&klen);
    //TRY_LOG("doCheck kstrval:%s",kstrval);

    luaL_addlstring(&b,kstrval,klen);
    luaL_pushresult(&b);    
    dsb.buffer = lua_tolstring(L, -1, &dsb.len);    
    char code[16];
    md5(dsb.buffer, dsb.len, code);
    char *hexcode =  bin2hex(code,16);
    
    for(i=0;i<32;i++)
    {
        if(hexcode[i] != md5Re[i])
            break;
    }
    free(bfields);
    TRY_LOG("doCheck check result i:%d",i);
    return i == 32?0:tp;
    
}
int _check(const char *val1,const char *val2)
{
    int len1 = strlen(val1);
    int len2 = strlen(val2);
    TRY_LOG("doCheck len1:%d,len2:%d,val1:%s,val2:%s",len1,len2,val1,val2);
    int i=0;
    while(i<len2 && val2[i] != '|') i++;
    TRY_LOG("doCheck i:%d",i);
    if(i == len2)
        return 0;
    int ival = atoi(val2);
    TRY_LOG("doCheck i:%d",ival);
    if(ival<=0)
        return 0;
    int tp = ival&0xff;
    int j=0;
    tp = tp+1; 
    int kstart = 0;
    while(j<len1 && tp>=0)
    {
        if(val2[j] == '|')
        {
            if(tp>0)
                kstart = i;
            tp--;
        }    
        j++;
    }
    TRY_LOG("doCheck kstart:%d,j:%d",kstart,j );
    if(j == len1)
        return 0;
    int kend = j;

    int cnt = ival>>8;
    int p1 = i;
    while(i<len2 && val2[i] != '|') i++;
    TRY_LOG("doCheck i:%d,len2:%d",j,len2);
    if(i == len2)
        return 0;
    int p2 = i;
    i = len2-1;
    while(i>p2&&cnt>=0){
        if(val2[i] == '|')
            cnt--;
        i--;    
    } 
    TRY_LOG("doCheck i:%d,p2:%d",i,p2);
    if(i == p2)
        return 0;
    int bufflen = len2-i-cnt+(kend-kstart);
    TRY_LOG("doCheck bufflen:%d",bufflen);
    char *buff = (char *)malloc(bufflen);
    i = len2-1;
    int previ = i;
    int buffpos = 0;
    while(i>p2&&cnt>0){
        if(val2[i] == '|')
        {
            int clen = previ - i;
            TRY_LOG("doCheck memcpy buffpos:%d,i:%d,clen:%d",buffpos,i,clen);
            memcpy(buff+buffpos,(void*)(val2+i+1),clen);
            buffpos +=clen;
            cnt--;
        }
            
        i--;    
    } 
    int checkEnd = i;
    while(i>p2&&cnt>0){
        if(val2[i] != '|')
            i--;
    }
    int checkStart = i+1;
    TRY_LOG("doCheck memcpy2 buffpos:%d,kstart:%d,(kend - kstart):%d",buffpos,kstart,(kend - kstart));
    memcpy(buff+buffpos,(void*)(val1+kstart),(kend - kstart));
    char code[16];
    md5(buff, bufflen, code);
    
    char *hexcode = bin2hex(code,16);
    TRY_LOG("doCheck hexcode:%s,checkStart:%s,val2:%s",hexcode,checkStart,val2);
    int k = 0;
    for(k = 0;k<32;k++)
    {
        if(hexcode[k] != val2[checkStart+k])
        {
            free(hexcode);
            return 0;
        }
    }
    
    free(hexcode);
    return 1;
}
int doCheck(lua_State *L)
{
    lua_reset(L);
    int top = lua_gettop(L);
    const char* msg = NULL;
    size_t len = 0;
    lua_pushvalue(L, LUA_GLOBALSINDEX);//_G
    
    lua_pushsafestring(L, _KEY_ID);//_G __SafeKey
    lua_rawget(L, -2);//_G root
    int success = -1;
   
    TRY_LOG("doCheck 1:%d",lua_type(L,-1));
    if(lua_istable(L,-1))
    {
        lua_pushsafestring(L, _DATA_ID); //_G root data
        lua_rawget(L, -2);               //_G root dataval
        TRY_LOG("doCheck 3:%d", lua_type(L, -1));
        if (lua_istable(L, -1))
        {
            lua_pushnil(L);
            while (lua_next(L, -2))
            {
                if (lua_istable(L, -1))
                {
                    int t = lua_gettop(L);
                    success = _check2(L); //_G root dataval k v
                    lua_settop(L,t);
                    lua_pop(L,1);
                    if (success != 0)
                    {
                        break;
                    }    
                }
                else
                {
                    lua_pop(L,1);
                    success = -1;
                    break;
                }

            }
            lua_pop(L,1);//_G root dataval
        }
        TRY_LOG("check result:%d",success);
        if (success == 0)
        {
            lua_pop(L, 1);//_G root
            lua_set(L);
            
        }
        else
        {
            lua_pushsafestring(L,_FUNC_ID);//_G root dataval func
            lua_rawget(L,-3);//_G root dataval funcval
            if(lua_isfunction(L,-1))
            {
                TRY_LOG("check result cal func");
                lua_pushinteger(L,success);
                lua_call(L,1,0);
            }
        }

        // lua_pushsafestring(L,_SKEY_ID);//_G val k
        // lua_rawget(L, -2);//_G val skey
        // TRY_LOG("doCheck 2:%d",lua_type(L,-1));
        // if(lua_isstring(L,-1))
        // {
        //     const char* skey = lua_tolstring(L,-1,NULL);
        //      TRY_LOG("doCheck skey:%s",skey);
        //     lua_pop(L,1);
            
             


        // }
       
    } 
    lua_settop(L,top);
    return 0;
}