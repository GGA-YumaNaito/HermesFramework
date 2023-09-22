using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hermes.UI
{
    /// <summary>
    /// バックボタン
    /// </summary>
    public class UIBackButton : UIButton
    {
        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(() => UIManager.Instance.BackAsync(this.GetCancellationTokenOnDestroy()).Forget());
        }
    }

    //==============================================================================================

#if UNITY_EDITOR
    /// <summary>
    /// UIBackButtonEditor
    /// </summary>
    [CustomEditor(typeof(UIBackButton))]
    public class UIBackButtonEditor : UIButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}