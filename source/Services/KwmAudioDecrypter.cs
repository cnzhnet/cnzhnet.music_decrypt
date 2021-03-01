using System;
using System.Threading;
using cnzhnet.music_decrypt.Models;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 用于实现 *.kwm 酷我音乐加密格式的音频解密.
    /// </summary>
    public class KwmAudioDecrypter : AudioDecrypter
    {
        private byte[] kwm_headers;
        private const string pre_defined_key = "MoOtOiTvINGwd2E6n0E1i7L5t2IoOoNk";

        /// <summary>
        /// 创建一个 <see cref="KwmAudioDecrypter"/> 的对象实例.
        /// </summary>
        public KwmAudioDecrypter() : base()
        {
            kwm_headers = new byte[16] { 0x79, 0x65, 0x65, 0x6c, 0x69, 0x6f, 0x6e, 0x2d, 0x6b, 0x75, 0x77, 0x6f, 0x2d, 0x74, 0x6d, 0x65 };
        }

        /// <summary>
        /// 执行解密任务.
        /// </summary>
        /// <param name="item">解密的音频项.</param>
        protected override void DoDecrypt(DecryptAudioItem item)
        {
            Exception tmpEx = null;
            try
            {
                Source.Position = 0;
                byte[] buffer = new byte[4096];
                Source.Read(buffer, 0, buffer.Length);
                if (!BytesEqual(kwm_headers, 0, buffer, 0, kwm_headers.Length))
                    throw new Exception("无效的 kwm 文件.");

                double progressBytes = Convert.ToDouble(Source.Length - 1024);
                byte[] key = FindDecryptKey();
                Source.Position = 1024;
                Output.Position = 0;
                int processed = 0, i, readLen;                
                do
                {
                    readLen = Source.Read(buffer, 0, buffer.Length);
                    if (readLen < 1)
                        break;

                    for (i = 0; i < readLen; ++i)
                        buffer[i] ^= key[i & 0x1f];
                    Output.Write(buffer, 0, readLen);
                    processed += readLen;
                    OnProgress(item, (float)(processed / progressBytes * 100));
                }
                while (readLen > 0);
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

        //private byte[] GetKeyMask(byte[] keyBytes)
        //{
        //    string ks = BitConverter.ToUInt64(keyBytes, 0).ToString();
        //    if (ks.Length > 32)
        //        ks = ks.Substring(0, 32);
        //    else if (ks.Length < 32)
        //        ks = ks.PadEnd(32, ks);

        //    byte[] key = new byte[32];
        //    for (int i = 0; i < 32; ++i)
        //        key[i] = (byte)(((byte)pre_defined_key[i]) ^ ((byte)ks[i]));
        //    return key;
        //}


        /// <summary>
        /// 在分析源数据流，并在其中找到解密所需的密钥.
        /// </summary>
        /// <returns></returns>
        private byte[] FindDecryptKey()
        {
            const int find_key_maxlen = 468;
            byte[] key = new byte[32];
            byte[] old_key = new byte[32];
            bool foundKey = false;

            Source.Position = 1024L;
            int i = 0;
            for (; i < find_key_maxlen; ++i)
            {
                Source.Read(key, 0, 32);
                if (BytesEqual(key, 0, old_key, 0, 32))
                {
                    foundKey = true;
                    break;
                }
                Array.Copy(key, 0, old_key, 0, 32);
            }
            if (!foundKey)
            {
                byte[] tmp = new byte[32];
                for (i = 0; i < 16; ++i)
                    tmp[i] = key[i + 16];
                for (i = 16; i < 32; ++i)
                    tmp[i] = key[i - 16];
                Array.Copy(tmp, 0, key, 0, 32);
            }
            return key;
        }
    }
}
