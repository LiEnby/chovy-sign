/*
 *  dnas.c  -- Reverse engineering of iofilemgr_dnas.prx
 *               written by tpu.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "kirk_engine.h"
#include "crypto.h"
#include "psp_headers.h"
#include "amctrl.h"

/*************************************************************/

typedef struct {
	unsigned __int8  key[16];     // 00: used to decrypt data content.
	unsigned __int32 version;     // 10: always 00
	unsigned __int32 data_size;   // 14
	unsigned __int32 block_size;  // 18
	unsigned __int32 data_offset; // 1C
	unsigned __int8 unk_20[16];
}PGD_DESC;


typedef struct {
	PGD_DESC pgdesc;
	unsigned __int32 key_index;   // 0x30
	unsigned __int8  pgd_key[16]; // 0x34
	unsigned __int32 flag;        // 0x44
	unsigned __int32 flag_open;   // 0x48
	unsigned __int32 pgd_offset;  // 0x4C
	int seek_offset; // 0x50
	unsigned __int32 data_offset; // 0x54
	unsigned __int32 table_offset;// 0x58
	unsigned __int32 unk_5c;
	unsigned __int32 unk_60;
}PspIoHookParam;


unsigned __int8 dnas_key1A90[] = {0xED,0xE2,0x5D,0x2D,0xBB,0xF8,0x12,0xE5,0x3C,0x5C,0x59,0x32,0xFA,0xE3,0xE2,0x43};
unsigned __int8 dnas_key1AA0[] = {0x27,0x74,0xFB,0xEB,0xA4,0xA0, 1,0xD7,2,0x56,0x9E,0x33,0x8C,0x19,0x57,0x83};

__declspec(dllexport) int process_pgd(unsigned __int8 *pgd_buf, int pgd_size, int pgd_flag)
{
	MAC_KEY mkey;
	CIPHER_KEY ckey;
	unsigned __int8 *fkey, vkey[16];
	int key_index, mac_type, cipher_type, drm_type;
	int retv, file_size, block_size, data_offset, table_size, align_size;


	key_index = *(unsigned __int32*)(pgd_buf+4);
	drm_type  = *(unsigned __int32*)(pgd_buf+8);

	if(drm_type==1){
		mac_type = 1;
		pgd_flag |= 4;
		if(key_index>1){
			mac_type = 3;
			pgd_flag |= 8;
		}
		cipher_type = 1;
	}else{
		mac_type = 2;
		cipher_type = 2;
	}

	// select fixed key
	fkey = NULL;
	if(pgd_flag&2)
		fkey = dnas_key1A90;
	if(pgd_flag&1)
		fkey = dnas_key1AA0;
	if(fkey==NULL){
		printf("invalid pgd_flag! %08x\n", pgd_flag);
		return -1;
	}

	// MAC_0x80 check
	sceDrmBBMacInit(&mkey, mac_type);
	sceDrmBBMacUpdate(&mkey, pgd_buf+0x00, 0x80);
	retv = sceDrmBBMacFinal2(&mkey, pgd_buf+0x80, fkey);
	if(retv){
		printf("MAC_80 check failed!: %08x(%d)\n", retv, retv);
		return -2;
	}

	// MAC_0x70
	sceDrmBBMacInit(&mkey, mac_type);
	sceDrmBBMacUpdate(&mkey, pgd_buf+0x00, 0x70);
	bbmac_getkey(&mkey, pgd_buf+0x70, vkey);
	hex_dump("Get version_key from MAC_70:", vkey, 16);

	// decrypt PGD_DESC
	sceDrmBBCipherInit(&ckey, cipher_type, 2, pgd_buf+0x10, vkey, 0);
	sceDrmBBCipherUpdate(&ckey, pgd_buf+0x30, 0x30);
	sceDrmBBCipherFinal(&ckey);
	hex_dump("PGD header", pgd_buf, 0x90);

	file_size   = *(unsigned __int32*)(pgd_buf+0x44);
	block_size  = *(unsigned __int32*)(pgd_buf+0x48);
	data_offset = *(unsigned __int32*)(pgd_buf+0x4c);

	file_size = (file_size+15)&~15;
	align_size = (file_size+block_size-1)&~(block_size-1);
	table_size = align_size/block_size;
	table_size *= 16;

	if(file_size+table_size>pgd_size){
		printf("invalid pgd data!\n");
		return -3;
	}

	// table MAC check
	sceDrmBBMacInit(&mkey, mac_type);
	sceDrmBBMacUpdate(&mkey, pgd_buf+data_offset+file_size, table_size);
	retv = sceDrmBBMacFinal(&mkey, pgd_buf+0x60, vkey);
	if(retv){
		printf("MAC_table check failed!: %08x(%d)\n", retv, retv);
		return -4;
	}

	// decrypt data
	sceDrmBBCipherInit(&ckey, cipher_type, 2, pgd_buf+0x30, vkey, 0);
	sceDrmBBCipherUpdate(&ckey, pgd_buf+0x90, file_size);
	sceDrmBBCipherFinal(&ckey);
	hex_dump("PGD data", pgd_buf+0x90, (file_size>0x100)? 0x100 : file_size);

	file_size   = *(unsigned __int32*)(pgd_buf+0x44);
	return file_size;
}

int decrypt_pgd(unsigned __int8 *pgd_buf, int pgd_size, int pgd_flag, unsigned __int8 *pgd_vkey);

__declspec(dllexport) int process_pgd_file(char *pgd_file)
{
	unsigned __int8 *data_buf, *pgd_buf;
	char fname[256];
	int retv, data_size, pgd_size, pgd_flag;

	data_buf = load_file(pgd_file, &data_size);
	if(data_buf==NULL){
		printf("Open input file <%s> error!\n", pgd_file);
		return -1;
	}
	if(data_size<0x90){
		free(data_buf);
		return -1;
	}

	if(*(unsigned __int32*)(data_buf+0)==0x44475000){
		pgd_buf = data_buf;
		pgd_size = data_size;
	}else if(*(unsigned __int32*)(data_buf+0x90)==0x44475000){
		pgd_buf = data_buf+0x90;
		pgd_size = data_size-0x90;
	}else{
		free(data_buf);
		return -1;
	}
	printf("\nProcess %s ...\n", pgd_file);

	// 0x40xxxxxx : 2
	// 0x44xxxxxx : 1
	// default as 2
	pgd_flag = 2;

	retv = decrypt_pgd(pgd_buf, pgd_size, pgd_flag, NULL);
	if(retv>0){
		sprintf(fname, "%s.decrypt", pgd_file);
		write_file(fname, pgd_buf+0x90, retv);
		printf("Save %s ...\n", fname);
	}

	free(data_buf);
	return 0;
}



