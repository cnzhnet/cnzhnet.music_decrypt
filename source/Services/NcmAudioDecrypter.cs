using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using cnzhnet.music_decrypt.Models;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 用于实现 *.ncm 网易云音乐加密格式的音频解密.
    /// </summary>
    public class NcmAudioDecrypter : AudioDecrypter
    {
        private static readonly byte[] AES_CORE_KEY = new byte[] { 0x68, 0x7a, 0x48, 0x52, 0x41, 0x6d, 0x73, 0x6f, 0x35, 0x6b, 0x49, 0x6e, 0x62, 0x61, 0x78, 0x57 };
        private static readonly byte[] AES_MODIFY_KEY = new byte[] { 0x23, 0x31, 0x34, 0x6c, 0x6a, 0x6b, 0x5f, 0x21, 0x5c, 0x5d, 0x26, 0x30, 0x55, 0x3c, 0x27, 0x28 };
        private static readonly byte[] ncm_headers = new byte[] { 0x43, 0x54, 0x45, 0x4e, 0x46, 0x44, 0x41, 0x4d };

        /// <summary>
        /// 创建一个 <see cref="NcmAudioDecrypter"/> 的对象实例.
        /// </summary>
        public NcmAudioDecrypter() : base()
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
                const int read_size = 1024;
                byte[] buffer = new byte[read_size];
                Source.Read(buffer, 0, 8);
                if (!BytesEqual(ncm_headers, 0, buffer, 0, 8))
                    throw new Exception("无效的 ncm 文件.");
                Source.Seek(2, SeekOrigin.Current);
                byte[] keyData = SubBytes(DecryptAes128ECB(AES_CORE_KEY, ReadKeyBytes()), 17);
                byte[] keyBox = BuildKeyBox(keyData);
                byte[] modifyData = ReadModifyData();
                /*** 此为网易云音乐针对 ncm 格式的附加信息解析，此处忽略.
                byte[] decryptModifyData = Convert.FromBase64String(Encoding.UTF8.GetString(SubBytes(modifyData, 22)));
                decryptModifyData = DecryptAes128ECB(AES_MODIFY_KEY, decryptModifyData);
                string ncmjson = Encoding.UTF8.GetString(SubBytes(decryptModifyData, 6));
                */
                Source.Seek(9, SeekOrigin.Current); // 跳过 CRC 校验
                // 跳过专辑图片.
                int rlen = ReadInt32(Source);
                if (rlen > 0)
                    Source.Seek(rlen, SeekOrigin.Current);
                // 解密音频数据.      
                int i, j;
                Output.Position = 0;
                while ((rlen = Source.Read(buffer, 0, read_size)) > 0)
                {
                    for (i = 0; i < read_size; ++i)
                    {
                        j = (byte)((i + 1) & 0xff);
                        buffer[i] ^= keyBox[keyBox[j] + keyBox[(keyBox[j] + j) & 0xff] & 0xff];
                    }
                    Output.Write(buffer, 0, rlen);
                }
                // 获取音频的格式.
                Output.Position = 0;
                Output.Read(buffer, 0, buffer.Length);
                item.OutputExt = GetAudioExt(buffer, 0);
                Output.Position = 0;
                if (UseMultithreaded)
                    OnCompleted(new CompletedEventArgs(true, item));
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

        private byte[] ReadKeyBytes()
        {
            byte[] keyBytes = new byte[ReadInt32(Source)];
            Source.Read(keyBytes, 0, keyBytes.Length);
            for (int i = 0; i < keyBytes.Length; ++i)
                keyBytes[i] ^= 0x64;
            return keyBytes;
        }
        private byte[] ReadModifyData()
        {
            byte[] modifyData = new byte[ReadInt32(Source)];
            Source.Read(modifyData, 0, modifyData.Length);
            for (int i = 0; i < modifyData.Length; ++i)
                modifyData[i] ^= 0x63;
            return modifyData;
        }
        private byte[] BuildKeyBox(byte[] key)
        {
            byte[] box = new byte[256];
            int i = 0;
            for (; i < 256; ++i)
                box[i] = (byte)i;
            byte c, last = 0, offset = 0, swap;
            for (i = 0; i < 256; ++i)
            {
                swap = box[i];
                c = (byte)((swap + last + key[offset++]) & 0xff);
                if (offset >= (byte)(key.Length))
                    offset = 0;
                box[i] = box[c];
                box[c] = swap;
                last = c;
            }
            return box;
        }
        /// <summary>
        /// 截取字节数组.
        /// </summary>
        /// <param name="src">从该字节数组中截取.</param>
        /// <param name="offset">src 的起始偏移量.</param>
        /// <param name="length">要截取的数字数量.</param>
        /// <returns></returns>
        private byte[] SubBytes(byte[] src, int offset, int length = 0)
        {
            byte[] result;
            if (length == 0)
            {
                result = new byte[src.Length - offset];
                Array.Copy(src, offset, result, 0, result.Length);
            }
            else
            {
                result = new byte[length];
                Array.Copy(src, offset, result, 0, length);
            }
            return result;
        }

        /// <summary>
        /// 使用128位 Aes 解密数据.
        /// </summary>
        /// <param name="keyBytes">包含 Key 数据的字节数组.</param>
        /// <param name="data">要解密的数据.</param>
        /// <returns></returns>
        private byte[] DecryptAes128ECB(byte[] keyBytes, byte[] data)
        {
            byte[] buffer = null;
            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.ECB;
                using (ICryptoTransform transform = aes.CreateDecryptor(keyBytes, null))
                    buffer = transform.TransformFinalBlock(data, 0, data.Length);
            }
            return buffer;
        }
    }
}
