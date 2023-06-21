using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Hermes.API.Sample
{
    /// <summary>
    /// RequestList
    /// </summary>
    public class RequestList : MonoBehaviour
    {
        /// <summary>Content</summary>
        [SerializeField] RectTransform content;
        /// <summary>リクエストアイテム</summary>
        [SerializeField] RequestItem requestItem;

        /// <summary>リクエストアイテムリスト</summary>
        List<RequestItem> requestItemList;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialized()
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
            requestItemList = new List<RequestItem>();
        }

        /// <summary>
        /// 子オブジェクト破棄
        /// </summary>
        void ChildrenDestroy()
        {
            //var children = content.GetComponentsInChildren<RequestItem>();
            //for (var i = 0; i < children.Length; i++)
            //{
            //    Destroy(children[i].gameObject);
            //}

            for (var i = 0; i < requestItemList.Count; i++)
            {
                Destroy(requestItemList[i].gameObject);
            }
            requestItemList.Clear();
        }

        /// <summary>
        /// Set item
        /// </summary>
        /// <param name="type">type</param>
        public void SetItem(Type type)
        {
            ChildrenDestroy();

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            content.sizeDelta = new Vector2(content.sizeDelta.x, ((RectTransform)requestItem.transform).sizeDelta.y * fields.Length);

            foreach (var field in fields)
            {
                var item = Instantiate(requestItem, content);
                item.Set(field.Name, field.FieldType);
                item.gameObject.SetActive(true);
                requestItemList.Add(item);
            }
        }

        /// <summary>
        /// Set instance
        /// </summary>
        /// <param name="instance">Instance</param>
        public void SetInstance(object instance)
        {
            var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                foreach (var item in requestItemList)
                {
                    if (item.IsMatch(field.Name))
                    {
                        field.SetValue(instance, item.GetValue());
                    }
                }
            }
        }
    }
}