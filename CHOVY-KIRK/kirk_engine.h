#ifndef KIRK_ENGINE
#define KIRK_ENGINE

typedef unsigned char u8;
typedef unsigned short int u16;
typedef unsigned int u32;

//Kirk return values
#define KIRK_OPERATION_SUCCESS 0
#define KIRK_NOT_ENABLED 1
#define KIRK_INVALID_MODE 2
#define KIRK_HEADER_HASH_INVALID 3
#define KIRK_DATA_HASH_INVALID 4
#define KIRK_SIG_CHECK_INVALID 5
#define KIRK_UNK_1 6
#define KIRK_UNK_2 7
#define KIRK_UNK_3 8
#define KIRK_UNK_4 9
#define KIRK_UNK_5 0xA
#define KIRK_UNK_6 0xB
#define KIRK_NOT_INITIALIZED 0xC
#define KIRK_INVALID_OPERATION 0xD
#define KIRK_INVALID_SEED_CODE 0xE
#define KIRK_INVALID_SIZE 0xF
#define KIRK_DATA_SIZE_ZERO 0x10

typedef struct
{
	int mode;    //0
	int unk_4;   //4
	int unk_8;   //8
	int keyseed; //C
	int data_size;   //10
} KIRK_AES128CBC_HEADER; //0x14

typedef struct
{
	u8 AES_key[16];            //0
	u8 CMAC_key[16];           //10
	u8 CMAC_header_hash[16];   //20
	u8 CMAC_data_hash[16];     //30
	u8 unused[32];             //40
	u32 mode;                  //60
	u32 ecdsa;                 //64
	u8 unk3[8];                //68
	u32 data_size;             //70
	u32 data_offset;           //74  
	u8 unk4[8];                //78
	u8 unk5[16];               //80
} KIRK_CMD1_HEADER; //0x90

typedef struct
{
	u8 r[0x14];
	u8 s[0x14];
} ECDSA_SIG;

typedef struct
{
	u8 x[0x14];
	u8 y[0x14];
} ECDSA_POINT;


typedef struct
{
    u32 data_size;             //0     
} KIRK_SHA1_HEADER;            //4

typedef struct
{
	u8 private_key[0x14];
	ECDSA_POINT public_key;
} KIRK_CMD12_BUFFER;

typedef struct
{
	u8 multiplier[0x14];
	ECDSA_POINT public_key;
} KIRK_CMD13_BUFFER;

typedef struct
{
	u8 enc_private[0x20];
	u8 message_hash[0x14];
} KIRK_CMD16_BUFFER;

typedef struct
{
	ECDSA_POINT public_key;
	u8 message_hash[0x14];
	ECDSA_SIG signature;
} KIRK_CMD17_BUFFER;


// sceUtilsBufferCopyWithRange modes
#define KIRK_CMD_DECRYPT_PRIVATE 1
#define KIRK_CMD_2 2
#define KIRK_CMD_3 3
#define KIRK_CMD_ENCRYPT_IV_0 4
#define KIRK_CMD_ENCRYPT_IV_FUSE 5
#define KIRK_CMD_ENCRYPT_IV_USER 6
#define KIRK_CMD_DECRYPT_IV_0 7
#define KIRK_CMD_DECRYPT_IV_FUSE 8
#define KIRK_CMD_DECRYPT_IV_USER 9
#define KIRK_CMD_PRIV_SIGN_CHECK 10
#define KIRK_CMD_SHA1_HASH 11
#define KIRK_CMD_ECDSA_GEN_KEYS 12
#define KIRK_CMD_ECDSA_MULTIPLY_POINT 13
#define KIRK_CMD_PRNG 14
#define KIRK_CMD_15 15
#define KIRK_CMD_ECDSA_SIGN 16
#define KIRK_CMD_ECDSA_VERIFY 17

//"mode" in header
#define KIRK_MODE_CMD1 1
#define KIRK_MODE_CMD2 2
#define KIRK_MODE_CMD3 3
#define KIRK_MODE_ENCRYPT_CBC 4
#define KIRK_MODE_DECRYPT_CBC 5

//sceUtilsBufferCopyWithRange errors
#define SUBCWR_NOT_16_ALGINED 0x90A
#define SUBCWR_HEADER_HASH_INVALID 0x920
#define SUBCWR_BUFFER_TOO_SMALL 0x1000

/*
      // Private Sig + Cipher
      0x01: Super-Duper decryption (no inverse)
      0x02: Encrypt Operation (inverse of 0x03)
      0x03: Decrypt Operation (inverse of 0x02)

      // Cipher
      0x04: Encrypt Operation (inverse of 0x07) (IV=0)
      0x05: Encrypt Operation (inverse of 0x08) (IV=FuseID)
      0x06: Encrypt Operation (inverse of 0x09) (IV=UserDefined)
      0x07: Decrypt Operation (inverse of 0x04)
      0x08: Decrypt Operation (inverse of 0x05)
      0x09: Decrypt Operation (inverse of 0x06)
	  
      // Sig Gens
      0x0A: Private Signature Check (checks for private SCE sig)
      0x0B: SHA1 Hash
      0x0C: Mul1
      0x0D: Mul2
      0x0E: Random Number Gen
      0x0F: (absolutely no idea – could be KIRK initialization)
      0x10: Signature Gen
      // Sig Checks
      0x11: Signature Check (checks for generated sigs)
      0x12: Certificate Check (idstorage signatures)
*/

//kirk-like funcs
int kirk_CMD0(void* outbuff, void* inbuff, int size);
int kirk_CMD1(void* outbuff, void* inbuff, int size);
int kirk_CMD4(void* outbuff, void* inbuff, int size);
int kirk_CMD7(void* outbuff, void* inbuff, int size);
int kirk_CMD10(void* inbuff, int insize);
int kirk_CMD11(void* outbuff, void* inbuff, int size);
int kirk_CMD14(void* outbuff, int size);
__declspec(dllexport) int kirk_init(); //CMD 0xF?

//helper funcs
u8* kirk_4_7_get_key(int key_type);

//kirk "ex" functions
int kirk_CMD1_ex(void* outbuff, void* inbuff, int size, KIRK_CMD1_HEADER* header);

//sce-like funcs
int sceUtilsSetFuseID(void*fuse);
__declspec(dllexport) int sceUtilsBufferCopyWithRange(void* outbuff, int outsize, void* inbuff, int insize, int cmd);

/* ECC Curves for Kirk 1 and Kirk 0x11 */
// Common Curve paramters p and a
static u8 ec_p[0x14] = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
static u8 ec_a[0x14] = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC }; // mon

// Kirk 0xC,0xD,0x10,0x11,(likely 0x12)- Unique curve parameters for b, N, and base point G for Kirk 0xC,0xD,0x10,0x11,(likely 0x12) service
// Since public key is variable, it is not specified here
static u8 ec_b2[0x14] = { 0xA6, 0x8B, 0xED, 0xC3, 0x34, 0x18, 0x02, 0x9C, 0x1D, 0x3C, 0xE3, 0x3B, 0x9A, 0x32, 0x1F, 0xCC, 0xBB, 0x9E, 0x0F, 0x0B };// mon
static u8 ec_N2[0x15] = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xB5, 0xAE, 0x3C, 0x52, 0x3E, 0x63, 0x94, 0x4F, 0x21, 0x27 };
static u8 Gx2[0x14] = { 0x12, 0x8E, 0xC4, 0x25, 0x64, 0x87, 0xFD, 0x8F, 0xDF, 0x64, 0xE2, 0x43, 0x7B, 0xC0, 0xA1, 0xF6, 0xD5, 0xAF, 0xDE, 0x2C };
static u8 Gy2[0x14] = { 0x59, 0x58, 0x55, 0x7E, 0xB1, 0xDB, 0x00, 0x12, 0x60, 0x42, 0x55, 0x24, 0xDB, 0xC3, 0x79, 0xD5, 0xAC, 0x5F, 0x4A, 0xDF };

// KIRK 1 - Unique curve parameters for b, N, and base point G
// Since public key is hard coded, it is also included
static u8 ec_b1[0x14] = { 0x65, 0xD1, 0x48, 0x8C, 0x03, 0x59, 0xE2, 0x34, 0xAD, 0xC9, 0x5B, 0xD3, 0x90, 0x80, 0x14, 0xBD, 0x91, 0xA5, 0x25, 0xF9 };
static u8 ec_N1[0x15] = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0xB5, 0xC6, 0x17, 0xF2, 0x90, 0xEA, 0xE1, 0xDB, 0xAD, 0x8F };
static u8 Gx1[0x14] = { 0x22, 0x59, 0xAC, 0xEE, 0x15, 0x48, 0x9C, 0xB0, 0x96, 0xA8, 0x82, 0xF0, 0xAE, 0x1C, 0xF9, 0xFD, 0x8E, 0xE5, 0xF8, 0xFA };
static u8 Gy1[0x14] = { 0x60, 0x43, 0x58, 0x45, 0x6D, 0x0A, 0x1C, 0xB2, 0x90, 0x8D, 0xE9, 0x0F, 0x27, 0xD7, 0x5C, 0x82, 0xBE, 0xC1, 0x08, 0xC0 };


#endif
