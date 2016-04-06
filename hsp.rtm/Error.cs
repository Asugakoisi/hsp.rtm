using System;

namespace hsp.rtm
{
    /// <summary>
    /// エラー処理に関するクラス
    /// </summary>
    public static class Error
    {
        /// <summary>
        /// HSPのようにエラー出力
        /// </summary>
        /// <param name="message"></param>
        public static void AlertError(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// C#のようなエラー出力
        /// </summary>
        /// <param name="ex"></param>
        public static void AlertError(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
