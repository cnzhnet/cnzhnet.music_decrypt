using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cnzhnet.music_decrypt.Services;

namespace cnzhnet.music_decrypt.Models
{
    /// <summary>
    /// 表示支持的
    /// </summary>
    public sealed class AudioSupported
    {
        /// <summary>
        /// 创建一个 <see cref="AudioSupported"/> 的对象实例.
        /// </summary>
        /// <param name="ext">表示加密音频的文件扩展名（带点）.</param>
        /// <param name="factory">此加密音频的厂商说明.</param>
        /// <param name="decrypter">支持此音频解密的解密器类型.</param>
        private AudioSupported(string ext, string factory, Type decrypter)
        {
            Extension = ext;
            Manufacturer = factory;
            DecrypterType = decrypter;
        }

        /// <summary>
        /// 表示加密音频的文件扩展名（带点）.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// 此加密音频的厂商说明.
        /// </summary>
        public string Manufacturer { get; private set; }

        /// <summary>
        /// 支持此音频解密的解密器类型.
        /// </summary>
        public Type DecrypterType { get; private set; }

        /// <summary>
        /// 创建一个音频解密支持.
        /// </summary>
        /// <param name="ext">表示加密音频的文件扩展名（带点）.</param>
        /// <param name="factory">此加密音频的厂商说明.</param>
        /// <param name="decrypter">支持此音频解密的解密器类型.</param>
        /// <returns></returns>
        public static AudioSupported Create(string ext, string factory, Type decrypter)
        {
            if (string.IsNullOrEmpty(ext))
                return null;
            return new AudioSupported(ext.ToLower(), factory, decrypter);
        }
    }
}
