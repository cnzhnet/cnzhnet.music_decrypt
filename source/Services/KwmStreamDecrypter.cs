using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 用于实现 *.kwm 酷我音乐无损加密格式的音乐破解.
    /// </summary>
    public class KwmStreamDecrypter : StreamDecrypter
    {
        private object _doWorking;

        /// <summary>
        /// 创建一个 <see cref="KwmStreamDecrypter"/> 的对象实例.
        /// </summary>
        public KwmStreamDecrypter() : base()
        {
            _doWorking = false;
        }

        #region 属性实现
        /// <summary>
        /// 若该实例有处理任务正在执行则返回 true，否则返回 false .
        /// </summary>
        public override bool DoWorking => (bool)_doWorking;

        #endregion

        /// <summary>
        /// 执行解密任务.
        /// </summary>
        /// <param name="id">解密项的唯一标识.</param>
        private void DoDecrypt(string id)
        {
            Exception tmpEx = null;
            try
            {
                long processed = 1024;
                byte[] key = FindDecryptKey();

                Source.Position = 1024;
                Output.Position = 0;
                int i, readLen;
                byte[] buffer = new byte[1024];
                do
                {
                    readLen = Source.Read(buffer, 0, buffer.Length);
                    for (i = 0; i < readLen; ++i)
                        buffer[i] ^= key[i & 0x1f];
                    Output.Write(buffer, 0, readLen);
                    processed += readLen;
                }
                while (processed < Source.Length);
                Output.Position = 0;
                if (UseMultithreaded)
                    OnCompleted(new CompletedEventArgs(true, id));
            }
            catch (Exception Ex)
            {
                if (UseMultithreaded)
                    OnCompleted(new CompletedEventArgs(false, id, Ex));
                else
                    tmpEx = Ex;
            }
            Interlocked.Exchange(ref _doWorking, false);
            if (!UseMultithreaded && tmpEx != null)
                throw tmpEx;
        }

        /// <summary>
        /// 执行数据解密.
        /// </summary>
        /// <param name="id">解密项的唯一标识.</param>
        public override void Decrypt(string id)
        {
            if (DoWorking)
                return;
            if (Source == null)
                throw new ArgumentNullException(nameof(Source));
            if (Output == null)
                throw new ArgumentNullException(nameof(Output));
            if (DoWorking)
                return;

            Interlocked.Exchange(ref _doWorking, true);
            if (UseMultithreaded)
                Task.Factory.StartNew(() => DoDecrypt(id));
            else
                DoDecrypt(id);
        }
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
                if (CompareBytes(key, old_key))
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
