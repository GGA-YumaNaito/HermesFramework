using System;
using Cysharp.Threading.Tasks;
using Hermes.API;
using UnityEngine;

namespace API
{
    /// <summary>
    /// ユーザーデータ
    /// </summary>
    [Serializable]
    public class UserData : APIData<UserData, UserData.Request, UserData.Response>
    {
        protected override string api => "user/data";

        protected override bool isPost => true;

        public override Request request { get; set; }
        public override Response response { get; set; }

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase, ISerializationCallbackReceiver
        {
            [SerializeField] string user_name;
            /// <summary>ユーザー名</summary>
            public string UserName { get => user_name; }

            [SerializeField] int rank;
            /// <summary>ランク</summary>
            public int Rank { get => rank; }

            [SerializeField] int icon_chara_id;
            /// <summary>アイコンのキャラID</summary>
            public int IconCharaId { get => icon_chara_id; }

            [SerializeField] int main_chara_id;
            /// <summary>メインキャラID</summary>
            public int MainCharaId { get => main_chara_id; }

            [SerializeField] int max_life;
            /// <summary>最大ライフ</summary>
            public int MaxLife { get => max_life; }

            [SerializeField] int life;
            /// <summary>ライフ</summary>
            public int Life { get => life; }

            [SerializeField] string remaining_life_time;
            DateTime _remaining_life_time;
            /// <summary>残りライフが回復する時の時間</summary>
            public DateTime RemainingLifeTime { get => _remaining_life_time; }

            [SerializeField] int free_crystal;
            /// <summary>無償クリスタル</summary>
            public int FreeCrystal { get => free_crystal; }

            [SerializeField] int charge_crystal;
            /// <summary>有償クリスタル</summary>
            public int ChargeCrystal { get => charge_crystal; }

            [SerializeField] int money;
            /// <summary>ゲーム内のお金</summary>
            public int Money { get => money; }

            [SerializeField] int tutorial_num;
            /// <summary>チュートリアル</summary>
            public int TutorialNum { get => tutorial_num; }

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                if (string.IsNullOrEmpty(remaining_life_time))
                    return;
                var date = new DateTime();
                if (DateTime.TryParse(remaining_life_time, out date))
                    _remaining_life_time = date;
            }
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public override async UniTask SendWebRequest(Action<UserData> onSuccess = null, Action<UserData> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<UserData, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}