// Copyright 2010            Sven Peter <svenpeter@gmail.com>
// Copyright 2007,2008,2010  Segher Boessenkool  <segher@kernel.crashing.org>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt

#ifndef ECDSA_H__
#define ECDSA_H__ 1

int ecdsa_set_curve(u8* p, u8* a, u8* b, u8* N, u8* Gx, u8* Gy);
void ecdsa_set_pub(u8 *Qx);
void ecdsa_set_priv(u8 *k);
int  ecdsa_verify(u8 *hash, u8 *R, u8 *S);
void ecdsa_sign(u8* hash, u8* R, u8* S);

void bn_dump(char *msg, u8 *d, u32 n);
int  bn_is_zero(u8 *d, u32 n);
void bn_copy(u8 *d, u8 *a, u32 n);
int  bn_compare(u8 *a, u8 *b, u32 n);
void bn_reduce(u8 *d, u8 *N, u32 n);
void bn_add(u8 *d, u8 *a, u8 *b, u8 *N, u32 n);
void bn_sub(u8 *d, u8 *a, u8 *b, u8 *N, u32 n);
void bn_to_mon(u8 *d, u8 *N, u32 n);
void bn_from_mon(u8 *d, u8 *N, u32 n);
void bn_mon_mul(u8 *d, u8 *a, u8 *b, u8 *N, u32 n);
void bn_mon_inv(u8 *d, u8 *a, u8 *N, u32 n);

void bn_mul(u8 *d, u8 *a, u8 *b, u8 *N, u32 n);
void bn_gcd(u8 *out_d, u8 *in_a, u8 *in_b, u32 n);

#endif

