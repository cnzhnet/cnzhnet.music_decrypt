using cnzhnet.music_decrypt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Compressors.Xz;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 用于实现 *.kgm/*.vpr 酷狗音乐加密格式的音频解密.
    /// </summary>
    public class KgmAudioDecrypter : AudioDecrypter
    {
        private static readonly byte[] vpr_header = new byte[16] { 0x05, 0x28, 0xBC, 0x96, 0xE9, 0xE4, 0x5A, 0x43, 0x91, 0xAA, 0xBD, 0xD0, 0x7A, 0xF5, 0x36, 0x31 };
        private static readonly byte[] kgm_header = new byte[16] { 0x7C, 0xD5, 0x32, 0xEB, 0x86, 0x02, 0x7F, 0x4B, 0xA8, 0xAF, 0xA6, 0x8E, 0x0F, 0xFF, 0x99, 0x14 };
        private static readonly byte[] vpr_mask_diff = new byte[17] { 0x25, 0xDF, 0xE8, 0xA6, 0x75, 0x1E, 0x75, 0x0E, 0x2F, 0x80, 0xF3, 0x2D, 0xB8, 0xB6, 0xE3, 0x11, 0x00 };
        private static readonly byte[] MaskV2PreDef = new byte[272] {
            0xB8, 0xD5, 0x3D, 0xB2, 0xE9, 0xAF, 0x78, 0x8C, 0x83, 0x33, 0x71, 0x51, 0x76, 0xA0, 0xCD, 0x37, 
            0x2F, 0x3E, 0x35, 0x8D, 0xA9, 0xBE, 0x98, 0xB7, 0xE7, 0x8C, 0x22, 0xCE, 0x5A, 0x61, 0xDF, 0x68, 
            0x69, 0x89, 0xFE, 0xA5, 0xB6, 0xDE, 0xA9, 0x77, 0xFC, 0xC8, 0xBD, 0xBD, 0xE5, 0x6D, 0x3E, 0x5A, 
            0x36, 0xEF, 0x69, 0x4E, 0xBE, 0xE1, 0xE9, 0x66, 0x1C, 0xF3, 0xD9, 0x02, 0xB6, 0xF2, 0x12, 0x9B, 
            0x44, 0xD0, 0x6F, 0xB9, 0x35, 0x89, 0xB6, 0x46, 0x6D, 0x73, 0x82, 0x06, 0x69, 0xC1, 0xED, 0xD7, 
            0x85, 0xC2, 0x30, 0xDF, 0xA2, 0x62, 0xBE, 0x79, 0x2D, 0x62, 0x62, 0x3D, 0x0D, 0x7E, 0xBE, 0x48, 
            0x89, 0x23, 0x02, 0xA0, 0xE4, 0xD5, 0x75, 0x51, 0x32, 0x02, 0x53, 0xFD, 0x16, 0x3A, 0x21, 0x3B, 
            0x16, 0x0F, 0xC3, 0xB2, 0xBB, 0xB3, 0xE2, 0xBA, 0x3A, 0x3D, 0x13, 0xEC, 0xF6, 0x01, 0x45, 0x84, 
            0xA5, 0x70, 0x0F, 0x93, 0x49, 0x0C, 0x64, 0xCD, 0x31, 0xD5, 0xCC, 0x4C, 0x07, 0x01, 0x9E, 0x00, 
            0x1A, 0x23, 0x90, 0xBF, 0x88, 0x1E, 0x3B, 0xAB, 0xA6, 0x3E, 0xC4, 0x73, 0x47, 0x10, 0x7E, 0x3B, 
            0x5E, 0xBC, 0xE3, 0x00, 0x84, 0xFF, 0x09, 0xD4, 0xE0, 0x89, 0x0F, 0x5B, 0x58, 0x70, 0x4F, 0xFB, 
            0x65, 0xD8, 0x5C, 0x53, 0x1B, 0xD3, 0xC8, 0xC6, 0xBF, 0xEF, 0x98, 0xB0, 0x50, 0x4F, 0x0F, 0xEA, 
            0xE5, 0x83, 0x58, 0x8C, 0x28, 0x2C, 0x84, 0x67, 0xCD, 0xD0, 0x9E, 0x47, 0xDB, 0x27, 0x50, 0xCA, 
            0xF4, 0x63, 0x63, 0xE8, 0x97, 0x7F, 0x1B, 0x4B, 0x0C, 0xC2, 0xC1, 0x21, 0x4C, 0xCC, 0x58, 0xF5, 
            0x94, 0x52, 0xA3, 0xF3, 0xD3, 0xE0, 0x68, 0xF4, 0x00, 0x23, 0xF3, 0x5E, 0x0A, 0x7B, 0x93, 0xDD, 
            0xAB, 0x12, 0xB2, 0x13, 0xE8, 0x84, 0xD7, 0xA7, 0x9F, 0x0F, 0x32, 0x4C, 0x55, 0x1D, 0x04, 0x36, 
            0x52, 0xDC, 0x03, 0xF3, 0xF9, 0x4E, 0x42, 0xE9, 0x3D, 0x61, 0xEF, 0x7C, 0xB6, 0xB3, 0x93, 0x50
        };

        /// <summary>
        /// 创建一个 <see cref="KgmAudioDecrypter"/> 的对象实例.
        /// </summary>
        public KgmAudioDecrypter() : base()
        { }

        /// <summary>
        /// 执行解密任务.
        /// </summary>
        /// <param name="item">解密的音频项.</param>
        protected override void DoDecrypt(DecryptAudioItem item)
        {
            Exception tmpEx = null;
            try
            {
                bool extVpr = Path.GetExtension(item.File).ToLower() == ".vpr";
                ValidationThrowException(extVpr);
                int headLen = ReadInt32(Source);           
                byte[] maskV2 = GetMaskV2();
                byte[] key1 = new byte[17];
                Source.Seek(8, SeekOrigin.Current); // 流中的第 28 个字节开始.
                Source.Read(key1, 0, 16);
                key1[16] = 0x00;
                Source.Seek(headLen, SeekOrigin.Begin);
                Output.Position = 0;
                byte[] buffer = new byte[4096];
                double progressBytes = Convert.ToDouble(Source.Length - headLen);
                int offset = 0, rlen, pos, i;
                byte med8, mask8;
                do
                {
                    rlen = Source.Read(buffer, 0, buffer.Length);
                    if (rlen < 1)
                        break;

                    for (i = 0; i < rlen; ++i)
                    {
                        pos = offset + i;
                        med8 = (byte)(key1[pos % 17] ^ buffer[i]);
                        med8 ^= (byte)((med8 & 0x0f) << 4);
                        mask8 = (byte)(MaskV2PreDef[pos % 272] ^ maskV2[pos >> 4]);
                        mask8 ^= (byte)((mask8 & 0x0f) << 4);
                        buffer[i] = (byte)(med8 ^ mask8);
                        if (extVpr)
                            buffer[i] ^= vpr_mask_diff[pos % 17];
                    }
                    offset += rlen;
                    Output.Write(buffer, 0, rlen);
                    OnProgress(item, (float)(offset / progressBytes * 100));
                }
                while (rlen > 0);
                Output.Flush();
            }
            catch (Exception Ex)
            {
                if (UseMultithreaded)
                    OnCompleted(new CompletedEventArgs(false, item, Ex));
                else
                    tmpEx = Ex;
            }
            if (!UseMultithreaded && tmpEx != null)
                throw tmpEx;
        }

        private void ValidationThrowException(bool extVpr)
        {
            byte[] buffer = new byte[16];
            Source.Read(buffer, 0, buffer.Length);
            if (extVpr)
            {
                if (!BytesEqual(buffer, 0, vpr_header, 0, vpr_header.Length))
                    throw new Exception("无效的 vpr 音频.");
            }
            else
            {
                if (!BytesEqual(buffer, 0, kgm_header, 0, kgm_header.Length))
                    throw new Exception("无效的 kgm 音频.");
            }
        }
        /*
        private byte[] GetMaskV2()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XZInputStream xzStream = new XZInputStream(global::cnzhnet.music_decrypt.DefaultResource.kgm_mask))
                {
                    buffer = new byte[2048];
                    int rlen = 0;
                    do
                    {
                        rlen = xzStream.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, rlen);
                    } while (rlen > 0);
                    ms.Flush();
                    buffer = ms.ToArray();
                }
            }
            return buffer;
        }*/
        private byte[] GetMaskV2()
        {
            byte[] buffer;
            using (MemoryStream src = new MemoryStream(global::cnzhnet.music_decrypt.DefaultResource.kgm_mask))
            {
                using (XZStream xz = new XZStream(src))
                {
                    using (MemoryStream dest = new MemoryStream())
                    {
                        buffer = new byte[2048];
                        int rlen = 0;
                        do
                        {
                            rlen = xz.Read(buffer, 0, buffer.Length);
                            dest.Write(buffer, 0, rlen);
                        } while (rlen > 0);
                        dest.Flush();
                        buffer = dest.ToArray();
                    }
                }
            }
            return buffer;
        }
    }
}
