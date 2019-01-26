#include "pdb.h"

#include "stdio.h"
#include "lua.h"
#include "lauxlib.h"
#include "hashmap.h"
#include <stdint.h>
#include <string.h>
//#include <malloc.h>
#include "lib_log.h"
#include "pbc.h"
#include <math.h>
#include <time.h>
#include <stdlib.h>
#ifdef CHECK_MEM
#include "checkmem.h"

#endif
typedef struct spdb spdb;
#define MAGIC1 0x9f
#define MAGIC2 0xda
#define VERSION 1
 
#define LEAF_NODE_MASK 0x80000000
#define LEAF_NODE_VALUE 0x7fffffff

#define PB_LEN_SIZE  sizeof(uint16_t)
#define CHECK_EOF(x) \
    if (feof(x))     \
        return PDB_EOF;
#define CHECK_RESULT(x) \
    { \
    int __ret = (x);      \
    if (__ret != PDB_OK)  \
        return __ret; \
    }    

struct spdb
{
    FILE *fp;
    map_t tables;
    struct pbc_env *env;
};

struct table_id
{
    char *name;
    uint8_t tp;
};
typedef struct 
{
    void *idx;
    int len;
    struct table_id *ids;
    uint8_t id_len;
}table_index_item;
struct table_data
{
    uint32_t offset;
    uint32_t len;
    uint32_t total_row;
    char *tablename;
    //map_t records;
    table_index_item mainIdx;    
    head_item *head_items;

    uint16_t pb_len;
    uint16_t pb_off;
    uint32_t data_off;
    uint16_t head_item_len;
    uint8_t pb_loaded;
    
    table_index_item *subIdx;
    int subIdxLen;

};
struct table_record
{
    uint32_t offset;
};
typedef struct
{
    map_t parent, cur;
    uint8_t depth;
} table_map_node;

/**************************************/
//table_record_index and table_record_string_index should be same field name order
typedef struct
{
    int id;
    uint32_t child_len;
    uint32_t block_len;    
    uint32_t offset;
    void *child;
    

} table_record_index;
typedef struct
{
    table_record_index p;
    char* key;

} table_record_string_index;


static int readStringIdx(read_buff *buff,  table_index_item *item, table_record_string_index * records, int depth, int cnt);
static int readIntIdx(read_buff *buff,  table_index_item *item, table_record_index *records, int depth, int cnt);


static uint32_t calcStringHash(const char* str)
{
    int len = strlen(str);
    uint32_t h = 0;
    for(int i=0;i<len;i++)
    {
        h = 31 * h + str[i];
    }
    return h;
}

static inline void *
checkuserdata(lua_State *L, int index)
{
    void *ud = lua_touserdata(L, index);
    if (ud == NULL)
    {
        luaL_error(L, "userdata %d is nil", index);
    }
    return ud;
}
static inline int calcHashSize(uint32_t cnt)
{
    uint32_t t = 0;
    while (cnt >>= 1)
        t++;

    //TRY_LOG("calcHashSize orig:%d,cnt:%d,real:%d",cnt,t,1<<(t+2));
    return 1 << (t + 2);
}
static void freeRecordIndex(table_record_index *p)
{
}
int32_t readInt32(FILE *fp)
{
    uint8_t buff[4];
    if (fread(buff, 1, 4, fp) < 4)
        return 0;
    int32_t ret = 0;
    for (int i = 0; i < 4; i++)
        ret += buff[i] << (8 * i);

    return ret;
}
uint32_t readUint32(FILE *fp)
{
    uint8_t buff[4];
    if (fread(buff, 1, 4, fp) < 4)
        return 0;
    uint32_t ret = 0;
    for (int i = 0; i < 4; i++)
        ret += buff[i] << (8 * i);

    return ret;
}
uint16_t readUint16(FILE *fp)
{
    uint8_t buff[2];
    if (fread(buff, 1, 2, fp) < 2)
        return 0;

    return (uint16_t)buff[0] + ((uint16_t)buff[1] << 8);
}
uint8_t readUint8(FILE *fp)
{
    uint8_t val;
    if (fread(&val, 1, 1, fp) < 1)
        return 0;

    return val;
}
int readBuff(FILE *fp, int len, read_buff *buff)
{
    if(len == 0)
    {
        buff->pos = 0;
        buff->len = 0;
        buff->buff = NULL;
        return PDB_OK;
    }
    buff->buff = (uint8_t *)malloc(len);
    buff->pos = 0;
    buff->error = 0;
    if (fread(buff->buff, 1, len, fp) < len)
    {
        free(buff->buff);

        buff->buff = NULL;
        buff->len = 0;
        return PDB_EOF;
    }
    buff->len = len;
    return PDB_OK;
}
void freeBuff(read_buff *buff)
{
    if (buff->buff != NULL)
    {
        //mc_check(buff->buff,"buff check");
        free(buff->buff);
        buff->buff = NULL;
    }
}
int32_t readBuffInt32(read_buff *buff)
{

    if ((buff->len - buff->pos) < 4)
    {
        buff->error = 1;
        return 0;
    }
    int32_t ret = 0;
    for (int i = 0; i < 4; i++)
        ret += buff->buff[buff->pos + i] << (8 * i);
    buff->pos += 4;
    return ret;
}
int64_t readBuffInt64(read_buff *buff)
{

    if ((buff->len - buff->pos) < 8)
    {
        buff->error = 1;
        return 0;
    }
    int64_t ret = 0;
    for (int i = 7; i >=0; i--)
    {
        //TRY_LOG("read int64 i:%d,val:%x",i,buff->buff[buff->pos + i]);
        //ret |= buff->buff[buff->pos + i] << (8 * i);    
        ret = (ret<<8) | buff->buff[buff->pos + i];
    }    
    buff->pos += 8;
    TRY_LOG("read int64:%x%x,size:%d",(int)(ret>>32),(int)ret,sizeof(ret));
    return ret;
}
uint32_t readBuffUint32(read_buff *buff)
{
    if ((buff->len - buff->pos) < 4)
    {
        buff->error = 1;
        return 0;
    }
    uint32_t ret = 0;
    for (int i = 0; i < 4; i++)
        ret += buff->buff[buff->pos + i] << (8 * i);
    buff->pos += 4;
    return ret;
}
float readBuffFloat(read_buff *buff)
{
    if ((buff->len - buff->pos) < 4)
    {
        buff->error = 1;
        return 0;
    }
    float ret = *(float *)(buff->buff+buff->pos);
    
    buff->pos += 4;
    return ret;
}

uint16_t readBuffUint16(read_buff *buff)
{
    if ((buff->len - buff->pos) < 2)
    {
        buff->error = 1;
        return 0;
    }
    uint16_t ret = 0;
    for (int i = 0; i < 2; i++)
        ret += buff->buff[buff->pos + i] << (8 * i);
    buff->pos += 2;
    return ret;
}
uint8_t readBuffUint8(read_buff *buff)
{
    if ((buff->len - buff->pos) < 1)
    {
        buff->error = 1;
        return 0;
    }
    buff->pos += 1;
    return buff->buff[buff->pos - 1];
}

const char *readBuffString(read_buff *buff)
{
    int pos = buff->pos;
    while (pos < buff->len && buff->buff[pos] != 0)
        pos++;
    const char *ret = (const char *)(buff->buff + buff->pos);
    buff->pos = pos + 1;
    if (pos == buff->len)
    {
        buff->error = 1;
        return NULL;
    }
    return ret;
}
void copyString(char **target, const char *src)
{
    if (src == NULL)
    {
        *target = NULL;
        return;
    }
    int len = strlen(src);
    char *dest = (char *)malloc(len + 1);
    strncpy(dest, src, len);
    dest[len] = 0;
    *target = dest;
}
/*read string length less than 2048 */
char *readString(FILE *fp)
{
    char buff[2048];
    long pos = ftell(fp);
    int cnt = 0;
    int len = fread(buff, 1, 2048, fp);
    while (cnt < len && buff[cnt] != 0)
        cnt++;
    if (cnt == 2048) //buff is not enough,some error occur ,return NULL
    {
        return NULL;
    }
    fseek(fp, pos + cnt + 1, SEEK_SET);
    char *ret = (char *)malloc((cnt + 1) * sizeof(char));
    memcpy(ret, buff, cnt);
    ret[cnt] = 0;
    return ret;
}
static int tableCnt=0;
int parseHead(spdb *db)
{

    if (readUint8(db->fp) != MAGIC1 || readUint8(db->fp) != MAGIC2 || readUint16(db->fp) != VERSION)
        return PDB_FORMAT_ERROR;

    uint16_t tableNum = readUint16(db->fp);
    // TRY_LOG("tablenum:%d",tableNum);
    db->tables = hashmap_new(KEY_STRING, calcHashSize((uint32_t)tableNum));
    int cnt = 0;
    while (!feof(db->fp))
    {
        //TRY_LOG("open table pos3:%d",ftell(db->fp));
        long pos = ftell(db->fp);
        uint32_t len = readUint32(db->fp);
        if (len == 0)
        {
            //TRY_LOG("read len zero pos:%x",pos);
            break;
        }    
        uint32_t rowLen = readUint32(db->fp);
        //TRY_LOG("read table pos:%d",pos);
        char *tablename = readString(db->fp);

        struct table_data *data = (struct table_data *)calloc(1,sizeof(struct table_data));
        data->len = len;
        data->total_row = rowLen;
        data->offset = (uint32_t)pos;
        data->pb_off = ftell(db->fp) - pos;
        data->pb_len = readUint16(db->fp);
        data->mainIdx.idx = NULL;
        data->data_off = 0;
        data->tablename = tablename;
        data->pb_loaded = 0;
        data->mainIdx.ids=NULL;
        data->mainIdx.id_len = 0;
        data->mainIdx.len = 0;
        cnt++;
        tableCnt++;
        //TRY_LOG("read table name:%s,len:%d,pblen:%d,offset:%d,pos:%x,orig pos:%d,cnt:%d,total_row:%d",tablename,len,data->pb_len,data->offset,ftell(db->fp),pos,cnt,data->total_row);
        hashmap_put(db->tables, tablename, data);
        fseek(db->fp, pos + len, SEEK_SET);
    }
    return PDB_OK;
}

int protodb_open(const char *name, spdb **db)
{
    *db = (spdb *)calloc(1,sizeof(spdb));
    (*db)->fp = fopen(name, "rb");
    if ((*db)->fp == NULL)
    {
        TRY_LOG("failed to open file:%s",name);
        return PDB_IOERROR;
    }
    //TRY_LOG("open table pos:%d",ftell((*db)->fp));
    (*db)->env = pbc_new();
    return parseHead(*db);
    //luaL_argerror(L, 1, lj_strfmt_pushf(L, "%s: %s", name, strerror(errno)));
}
static inline void safeFree(void **ptr)
{
    //TRY_LOG("safeFree:%x",*ptr);
    if(*ptr != NULL)
        free(*ptr);
    *ptr = NULL;    
}
void freeIndex(void* idx,int idxLen,struct table_id *ids,int depth,int idLen)
{
        
    if(depth>=idLen)
        return; 
    //TRY_LOG("begin free index len:%d,depth:%d,idLen:%d,tp:%d",idxLen,depth,idLen,ids[depth].tp);
    switch(ids[depth].tp)
    {
        case ID_STRING:
        {
            table_record_string_index *stridx = (table_record_string_index*)idx;
            for(int i=0;i<idxLen;i++)
            {
                //TRY_LOG("free key:%s,i:%d",stridx[i].key,i);
                safeFree((void**)&stridx[i].key);
                if(stridx[i].p.child != NULL)
                {   
                    freeIndex(stridx[i].p.child,stridx[i].p.child_len,ids,depth+1,idLen);
                    safeFree((void**)&stridx[i].p.child);
                }

            }
            //TRY_LOG("free end");
            break;
        }
        case ID_INTEGER:
        {
            table_record_index *iidx = (table_record_index*)idx;
            for(int i=0;i<idxLen;i++)
            {
                
                if(iidx[i].child != NULL)
                {    
                    freeIndex(iidx[i].child,iidx[i].child_len,ids,depth+1,idLen);
                    safeFree((void**)&iidx[i].child);
                }
            }
            break;
        }
    }    
}
static int
free_table(void *p,void *item) {
    struct table_data *tb = (struct table_data *)item;
    tableCnt--;  
    if(tb == NULL)
        return MAP_OK;
      
    //TRY_LOG("begin free:%s",tb->tablename);    
    safeFree((void**)&tb->tablename); 
    //TRY_LOG("begin free 1");
    if(tb->mainIdx.idx != NULL) 
        freeIndex(tb->mainIdx.idx,tb->mainIdx.len,tb->mainIdx.ids,0,tb->mainIdx.id_len);
    //TRY_LOG("begin free 2:%x",tb->mainIdx.idx);    
    safeFree((void**)&tb->mainIdx.idx);//TRY_LOG("begin free 3");
    for(int i=0;i<tb->mainIdx.id_len;i++)
        safeFree((void**)&tb->mainIdx.ids[i].name);      
           
    //TRY_LOG("begin free 4");    
    safeFree((void**)&tb->mainIdx.ids);
    for(int i=0;i<tb->head_item_len;i++)
        safeFree((void**)&tb->head_items[i].field);
    safeFree((void**)&tb->head_items);   
    tb->head_item_len = 0;
    //TRY_LOG("begin free 5");    
    safeFree((void**)&tb);
    return MAP_OK;
}
int protodb_close(spdb *db)
{
    TRY_LOG("protodb_close db->fp:%d",db->fp);
    if (db->fp != NULL)
    {
        fclose(db->fp);
        db->fp = NULL;
    }
    TRY_LOG("before release:%d,tables len:%d,tableCnt:%d",mc_count(),hashmap_length(db->tables),tableCnt);  
    // if(db->env != NULL)
    //     pbc_delete(db->env);
    // db->env = NULL;    
    if(db->tables != NULL)
        hashmap_iterate(db->tables,free_table,NULL);
    hashmap_free(db->tables);
    db->tables=NULL;
    free(db);
    TRY_LOG("after release:%d,tableCnt:%d",mc_count(),tableCnt);     
    return PDB_OK;
}
struct pbc_env *protodb_getEnv(spdb *db)
{
    return db->env;
}
int loadTableHead(spdb *db,struct table_data *tb)
{

    uint32_t off = tb->offset + tb->pb_off+PB_LEN_SIZE;
    //TRY_LOGTRY_LOG("loadTablePB off:%x",off);
    fseek(db->fp, off, SEEK_SET);
    if (feof(db->fp))
        return PDB_EOF;
    read_buff buff;
    //TRY_LOG("read buff from:%x,len:%d",ftell(db->fp),len-4);
    if (PDB_OK != readBuff(db->fp, tb->pb_len-PB_LEN_SIZE, &buff))
    {
        return PDB_EOF;
    }    
    uint16_t len =  readBuffUint16(&buff);
    
    head_item *items = (head_item*)calloc(len,sizeof(head_item));
    for(int i=0;i<len;i++)
    {

        const char* str = readBuffString(&buff);
        copyString(&items[i].field,str);
        items[i].tp = readBuffUint8(&buff); 
    }
    tb->head_items = items;
    tb->head_item_len = len;
    freeBuff(&buff);
    return PDB_OK;
}
int loadTableProto(spdb *db, struct table_data *tb)
{

    uint32_t off = tb->offset + tb->pb_off+PB_LEN_SIZE;
    //TRY_LOGTRY_LOG("loadTablePB off:%x",off);
    fseek(db->fp, off, SEEK_SET);
    if (feof(db->fp))
        return PDB_EOF;
    char *buff = (char *)malloc(tb->pb_len-PB_LEN_SIZE);
    if (fread(buff, 1, tb->pb_len-PB_LEN_SIZE, db->fp) < tb->pb_len-PB_LEN_SIZE)
    {
        free(buff);
        return PDB_EOF;
    }

    struct pbc_slice slice;
    slice.buffer = (void *)buff;
    slice.len = (int)tb->pb_len-PB_LEN_SIZE;
    /*
    FILE *fp = fopen("d:\\proto.db","wb");
    fwrite(buff,1,tb->pb_len,fp);
    fclose(fp);
    */
    int ret = pbc_register(db->env, &slice);
    free(buff);
    if (ret)
    {
        //TRY_LOG("register pb error:%s", pbc_error(db->env));
        return PDB_TABLE_PB_ERROR;
    }

    return PDB_OK;
}
int readSubIdx(read_buff *buff, table_index_item *item, int depth, void **m,uint32_t *subBlockLen,uint32_t *idlen)
{
    if (depth < item->id_len)
    {
        switch (item->ids[depth].tp)
        {
        case ID_INTEGER:
        {
            *idlen = readBuffUint32(buff);
            *subBlockLen = readBuffUint32(buff);
            //TRY_LOG("read subblocklen:%d,buff pos:%d,depth:%d",*subBlockLen,buff->pos,depth);
            table_record_index *records = (table_record_index *)calloc(*idlen, sizeof(table_record_index));
            *m = records;
            //*m = hashmap_new(KEY_STRING,calcHashSize(cnt));

            int ret = readIntIdx(buff, item, records, depth, *idlen);
            if (ret != PDB_OK)
            {
                free(*m);

                return ret;
            }
            return PDB_OK;
            break;
        }
        case ID_STRING: 
        {
            *idlen = readBuffUint32(buff);
            *subBlockLen = readBuffUint32(buff);
            table_record_string_index *records = (table_record_string_index *)calloc(*idlen, sizeof(table_record_string_index));
            *m = records;

            int ret = readStringIdx(buff, item, records, depth, *idlen);
            if (ret != PDB_OK)
            {
                free(*m);
                return ret;
            }
            return PDB_OK;
            break;
        }
        default:
            return PDB_UNKNOWN_ID_TYPE;
        }
    }
    else
    {
        //TRY_LOG("readSubIdx depth < tb->id_len,depth:%d,tb->id_len:%d",depth,tb->id_len);
        return PDB_FORMAT_ERROR;
    }   
}

static int readIntIdx(read_buff *buff, table_index_item *item, table_record_index *records, int depth, int cnt)
{
    int i = 0;
    while (i < cnt && buff->pos < buff->len)
    {

        int32_t val = readBuffInt32(buff);
        uint32_t off = readBuffUint32(buff);
        //TRY_LOG("readIntIdx depth:%d,val:%d,off:%x,buff pos:%d",depth,val,off,buff->pos);
        if (buff->error)
        {

            return PDB_EOF;
        }
        if (off&LEAF_NODE_MASK) //sub node
        {
            void *m;
            uint32_t blocklen;
            uint32_t idlen;
            int ret = readSubIdx(buff, item, depth+1, &m,&blocklen,&idlen);
            if(PDB_OK != ret)
                return ret;
             
            records[i].child = m;
            records[i].id = val; 
            records[i].offset = off&  LEAF_NODE_VALUE ;
            records[i].block_len = blocklen;
            records[i].child_len = idlen;
            //if(i == 1308)
            //    TRY_LOG("readd subnode block_len:%d,i:%d,off:%d,id:%d,m:%x,records[i]:%x,buff pos:%x",records[i].block_len,i,records[i].offset,val,m,&records[i],buff->pos);
            
        }
        else //leaf node
        {
           records[i].id = val;
           records[i].offset = off ;
            // struct table_record* record = (struct table_record*)malloc(sizeof(struct table_record));
            // record->offset = off-RECORD_OFF_START;
            // ihashmap_put(root,val,record);
        }
        
        i++;
    }
    //TRY_LOG("readintindex rend pos:%d,len:%d,i:%d,depth:%d",buff->pos,buff->len,i,depth);
    return i>=cnt ? PDB_OK : PDB_EOF;
}
static int readStringIdx(read_buff *buff, table_index_item *item, table_record_string_index * records, int depth, int cnt)
{
    int i = 0;
    //TRY_LOG("readStringIdx pos:%d,str:%s,cnt:%d",buff->pos,(char*)(buff->buff+buff->pos),cnt);
    const char * k = (const char*)(buff->buff+buff->pos);
    //mc_check(records,"begin");
    while (i < cnt && buff->pos < buff->len)
    {
        
        const char *val = readBuffString(buff);
        uint32_t off = readBuffUint32(buff);
        if (buff->error)
        {

            return PDB_FORMAT_ERROR;
        }
        if (off & LEAF_NODE_MASK) //sub node
        {
            void *m;
            uint32_t blocklen;
            uint32_t idlen;
            int ret = readSubIdx(buff, item, depth+1, &m,&blocklen,&idlen);
            if (PDB_OK == ret)
            {
                uint32_t hashCode = calcStringHash(val);
                records[i].p.child = m;
                records[i].p.id = hashCode; 
                records[i].p.offset = off &LEAF_NODE_VALUE;
                copyString(&(records[i].key),val);
                records[i].p.block_len = blocklen;
                records[i].p.child_len = idlen;
            }    
            else
                return ret; 
        }
        else //leaf node
        {
            uint32_t hashCode = calcStringHash(val);
            //TRY_LOG("i:%d,pos:%d,node val:%s,%x,hashcode:%ud,off:%d,k:%s,%x",i,buff->pos,val,val,hashCode,off,k,k);
            records[i].p.id = hashCode;
            records[i].p.offset = off ;
            records[i].p.block_len = 0;
            records[i].p.child = NULL;
            records[i].p.child_len = 0;
            copyString(&(records[i].key),val);
        }
        i++;
        //mc_check(records,"loop");
    }
    //TRY_LOG("readstringindex end i:%d,cnt:%d",i,cnt);
    return i>=cnt ? PDB_OK : PDB_EOF;
}
int loadOneIndex(spdb *db, table_index_item *item,int *idxDataLen)
{
    CHECK_EOF(db->fp)
    uint32_t len = readUint32(db->fp);
    //TRY_LOG("loadRecordIndex len:%d,off:%x",len,off);
    memset(item,0,sizeof(table_index_item));
    if (len == 0)
    {
        *idxDataLen = 4;
        return PDB_OK;
    }    
    *idxDataLen = len;
    read_buff buff;
    //TRY_LOG("read buff from:%x,len:%d",ftell(db->fp),len-4);
    if (PDB_OK != readBuff(db->fp, len - 4, &buff))
    {
        return PDB_EOF;
    }

    uint16_t idlen = readBuffUint8(&buff);
    if (idlen == 0)
    {
        freeBuff(&buff);
        return PDB_FORMAT_ERROR;
    }

    item->ids = (struct table_id *)calloc(idlen , sizeof(struct table_id));
    item->id_len = idlen;
    //TRY_LOG("idlen:%d,pos:%d",idlen,buff.pos);
    for (int i = 0; i < idlen; i++)
    {
        const char *name = readBuffString(&buff);
        uint8_t tp = readBuffUint8(&buff);
        if (buff.error)
        {
            freeBuff(&buff);
            return PDB_EOF;
        }
        copyString(&(item->ids[i].name), name);
        item->ids[i].tp = tp;
    }
    //TRY_LOG("begin read idx pos:%d", buff.pos);
    switch (item->ids[0].tp)
    {
    case ID_INTEGER:
    {
        uint32_t cnt = readBuffUint32(&buff);
        table_record_index *records = (table_record_index *)calloc(cnt, sizeof(table_record_index));
        item->idx = (void *)records;
        item->len = cnt;
        uint32_t blocklen = readBuffUint32(&buff);
         
        //TRY_LOG("records cnt:%d",cnt);
        //tb->records = hashmap_new(KEY_INT,calcHashSize(cnt));
        int c1 = clock();
        int ret = readIntIdx(&buff, item, records, 0, cnt);
        int c2 = clock();
        
        //TRY_LOG("readIntIdx  time cost is: %d msecs",    c2 - c1 );

        //TRY_LOG("readintidx:%d,ret:%d",hashmap_length(tb->records),ret);
        if (ret != PDB_OK)
        {
            freeBuff(&buff);
            return ret;
        }    
        break;
    }

    case ID_STRING: 
    {
        uint32_t cnt = readBuffUint32(&buff);
        //TRY_LOG("cnt:%d",cnt);
        table_record_string_index *records = (table_record_string_index *)calloc(cnt, sizeof(table_record_string_index));
        item->idx = (void *)records;
        item->len = cnt;
        uint32_t blocklen = readBuffUint32(&buff);
         
        int ret = readStringIdx(&buff, item, records, 0, cnt);
        //mc_check(records,"records");
        
        //TRY_LOG("return ret:%d",ret);
        if (ret != PDB_OK)
        {
            freeBuff(&buff);
            return ret;
        }    
        break;
    }

    default:
        freeBuff(&buff);
        return PDB_UNKNOWN_ID_TYPE;
        break;
    }
    freeBuff(&buff);
    return PDB_OK;
}
int loadRecordIndex(spdb *db, struct table_data *tb)
{
    uint32_t off = tb->offset + tb->pb_off + tb->pb_len;
    fseek(db->fp, off, SEEK_SET);
    int len = 0;
    int ret;
    tb->subIdx = NULL;
    tb->subIdxLen = 0;    
    if(PDB_OK != (ret = loadOneIndex(db,&tb->mainIdx,&len)))
    {
        return ret;
    }
    int totalLen = len;
    tb->data_off = tb->pb_off + tb->pb_len + totalLen;
    if(tb->mainIdx.id_len == 0)
        return PDB_OK;
    
    table_index_item item;    
    
    while(!feof(db->fp))
    {
        if(PDB_OK != (ret = loadOneIndex(db,&item,&len)))
        {
            return ret;
        }
        totalLen+=len;    
        if(item.id_len == 0)
        {
            break;
        }
        if(tb->subIdx == NULL)
        {
            tb->subIdx = (table_index_item *)calloc(1,sizeof(table_index_item));
            memcpy(tb->subIdx,&item,sizeof(table_index_item));
            tb->subIdxLen  =1;
        }
        else
        {
            tb->subIdxLen++;
            tb->subIdx = (table_index_item *)realloc(tb->subIdx,(tb->subIdxLen)*sizeof(table_index_item));
            memcpy(&(tb->subIdx[tb->subIdxLen-1]),&item,sizeof(table_index_item));
        }
    }    
    tb->data_off = tb->pb_off + tb->pb_len + totalLen;
    //TRY_LOG("loadRecordIndex %s main idx len:%d,subidx:%x,totalLen:%d",tb->tablename,tb->mainIdx.id_len,tb->subIdx,totalLen);
    
    //TRY_LOG("table row:%d,name:%s",row,tb->tablename);
    return PDB_OK;
}
static int findRecordIndex(void *val, void *re, int cnt)
{
    table_record_index *records = (table_record_index *)re; 
    int id = *(int *)val;
    int min = 0, max = cnt - 1;
    while (min <= max)
    {
        int half = (min + max) / 2;
        if (records[half].id == id)
            return records[half].offset;
        else if (records[half].id > id)
            max = half - 1;
        else
            min = half + 1;
    }

    return -1;
}
static void * findSubRecordIndex(void *val, void *re, int cnt,int *childLen)
{
    
    table_record_index *records = (table_record_index *)re; 
    int id = *(int *)val;
    int min = 0, max = cnt - 1;
    //TRY_LOG("findSubRecordIndex id:%d,cnt:%d,records:%x,first id:%d",id,cnt,records,records[1308].id);
    while (min <= max)
    {
        int half = (min + max) / 2;
        //TRY_LOG("findSubRecordIndex half:%d,record:%x",half,records[half]);
        //TRY_LOG("findSubRecordIndex half:%d,val:%d",half,records[half].id);
        if (records[half].id == id)
        {
            *childLen = records[half].child_len;
            //if(records[half].child == NULL)
            //    TRY_LOG("findSubRecordIndex is null half:%d,val:%d,records[half]:%x",half,records[half].id,&records[half]);
            return &records[half];
        }
            
        else if (records[half].id > id)
            max = half - 1;
        else
            min = half + 1;
    }
   
    return NULL;
}
static int findRecordStringIndex(void* val, void *re, int cnt)
{
    table_record_string_index *records = (table_record_string_index *)re; 
    const char *id = *(const char **)val;

    uint32_t h = calcStringHash(id);
    // TRY_LOG("findRecordStringIndex val:%s,h:%ud",id,h);
    int min = 0, max = cnt - 1;
    int idx = -1;
    while (min <= max)
    {
        int half = (min + max) / 2;
        if (records[half].p.id == h)
        {
            idx = half;
            break;
        }
        else if (records[half].p.id > h)
            max = half - 1;
        else
            min = half + 1;
    }
    if(idx<0)
        return -1;
    int l = idx;
    while(l>=0)
    {
        if(h != records[l].p.id)
            break;
        if(strcmp(id,records[l].key)==0)
        {
        //    TRY_LOG("find index1 id:%s,key:%s,i:%d",id, records[l].key,l);
            return records[l].p.offset;
        }    
        l--;    
    }    
    int r = idx+1;
    while(r<cnt)
    {
        if(h != records[r].p.id)
            break;
        if(strcmp(id,records[r].key)==0)
        {
            // TRY_LOG("find index id:%s,key:%s,i:%d",id,records[r].key,r);
            return records[r].p.offset;
        }    
        r++;   
    }
    
    return -1;
}
static void * findSubStringRecordIndex(void* val, void *re, int cnt,int *childLen)
{
    table_record_string_index *records = (table_record_string_index *)re; 
    const char *id = *(const char **)val;
    if(id == NULL)
        return NULL;
    uint32_t h = calcStringHash(id);
    //TRY_LOG("findRecordStringIndex val:%s,h:%ud",id,h);
    int min = 0, max = cnt - 1;
    int idx = -1;
    while (min <= max)
    {
        int half = (min + max) / 2;
        if (records[half].p.id == h)
        {
            idx = half;
            break;
        }
        else if (records[half].p.id > h)
            max = half - 1;
        else
            min = half + 1;
    }
    if(idx<0)
        return NULL;
    int l = idx;
    while(l>=0)
    {
        if(h != records[l].p.id)
            break;
        if(strcmp(id,records[l].key)==0)
        {
            // TRY_LOG("find index id:%s,key:%s,i:%d",id,records[l].key,l);
            *childLen = records[l].p.child_len;
            return &records[l];
        }    
        l--;    
    }    
    int r = idx+1;
    while(r<cnt)
    {
        if(h != records[r].p.id)
            break;
        if(strcmp(id,records[r].key)==0)
        {
            // TRY_LOG("find index id:%s,key:%s,i:%d",id,records[r].key,r);
            *childLen = records[r].p.child_len;
            return &records[r];
        }    
        r++;   
    }
    return NULL;
}
int checkTableLoad(spdb *db, struct table_data *tb)
{
    if (!tb->pb_loaded)
    {
        int ret = loadTableHead(db,tb);
        //int ret = loadTableProto(db, tb);

        if (ret != PDB_OK)
        {
            //TRY_LOG("failed to loadTableProto result:%d", ret);
            return ret;
        }
        int c1 = clock();

        ret = loadRecordIndex(db, tb);
        int c2 = clock();
        double cost = (double)(c2 - c1) / CLOCKS_PER_SEC;
        //TRY_LOG("constant CLOCKS_PER_SEC is: %d, time cost is: %f secs", CLOCKS_PER_SEC, cost);
        if (ret != PDB_OK)
        {
            //TRY_LOG("failed to loadRecordIndex result:%d", ret);
            return ret;
        }
        tb->pb_loaded = 1;
    }
    return PDB_OK;
}


static int _getById(spdb *db, const char *table, void* val,uint8_t valtp, void **data, int *datalen,   head_item ** items,int *headItemLen, int (*findFunc)(void * , void *, int ) )
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    CHECK_RESULT(checkTableLoad(db, tb));

    //TRY_LOG("pbloaded :%d",tb->pb_loaded);

    //TRY_LOG("load end");
    if (tb->mainIdx.idx == NULL)
        return PDB_NO_INDEX;
    if (tb->mainIdx.id_len <= 0)
        return PDB_NO_ID;
    if (tb->mainIdx.id_len > 1)
        return PDB_INSUFFICIENT_ID;
    if (tb->mainIdx.ids[0].tp != valtp)
        return PDB_ID_TYPE_ERROR;
    int offset = findFunc(val, tb->mainIdx.idx, tb->mainIdx.len);
    if (offset<0)
    {
        //TRY_LOG("not found record:%d", val);
        return PDB_NOT_FOUND;
    }

    uint32_t off = tb->offset + tb->data_off + offset;
    if (EOF == fseek(db->fp, off, SEEK_SET))
        return PDB_EOF;
    uint32_t len = readUint32(db->fp);
    if (len <= 4)
    {
        TRY_LOG("table:%s file len is invalid len:%d,offset:%x",tb->tablename,len,offset);
        return PDB_FORMAT_ERROR;
    }    
    len -= 4;
    void *buff = malloc(len);
    if (fread(buff, 1, len, db->fp) < len)
    {
        free(buff);
        return PDB_EOF;
    }
    *data = buff;
    *datalen = len;
    // *env = db->env;
    *items = tb->head_items;
    *headItemLen = tb->head_item_len;
    // FILE *fp =  fopen("d:\\buff.db","wb");
    // fwrite(buff,1,len,fp);
    // fclose(fp);
    //TRY_LOG("get record offset:%x,tb.offset:%x,tb.data_off:%x,data.offset:%x",off,tb->offset,tb->data_off,record->offset);
    return PDB_OK;
}

int protodb_getByIntId(spdb *db, const char *table, int val, void **data, int *datalen,   head_item ** items,int *headItemLen)
{
    return _getById(db,table,(void *)&val,ID_INTEGER, data,datalen,items,headItemLen,findRecordIndex);
}
int protodb_getById(spdb *db, const char *table, const char *val, void **data, int *datalen,   head_item ** items,int *headItemLen)
{
    return _getById(db,table,(void *)&val,ID_STRING,data,datalen,items,headItemLen,findRecordStringIndex);
}
const char *protodb_getLastError(spdb *db)
{
    return pbc_error(db->env);
}

int protodb_getAll(spdb *db, const char *table, read_buff *buff,   head_item ** items,int *headItemLen)
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    // *env = db->env;
    CHECK_RESULT(checkTableLoad(db, tb));
    uint32_t off = tb->offset + tb->data_off;
    //TRY_LOG("protodb_getAll off:%x",off);
    if (EOF == fseek(db->fp, off, SEEK_SET))
        return PDB_EOF;
    uint32_t len = tb->len - tb->data_off;
    *items = tb->head_items;
    *headItemLen = tb->head_item_len;
    //TRY_LOG("protodb_getAll read buff len:%d",len);
    return readBuff(db->fp,len,buff);
      
        
}
void * searchCascade(void *index,int idxlen,struct table_data *tb,int *depth,param_pair *idvals,int idvallen)
{
     
     //TRY_LOG("searchCascade table:%s tp:%d,depth:%d,index:%x,valtp:%d",tb->tablename,tb->mainIdx.ids[*depth].tp,*depth,index,idvals[*depth].valtp);
    if(tb->mainIdx.ids[*depth].tp != idvals[*depth].valtp )
    {
        if(tb->mainIdx.ids[*depth].tp == ID_INTEGER)
        {
            
            idvals[*depth].valtp == ID_INTEGER;
            idvals[*depth].ival =strtoul(idvals[*depth].strval,NULL,0);
        }
        else if(tb->mainIdx.ids[*depth].tp == ID_STRING)
        {
            char buff[20];
            sprintf(buff,"%ul",idvals[*depth].ival);
       
            idvals[*depth].valtp == ID_STRING_NEW;
            copyString(&idvals[*depth].nstrval,buff);

        }
        else
        {
            TRY_LOG("search %s error,type mismatch expect:%d,cur:%d",tb->tablename,tb->mainIdx.ids[*depth].tp,idvals[*depth].valtp);
            return NULL;
        }   
    }    
   
    switch(tb->mainIdx.ids[*depth].tp)
    {
        case ID_INTEGER:  
        {
            int childlen;   
            table_record_index * item =(table_record_index *) findSubRecordIndex(&idvals[*depth].ival,index,idxlen,&childlen);
            //TRY_LOG("table_record_index:%x,idvallen:%d,depth:%d",item,idvallen,*depth);
            if(item == NULL)
                return NULL;
            if(*depth<idvallen-1)
            {
                (*depth)++;
                if(item->child == NULL)
                {
                    //TRY_LOG("child is null item:%x",item);
                    return NULL;
                }
                return  searchCascade(item->child,childlen,tb,depth,idvals,idvallen);   
            }    
            return item;    
            break;
        }          
          
        case ID_STRING: 
        {
            int childlen;
            // TRY_LOG("search string index val:%x",idvals[*depth].strval);
            table_record_string_index * item = (table_record_string_index *)findSubStringRecordIndex(&idvals[*depth].strval,index,idxlen,&childlen);
            //TRY_LOG("table_record_string_index:%x,idvallen:%d,depth:%d",item,idvallen,*depth);
            if(item == NULL)
                return NULL;
            if(*depth<idvallen-1)
            {
                (*depth)++;
                if(item->p.child == NULL)
                {
                    //TRY_LOG("child is null item:%x",item);
                    return NULL;
                }
                return  searchCascade(item->p.child,childlen,tb,depth,idvals,idvallen);   
            }        
            return item;        
            break;    
        }    
        default:
            return NULL;    
    }
}
int protodb_getAllByIds(spdb *db, const char *table,param_pair *idvals,int idvallen, read_buff *buff,   head_item ** items,int *headItemLen)
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    CHECK_RESULT(checkTableLoad(db, tb));

    //TRY_LOG("pbloaded :%d",tb->pb_loaded);

    //TRY_LOG("load end");
    if (tb->mainIdx.idx == NULL)
        return PDB_NO_INDEX;
    if (tb->mainIdx.id_len <= 0)
        return PDB_NO_ID;
    if(idvallen>tb->mainIdx.id_len)
        return PDB_TOOMANY_ID;
    int depth = 0;    
    void *index = searchCascade(tb->mainIdx.idx,tb->mainIdx.len,tb,&depth,idvals,idvallen);
    if(index == NULL)
        return PDB_NOT_FOUND;

    table_record_index * tindex = (table_record_index *)index;   
    // TRY_LOG("find end:%x",index);
    int bufflen=0; 
    if(tindex->child == NULL)//leaf node
    {
        uint32_t off = tb->offset + tb->data_off + tindex->offset;
        if (EOF == fseek(db->fp, off, SEEK_SET))
            return PDB_EOF;
        uint32_t len = readUint32(db->fp);
        if (len <= 4)
            return PDB_FORMAT_ERROR;        
        bufflen = len;
        if (EOF == fseek(db->fp, off, SEEK_SET))
            return PDB_EOF;
        // TRY_LOG("find leaf node len:%x,off:%x",len,off);    
    }
    else
    {     
        uint32_t offset =tb->offset+tb->data_off+ tindex->offset;
        // TRY_LOG("find index offset:%x,block_len:%d",offset,tindex->block_len);  
        if(EOF == fseek(db->fp,offset,SEEK_SET))
            return PDB_EOF;
        bufflen = tindex->block_len;    
    }
    *items = tb->head_items;
    *headItemLen = tb->head_item_len;   
    CHECK_RESULT(readBuff(db->fp,bufflen,buff));    
    return PDB_OK;
}
int protodb_getTableHeadItem(spdb *db,const char *table, head_item **items,int *headItemLen)
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    CHECK_RESULT(checkTableLoad(db, tb));
     
    *items = tb->head_items;
    *headItemLen = tb->head_item_len;
    return PDB_OK;
}
int protodb_getByIds(spdb *db, const char *table,param_pair *idvals,int idvallen, read_buff *buff, head_item **items,int *headItemLen)
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    CHECK_RESULT(checkTableLoad(db, tb));
    
    //TRY_LOG("pbloaded :%d",tb->pb_loaded);

    //TRY_LOG("load end");
    if (tb->mainIdx.idx == NULL)
        return PDB_NO_INDEX;
    if (tb->mainIdx.id_len <= 0)
        return PDB_NO_ID;
    if(idvallen < tb->mainIdx.id_len)
        return PDB_INSUFFICIENT_ID;
    if(idvallen>tb->mainIdx.id_len)
        return PDB_TOOMANY_ID;    
    // for(int i=0;i<idvallen;i++)
    // {
    //     if(tb->mainIdx.ids[i].tp != idvals[i].valtp)
    //     {
    //         TRY_LOG("table %s,index:%d,param format error expect:%d,cur:%d",tb->tablename,i,tb->mainIdx.ids[i].tp,idvals[i].valtp);
    //         return PDB_FORMAT_ERROR;     
    //     }   
    // }    
    int depth = 0;    
    void *index = searchCascade(tb->mainIdx.idx,tb->mainIdx.len,tb,&depth,idvals,idvallen);
    if(index == NULL)
        return PDB_NOT_FOUND;
    table_record_index * tindex = (table_record_index *)index;   
    int bufflen=0; 
    if(tindex->child == NULL)//leaf node
    {
        uint32_t off = tb->offset + tb->data_off + tindex->offset;
        if (EOF == fseek(db->fp, off, SEEK_SET))
            return PDB_EOF;
        uint32_t len = readUint32(db->fp);
        if (len <= 4)
        {
            //TRY_LOG("table:%s file len is invalid len:%d,tindex->offset:%x,offset:%x",tb->tablename,len,tindex->offset,off);
            return PDB_FORMAT_ERROR;
        }      
        bufflen = len;
        if (EOF == fseek(db->fp, off, SEEK_SET))
            return PDB_EOF;
        //TRY_LOG("find leaf node len:%x,off:%x",len,off);  
        //  *env = db->env;   
        CHECK_RESULT(readBuff(db->fp,bufflen,buff));
        *headItemLen = tb->head_item_len;
        *items = tb->head_items;    
        return PDB_OK;  
    }
    else
    {     
       return PDB_INSUFFICIENT_ID;
    }
   
}

int protodb_getLen(spdb *db,const char* table,int *len)
{
    if (db->fp == NULL)
        return PDB_NOT_OPEN;
    int ret;
    struct table_data *tb;
    if (MAP_OK != (ret = hashmap_get(db->tables, table, (void **)&tb)))
    {
        //TRY_LOG("find table failed :%s,map count:%d", table, hashmap_length(db->tables));
        return PDB_NO_TABLE;
    }
    CHECK_RESULT(checkTableLoad(db, tb));
    *len =  tb->total_row;
    return PDB_OK;
}