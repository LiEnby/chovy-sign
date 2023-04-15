using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace PspCrypto
{
    public class Lzrc
    {
        private bool np9660;

        private byte[] input;
        private int in_ptr;
        private int in_len;

        private byte[] output;
        private int out_ptr;
        private int out_len;

        private uint range;
        private uint code;
        private uint out_code;
        private byte lc;
        private byte[][] bm_literal;
        private byte[][] bm_dist_bits;
        private byte[][] bm_dist;
        private byte[][] bm_match;
        private byte[][] bm_len;

        const int max_tbl_sz = 65280;
        const int tbl_sz = 65536;

        static byte[] text_buf = new byte[tbl_sz];
        static int t_start, t_end, t_fill, sp_fill;
        static int t_len, t_pos;

        static int[] prev = new int[tbl_sz], next = new int[tbl_sz];
        static int[] root = new int[tbl_sz];

        public Lzrc(bool np9660 = false)
        {
            this.np9660 = np9660;

            if (np9660)
            {
                this.bm_literal = new byte[8][];
                this.bm_dist_bits = new byte[8][];
                this.bm_dist = new byte[18][];
                this.bm_match = new byte[8][];
                this.bm_len = new byte[8][];
            }
            else
            {
                this.bm_literal = new byte[8][];
                this.bm_dist_bits = new byte[8][];
                this.bm_dist = new byte[16][];
                this.bm_match = new byte[8][];
                this.bm_len = new byte[8][];
            }
        }

        static void Init(byte[][] arr, byte value, int length)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new byte[length];
                for (int j = 0; j < length; j++)
                {
                    arr[i][j] = value;
                }
            }
        }

        private byte rc_getbyte()
        {
            if (in_ptr == in_len)
            {
                throw new Exception("End of input!");
            }

            return input[in_ptr++];
        }

        void rc_putbyte(byte b)
        {
            if (out_ptr == out_len)
            {
                throw new Exception("Output overflow!");
            }

            output[out_ptr++] = b;
        }

        void re_init(ref byte[] out_buf, int out_len, ref byte[] in_buf, int in_len)
        {
            input = in_buf;
            this.in_len = in_len;
            in_ptr = 0;

            output = out_buf;
            this.out_len = out_len;
            out_ptr = 0;

            range = 0xffffffff;
            code = 0x00000000;
            lc = 5;
            out_code = 0xffffffff;

            re_putbyte(lc);


            if (this.np9660)
            {
                Init(bm_literal, 0x80, 256);
                Init(bm_dist_bits, 0x80, 39);
                Init(bm_dist, 0x80, 8);
                Init(bm_match, 0x80, 8);
                Init(bm_len, 0x80, 31);
            }
            else
            {
                Init(bm_literal, 0x80, 256); // 2048  2680 2656
                Init(bm_dist_bits, 0x80, 23); // 184
                Init(bm_dist, 0x80, 8); // 128
                Init(bm_match, 0x80, 8); // 64
                Init(bm_len, 0x80, 32); // 256 

            }
            //memset(re->bm_literal, 0x80, 2048);
            //memset(re->bm_dist_bits, 0x80, 312);
            //memset(re->bm_dist, 0x80, 144);
            //memset(re->bm_match, 0x80, 64);
            //memset(re->bm_len, 0x80, 248);
        }

        void rc_init(byte[] out_buf, int out_len, byte[] in_buf, int in_len)
        {
            input = in_buf;
            this.in_len = in_len;
            in_ptr = 0;

            output = out_buf;
            this.out_len = out_len;
            out_ptr = 0;

            range = 0xffffffff;
            lc = rc_getbyte();
            code = (uint)((rc_getbyte() << 24) |
                   (rc_getbyte() << 16) |
                   (rc_getbyte() << 8) |
                   rc_getbyte());
            out_code = 0xffffffff;
            
            if (this.np9660)
            {
                Init(bm_literal, 0x80, 256);
                Init(bm_dist_bits, 0x80, 39);
                Init(bm_dist, 0x80, 8);
                Init(bm_match, 0x80, 8);
                Init(bm_len, 0x80, 31);
            }
            else
            {
                Init(bm_literal, 0x80, 256); // 2048  2680 2656
                Init(bm_dist_bits, 0x80, 23); // 184
                Init(bm_dist, 0x80, 8); // 128
                Init(bm_match, 0x80, 8); // 64
                Init(bm_len, 0x80, 32); // 256 

            }

        }

        void normalize()
        {
            if (range < 0x01000000)
            {
                range <<= 8;
                code = (code << 8) + input[in_ptr];
                in_ptr++;
            }
        }

        int rc_bit(byte[] probs, int index)
        {
            uint bound;

            normalize();

            bound = (range >> 8) * probs[index];
            probs[index] -= (byte)(probs[index] >> 3);

            if (code < bound)
            {
                range = bound;
                probs[index] += 31;
                return 1;
            }
            else
            {
                code -= bound;
                range -= bound;
                return 0;
            }
        }

        int rc_bittree(byte[] probs, int index, int limit)
        {
            int number = 1;
            do
            {
                number = (number << 1) + rc_bit(probs, index + number);
            } while (number < limit);

            number -= limit;

            return number;
        }

        int rc_number(byte[] prob, int index, int n)
        {
            int i, number = 1;

            if (n > 3)
            {
                number = (number << 1) + rc_bit(prob, index + 3);
                if (n > 4)
                {
                    number = (number << 1) + rc_bit(prob, index + 3);
                }

                if (n > 5)
                {
                    normalize();
                    for (i = 0; i < n - 5; i++)
                    {
                        range >>= 1;
                        number <<= 1;
                        if (code < range)
                        {
                            number += 1;
                        }
                        else
                        {
                            code -= range;
                        }
                    }
                }
            }

            if (n > 0)
            {
                number = (number << 1) + rc_bit(prob, index + 0);
                if (n > 1)
                {
                    number = (number << 1) + rc_bit(prob, index + 1);
                    if (n > 2)
                    {
                        number = (number << 1) + rc_bit(prob, index + 2);
                    }
                }
            }

            return number;
        }

        public void init_tree()
        {
            int i;

            for (i = 0; i < tbl_sz; i++)
            {
                root[i] = -1;
                prev[i] = -1;
                next[i] = -1;
            }

            t_start = 0;
            t_end = 0;
            t_fill = 0;
            sp_fill = 0;
        }
        
        void fill_buffer()
        {
            //void *memcpy(void *dest, const void * src, size_t n)

            int content_size, back_size, front_size;

            if (sp_fill == in_len)
                return;

            content_size = (t_fill < t_end) ? (max_tbl_sz + t_fill - t_end) : (t_fill - t_end);
            if (content_size >= 509)
                return;

            if (t_fill < t_start)
            {
                back_size = t_start - t_fill - 1;
                if (sp_fill + back_size > in_len)
                    back_size = in_len - sp_fill;

                Array.ConstrainedCopy(input, sp_fill, text_buf, t_fill, back_size);
                // memcpy(text_buf + t_fill, re->input + sp_fill, back_size);

                sp_fill += back_size;
                t_fill += back_size;
            }
            else
            {
                back_size = max_tbl_sz - t_fill;
                if (t_start == 0)
                    back_size -= 1;
                if (sp_fill + back_size > in_len)
                    back_size = in_len - sp_fill;

                Array.ConstrainedCopy(input, sp_fill, text_buf, t_fill, back_size);
                //memcpy(text_buf + t_fill, re->input + sp_fill, back_size);

                sp_fill += back_size;
                t_fill += back_size;

                front_size = t_start;
                if (t_start != 0)
                    front_size -= 1;
                if (sp_fill + front_size > in_len)
                    front_size = in_len - sp_fill;

                Array.ConstrainedCopy(input, sp_fill, text_buf, 0, front_size);
                //memcpy(text_buf, re->input + sp_fill, front_size);

                sp_fill += front_size;

                Array.ConstrainedCopy(text_buf, 255, text_buf, max_tbl_sz, front_size);
                //memcpy(text_buf + max_tbl_sz, text_buf, 255);

                t_fill += front_size;
                if (t_fill >= max_tbl_sz)
                    t_fill -= max_tbl_sz;
            }
        }
        void remove_node(int p)
        {
            int t, q;

            if (prev[p] == -1)
                return;

            t = text_buf[p + 0];
            t = (t << 8) | text_buf[p + 1];

            q = next[p];
            if (q != -1)
                prev[q] = prev[p];

            if (prev[p] == -2)
                root[t] = q;
            else
                next[prev[p]] = q;

            prev[p] = -1;
            next[p] = -1;
        }

        int insert_node(int pos, out int match_len, out int match_dist, int do_cmp)
        {
            //Span<byte> src, win;
            int i, t, p;
            int content_size;

            //src = text_buf[pos..];
            //win = text_buf[t_start..];
            content_size = (t_fill < pos) ? (max_tbl_sz + t_fill - pos) : (t_fill - pos);
            t_len = 1;
            t_pos = 0;
            match_len = t_len;
            match_dist = t_pos;

            if (in_ptr >= in_len)
            {
                match_len = 256;
                return 0;
            }

            if (in_ptr >= (in_len - 1))
                return 0;

            t = text_buf[pos];
            t = (t << 8) | text_buf[pos+1];
            if (root[t] == -1)
            {
                root[t] = pos;
                prev[pos] = -2;
                next[pos] = -1;
                return 0;
            }

            p = root[t];
            root[t] = pos;
            prev[pos] = -2;
            next[pos] = p;

            if (p != -1)
                prev[p] = pos;

            while (do_cmp == 1 && p != -1)
            {
                for (i = 0; (i < 255 && i < content_size); i++)
                {
                    if (text_buf[pos+i] != text_buf[p + i])
                        break;
                }

                if (i > t_len)
                {
                    t_len = i;
                    t_pos = pos - p;
                }
                else if (i == t_len)
                {
                    int mp = pos - p;
                    if (mp < 0)
                        mp += max_tbl_sz;
                    if (mp < t_pos)
                    {
                        t_len = i;
                        t_pos = pos - p;
                    }
                }
                if (i == 255)
                {
                    remove_node(p);
                    break;
                }

                p = next[p];
            }

            if (this.np9660) 
            {
                // have we calculated match_dist of 256 when its not the end?
                if (t_len == 256 && in_ptr < in_len)
                    return 1;
            }
            if (t_pos < 0) throw new Exception("t_pos was < 0 :?"); // TODO: figure out why this happens on np9660.

            match_len = t_len;
            match_dist = t_pos;

            return 1;
        }
        void update_tree(int length)
        {
            int i, win_size;
            int tmp_len, tmp_pos;

            win_size = (t_end >= t_start) ? (t_end - t_start) : (max_tbl_sz + t_end - t_start);

            for (i = 0; i < length; i++)
            {
                if (win_size == 16384)
                {
                    remove_node(t_start);
                    t_start += 1;
                    if (t_start == max_tbl_sz)
                        t_start = 0;
                }
                else
                {
                    win_size += 1;
                }

                if (i > 0)
                {
                    insert_node(t_end, out tmp_len, out tmp_pos, 0);
                }
                t_end += 1;
                if (t_end >= max_tbl_sz)
                    t_end -= max_tbl_sz;
            }
        }
        void re_bittree(ref byte[] probs,int index, int limit, int number)
        {
            int n, tmp, bit;

            number += limit;

            // Get total bits used by number
            tmp = number;
            n = 0;
            while (tmp > 1)
            {
                tmp >>= 1;
                n++;
            }

            do
            {
                tmp = number >> n;
                bit = (number >> (n - 1)) & 1;
                re_bit(ref probs, index + tmp, bit);

                n -= 1;
            } while (n > 0);
        }

        void re_bit(ref byte[] prob, int index, int bit)
        {
            uint bound;
            uint old_r, old_c;
            byte old_p;

            re_normalize();

            old_r = range;
            old_c = code;
            old_p = prob[index];

            var pProb = prob[index];

            bound = (range >> 8) * (pProb);
            pProb -= (byte)(pProb >> 3);

            if (bit != 0)
            {
                range = bound;
                pProb += 31;
            }
            else
            {
                code += bound;
                if (code < old_c)
                    out_code += 1;
                range -= bound;
            }

            prob[index] = pProb;
        }

        void re_normalize()
        {
            if (range < 0x01000000)
            {
                if (out_code != 0xffffffff)
                {
                    if (out_code > 255)
                    {
                        int p, old_c;
                        p = out_ptr - 1;
                        do
                        {
                            old_c = output[p];
                            output[p] += 1;
                            p -= 1;
                        } while (old_c == 0xff);
                    }

                    re_putbyte((byte)(out_code & 0xff));
                }
                out_code = (code >> 24) & 0xff;
                range <<= 8;
                code <<= 8;
            }
        }

        void re_putbyte(byte out_byte)
        {
            if (out_ptr == out_len)
            {
                out_len += 0x100;
                Array.Resize(ref output, out_len);
            }

            output[out_ptr++] = out_byte;
        }
        byte re_getbyte()
        {
            if (in_ptr == in_len)
            {
                throw new Exception("End of input!");
            }

            return input[in_ptr++];
        }

        void re_number(ref byte[] prob, int index, int n, int number)
        {
            int i;
            UInt32 old_c;

            i = 1;

            if (n > 3)
            {
                re_bit(ref prob, index + 3,(number >> (n - i)) & 1);
                i += 1;
                if (n > 4)
                {
                    re_bit(ref prob, index + 3, (number >> (n - i)) & 1);
                    i += 1;
                    if (n > 5)
                    {
                        re_normalize();
                        for (i = 3; i < n - 2; i++)
                        {
                            range >>= 1;
                            if (((number >> (n - i)) & 1) == 0)
                            {
                                old_c = code;
                                code += range;
                                if (code < old_c)
                                    out_code += 1;
                            }
                        }
                    }
                }
            }

            if (n > 0)
            {
                re_bit(ref prob, index + 0, (number >> (n - i - 0)) & 1);
                if (n > 1)
                {
                    re_bit(ref prob, index + 1, (number >> (n - i - 1)) & 1);
                    if (n > 2)
                    {
                        re_bit(ref prob, index + 2, (number >> (n - i - 2)) & 1);
                    }
                }
            }
        }
        void re_flush()
        {
            re_putbyte((byte)((out_code) & 0xff));
            re_putbyte((byte)((code >> 24) & 0xff));
            re_putbyte((byte)((code >> 16) & 0xff));
            re_putbyte((byte)((code >> 8) & 0xff));
            re_putbyte((byte)((code >> 0) & 0xff));
        }
        public int lzrc_compress(ref byte[] out_buf, int out_len, byte[] in_buf, int in_len)
        {
            int match_step, re_state, len_state, dist_state;
            int i, cur_byte, last_byte;
            int match_len, len_bits;
            int match_dist, dist_bits, limit;
            int round = -1;

            len_state = 0;

            // initalize buffers to all 0x80
            re_init(ref out_buf, out_len, ref in_buf, in_len);

            // initalize the tree
            init_tree();

            // initalize variable to 0 ...
            re_state = 0;
            last_byte = 0;
            match_len = 0;
            match_dist = 0;
            
            bool flg = false;

            while (true)
            {
                round += 1;
                match_step = 0;

                fill_buffer();
                insert_node(t_end, out match_len, out match_dist, 1);

                if (match_len < 256)
                {
                    // condition is different if np9660 vs pops
                    if (this.np9660)
                        flg = (match_len < 4 && match_dist > 255);
                    else
                        flg = (match_len <= 4 && match_dist > 255);

                    if(flg) // if (condition)
                        match_len = 1;
                    update_tree(match_len);
                }

                // condition is different if np9660 vs pops
                if (this.np9660)
                    flg = (match_len == 1 || (match_len < 4 && match_dist > 255));
                else
                    flg = (match_len == 1 || (match_len <= 4 && match_dist > 255));

                if (flg)
                {
                    re_bit(ref bm_match[re_state], match_step, 0);

                    if (re_state > 0)
                        re_state -= 1;

                    cur_byte = re_getbyte();
                    re_bittree(ref bm_literal[((last_byte >> lc) & 0x07)], 0, 0x100, cur_byte);

                    if (in_ptr >= in_len)
                    {
                        re_normalize();
                        re_flush();
                        return out_ptr;
                    }
                }
                else
                {
                    re_bit(ref bm_match[re_state], match_step, 1);

                    // write bitstream length (8 bits, where 0 marks the end of it)
                    len_bits = 0;
                    for (i = 1; i < 8; i++)
                    {
                        match_step += 1;
                        if ((match_len - 1) < (1 << i))
                            break;
                        re_bit(ref bm_match[re_state], match_step, 1);
                        len_bits += 1;
                    }
                    if (i != 8)
                        re_bit(ref bm_match[re_state], match_step, 0);

                    if (len_bits > 0)
                    {
                        len_state = ((len_bits - 1) << 2) + ((in_ptr << (len_bits - 1)) & 0x03);
                        re_number(ref bm_len[re_state], len_state, len_bits, (match_len - 1));

                        if (this.np9660)
                            flg = (match_len == 256);
                        else
                            flg = (in_ptr >= in_len);

                        if (flg)
                        {
                            re_normalize();
                            re_flush();
                            return out_ptr;
                        }
                    }
                    
                    // determine limit ...
                    dist_state = 0;
                    limit = 8;

                    // condition is different if np9660 vs pops
                    if (this.np9660)
                        flg = (match_len > 3);
                    else
                        flg = (match_len > 4);


                    if (flg) // if (condition)
                    {
                        dist_state += 7;

                        if (this.np9660)
                            limit = 44;
                        else
                            limit = 16;

                    }

                    // find total 1s in the match_dist
                    dist_bits = 0;
                    if(match_dist > 0) {
                        while ((match_dist >> dist_bits) != 1)
                            dist_bits += 1;
                    }
                    else
                    {
                        throw new Exception("Match dist is 0.. uhh cant match with yourself..");
                    }

                    re_bittree(ref bm_dist_bits[len_bits], dist_state, limit, dist_bits);

                    if (dist_bits > 0)
                        re_number(ref bm_dist[dist_bits], 0, dist_bits, match_dist);
                    

                    in_ptr += match_len;
                    re_state = 6 + ((in_ptr + 1) & 1);
                }
                last_byte = input[in_ptr - 1];
            } 
            
        }

        public int lzrc_decompress(byte[] out_buf, int out_len, byte[] in_buf, int in_len)
        {
            int match_step, rc_state, len_state, dist_state = 0;
            int i, bit, cur_byte, last_byte;
            int match_len, len_bits;
            int match_dist, dist_bits, limit;
            int match_src;
            int round = -1;

            bool flg = false;
            len_state = 0;

            rc_init(out_buf, out_len, in_buf, in_len);

            /*if ((lc & 0x80) != 0)
            {
                Buffer.BlockCopy(in_buf, 5, out_buf, 0, (int)code);
                return (int)code;
            }*/

            rc_state = 0;
            last_byte = 0;

            while (true)
            {
                round += 1;
                match_step = 0;
                bit = rc_bit(bm_match[rc_state], match_step);
                // if bit is 0, just copy from the bit tree into the output ?
                if (bit == 0) 
                {
                    if (rc_state > 0)
                        rc_state -= 1;

                    cur_byte = rc_bittree(bm_literal[(((out_ptr & 7) << 8) + last_byte >> lc) & 0x07], 0, 0x100);

                    rc_putbyte((byte)cur_byte);
                    if (out_ptr == out_len) return out_ptr;
                }
                else // This essentially goes; "hey the bytes that go here, are the same ones that were already *here* 
                {
                    // Determine bit length?
                    len_bits = 0;
                    for (i = 0; i < 7; i++)
                    {
                        match_step += 1;
                        bit = rc_bit(bm_match[rc_state], match_step);
                        if (bit == 0)
                            break;
                        len_bits += 1;
                    }

                    // Get the match length ..
                    if (len_bits == 0)
                    {
                        match_len = 1;
                    }
                    else
                    {
                        len_state = ((len_bits - 1) << 2) + ((out_ptr << (len_bits - 1)) & 0x03);
                        match_len = rc_number(bm_len[rc_state], len_state, len_bits);
                        if (this.np9660 && match_len == 0xFF)
                            return out_ptr;
                    }

                    dist_state = 0;
                    limit = 8;
                    if (this.np9660)
                        flg = (match_len > 2);
                    else
                        flg = (match_len > 3);

                    if (flg)
                    {
                        dist_state += 7;
                        if (this.np9660) 
                            limit = 44;
                        else
                            limit = 16;
                    }
                    dist_bits = rc_bittree(bm_dist_bits[len_bits], dist_state, limit);

                    if (dist_bits > 0)
                        match_dist = rc_number(bm_dist[dist_bits], 0, dist_bits);
                    else
                        match_dist = 1;

                    match_src = out_ptr - match_dist;
                    if (match_dist > out_ptr || match_dist < 0)
                    {
                        //Console.WriteLine($"match_dist out of range! 0x{match_dist:x8}");
                        //return -1; // test
                        throw new Exception($"match_dist out of range! 0x{match_dist:x8}");
                    }


                    for (i = 0; i < match_len + 1; i++)
                    {
                        rc_putbyte(output[match_src++]);
                    }
                    rc_state = 6 + ((out_ptr + 1) & 1);
                    if (out_ptr == out_len) return out_ptr;
                }
                last_byte = output[out_ptr - 1];
            }
        }
    }
}
