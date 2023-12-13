using System;

/// <summary>
/// 列挙型のフィールドにラベル文字列を付加するカスタム属性
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class LabeledEnum : Attribute
{

    /// <summary>ラベル文字列.</summary>
    public string Label { get; private set; }

    /// <summary>
    /// LabeledEnumAttribute クラスの新しいインスタンスを初期化
    /// </summary>
    /// <param name="label">ラベル文字列.</param>
    public LabeledEnum(string label)
    {
        Label = label;
    }
}

public static class LabeledEnumExt
{
    /// <summary>
    /// 属性で指定されたラベル文字列を取得
    /// </summary>
    /// <param name="value">ラベル付きフィールド</param>
    /// <returns>ラベル文字列</returns>
    public static string GetLabel(this Enum value)
    {
        //EnumのTypeを取得
        // type = {Name = "フォーマット形式共通" FullName = "共通.フォーマット形式共通"}
        Type type = value.GetType();

        //Enumのフィールドを取得
        // name = "Default"
        string name = Enum.GetName(type, value);

        //クラスの配列を取得
        // attributes = {共通.LabeledEnum[1]}
        if (type.GetField(name).GetCustomAttributes(typeof(LabeledEnum), false) is LabeledEnum[] attributes && attributes.Length > 0)
        {
            return attributes[0].Label;
        }
        else
        {
            return name;
        }
    }
}