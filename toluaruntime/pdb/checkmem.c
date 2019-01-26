#if defined(__APPLE__)
    #include <malloc/malloc.h>
#else
    #include <malloc.h>
#endif
#define CHECKMEM_HEAD
#include "checkmem.h"
#define orig_free free
#define orig_malloc malloc
#define orig_calloc calloc
#define orig_realloc realloc
#include <stdio.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>
#include "lib_log.h"
#undef CHECKMEM_HEAD
 
// 控制台打印错误信息, fmt必须是双引号括起来的宏
#define CERR(fmt, ...) \
    fprintf(stderr,"[%s:%s:%d][error %d:%s]" fmt "\r\n",\
         __FILE__, __func__, __LINE__, errno, strerror(errno), ##__VA_ARGS__)
//控制台打印错误信息并退出, t同样fmt必须是 ""括起来的字符串常量
#define CERR_EXIT(fmt,...) \
    CERR(fmt,##__VA_ARGS__),exit(EXIT_FAILURE)

// 插入字节块的个数
#define _INT_CHECK        (1<<4)
static size_t _used_memory = 0;
#define SHOW_TRACE
/*
char* my_backtrace()
{
#ifdef _WIN32
    unsigned int   i;
     void         * stack[ 100 ];
     unsigned short frames;
     SYMBOL_INFO  * symbol;
     HANDLE         process;

     process = GetCurrentProcess();

     SymInitialize( process, NULL, TRUE );

     frames               = CaptureStackBackTrace( 0, 100, stack, NULL );
     symbol               = ( SYMBOL_INFO * )calloc( sizeof( SYMBOL_INFO ) + 256 * sizeof( char ), 1 );
     symbol->MaxNameLen   = 255;
     symbol->SizeOfStruct = sizeof( SYMBOL_INFO );
    char *ret = (char*)orig_malloc(2048);
    
    int pos = 0;
     for( i = 0; i < frames; i++ )
     {
         SymFromAddr( process, ( DWORD64 )( stack[ i ] ), 0, symbol );
        int len = sprintf_s(ret+pos,2048-pos,"%i: %s - 0x%0X\n", frames - i - 1, symbol->Name, symbol->Address );
        pos+=len;
        if(pos>=2048)
        {
            pos = 2047;
            break;
        }    
     }
     ret[pos] = 0;
     return ret;
#else  
    void *buffer[10] = { NULL };CaptureStackBackTrace()
    char **trace = NULL;

    int size = backtrace(buffer, 10);
    trace = backtrace_symbols(buffer, size);
    if (NULL == trace) {
        return NULL;
    }
    
    
    int cnt = 0;
    for (int i = 0; i < size; ++i) {
        cnt += strlen(size[i])+1;
    }
    char *ret = (char*)malloc(cnt+1);
    char *tmp = ret;
    for (int i = 0; i < size; ++i) {
        int len = strlen(size[i]);
        strncpy(tmp,size[i],len);
        tmp[len] = 0;
        tmp+=len+1;
    }

    free(trace);
    return ret;
    
#endif
}
*/
/*
* 对malloc进行的封装, 添加了边界检测内存块
* sz        : 申请内存长度
*            : 返回得到的内存首地址
*/
inline void* 
mc_malloc(const char* filename,int line,size_t sz) {
    // 头和尾都加内存检测块, 默认0x00
    char* ptr = orig_calloc(1, sz + 2 * _INT_CHECK);
    if (NULL == ptr) {
        CERR_EXIT("malloc sz + sizeof struct check is error!");
    }

    //前四个字节保存 最后一个内存块地址 大小
    size_t* iptr = (size_t*)ptr;
    *iptr = sz + _INT_CHECK;
    _used_memory +=sz;
 
   
    // TRY_LOG("mc_malloc addr:%x,size:%d,filename:%s,line:%d", ptr + _INT_CHECK,sz,filename,line);
   
 
    return ptr + _INT_CHECK;
}

/*
* 对calloc进行封装, 添加边界检测内存块
* cut        : 申请的个数
* sz        : 每个的大小
*/
inline void* 
mc_calloc(const char* filename,int line,size_t cut, size_t sz) {
    return mc_malloc(filename,line,cut*sz);
}

/*
* 对relloc进行了封装, 同样添加了边间检测内存块
*/
inline void* 
mc_realloc(const char* filename,int line,void* ptr, size_t sz) {
    // 先检测一下内存
    mc_check(ptr,"realloc");

    // 重新申请内存
    char* cptr = (char*)ptr - _INT_CHECK;
    char* nptr = orig_calloc(1, sz + 2 * _INT_CHECK);
    if (NULL == nptr) {
        CERR_EXIT("realloc is error:%p.", ptr);
    }
    // 内存移动
    size_t* bsz = (size_t*)cptr;
    memcpy(nptr, cptr, *bsz < (sz+_INT_CHECK) ? *bsz : (sz+_INT_CHECK));
    _used_memory +=sz - *bsz+_INT_CHECK ;
    *(size_t*)nptr = sz+_INT_CHECK;
    
    orig_free(cptr);
 
    // TRY_LOG("mc_realloc addr:%x,size:%d,filename:%s,line:%d", nptr + _INT_CHECK,sz,filename,line);

    return nptr+_INT_CHECK;
}
inline void  mc_free(const char* filename,int line,void* ptr){
 
    // TRY_LOG("mc_free addr:%x,filename:%s,line:%d", ptr,filename,line);
   
    mc_check(ptr,"free");
    char* cptr = (char*)ptr - _INT_CHECK;
    _used_memory -= *((size_t*)cptr)-_INT_CHECK;
    orig_free(cptr);
}
inline void mc_dump(){
    TRY_LOG("alloc memory:%d",_used_memory);
}
extern inline size_t mc_count()
{
    return _used_memory;
}
// 检测内存是否错误, 错误返回 true, 在控制台打印信息
static void _iserror(char* s, char* e,const char* msg) {
    while (s < e) {
        if (*s) {
            TRY_LOG("Need to debug test!!! ptr is : (%p, %p).check is %d!msg:%s",s, e, *s,msg);
        }
        ++s;
    }
}

/*
* 对内存检测, 看是否出错, 出错直接打印错误信息
* 只能检测, check_* 得到的内存
*/
inline void 
mc_check(void* ptr,const char* msg) {
    char *sptr = (char*)ptr - _INT_CHECK;

    //先检测头部
    char* s = sptr + sizeof(size_t);
    char* e = sptr + _INT_CHECK;
    _iserror(s, e,msg);

    //后检测尾部
    size_t sz = *(size_t*)sptr;
    s = sptr + sz;
    e = s + _INT_CHECK;
    _iserror(s, e,msg);
}