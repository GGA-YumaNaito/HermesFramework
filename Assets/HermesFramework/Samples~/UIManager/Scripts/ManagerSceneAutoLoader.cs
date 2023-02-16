using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hermes.Common
{
    /// <summary>
    /// Awake前にManagerSceneを自動でロードするクラス
    /// </summary>
    public class ManagerSceneAutoLoader
    {
        /// <summary>ManagerSceneName</summary>
        const string managerSceneName = "ManagerScene";

        /// <summary>
        /// ゲーム開始時(シーン読み込み前)に実行される
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadManagerScene()
        {
            // ManagerSceneが有効でない時(まだ読み込んでいない時)だけ追加ロードするように
            if (!SceneManager.GetSceneByName(managerSceneName).IsValid())
            {
                SceneManager.LoadScene(managerSceneName, LoadSceneMode.Additive);
            }
        }
    }
}