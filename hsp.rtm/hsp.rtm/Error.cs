using System;
using System.Diagnostics;
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
        /// エラー出力
        /// </summary>
        /// <param name="message">エラー文</param>
        public static void AlertError(string message)
        {
            ErrorMessages.Add(message + "\ntracelog -> " + GetCallerMethodName());
        }

        /// <summary>
        /// エラー出力
        /// </summary>
        /// <param name="method">エラー箇所</param>
        /// <param name="message">エラー文</param>
        public static void AlertError(string method, string message)
        {
            ErrorMessages.Add(message + "\ntracelog -> " + method);
        }

        /// <summary>
        /// C#の例外を用いたエラー出力
        /// </summary>
        /// <param name="ex">例外</param>
        public static void AlertError(Exception ex)
        {
            ErrorMessages.Add(ex + "\ntracelog -> " + GetCallerMethodName());
        }

        /// <summary>
        /// 1つ前のスタックフレーム情報から呼び出し元のメソッド名を取得する
        /// </summary>
        /// <returns></returns>
        private static string GetCallerMethodName()
        {
            return new StackFrame(2).GetMethod().Name;
        }
    }
}
