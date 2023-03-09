/// <summary>
///  String extentions.
/// </summary>
public static partial class StringExtentions
{
    public static bool IsNullOrEmpty(this string target)
    {
        return target == null || target.Length == 0;
    }
}