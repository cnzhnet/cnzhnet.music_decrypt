using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cnzhnet.music_decrypt.Models
{
    /// <summary>
    /// 表示待解密的音频文件列表.
    /// </summary>
    [Serializable()]
    public class DecryptAudioItem
    {
        /// <summary>
        /// 创建一个 <see cref="DecryptAudioItem"/>
        /// </summary>
        public DecryptAudioItem()
        {
            Id = Guid.NewGuid().ToString("N");
            Status = "等待中.";
        }

        /// <summary>
        /// 表示此项的唯一标识ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// 表示文件名.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// 表示文件的完整路径.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 获取或设置解密输出的目标文件.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// 表示解密后输出的音频文件扩展名（带 .）
        /// </summary>
        public string OutputExt { get; set; }

        /// <summary>
        /// 表示处理状态.
        /// </summary>
        public string Status { get; set; }
    }
}
