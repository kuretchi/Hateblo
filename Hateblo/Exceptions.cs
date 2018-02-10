using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hateblo
{
    /// <summary>
    /// はてなブログ AtomPub からエラーが返却された場合にスローされる例外。このクラスは抽象クラスです。
    /// </summary>
    public abstract class HatebloException : Exception { }

    /// <summary>
    /// 存在しないリソースにアクセスした場合にスローされる例外。
    /// </summary>
    public class ResourceNotFoundException : HatebloException
    {
        public override string Message => "Resource not found.";
    }

    /// <summary>
    /// はてなブログ AtomPub で問題が発生した場合にスローされる例外。
    /// </summary>
    public class InternalServerErrorException : HatebloException
    {
        public override string Message => "Internal server error.";
    }
}
