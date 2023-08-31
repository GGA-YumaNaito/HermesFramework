using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.API
{
    /// <summary>APIエラーコード.</summary>
    public enum eAPIErrorCode
    {
        /// <summary>None.</summary>
        [eAPIErrorCodeExtensions.Attributes("None")] None = 0,

        // <summary>予期せぬエラーが発生しました</summary>
        [eAPIErrorCodeExtensions.Attributes("UNEXPECTED_ERROR")] UNEXPECTED_ERROR = 99999,
        // <summary>APIテストのエラーが発生しました</summary>
        [eAPIErrorCodeExtensions.Attributes("API_TEST_ERROR")] API_TEST_ERROR = 99998,

        // <summary>入力内容が間違いがあります</summary>
        [eAPIErrorCodeExtensions.Attributes("VALIDATION_ERROR")] VALIDATION_ERROR = 10001,
        // <summary>NGワードが含まれております</summary>
        [eAPIErrorCodeExtensions.Attributes("NGWORD_ERROR")] NGWORD_ERROR = 10002,
        // <summary>すでに入力済みです</summary>
        [eAPIErrorCodeExtensions.Attributes("ALREADY_INPUT_ERROR")] ALREADY_INPUT_ERROR = 10003,

        // <summary>ログインに失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("LOGIN_ERROR")] LOGIN_ERROR = 20001,
        // <summary>ログイントークンが存在しません</summary>
        [eAPIErrorCodeExtensions.Attributes("NOT_LOGIN_TOKEN")] NOT_LOGIN_TOKEN = 20002,
        // <summary>アカウントが凍結されております</summary>
        [eAPIErrorCodeExtensions.Attributes("ACCOUNT_BAN")] ACCOUNT_BAN = 20003,
        // <summary>アカウントが永久に凍結されております</summary>
        [eAPIErrorCodeExtensions.Attributes("ACCOUNT_ETERNAL_BAN")] ACCOUNT_ETERNAL_BAN = 20004,
        // <summary>アカウントが凍結されました</summary>
        [eAPIErrorCodeExtensions.Attributes("ACCOUNT_BAN_AUTH_CHECK")] ACCOUNT_BAN_AUTH_CHECK = 20005,

        // <summary>ユーザー作成に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("USER_CREATE_ERROR")] USER_CREATE_ERROR = 30001,
        // <summary>ユーザーが存在しません</summary>
        [eAPIErrorCodeExtensions.Attributes("USER_NOT_FOUND_ERROR")] USER_NOT_FOUND_ERROR = 30002,
        // <summary>スタミナが足りません</summary>
        [eAPIErrorCodeExtensions.Attributes("SHORTAGE_STAMINA")] SHORTAGE_STAMINA = 31001,
        // <summary>コインが足りません</summary>
        [eAPIErrorCodeExtensions.Attributes("SHORTAGE_COIN")] SHORTAGE_COIN = 31002,
        // <summary>ジュエルが足りません</summary>
        [eAPIErrorCodeExtensions.Attributes("SHORTAGE_JEWEL")] SHORTAGE_JEWEL = 31003,
        // <summary>ジュエルが足りません</summary>
        [eAPIErrorCodeExtensions.Attributes("SHORTAGE_JEWEL_PAID")] SHORTAGE_JEWEL_PAID = 31004,
        // <summary>誕生日が登録できませんでした</summary>
        [eAPIErrorCodeExtensions.Attributes("USER_BIRTHDAY_INPUT_ERROR")] USER_BIRTHDAY_INPUT_ERROR = 31005,
        // <summary>アイコンを所持しておりません</summary>
        [eAPIErrorCodeExtensions.Attributes("NOT_POSSESSION_ICON")] NOT_POSSESSION_ICON = 31006,
        // <summary>フレームを所持しておりません</summary>
        [eAPIErrorCodeExtensions.Attributes("NOT_POSSESSION_FRAME")] NOT_POSSESSION_FRAME = 31007,
        // <summary>ユーザー名の変更に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("NOT_POSSESSION_EQUIP")] NOT_POSSESSION_EQUIP = 31008,
        // <summary>拠点スキルの習得に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("CHANGE_USER_NAME_ERROR")] CHANGE_USER_NAME_ERROR = 31009,
        // <summary>拠点スキルの前提条件を満たしておりません</summary>
        [eAPIErrorCodeExtensions.Attributes("PLAYER_BASESKILL_LEARN_ERROR")] PLAYER_BASESKILL_LEARN_ERROR = 31010,
        // <summary>スキル取得の費用を満たしておりません</summary>
        [eAPIErrorCodeExtensions.Attributes("PLAYER_BASESKILL_UNLOCKD_ERROR")] PLAYER_BASESKILL_UNLOCKD_ERROR = 31011,
        // <summary>拠点スキルの習得に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("EQUIP_EVOLUTION_MATERIAL_ERROR")] EQUIP_EVOLUTION_MATERIAL_ERROR = 31012,
        // <summary>拠点スキルの前提条件を満たしておりません</summary>
        [eAPIErrorCodeExtensions.Attributes("PLAYER_BASESKILL_UNLOCKD_COST_ERROR")] PLAYER_BASESKILL_UNLOCKD_COST_ERROR = 31013,
        // <summary>装備のレベルアップに失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("EQUIP_LEVELUP_ERROR")] EQUIP_LEVELUP_ERROR = 31014,
        // <summary>装備の進化に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("EQUIP_EVOLUTION_ERROR")] EQUIP_EVOLUTION_ERROR = 31015,

        // <summary>マスターデータの投入に失敗しました</summary>
        [eAPIErrorCodeExtensions.Attributes("BATCH_ERROR_MASTERDATA")] BATCH_ERROR_MASTERDATA = 40001,
    }

    /// <summary>APIエラーコード拡張.</summary>
    public static class eAPIErrorCodeExtensions
    {
        private static Dictionary<eAPIErrorCode, Attributes> errors;
        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class Attributes : Attribute
        {
            public readonly string label;
            public Attributes(string label)
            {
                this.label = label;
            }
        }
        static eAPIErrorCodeExtensions()
        {
            var type = typeof(eAPIErrorCode);
            var lookup = type.GetFields()
            .Where(fi => fi.FieldType == type)
            .SelectMany(fi => fi.GetCustomAttributes(false),

             (fi, Attribute) => new { Type = (eAPIErrorCode)fi.GetValue(null), Attribute })
            .ToLookup(a => a.Attribute.GetType());
            errors = lookup[typeof(Attributes)].ToDictionary(a => a.Type, a => (Attributes)a.Attribute);
        }
        public static string Label(this eAPIErrorCode type)
        {
            return errors[type].label;
        }
    }
}