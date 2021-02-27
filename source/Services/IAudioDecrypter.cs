using System;
using System.Collections.Generic;
using System.IO;
using cnzhnet.music_decrypt.Models;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 用于规定实现流解密的实现接口.
    /// </summary>
    public interface IAudioDecrypter
    {
        /// <summary>
        /// 表示要解密的源数据流（即解密该流）.
        /// </summary>
        Stream Source { get; set; }

        /// <summary>
        /// 表示解密结果的输出流.
        /// </summary>
        Stream Output { get; set; }

        /// <summary>
        /// 是否使用多线程.
        /// </summary>
        bool UseMultithreaded { get; set; }

        /// <summary>
        /// 当解密任务终止或执行完成时发生此事件.
        /// </summary>
        event CompletedEventHandler Completed;

        /// <summary>
        /// 执行数据解密.
        /// </summary>
        /// <param name="id">解密的音频项.</param>
        void Decrypt(DecryptAudioItem item);
    }

    /// <summary>
    /// 用于传递解密完成事件信息的对象.
    /// </summary>
    public class CompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 创建一个 <see cref="CompletedEventArgs"/> 的对象实例.
        /// </summary>
        /// <param name="_success">成功则为 true，否则为 false .</param>
        /// <param name="item">输出的文件完整路径</param>
        /// <param name="err">错误信息（若成功时则应为 null）.</param>
        public CompletedEventArgs(bool _success, DecryptAudioItem item, Exception err = null)
        {
            Success = _success;
            Item = item;
            Error = err;
        }

        /// <summary>
        /// 若成功处理任务则为 true，否则为 false .
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 任务输出的流.
        /// </summary>
        public DecryptAudioItem Item { get; private set; }
        /// <summary>
        /// 在任务处理终止时产生的错误（当 Success 为 true 时此值为 null）.
        /// </summary>
        public Exception Error { get; private set; }
    }
    /// <summary>
    /// 用于执行完成事件的处理程序的委托.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CompletedEventHandler(IAudioDecrypter sender, CompletedEventArgs e);
}
