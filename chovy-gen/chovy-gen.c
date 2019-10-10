//Project Chovy - __sce_ebootpbp generator by @dots_tb and motoharu
// With CBPS help especially: @SiliCart, @nyaaasen, @notzecoxao (and his friends?)

//Check out motoharu's project: https://github.com/motoharu-gosuto/psvcmd56/blob/master/src/CMD56Reversed/F00D/GcAuthMgrService.cpp#L1102
#define PACK( __Declaration__ ) __pragma( pack(push, 1) ) __Declaration__ __pragma( pack(pop) )

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <inttypes.h>

#include <openssl/evp.h>
#include <openssl/hmac.h>
#include <openssl/sha.h>
#include <openssl/obj_mac.h>
#include <openssl/ec.h>

#include "key_vault.h"
PACK(typedef struct
{
	uint8_t r[0x1c];
	uint8_t s[0x1c];
}  ECDSA_SIG_0x1c);

PACK(typedef struct sce_ebootpbp {
	uint64_t magic;
	uint32_t key_type;// set to 1 (maybe keytype?)
	uint32_t type;// 03 - ps1,  02 - psp
	uint8_t np_title[0x30];
	uint64_t aid;
	uint64_t secure_tick;
	uint64_t filesz;
	uint64_t sw_ver;
	uint8_t padding[0xf8];
	ECDSA_SIG_0x1c ebootpbp_hdr_sig;
	ECDSA_SIG_0x1c NPUMDIMG_sig; 
	ECDSA_SIG_0x1c sceebootpbp_sig;
} sce_ebootpbp);


typedef struct pbp_hdr {
	uint32_t magic;
	uint32_t unk;
	uint32_t sfo_offset;
	uint32_t icon0_offset;
	uint32_t icon1_offset;
	uint32_t pic0_offset;
	uint32_t pic1_offset;
	uint32_t snd0_offset;
	uint32_t data_psp_offset;
	uint32_t data_psar_offset;
} pbp_hdr;


#define PSAR_SZ 0x1C0000
#define WORK_BUF_SZ 0x7c0


//based motoharu
__declspec(dllexport) int can_be_reversed_80C17A(const uint8_t* src, int some_size, uint8_t* iv, uint8_t* src_xored_digest)
{
   unsigned char src_xored[0x20];
	memcpy(src_xored, iv, 0x20);

   if (some_size > 0x20)
      return 0x12;

   for(int i = 0; i < some_size; i++)
      src_xored[i] = src[i] ^ iv[i];

   int r0;
   
   SHA256_CTX sha256_ctx;
   SHA256_Init(&sha256_ctx);
   SHA256_Update(&sha256_ctx, src_xored, 0x20);
   r0 = SHA256_Final(src_xored_digest, &sha256_ctx);
   if(r0 != 1)
      return 0x11;

   for(int i = 0; i < 0x20; i++)
      iv[i] = src_xored_digest[i] ^ iv[i];

   for(int i = 0; i < 0x20; i++)
   {
      if(iv[i] != 0xFF)
      {
         iv[i] = iv[i] + 1;
         break;
      }
      
      iv[i] = 0;
   }

   return 0;
}


__declspec(dllexport) int f00d_KIRK0x22(const uint8_t *hash, ECDSA_SIG_0x1c *signature, int key_sel) {
	
    
	uint8_t hmac_in[0x38];
	uint8_t hmac_hash_iv[0x38];
	memcpy(&hmac_in, hash, 0x1c);
	memcpy(&hmac_in[0x1c], &keyvault_ec_privkey[key_sel], 0x1c);
	
	HMAC_CTX *hmac_ctx = HMAC_CTX_new();
	HMAC_Init_ex(hmac_ctx, &hmac_key_0x22, 0x40, EVP_sha256(), NULL);
    HMAC_Update(hmac_ctx, &hmac_in, 0x1c << 1);
	unsigned int  len;
    HMAC_Final(hmac_ctx, &hmac_hash_iv, &len);
	HMAC_CTX_free(hmac_ctx);
	
	uint8_t sha256_out[0x40];
	int ret = 0;
	do {
		ret = can_be_reversed_80C17A(hash, 0x1c, hmac_hash_iv, &sha256_out[0]);
		if(ret != 0 || (ret = can_be_reversed_80C17A(hash, 0x1c, hmac_hash_iv, &sha256_out[0x20])) != 0) 
			return 0;
		
	} while(ret != 0);
	
	//ECDSA
	
    BIGNUM  *a = BN_bin2bn(keyvault_ec_a, 0x1c, NULL),
			*b = BN_bin2bn(keyvault_ec_b, 0x1c, NULL), 
			*p = BN_bin2bn(keyvault_ec_p, 0x1c, NULL), 
			*order  = BN_bin2bn(keyvault_ec_N, 0x1c, NULL), 
			*x = BN_bin2bn(keyvault_ec_Gx, 0x1c, NULL), 
			*y = BN_bin2bn(keyvault_ec_Gy, 0x1c, NULL), 
			*priv_key = BN_bin2bn(keyvault_ec_privkey[key_sel], 0x1c, NULL), 
			*m = BN_bin2bn(sha256_out, 0x3c, NULL);
			
	BN_CTX *bn_ctx = BN_CTX_new();
	BN_MONT_CTX *bn_mon_ctx = BN_MONT_CTX_new();
	BN_MONT_CTX_set(bn_mon_ctx, order, bn_ctx);
	
	EC_GROUP *curve = EC_GROUP_new_curve_GFp(p, a, b, bn_ctx);
	EC_POINT *generator = EC_POINT_new(curve);
    EC_POINT_set_affine_coordinates_GFp(curve, generator, x, y, bn_ctx);
    EC_GROUP_set_generator(curve, generator, order, NULL);

	EC_KEY *eckey=EC_KEY_new();
	EC_KEY_set_group(eckey,curve);
	EC_KEY_set_private_key(eckey, priv_key);
	

	m = BN_bin2bn(sha256_out, 0x3c, NULL);
	BN_mod(m, m, order, bn_ctx);
		
	
	
	//Generate R in order to get custom "random number"
	BIGNUM *sig_r = BN_new();
	EC_POINT_mul(curve, generator, m, NULL, NULL, bn_ctx);
	EC_POINT_get_affine_coordinates_GFp(curve, generator, sig_r, NULL, bn_ctx);
	BN_nnmod(sig_r, sig_r, order, bn_ctx);
	
	//Generate M^-1
	BIGNUM *exp = BN_new();
	BIGNUM *minv = BN_new();
	
	BN_set_word(exp, (BN_ULONG)2);
    BN_sub(exp, order, exp);
	BN_mod_exp_mont(minv, m, exp, order, bn_ctx, bn_mon_ctx);
	
	
	ECDSA_SIG *sig = ECDSA_do_sign_ex(hash, 0x1c,  minv, sig_r, eckey);

	
	if(!sig) {
		ret = 0;
		goto error;
		
	}
	BIGNUM *sig_s;
	ECDSA_SIG_get0(sig, NULL, &sig_s);
	BN_bn2bin(sig_r, &signature->r);
	BN_bn2bin(sig_s, &signature->s);
	ECDSA_SIG_free(sig);
	//BN_free(sig_s);
	ret = 1;
	
error:
	BN_free(sig_r);
	
	EC_POINT_free(generator);
    BN_free(y);
    BN_free(x);
    BN_free(order);
    BN_free(p);
    BN_free(b);
    BN_free(a);
    BN_free(minv);
    BN_free(exp);
    BN_free(priv_key);
    BN_CTX_free(bn_ctx);
    BN_MONT_CTX_free(bn_mon_ctx);
	return ret;

}


__declspec(dllexport) int chovy_gen(char *ebootpbp, uint64_t signaid, char *outscefile)
{

	int ret  = 1;
	FILE *fin = 0, *fout = 0;

	uint8_t *work_buf = (unsigned char*) calloc (1, WORK_BUF_SZ);
	sce_ebootpbp *sceebootpbp_file = (unsigned char*) calloc (1, sizeof(sce_ebootpbp));
	
	
	fin = fopen(ebootpbp, "rb");
	if (!fin) {

		goto error;
	}
	memcpy(&sceebootpbp_file->magic, "NPUMDSIG", 0x8);
	sceebootpbp_file->type = 2;
	sceebootpbp_file->key_type = 1;
	sceebootpbp_file->aid = signaid;
	
	
	fseek(fin, 0, SEEK_END);
	sceebootpbp_file->filesz = ftell(fin);
	
	pbp_hdr hdr;
	fseek(fin, 0, SEEK_SET);
	fread(&hdr, sizeof(pbp_hdr),1,fin);
	
	
	fseek(fin, 0, SEEK_SET);
	fread(work_buf, hdr.icon0_offset, 1,fin);
	
	uint8_t work_hash[0x1c]; 
	SHA256_CTX sha256_ctx;
	SHA224_Init(&sha256_ctx);
	SHA224_Update(&sha256_ctx, work_buf, hdr.icon0_offset);
	SHA224_Final(work_hash, &sha256_ctx);
	f00d_KIRK0x22(work_hash, &sceebootpbp_file->ebootpbp_hdr_sig, sceebootpbp_file->key_type);
	
	SHA224_Init(&sha256_ctx);	
	fseek(fin, hdr.data_psar_offset, SEEK_SET);

	size_t size = PSAR_SZ;
	int to_read = size > WORK_BUF_SZ ? WORK_BUF_SZ : size;
	
	
	fread(work_buf, to_read, 1,fin);
	if(memcmp(work_buf, "NPUMDIMG", 0x8) == 0)
		memcpy(&sceebootpbp_file->np_title, work_buf + 0x10, sizeof(sceebootpbp_file->np_title));
	else {
		memcpy(&sceebootpbp_file->magic, "NPPS1SIG", sizeof(sceebootpbp_file->magic));
		sceebootpbp_file->type = 3;
	}
	
	do {
		size -= to_read;
		SHA224_Update(&sha256_ctx, work_buf, to_read);
		to_read = size > WORK_BUF_SZ ? WORK_BUF_SZ : size;
		fread(work_buf, to_read, 1,fin);
	} while(size > 0);
	
	SHA224_Final(work_hash, &sha256_ctx);
	
	f00d_KIRK0x22(work_hash, &sceebootpbp_file->NPUMDIMG_sig, sceebootpbp_file->key_type);

	SHA224_Init(&sha256_ctx);	
	SHA224_Update(&sha256_ctx, sceebootpbp_file, 0x1C8);
	SHA224_Final(work_hash, &sha256_ctx);
	
	f00d_KIRK0x22(work_hash, &sceebootpbp_file->sceebootpbp_sig, sceebootpbp_file->key_type);
	
	fout = fopen(outscefile, "wb");
	if (!fout) {
		goto error;
	}
	fwrite(sceebootpbp_file,  1, sizeof(sce_ebootpbp), fout);
	
	ret = 0;
error:
	if (fin)
		fclose(fin);
	if (fout)
		fclose(fout);	
	free(work_buf);
	free(sceebootpbp_file);
	return ret;
}
