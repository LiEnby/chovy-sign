using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PspCrypto
{
    public static class KIRKEngine
    {
        // KIRK return values
        public const int KIRK_OPERATION_SUCCESS = 0;
        public const int KIRK_NOT_ENABLED = 1;
        public const int KIRK_INVALID_MODE = 2;
        public const int KIRK_HEADER_HASH_INVALID = 3;
        public const int KIRK_DATA_HASH_INVALID = 4;
        public const int KIRK_SIG_CHECK_INVALID = 5;
        public const int KIRK_UNK_1 = 6;
        public const int KIRK_UNK_2 = 7;
        public const int KIRK_UNK_3 = 8;
        public const int KIRK_UNK_4 = 9;
        public const int KIRK_UNK_5 = 0xA;
        public const int KIRK_UNK_6 = 0xB;
        public const int KIRK_NOT_INITIALIZED = 0xC;
        public const int KIRK_INVALID_OPERATION = 0xD;
        public const int KIRK_INVALID_SEED_CODE = 0xE;
        public const int KIRK_INVALID_SIZE = 0xF;
        public const int KIRK_DATA_SIZE_ZERO = 0x10;

        // sceUtilsBufferCopyWithRange modes
        public const int KIRK_CMD_DECRYPT_PRIVATE = 1;
        public const int KIRK_CMD_2 = 2;
        public const int KIRK_CMD_3 = 3;
        public const int KIRK_CMD_ENCRYPT_IV_0 = 4;
        public const int KIRK_CMD_ENCRYPT_IV_FUSE = 5;
        public const int KIRK_CMD_ENCRYPT_IV_USER = 6;
        public const int KIRK_CMD_DECRYPT_IV_0 = 7;
        public const int KIRK_CMD_DECRYPT_IV_FUSE = 8;
        public const int KIRK_CMD_DECRYPT_IV_USER = 9;
        public const int KIRK_CMD_PRIV_SIGN_CHECK = 10;
        public const int KIRK_CMD_SHA1_HASH = 11;
        public const int KIRK_CMD_ECDSA_GEN_KEYS = 12;
        public const int KIRK_CMD_ECDSA_MULTIPLY_POINT = 13;
        public const int KIRK_CMD_PRNG = 14;
        public const int KIRK_CMD_15 = 15;
        public const int KIRK_CMD_ECDSA_SIGN = 16;
        public const int KIRK_CMD_ECDSA_VERIFY = 17;

        // KIRK header modes
        public const int KIRK_MODE_CMD1 = 1;
        public const int KIRK_MODE_CMD2 = 2;
        public const int KIRK_MODE_CMD3 = 3;
        public const int KIRK_MODE_ENCRYPT_CBC = 4;
        public const int KIRK_MODE_DECRYPT_CBC = 5;

        // sceUtilsBufferCopyWithRange errors
        public const int SUBCWR_NOT_16_ALGINED = 0x90A;
        public const int SUBCWR_HEADER_HASH_INVALID = 0x920;
        public const int SUBCWR_BUFFER_TOO_SMALL = 0x1000;

        [StructLayout(LayoutKind.Sequential)]
        public struct KIRK_AES128CBC_HEADER
        {
            public int mode;
            public int unk_4;
            public int unk_8;
            public int keyseed;
            public int data_size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct KIRK_CMD1_HEADER
        {
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private fixed byte _AES_key[16];
            public Span<byte> AES_Key
            {
                get
                {
                    fixed (byte* ptr = _AES_key)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
            public Span<byte> AESKeys
            {
                get
                {
                    fixed (byte* ptr = _AES_key)
                    {
                        return new Span<byte>(ptr, 32);
                    }
                }
            }

            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private fixed byte _CMAC_key[16]; // 0x10
            public Span<byte> CMAC_key
            {
                get
                {
                    fixed (byte* ptr = _CMAC_key)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }

            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private fixed byte _CMAC_header_hash[16]; // 0x20
            public Span<byte> CMAC_header_hash
            {
                get
                {
                    fixed (byte* ptr = _CMAC_header_hash)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private fixed byte _CMAC_data_hash[16]; // 0x30
            public Span<byte> CMAC_data_hash
            {
                get
                {
                    fixed (byte* ptr = _CMAC_data_hash)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }

            public Span<byte> content
            {
                get
                {
                    fixed (byte* ptr = unused)
                    {
                        return new Span<byte>(ptr, 0x40);
                    }
                }
            }
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            fixed byte unused[32]; // 0x40
            public uint mode; // 0x60
            public byte ecdsa_hash; // 0x64
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            fixed byte unk3[11]; // 0x65
            public int data_size; // 0x70

            public Span<byte> off70
            {
                get
                {
                    fixed (int* ptr = &data_size)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public int data_offset; // 0x74
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            fixed byte unk4[8]; // 0x78
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            fixed byte unk5[16]; // 0x80
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct KIRK_CMD1_ECDSA_HEADER
        {
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            private fixed byte _AES_key[16];

            public Span<byte> AES_Key
            {
                get
                {
                    fixed (byte* ptr = _AES_key)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }

            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            private fixed byte _header_sig_r[20]; // 0x10

            public Span<byte> header_sig_r
            {
                get
                {
                    fixed (byte* ptr = _header_sig_r)
                    {
                        return new Span<byte>(ptr, 20);
                    }
                }
            }
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            private fixed byte _header_sig_s[20]; // 0x24

            public Span<byte> header_sig_s
            {
                get
                {
                    fixed (byte* ptr = _header_sig_s)
                    {
                        return new Span<byte>(ptr, 20);
                    }
                }
            }

            public Span<byte> header_sig
            {
                get
                {
                    fixed (byte* ptr = _header_sig_r)
                    {
                        return new Span<byte>(ptr, 40);
                    }
                }
            }


            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            private fixed byte _data_sig_r[20];  // 0x38

            public Span<byte> data_sig_r
            {
                get
                {
                    fixed (byte* ptr = _data_sig_r)
                    {
                        return new Span<byte>(ptr, 20);
                    }
                }
            }
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            private fixed byte _data_sig_s[20]; // 0x4c
            public Span<byte> data_sig_s
            {
                get
                {
                    fixed (byte* ptr = _data_sig_s)
                    {
                        return new Span<byte>(ptr, 20);
                    }
                }
            }

            public Span<byte> data_sig
            {
                get
                {
                    fixed (byte* ptr = _data_sig_r)
                    {
                        return new Span<byte>(ptr, 40);
                    }
                }
            }


            public uint mode; //0x60
            public byte ecdsa_hash;
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            fixed byte unk3[11];
            uint data_size;
            uint data_offset;
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            fixed byte unk4[8];
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            fixed byte unk5[16];
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ECDSA_SIG
        {
            private fixed byte _r[0x14];

            public Span<byte> r
            {
                get
                {
                    fixed (byte* ptr = _r)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }
            private fixed byte _s[0x14];

            public Span<byte> s
            {
                get
                {
                    fixed (byte* ptr = _s)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }

            public Span<byte> sig
            {
                get
                {
                    fixed (byte* ptr = _r)
                    {
                        return new Span<byte>(ptr, 0x28);
                    }
                }
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ECDSA_POINT
        {
            private fixed byte _x[0x14];

            public Span<byte> x
            {
                get
                {
                    fixed (byte* ptr = _x)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }
            private fixed byte _y[0x14];


            public Span<byte> y
            {
                get
                {
                    fixed (byte* ptr = _y)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }

            public Span<byte> point
            {
                get
                {
                    fixed (byte* ptr = _x)
                    {
                        return new Span<byte>(ptr, 0x28);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KIRK_SHA1_HEADER
        {
            public int data_size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct KIRK_CMD12_BUFFER
        {
            private fixed byte private_key_[0x14];

            public Span<byte> private_key
            {
                get
                {
                    fixed (byte* ptr = private_key_)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }
            public ECDSA_POINT public_key;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KIRK_CMD13_BUFFER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x14)]
            public byte[] multiplier;
            public ECDSA_POINT public_key;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct KIRK_CMD16_BUFFER
        {
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            private fixed byte enc_private_[0x20];

            public Span<byte> enc_private
            {
                get
                {
                    fixed (byte* ptr = enc_private_)
                    {
                        return new Span<byte>(ptr, 0x20);
                    }
                }
            }
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x14)]
            private fixed byte message_hash_[0x14];

            public Span<byte> message_hash
            {
                get
                {
                    fixed (byte* ptr = message_hash_)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct KIRK_CMD17_BUFFER
        {
            public ECDSA_POINT public_key;
            private fixed byte _message_hash[0x14];

            public Span<byte> message_hash
            {
                get
                {
                    fixed (byte* ptr = _message_hash)
                    {
                        return new Span<byte>(ptr, 0x14);
                    }
                }
            }
            public ECDSA_SIG signature;
        }
        // KIRK commands
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
            0x0F: (absolutely no idea  could be KIRK initialization)
            0x10: Signature Gen

            // Sig Checks
            0x11: Signature Check (checks for generated sigs)
            0x12: Certificate Check (idstorage signatures)
        */

        // Internal variables
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct kirk16_data
        {
            private fixed byte fuseid_[8];

            public Span<byte> fuseid
            {
                get
                {
                    fixed (byte* ptr = fuseid_)
                    {
                        return new Span<byte>(ptr, 8);
                    }
                }
            }
            private fixed byte mesh_[0x40];

            public Span<byte> mesh
            {
                get
                {
                    fixed (byte* ptr = mesh_)
                    {
                        return new Span<byte>(ptr, 0x40);
                    }
                }
            }
        }

        //[StructLayout(LayoutKind.Sequential)]
        //private unsafe struct header_keys
        //{
        //    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        //    public fixed byte AES[16];
        //    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        //    public fixed byte CMAC[16];
        //}

        private static uint g_fuse90;
        private static uint g_fuse94;

        private static Aes aes_kirk1;
        private static byte[] PRNG_DATA = new byte[0x14];

        private static bool is_kirk_initialized;
        private static readonly byte[] IV0 = new byte[16];

        // Internal functions
        private static byte[] kirk_4_7_get_key(int key_type)
        {
            if ((key_type < 0) || (key_type >= 0x80)) throw new Exception("KIRK_INVALID_SIZE");
            return KeyVault.kirkKeys[key_type];
        }

        private static Aes CreateAes()
        {
            var aes_ctx = Aes.Create();
            aes_ctx.Mode = CipherMode.CBC;
            aes_ctx.Padding = PaddingMode.None;
            aes_ctx.IV = IV0;
            return aes_ctx;
        }

        public static void decrypt_kirk16_private(Span<byte> dA_out, Span<byte> dA_enc)
        {
            int i, k;
            kirk16_data keydata;
            byte[] subkey_1 = new byte[0x10], subkey_2 = new byte[0x10];
            using var aes_ctx = CreateAes();

            keydata.fuseid[7] = (byte)(g_fuse90 & 0xFF);
            keydata.fuseid[6] = (byte)((g_fuse90 >> 8) & 0xFF);
            keydata.fuseid[5] = (byte)((g_fuse90 >> 16) & 0xFF);
            keydata.fuseid[4] = (byte)((g_fuse90 >> 24) & 0xFF);
            keydata.fuseid[3] = (byte)(g_fuse94 & 0xFF);
            keydata.fuseid[2] = (byte)((g_fuse94 >> 8) & 0xFF);
            keydata.fuseid[1] = (byte)((g_fuse94 >> 16) & 0xFF);
            keydata.fuseid[0] = (byte)((g_fuse94 >> 24) & 0xFF);

            /* set encryption key */
            aes_ctx.Key = KeyVault.kirk16_key;

            /* set the subkeys */
            for (i = 0; i < 0x10; i++)
            {
                /* set to the fuseid */
                subkey_2[i] = subkey_1[i] = keydata.fuseid[i % 8];
            }
            /* do aes crypto */
            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                using var decryptor = aes_ctx.CreateDecryptor();
                for (i = 0; i < 3; i++)
                {
                    /* encrypt + decrypt */
                    encryptor.TransformBlock(subkey_1, 0, subkey_1.Length, subkey_1, 0);
                    decryptor.TransformBlock(subkey_2, 0, subkey_2.Length, subkey_2, 0);
                }
            }

            /* set new key */
            aes_ctx.Key = subkey_1;
            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                /* now lets make the key mesh */
                for (i = 0; i < 3; i++)
                {
                    /* do encryption in group of 3 */
                    for (k = 0; k < 3; k++)
                    {
                        /* crypto */
                        encryptor.TransformBlock(subkey_2, 0, subkey_2.Length, subkey_2, 0);
                    }

                    /* copy to out block */
                    subkey_2.AsSpan().CopyTo(keydata.mesh[(i * 0x10)..]);
                }
            }

            /* set the key to the mesh */
            aes_ctx.Key = keydata.mesh.Slice(0x20, 0x10).ToArray();

            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                /* do the encryption routines for the aes key */
                for (i = 0; i < 2; i++)
                {
                    /* encrypt the data */
                    var tmp = encryptor.TransformFinalBlock(keydata.mesh.ToArray(), 0x10, 0x30);
                    tmp.CopyTo(keydata.mesh[0x10..]);
                }
            }

            /* set the key to that mesh shit */
            using var aes = CreateAes();
            aes.Key = keydata.mesh.Slice(0x10, 0x10).ToArray();


            /* cbc decrypt the dA */
            AesHelper.AesDecrypt(aes,  dA_out, dA_enc, 0x20);
        }

        public static void encrypt_kirk16_private(Span<byte> dA_out, Span<byte> dA_dec)
        {
            int i, k;
            kirk16_data keydata;
            byte[] subkey_1 = new byte[0x10], subkey_2 = new byte[0x10];
            using var aes_ctx = CreateAes();

            keydata.fuseid[7] = (byte)(g_fuse90 & 0xFF);
            keydata.fuseid[6] = (byte)((g_fuse90 >> 8) & 0xFF);
            keydata.fuseid[5] = (byte)((g_fuse90 >> 16) & 0xFF);
            keydata.fuseid[4] = (byte)((g_fuse90 >> 24) & 0xFF);
            keydata.fuseid[3] = (byte)(g_fuse94 & 0xFF);
            keydata.fuseid[2] = (byte)((g_fuse94 >> 8) & 0xFF);
            keydata.fuseid[1] = (byte)((g_fuse94 >> 16) & 0xFF);
            keydata.fuseid[0] = (byte)((g_fuse94 >> 24) & 0xFF);

            /* set encryption key */
            aes_ctx.Key = KeyVault.kirk16_key;

            /* set the subkeys */
            for (i = 0; i < 0x10; i++)
            {
                /* set to the fuseid */
                subkey_2[i] = subkey_1[i] = keydata.fuseid[i % 8];
            }
            /* do aes crypto */
            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                using var decryptor = aes_ctx.CreateDecryptor();
                for (i = 0; i < 3; i++)
                {
                    /* encrypt + decrypt */
                    subkey_1 = encryptor.TransformFinalBlock(subkey_1, 0, subkey_1.Length);
                    subkey_2 = decryptor.TransformFinalBlock(subkey_2, 0, subkey_2.Length);
                }
            }

            /* set new key */
            aes_ctx.Key = subkey_1;
            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                /* now lets make the key mesh */
                for (i = 0; i < 3; i++)
                {
                    /* do encryption in group of 3 */
                    for (k = 0; k < 3; k++)
                    {
                        /* crypto */
                        encryptor.TransformBlock(subkey_2, 0, subkey_2.Length, subkey_2, 0);
                    }

                    /* copy to out block */
                    subkey_2.AsSpan().CopyTo(keydata.mesh.Slice(i * 0x10));
                }
            }

            /* set the key to the mesh */
            aes_ctx.Key = keydata.mesh.Slice(0x20, 0x10).ToArray();

            using (var encryptor = aes_ctx.CreateEncryptor())
            {
                /* do the encryption routines for the aes key */
                for (i = 0; i < 2; i++)
                {
                    /* encrypt the data */
                    var tmp = encryptor.TransformFinalBlock(keydata.mesh.ToArray(), 0x10, 0x30);
                    tmp.CopyTo(keydata.mesh.Slice(0x10));
                }
            }

            /* set the key to that mesh shit */
            using var aes = CreateAes();
            aes.Key = keydata.mesh.Slice(0x10, 0x10).ToArray();


            /* cbc encrypt the dA */
            AesHelper.AesEncrypt(aes, dA_out, dA_dec, 0x20);
        }

        static KIRKEngine()
        {
            kirk_init();
        }

        // KIRK commands

        public static int kirk_init()
        {
            //94 90
            //0x0000332 e6e050311 3000
            //0x0000772 ccda50f12 2000
            //0x1008010 1ef1e7101 vita
            return kirk_init2(Encoding.ASCII.GetBytes("Lazy Dev should have initialized!"), 33, 0xBABEF00D, 0xDEADBEEF);
        }

        public static int kirk_init2(byte[] rnd_seed, int seed_size, uint fuseid_90, uint fuseid_94)
        {
            Span<byte> temp = stackalloc byte[0x104];

            // Another randomly selected data for a "key" to add to each randomization
            ReadOnlySpan<byte> key = stackalloc byte[] { 0x07, 0xAB, 0xEF, 0xF8, 0x96, 0x8C, 0xF3, 0xD6, 0x14, 0xE0, 0xEB, 0xB2, 0x9D, 0x8B, 0x4E, 0x74 };
            uint curtime;

            is_kirk_initialized = true;

            //Set PRNG_DATA initially, otherwise use what ever uninitialized data is in the buffer
            if (seed_size > 0)
            {
                Span<byte> seedbuf = stackalloc byte[seed_size + 4];
                RandomNumberGenerator.Fill(seedbuf);
                var seedheader = new KIRK_SHA1_HEADER
                {
                    data_size = seed_size
                };
                MemoryMarshal.Write(seedbuf, ref seedheader);
                kirk_CMD11(PRNG_DATA, seedbuf, seed_size + 4);
            }
            // Buffer.BlockCopy(PRNG_DATA, 0, header.data, 0, 0x14);
            PRNG_DATA.CopyTo(temp.Slice(4));

            // This uses the standard C time function for portability.
            curtime = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            temp[0x18] = (byte)(curtime & 0xFF);
            temp[0x19] = (byte)((curtime >> 8) & 0xFF);
            temp[0x1A] = (byte)((curtime >> 16) & 0xFF);
            temp[0x1B] = (byte)((curtime >> 24) & 0xFF);
            // Buffer.BlockCopy(key, 0, header.data, 0x18, 0x10);
            key.CopyTo(temp.Slice(0x1c));

            // This leaves the remainder of the 0x100 bytes in temp to whatever remains on the stack 
            // in an uninitialized state. This should add unpredicableness to the results as well

            var header = new KIRK_SHA1_HEADER
            {
                data_size = 0x100
            };
            MemoryMarshal.Write(temp, ref header);
            kirk_CMD11(PRNG_DATA, temp, 0x104);


            //Set Fuse ID
            g_fuse90 = fuseid_90;
            g_fuse94 = fuseid_94;

            // Set KIRK1 main key
            aes_kirk1 = CreateAes();
            aes_kirk1.Key = KeyVault.kirk1_key;
            return 0;
        }

        static int kirk_CMD0(Span<byte> outbuff, ReadOnlySpan<byte> inbuff, int size, bool generate_trash)
        {
            KIRK_CMD1_HEADER header = MemoryMarshal.AsRef<KIRK_CMD1_HEADER>(outbuff);
            // header_keys keys = Utils.AsRef<header_keys>(outbuff);
            int chk_size;
            Aes k1;
            Aes cmac_key;

            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;

            inbuff[..size].CopyTo(outbuff);

            if (header.mode != KIRK_MODE_CMD1) return KIRK_INVALID_MODE;

            // FILL PREDATA WITH RANDOM DATA
            if (generate_trash) kirk_CMD14(outbuff[Unsafe.SizeOf<KIRK_CMD1_HEADER>()..], header.data_offset);

            // Make sure data is 16 aligned
            chk_size = header.data_size;
            if (chk_size % 16 != 0) chk_size += 16 - (chk_size % 16);

            // ENCRYPT DATA

            // ENCRYPT DATA
            using (k1 = CreateAes())
            {
                k1.Key = header.AES_Key.ToArray();
                AesHelper.AesEncrypt(k1, outbuff[(Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset)..],
                    inbuff.Slice(Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset, header.data_size));
                //byte[] encData = AesHelper.AesEncrypt(k1, inbuff.ToArray(),
                //    Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset,
                //    chk_size);
                //encData.CopyTo(outbuff.Slice(Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset));
            }


            // CMAC HASHES
            using (cmac_key = CreateAes())
            {
                cmac_key.Key = header.CMAC_key.ToArray();
                //var cmac_header_hash = AesHelper.Cmac(cmac_key, outbuff.ToArray(), 0x60, 0x30);
                //var cmac_data_hash = AesHelper.Cmac(cmac_key, outbuff.ToArray(), 0x60, 0x30 + chk_size);
                Span<byte> cmac_header_hash = stackalloc byte[16];
                Span<byte> cmac_data_hash = stackalloc byte[16];
                AesHelper.Cmac(cmac_key, cmac_header_hash, outbuff.Slice(0x60, 0x30));
                AesHelper.Cmac(cmac_key, cmac_data_hash, outbuff.Slice(0x60, 0x30 + chk_size));
                cmac_header_hash.CopyTo(header.CMAC_header_hash);
                cmac_data_hash.CopyTo(header.CMAC_data_hash);
            }

            // ENCRYPT KEYS
            // AesHelper.AesEncrypt(aes_kirk1)
            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD1(Span<byte> outbuff, ReadOnlySpan<byte> inbuff, int size)
        {
            KIRK_CMD1_HEADER header = MemoryMarshal.AsRef<KIRK_CMD1_HEADER>(inbuff);
            // header_keys keys; //0-15 AES key, 16-31 CMAC key
            Aes k1;

            if (size < 0x90) return KIRK_INVALID_SIZE;
            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;
            if (header.mode != KIRK_MODE_CMD1) return KIRK_INVALID_MODE;
            // byte[] keytmp = AesHelper.AesDecrypt(aes_kirk1, inbuff.ToArray(), 0, 32);
            Span<byte> keytmp = stackalloc byte[32];
            AesHelper.AesDecrypt(aes_kirk1, keytmp, inbuff, 32);


            if (header.ecdsa_hash == 1)
            {
                KIRK_CMD1_ECDSA_HEADER eheader = MemoryMarshal.AsRef<KIRK_CMD1_ECDSA_HEADER>(inbuff);
                var curve = ECDsaHelper.SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b1, KeyVault.ec_N1, KeyVault.Gx1,
                    KeyVault.Gy1);
                unsafe
                {
                    var ecdsa = ECDsaHelper.Create(curve, KeyVault.ec_Priv1);
                    if (!ecdsa.VerifyData(inbuff.Slice(0x60, 0x30), eheader.header_sig.ToArray(),
                        HashAlgorithmName.SHA1))
                    {
                        return KIRK_HEADER_HASH_INVALID;
                    }
                    ecdsa = ECDsaHelper.Create(curve, KeyVault.ec_Priv1);
                    if (!ecdsa.VerifyData(inbuff[0x60..size], eheader.data_sig.ToArray(), HashAlgorithmName.SHA1))
                    {
                        return KIRK_DATA_HASH_INVALID;
                    }
                }
            }
            else
            {
                int ret = kirk_CMD10(inbuff, size);
                if (ret != KIRK_OPERATION_SUCCESS) return ret;
            }

            k1 = CreateAes();
            k1.Key = keytmp.Slice(0, 16).ToArray();
            //var outtmp = AesHelper.AesDecrypt(k1, inbuff.ToArray(), Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset, header.data_size);
            // Buffer.BlockCopy(outtmp, 0, outbuff, 0, (int)header.data_size);

            AesHelper.AesDecrypt(k1, outbuff, inbuff.Slice(Unsafe.SizeOf<KIRK_CMD1_HEADER>() + header.data_offset), header.data_size);

            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD4(Span<byte> outbuff, ReadOnlySpan<byte> inbuff, int size)
        {
            KIRK_AES128CBC_HEADER header = MemoryMarshal.AsRef<KIRK_AES128CBC_HEADER>(inbuff);
            byte[] key;
            Aes aes;

            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;
            if (header.mode != KIRK_MODE_ENCRYPT_CBC) return KIRK_INVALID_MODE;
            if (header.data_size == 0) return KIRK_DATA_SIZE_ZERO;
            // size = size - 20;
            using (aes = CreateAes())
            {
                key = kirk_4_7_get_key(header.keyseed);

                // Set the key
                aes.Key = key;
                //var enc = AesHelper.AesEncrypt(aes, inbuff.ToArray(), Unsafe.SizeOf<KIRK_AES128CBC_HEADER>(), size);
                // Buffer.BlockCopy(enc, 0, outbuff, Marshal.SizeOf<KIRK_AES128CBC_HEADER>() + outOffset, size);
                //enc.AsSpan(0, size).CopyTo(outbuff.Slice(Unsafe.SizeOf<KIRK_AES128CBC_HEADER>(), size));
                AesHelper.AesEncrypt(aes, outbuff.Slice(Unsafe.SizeOf<KIRK_AES128CBC_HEADER>()),
                    inbuff.Slice(Unsafe.SizeOf<KIRK_AES128CBC_HEADER>(), header.data_size));
            }
            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD7(Span<byte> outbuff, ReadOnlySpan<byte> inbuff, int size)
        {
            KIRK_AES128CBC_HEADER header = MemoryMarshal.AsRef<KIRK_AES128CBC_HEADER>(inbuff);
            byte[] key;
            Aes aes;

            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;
            if (header.mode != KIRK_MODE_DECRYPT_CBC) return KIRK_INVALID_MODE;
            if (header.data_size == 0) return KIRK_DATA_SIZE_ZERO;
            using (aes = CreateAes())
            {
                key = kirk_4_7_get_key(header.keyseed);

                // Set the key
                aes.Key = key;
                //var enc = AesHelper.AesDecrypt(aes, inbuff.ToArray(), Unsafe.SizeOf<KIRK_AES128CBC_HEADER>(), header.data_size);
                //Buffer.BlockCopy(enc, 0, outbuff, outOffset, header.data_size);
                //enc.AsSpan(0, header.data_size).CopyTo(outbuff);
                AesHelper.AesDecrypt(aes, outbuff, inbuff.Slice(Unsafe.SizeOf<KIRK_AES128CBC_HEADER>()), header.data_size);
            }
            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD10(ReadOnlySpan<byte> inbuff, int insize)
        {
            KIRK_CMD1_HEADER header = MemoryMarshal.AsRef<KIRK_CMD1_HEADER>(inbuff);
            // header_keys keys; //0-15 AES key, 16-31 CMAC key
            Span<byte> cmac_header_hash = stackalloc byte[16];
            Span<byte> cmac_data_hash = stackalloc byte[16];
            using Aes cmac_key = CreateAes();
            int chk_size;

            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;
            if (!(header.mode == KIRK_MODE_CMD1 || header.mode == KIRK_MODE_CMD2 || header.mode == KIRK_MODE_CMD3)) return KIRK_INVALID_MODE;
            if (header.data_size == 0) return KIRK_DATA_SIZE_ZERO;
            if (header.mode == KIRK_MODE_CMD1)
            {
                // var decdata = AesHelper.AesDecrypt(aes_kirk1, inbuff.ToArray(), 0, 32);
                Span<byte> tmpKey = stackalloc byte[32];
                AesHelper.AesDecrypt(aes_kirk1, tmpKey, inbuff, 32);
                cmac_key.Key = tmpKey[16..].ToArray();
                //cmac_header_hash = AesHelper.Cmac(cmac_key, inbuff.ToArray(), 0x60, 0x30);
                AesHelper.Cmac(cmac_key, cmac_header_hash, inbuff.Slice(0x60, 0x30));

                // Make sure data is 16 aligned
                chk_size = header.data_size;
                if (chk_size % 16 != 0) chk_size += 16 - (chk_size % 16);
                //cmac_data_hash = AesHelper.Cmac(cmac_key, inbuff.ToArray(), 0x60, 0x30 + chk_size + header.data_offset);
                AesHelper.Cmac(cmac_key, cmac_data_hash, inbuff.Slice(0x60, 0x30 + chk_size + header.data_offset));

                if (!header.CMAC_header_hash.SequenceEqual(cmac_header_hash)) return KIRK_HEADER_HASH_INVALID;
                if (!header.CMAC_data_hash.SequenceEqual(cmac_data_hash)) return KIRK_DATA_HASH_INVALID;

                return KIRK_OPERATION_SUCCESS;
            }

            return KIRK_SIG_CHECK_INVALID; //Checks for cmd 2 & 3 not included right now
        }

        static int kirk_CMD11(Span<byte> outbuff, ReadOnlySpan<byte> inbuff, int size)
        {
            KIRK_SHA1_HEADER header = MemoryMarshal.AsRef<KIRK_SHA1_HEADER>(inbuff);
            if (!is_kirk_initialized) return KIRK_NOT_INITIALIZED;
            if (header.data_size == 0 || size == 0) return KIRK_DATA_SIZE_ZERO;

            //byte[] buff = new byte[header.data_size + 4];
            //using (var ms = new MemoryStream(buff))
            //{
            //    using var bw = new BinaryWriter(ms);
            //    bw.Write(inbuff.data_size);
            //    bw.Write(inbuff.data);
            //}
            var sha = SHA1.Create();
            var hash = sha.ComputeHash(inbuff.Slice(4, header.data_size).ToArray());
            // Buffer.BlockCopy(hash, 0, outbuff, 0, hash.Length);
            hash.CopyTo(outbuff);
            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD12(Span<byte> outbuff, int outsize)
        {
            if (outsize != 0x3C) return KIRK_INVALID_SIZE;

            var (d, q) = ECDsaHelper.GenerateKey(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                 KeyVault.Gy2);
            d.AsSpan(0, 0x14).CopyTo(outbuff);
            q.X.AsSpan(0, 0x14).CopyTo(outbuff.Slice(0x14));
            q.Y.AsSpan(0, 0x14).CopyTo(outbuff.Slice(0x28));
            //Buffer.BlockCopy(d, 0, outbuff, 0, 0x14);
            //Buffer.BlockCopy(q.X, 0, outbuff, 0x14, 0x14);
            //Buffer.BlockCopy(q.Y, 0, outbuff, 0x28, 0x14);

            return KIRK_OPERATION_SUCCESS;
        }

        //static int kirk_CMD13()

        static int kirk_CMD14(Span<byte> outbuff, int outsize)
        {
            Span<byte> temp = stackalloc byte[0x104];

            // Some randomly selected data for a "key" to add to each randomization
            Span<byte> key = stackalloc byte[] { 0xA7, 0x2E, 0x4C, 0xB6, 0xC3, 0x34, 0xDF, 0x85, 0x70, 0x01, 0x49, 0xFC, 0xC0, 0x87, 0xC4, 0x77 };
            uint curtime;

            if (outsize <= 0) return KIRK_OPERATION_SUCCESS;
            // Buffer.BlockCopy(PRNG_DATA, 0, header.data, 0, 0x14);
            PRNG_DATA.CopyTo(temp.Slice(4));

            // This uses the standard C time function for portability.
            curtime = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            temp[0x18] = (byte)(curtime & 0xFF);
            temp[0x19] = (byte)((curtime >> 8) & 0xFF);
            temp[0x1A] = (byte)((curtime >> 16) & 0xFF);
            temp[0x1B] = (byte)((curtime >> 24) & 0xFF);
            // Buffer.BlockCopy(key, 0, header.data, 0x18, 0x10);
            key.CopyTo(temp[0x1C..]);

            // This leaves the remainder of the 0x100 bytes in temp to whatever remains on the stack 
            // in an uninitialized state. This should add unpredicableness to the results as well
            // header.data_size = 0x100;
            var header = new KIRK_SHA1_HEADER
            {
                data_size = 0x100
            };
            MemoryMarshal.Write(temp, ref header);
            kirk_CMD11(PRNG_DATA, temp, 0x104);

            while (outsize > 0)
            {
                int blockrem = outsize % 0x14;
                int block = outsize / 0x14;

                if (block > 0)
                {
                    // Buffer.BlockCopy(PRNG_DATA, 0, outbuff, outoff, 0x14);
                    PRNG_DATA.CopyTo(outbuff);
                    outsize -= 0x14;
                    kirk_CMD14(outbuff[0x14..], outsize);
                }
                else if (blockrem > 0)
                {
                    // Buffer.BlockCopy(PRNG_DATA, 0, outbuff, outoff, blockrem);
                    PRNG_DATA.AsSpan(0, blockrem).CopyTo(outbuff);
                    outsize -= blockrem;
                }
            }

            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD16(Span<byte> outbuff, int outsize, ReadOnlySpan<byte> inbuff, int insize)
        {
            byte[] dec_private = new byte[0x20];
            KIRK_CMD16_BUFFER signbuf = MemoryMarshal.AsRef<KIRK_CMD16_BUFFER>(inbuff);
            //ECDSA_SIG sig = BufferToStruct<ECDSA_SIG>(outbuff);

            if (insize != 0x34) return KIRK_INVALID_SIZE;
            if (outsize != 0x28) return KIRK_INVALID_SIZE;


            decrypt_kirk16_private(dec_private, signbuf.enc_private);

            // Clear out the padding for safety
            //Array.Clear(dec_private, 0x14, 0xC);

            var curve = ECDsaHelper.SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);
            var ecdsa = ECDsaHelper.Create(curve, dec_private.Take(0x14).ToArray());
            unsafe
            {
                var hashSig = ecdsa.SignHash(signbuf.message_hash.ToArray());
                // Buffer.BlockCopy(hashSig, 0, outbuff, outoffset, 0x28);
                hashSig.CopyTo(outbuff);
            }

            return KIRK_OPERATION_SUCCESS;
        }

        static int kirk_CMD17(ReadOnlySpan<byte> inbuff, int insize)
        {
            KIRK_CMD17_BUFFER sig = MemoryMarshal.AsRef<KIRK_CMD17_BUFFER>(inbuff);

            if (insize != 0x64) return KIRK_INVALID_SIZE;

            var curve = ECDsaHelper.SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);

            unsafe
            {
                var ecdsa = ECDsaHelper.Create(curve, sig.public_key.x, sig.public_key.y);

                if (ecdsa.VerifyHash(sig.message_hash, sig.signature.sig))
                {
                    return KIRK_OPERATION_SUCCESS;

                }
            }
            return KIRK_SIG_CHECK_INVALID;
        }

        // SCE functions
        public static int sceUtilsBufferCopyWithRange(Span<byte> outbuff, int outsize, ReadOnlySpan<byte> inbuff, int insize, int cmd)
        {
            switch (cmd)
            {
                case KIRK_CMD_DECRYPT_PRIVATE: return kirk_CMD1(outbuff, inbuff, insize);
                case KIRK_CMD_ENCRYPT_IV_0: return kirk_CMD4(outbuff, inbuff, insize);
                case KIRK_CMD_DECRYPT_IV_0: return kirk_CMD7(outbuff, inbuff, insize);
                case KIRK_CMD_PRIV_SIGN_CHECK: return kirk_CMD10(inbuff, insize);
                case KIRK_CMD_SHA1_HASH: return kirk_CMD11(outbuff, inbuff, insize);
                case KIRK_CMD_ECDSA_GEN_KEYS: return kirk_CMD12(outbuff, outsize);
                //case KIRK_CMD_ECDSA_MULTIPLY_POINT: return kirk_CMD13(outbuff, outoffset, outsize, inbuff, insize);
                case KIRK_CMD_PRNG: return kirk_CMD14(outbuff, outsize);
                case KIRK_CMD_ECDSA_SIGN: return kirk_CMD16(outbuff, outsize, inbuff, insize);
                case KIRK_CMD_ECDSA_VERIFY: return kirk_CMD17(inbuff, insize);
            }
            return -1;
        }
    }
}
