using System;
using System.Collections.Generic;

namespace hsp.rtm
{
    /// <summary>
    /// エラー処理に関するクラス
    /// </summary>
    public static class Error
    {
        public static List<string> ErrorMessages = new List<string>();

        /// <summary>
        /// HSPのようにエラー出力
        /// </summary>
        /// <param name="message"></param>
        public static void AlertError(string message)
        {
            ErrorMessages.Add(message);
        }

        /// <summary>
        /// C#のようなエラー出力
        /// </summary>
        /// <param name="ex"></param>
        public static void AlertError(Exception ex)
        {
            ErrorMessages.Add(ex.ToString());
        }
    }
}
