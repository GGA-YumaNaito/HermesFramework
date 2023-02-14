using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hermes.Common
{
    /// <summary>
    /// Awake�O��ManagerScene�������Ń��[�h����N���X
    /// </summary>
    public class ManagerSceneAutoLoader
    {
        /// <summary>ManagerSceneName</summary>
        const string managerSceneName = "ManagerScene";

        /// <summary>
        /// �Q�[���J�n��(�V�[���ǂݍ��ݑO)�Ɏ��s�����
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadManagerScene()
        {
            // ManagerScene���L���łȂ���(�܂��ǂݍ���ł��Ȃ���)�����ǉ����[�h����悤��
            if (!SceneManager.GetSceneByName(managerSceneName).IsValid())
            {
                SceneManager.LoadScene(managerSceneName, LoadSceneMode.Additive);
            }
        }
    }
}