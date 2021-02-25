using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 继承此类实现音频流的解密器.
    /// </summary>
    public abstract class StreamDecrypter : IStreamDecrypter
    {
        /// <summary>
        /// 创建一个 <see cref="StreamDecrypter"/> 的对象实例.
        /// </summary>
        protected StreamDecrypter()
        {
            UseMultithreaded = true;
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
        public abstract bool DoWorking { get; }

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
        /// 执行数据解密.
        /// </summary>
        /// <param name="id">解密项的唯一标识.</param>
        public abstract void Decrypt(string id);

        #region 静态成员
        private static Dictionary<string, Type> decrypters = new Dictionary<string, Type>();

        /// <summary>
        /// 获取支持的解密器.
        /// </summary>
        /// <param name="extension">要解密的音频文件的后缀.</param>
        /// <returns></returns>
        public static IStreamDecrypter GetDecrypter(string extension)
        {
            Type t = null;
            if (!decrypters.TryGetValue(extension, out t))
                return null;

            return Activator.CreateInstance(t) as IStreamDecrypter;
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
