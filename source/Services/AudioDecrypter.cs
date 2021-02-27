using cnzhnet.music_decrypt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 继承此类实现音频流的解密器.
    /// </summary>
    public abstract class AudioDecrypter : IAudioDecrypter
    {
        /// <summary>
        /// 用于描述支持的输出目标音频格式的 PE 头信息.
        /// </summary>
        private static readonly Dictionary<string, byte[]> audioHeaders = new Dictionary<string, byte[]> {
            { ".mp3",  new byte[3] { 0x49, 0x44, 0x33 } },
            { ".flac", new byte[4] { 0x66, 0x4c, 0x61, 0x43 } },
            { ".ogg",  new byte[4] { 0x4f, 0x67, 0x67, 0x53 } },
            { ".m4a",  new byte[4] { 0x66, 0x74, 0x79, 0x70 } },
            { ".wav",  new byte[4] { 0x52, 0x49, 0x46, 0x46 } },
            { ".wma",  new byte[16] { 0x30, 0x26, 0xb2, 0x75, 0x8e, 0x66, 0xcf, 0x11, 0xa6, 0xd9, 0x00, 0xaa, 0x00, 0x62, 0xce, 0x6c }}
        };
        protected object _doWorking;

        /// <summary>
        /// 创建一个 <see cref="AudioDecrypter"/> 的对象实例.
        /// </summary>
        protected AudioDecrypter()
        {
            UseMultithreaded = true;
            _doWorking = false;
        }

        /// <summary>
        /// 表示要解密的源数据流（即解密该流）.
        /// </summary>
        public Stream Source { get; set; }

        /// <summary>
        /// 表示解密结果的输出流.
        /// </summary>
        public Stream Output { get; set; }

        /// <summary>
        /// 是否使用多线程，默认值 true.
        /// </summary>
        public bool UseMultithreaded { get; set; }

        /// <summary>
        /// 若该实例有处理任务正在执行则返回 true，否则返回 false .
        /// </summary>
        public bool DoWorking => (bool)_doWorking;

        /// <summary>
        /// 当解密任务终止或执行完成时发生此事件.
        /// </summary>
        public event CompletedEventHandler Completed;

        /// <summary>
        /// 用于触发 <see cref="Completed"/> 事件.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCompleted(CompletedEventArgs e) => Completed?.Invoke(this, e);

        /// <summary>
        /// 比较两个字节数组，若所有元素完全相同则返回 true，否则返回 false .
        /// </summary>
        /// <param name="array1">比较的第一个数组</param>
        /// <param name="array2">比较的第二个数组</param>
        /// <returns></returns>
        protected bool CompareBytes(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array1.Length; ++i)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 比较两个字节数组中指定部份的数据是否相同.
        /// </summary>
        /// <param name="b1">第一个字节数组.</param>
        /// <param name="offset1">b1 的偏移量，从 b1 的该处开始比较.</param>
        /// <param name="b2">第二个字节数组.</param>
        /// <param name="offset2">b2 的偏移量,从 b2 的该处开始比较.</param>
        /// <param name="size">要比较的字节数量.</param>
        /// <returns></returns>
        public bool BytesEqual(byte[] b1, int offset1, byte[] b2, int offset2, int size)
        {
            if (b1.Length - offset1 < size)
                return false;
            if (b2.Length - offset2 < size)
                return false;
            for (int i = 0; i < size; ++i)
            {
                if (b1[offset1++] != b2[offset2++])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 根据给定的音频文件数据获取音频的格式扩展名.
        /// </summary>
        /// <param name="buffers">包含音频文件数据的字节数组.</param>
        /// <param name="offset">从 buffers 的该处开始分析音频类型.</param>
        /// <returns></returns>
        protected string GetAudioExt(byte[] buffers, int offset)
        {
            foreach (KeyValuePair<string, byte[]> head in audioHeaders)
            {
                if (head.Key == ".m4a")
                    offset += 4;
                if (BytesEqual(head.Value, 0, buffers, offset, head.Value.Length))
                    return head.Key;
            }
            return string.Empty;
        }

        /// <summary>
        /// 执行解密任务.
        /// </summary>
        /// <param name="item">解密的音频项.</param>
        protected abstract void DoDecrypt(DecryptAudioItem item);

        /// <summary>
        /// 执行数据解密.
        /// </summary>
        /// <param name="id">解密的音频项.</param>
        public void Decrypt(DecryptAudioItem item)
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
                Task.Factory.StartNew(() => DoDecrypt(item));
            else
                DoDecrypt(item);
        }

        #region 静态成员
        private static Dictionary<string, Type> decrypters = new Dictionary<string, Type>();

        /// <summary>
        /// 获取支持的解密器.
        /// </summary>
        /// <param name="extension">要解密的音频文件的后缀.</param>
        /// <returns></returns>
        public static IAudioDecrypter GetDecrypter(string extension)
        {
            Type t = null;
            if (!decrypters.TryGetValue(extension, out t))
                return null;

            return Activator.CreateInstance(t) as IAudioDecrypter;
        }

        /// <summary>
        /// 注册指定的加密音频文件扩展名所对应的解密器.
        /// </summary>
        /// <param name="extension">加密音频文件的扩展名</param>
        /// <param name="decrypterType">此类加密音频所对应的解密器类型.</param>
        public static void RegisterDecrypter(string extension, Type decrypterType)
        {
            if (string.IsNullOrEmpty(extension))
                return;
            if (decrypters.ContainsKey(extension))
                return;
            decrypters.Add(extension, decrypterType);
        }

        /// <summary>
        /// 获取受支持的解密音频文件扩展名.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSupportedExtensions()
        {
            if (decrypters.Count < 1)
                return new List<string>();
            string[] keys = new string[decrypters.Keys.Count];
            decrypters.Keys.CopyTo(keys, 0);
            return new List<string>(keys);
        }
        #endregion
    }
}
