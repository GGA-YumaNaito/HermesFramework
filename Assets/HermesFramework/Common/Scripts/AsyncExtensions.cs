using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

/// <summary>
/// AsyncExtensions
/// </summary>
static class AsyncExtensions
{
    /// <summary>
    /// �񓯊�������҂��Ȃ�
    /// </summary>
    /// <param name="t"></param>
    public static void NoAwait(this Task t)
    {
        void AwaitCatcher(Task _) { }
        t.ContinueWith(AwaitCatcher);
    }

    /// <summary>
    /// �񓯊�������҂��Ȃ�
    /// </summary>
    /// <param name="t"></param>
    public static void NoAwait(this UniTask t)
    {
        void AwaitCatcher() { }
        t.ContinueWith(AwaitCatcher);
    }
}