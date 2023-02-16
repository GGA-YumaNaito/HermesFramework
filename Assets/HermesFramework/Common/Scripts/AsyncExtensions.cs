using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

/// <summary>
/// AsyncExtensions
/// </summary>
static class AsyncExtensions
{
    /// <summary>
    /// 非同期処理を待たない
    /// </summary>
    /// <param name="t"></param>
    public static void NoAwait(this Task t)
    {
        void AwaitCatcher(Task _) { }
        t.ContinueWith(AwaitCatcher);
    }

    /// <summary>
    /// 非同期処理を待たない
    /// </summary>
    /// <param name="t"></param>
    public static void NoAwait(this UniTask t)
    {
        void AwaitCatcher() { }
        t.ContinueWith(AwaitCatcher);
    }
}