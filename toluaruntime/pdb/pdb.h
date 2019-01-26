#ifndef PDB_C_H
#define PDB_C_H
#include "lua.h"
#include "pbc.h"
typedef enum {
    PDB_OK = 0,
    PDB_NOT_OPEN,
    PDB_IOERROR ,
    PDB_FORMAT_ERROR ,
    PDB_EOF ,
    PDB_TABLE_PB_ERROR ,/* table protbuff register error*/
    PDB_NO_TABLE,/*table not found for this name*/
    PDB_NO_INDEX,/*cannot get by id from table without ids*/
    PDB_UNKNOWN_ID_TYPE,
    PDB_NO_ID,
    PDB_ID_TYPE_ERROR,/*query id type mismatch */
    PDB_INSUFFICIENT_ID,/*id is not enough to query */
    PDB_NOT_FOUND,
    PDB_PROTO_ERROR,
    PDB_TOOMANY_ID,

} PDB_RESULT;
typedef struct
{
    uint8_t *buff;
    int pos;
    int len;
    uint8_t error;
} read_buff; 
enum ID_TYPE
{    
    ID_INTEGER = 1,
    ID_STRING = 2,
    ID_INT64=3,
    ID_STRING_NEW = 4,//分配的字符串，需要手动释放
};
enum FIELD_TYPE
{
    FIELD_TYPE_INTEGER = 1,
    FIELD_TYPE_STRING = 2,
    FIELD_TYPE_INT64 = 3,
    FIELD_TYPE_FLOAT = 4
};
typedef struct
{
    union{
        lua_Integer ival;
        const char * strval;
        char *nstrval;
    };
 
    int valtp;
}param_pair;
typedef struct
{
    char *field;
    int tp;
} head_item;

typedef struct spdb spdb;
int protodb_open( const char* name,spdb ** db);
int protodb_close(spdb *db);
int protodb_getByIntId(spdb *db,const char* table,int val,void **data,int *datalen,   head_item ** items,int *headItemLen);
int protodb_getById(spdb *db, const char *table, const char *val, void **data, int *datalen,   head_item ** items,int *headItemLen);
int protodb_getAll(spdb *db, const char *table, read_buff *buff,   head_item ** items,int *headItemLen);
int protodb_getAllByIds(spdb *db, const char *table,param_pair *idvals,int idvallen, read_buff *buff,   head_item ** items,int *headItemLen);
int protodb_getByIds(spdb *db, const char *table,param_pair *idvals,int idvallen, read_buff *buff, head_item **items,int *headItemLen);
int protodb_getLen(spdb *db,const char* table,int *len);
int protodb_getTableHeadItem(spdb *db,const char *table, head_item **items,int *headItemLen);

const char * protodb_getLastError(spdb *db);
struct pbc_env *protodb_getEnv(spdb *db);

void freeBuff(read_buff *buff);
int32_t readBuffInt32(read_buff *buff);
uint32_t readBuffUint32(read_buff *buff);
int64_t readBuffInt64(read_buff *buff);
uint16_t readBuffUint16(read_buff *buff);
uint8_t readBuffUint8(read_buff *buff);
float readBuffFloat(read_buff *buff);
const char *readBuffString(read_buff *buff);

#endif