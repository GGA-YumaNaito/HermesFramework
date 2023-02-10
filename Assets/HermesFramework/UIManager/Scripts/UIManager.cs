using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Hermes.UI
{
    /// <summary>
    /// UIManager
    /// </summary>
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>���݂�View</summary>
        [SerializeField] ViewBase currentView;
        /// <summary>���݂�View</summary>
        public ViewBase CurrentView { get { return currentView; } private set { currentView = value; } }
        /// <summary>���݂�Scene</summary>
        [SerializeField] Screen currentScene;
        /// <summary>���݂�View</summary>
        public Screen CurrentScene { get { return currentScene; } private set { currentScene = value; } }
        /// <summary>�J��StackType</summary>
        [SerializeField] Stack<Type> stackType = new Stack<Type>();
        /// <summary>�J��StackOptions</summary>
        [SerializeField] Stack<object> stackOptions = new Stack<object>();
        /// <summary>�o���A</summary>
        [SerializeField] GameObject barrier;
        /// <summary>�_�C�A���O�pBG</summary>
        [SerializeField] GameObject dialogBG;
        /// <summary>�_�C�A���ORoot</summary>
        [SerializeField] Transform dialogRoot;

        /// <summary>
        /// ViewBase���p�������N���X��LoadAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public async UniTask LoadAsync<T>(object options = null) where T : ViewBase
        {
            var type = typeof(T);
            // ������ʂȂ�\�����Ȃ�
            if (CurrentScene != null && type == CurrentScene.GetType())
                return;

            // �o���AON
            barrier.SetActive(true);
            // ���ɑ��݂��Ă�����Stack����O���Ă���
            Action<Type> StackPopAction = type =>
            {
                if (!stackType.Contains(type))
                    return;
                var count = stackType.Count;
                for (int i = 0; i < count; i++)
                {
                    var t = stackType.Pop();
                    stackOptions.Pop();
                    if (t == type)
                        break;
                }
            };

            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                // ���݂̍ŐV���_�C�A���O��������폜����
                if (CurrentView is Dialog)
                {
                    dialogBG.SetActive(false);
                    var stackType = new Stack<Type>();
                    var stackOptions = new Stack<object>();
                    var count = this.stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        stackType.Push(this.stackType.Pop());
                        stackOptions.Push(this.stackOptions.Pop());
                        var t = stackType.Peek();
                        if (t.IsSubclassOf(typeof(Screen)))
                            break;
                        CurrentView = (ViewBase)FindObjectOfType(t);
                        await OnUnloadDialog(CurrentView);
                    }
                    count = stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        this.stackType.Push(stackType.Pop());
                        this.stackOptions.Push(stackOptions.Pop());
                    }
                }
                // ���ɃV�[�������݂�����
                StackPopAction(type);
                // �܂�CurrentScene���Ȃ�������Unload�͂��Ȃ�
                if (CurrentScene != null)
                {
                    await OnUnloadScreen(CurrentScene);
                }
                // �V�[�����[�h
                await SceneManager.LoadSceneAsync(type.Name, LoadSceneMode.Additive);
                
                CurrentScene = FindObjectOfType<T>() as Screen;
                CurrentView = CurrentScene;
            }
            // Dialog
            else
            {
                dialogBG.SetActive(true);
                // ���Ƀ_�C�A���O�����݂�����
                //StackPopAction(type); // TODO: ���͊O���Ă���

                // ���[�h�A�Z�b�g
                var handle = Addressables.LoadAssetAsync<GameObject>(type.Name);
                await handle.ToUniTask();
                var dialog = handle.Result;

                // Instantiate
                CurrentView = Instantiate(dialog, dialogRoot).GetComponent<T>();
                Addressables.Release(handle);
            }

            stackType.Push(type);
            stackOptions.Push(options);

            if (CurrentView == null)
                throw new Exception($"{typeof(T).Name} is Null");

            // Initialize & Load
            CurrentView.Initialize();
            CurrentView.OnLoad(options);
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display);

            // �o���AOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// �O��ʕ\��
        /// </summary>
        public async UniTask BackAsync()
        {
            barrier.SetActive(true);
            if (CurrentView != null && !CurrentView.IsBack)
            {
                // TODO:�O�̉�ʂɖ߂�Ȃ����̏���

            }
            if (stackType.Count > 1)
            {
                stackType.Pop();
                stackOptions.Pop();
                await BackProcess(CurrentView is Screen);
            }
            else
            {
                // TODO:�X�^�b�N������������Q�[���I��
            }
            // �_�C�A���O������������o���A��OFF�ɂ���
            dialogBG.SetActive(CurrentView is Dialog);

            barrier.SetActive(false);
        }

        /// <summary>
        /// �O��ʕ\������
        /// </summary>
        /// <param name="isScreen"></param>
        /// <returns></returns>
        async UniTask BackProcess(bool isScreen)
        {
            var type = stackType.Peek();
            var options = stackOptions.Peek();
            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                if (isScreen)
                {
                    await OnUnloadScreen(CurrentScene);
                    await SceneManager.LoadSceneAsync(type.Name, LoadSceneMode.Additive);
                    CurrentScene = FindObjectOfType(type) as Screen;
                    CurrentView = CurrentScene;
                }
                else
                {
                    await OnUnloadDialog(CurrentView);
                    CurrentView = (ViewBase)FindObjectOfType(type);
                    return;
                }
            }
            // Dialog
            else
            {
                if (isScreen)
                {
                    // stack���ꎞ�I�ɔ����Ă���
                    stackType.Pop();
                    stackOptions.Pop();

                    await BackProcess(true);

                    // stack��߂�
                    stackType.Push(type);
                    stackOptions.Push(options);

                    // ���[�h�A�Z�b�g
                    var handle = Addressables.LoadAssetAsync<GameObject>(type.Name);
                    await handle.ToUniTask();
                    var dialog = handle.Result;

                    // Instantiate
                    CurrentView = (ViewBase)Instantiate(dialog, dialogRoot).GetComponent(type);
                    Addressables.Release(handle);
                }
                else
                {
                    await OnUnloadDialog(CurrentView);
                    CurrentView = (ViewBase)FindObjectOfType(type);
                    return;
                }
            }
            if (CurrentView == null)
                throw new Exception($"{type.Name} is Null");

            CurrentView.Initialize();
            CurrentView.OnLoad(options);
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display);
        }

        /// <summary>
        /// �X�N���[���폜
        /// </summary>
        /// <param name="viewBase"></param>
        /// <returns></returns>
        async UniTask OnUnloadScreen(ViewBase viewBase)
        {
            viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End);
            await SceneManager.UnloadSceneAsync(viewBase.GetType().Name);
        }

        /// <summary>
        /// �_�C�A���O�폜
        /// </summary>
        /// <param name="viewBase"></param>
        /// <returns></returns>
        async UniTask OnUnloadDialog(ViewBase viewBase)
        {
            viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End);
            Destroy(viewBase.gameObject);
        }

        /// <summary>
        /// �X�^�b�N�N���A
        /// </summary>
        public void ClearStack()
        {
            stackType.Clear();
            stackOptions.Clear();
        }

        /// <summary>
        /// ��c���ăX�^�b�N���N���A����
        /// </summary>
        public void ClearStackLeaveOne()
        {
            // ��c���ăN���A����
            var count = stackType.Count - 1;
            for (int i = 0; i < count; i++)
            {
                stackType.Pop();
                stackOptions.Pop();
            }
        }
    }
}