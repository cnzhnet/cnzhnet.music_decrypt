using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cnzhnet.music_decrypt.Models
{
    /// <summary>
    /// 表示音频的 PE 文件头信息
    /// </summary>
    public class AudioPeHeader
    {
        /// <summary>
        /// 创建一个 <see cref="AudioPeHeader"/> 的对象实例.
        /// </summary>
        /// <param name="_ext">音频应为的文件扩展名（带点）.</param>
        /// <param name="_head">音频的 PE 头字节数组.</param>
        public AudioPeHeader(string _ext, byte[] _head)
        {
            Ext = _ext;
            Head = _head;
        }

        /// <summary>
        /// 表示音频应为的文件扩展名（带点）.
        /// </summary>
        public string Ext { get; private set; }

        /// <summary>
        /// 表示音频的 PE 头字节数组.
        /// </summary>
        public byte[] Head { get; private set; }
    }
}
