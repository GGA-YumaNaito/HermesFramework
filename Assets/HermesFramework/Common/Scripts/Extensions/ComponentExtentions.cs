using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace UnityEngine
{
    /// <summary>
    /// Component extentions.
    /// </summary>
    public static partial class ComponentExtentions
    {
        /// <summary>
        /// Returns the components of T in the its parents without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its parent's component.</param>
        public static IEnumerable<T> GetComponentsOnlyInParent<T>(this Component self, bool includeInactive = false, bool onlyItsParent = false) where T : Component
        {
            Transform parent = self.transform.parent;
            if (!parent)
                yield break;

            int parentId = parent.gameObject.GetInstanceID();
            int inctinceId = self.gameObject.GetInstanceID();
            int componetId;
            foreach (T component in self.GetComponentsInParent<T>(includeInactive))
            {
                componetId = component.gameObject.GetInstanceID();
                if ((componetId != inctinceId) && (!onlyItsParent || componetId == parentId))
                    yield return component;
            }
            yield break;
        }

        /// <summary>
        /// Returns the component of T in the its parents without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its parent's component.</param>
        public static T GetComponentOnlyInParent<T>(this Component self, bool includeInactive = false, bool onlyItsParent = false) where T : Component
        {
            return self.GetComponentsOnlyInParent<T>(includeInactive, onlyItsParent).FirstOrDefault();
        }

        /// <summary>
        /// Returns the components of T in the its children without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its children's component.</param>
        public static IEnumerable<T> GetComponentsOnlyInChildren<T>(this Component self, bool includeInactive = false, bool onlyItsChildren = false) where T : Component
        {
            int inctinceId = self.gameObject.GetInstanceID();
            int componetId;

            foreach (T component in self.GetComponentsInChildren<T>(includeInactive))
            {
                componetId = component.gameObject.GetInstanceID();
                if ((componetId != inctinceId)
                    && (!onlyItsChildren || component.transform.parent.gameObject.GetInstanceID() == inctinceId))
                    yield return component;
            }
            yield break;
        }

        /// <summary>
        /// Returns the components of T in the its children without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its children's component.</param>
        public static T GetComponentOnlyInChildren<T>(this Component self, bool includeInactive = false, bool onlyItsChildren = false) where T : Component
        {
            return self.GetComponentsOnlyInChildren<T>(includeInactive, onlyItsChildren).FirstOrDefault();
        }

        /// <summary>
        /// Returns the component if the gameObject has one attached, or added one.
        /// </summary>
        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            return self.GetComponent<T>() ?? self.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Returns the components if the gameObject has some attached, or added one.
        /// </summary>
        public static IEnumerable<T> GetOrAddComponents<T>(this Component self) where T : Component
        {
            T[] components = self.GetComponents<T>();
            foreach (T component in components)
            {
                yield return component;
            }

            if (components.Length == 0)
                yield return self.gameObject.AddComponent<T>();
            yield break;
        }

        /// <summary>
        /// Attach
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void Attach(this Component self, Component parent, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
        {
            Transform trans = self.transform;

            Vector3 pos = position ?? trans.localPosition;
            Quaternion rot = rotation ?? trans.localRotation;
            Vector3 scl = scale ?? trans.localScale;

            trans.SetParent(parent ? parent.transform : null);

            trans.localPosition = pos;
            trans.localRotation = rot;
            trans.localScale = scl;
        }

        /// <summary>
        /// Do the parent event system handler.
        /// </summary>
        /// <param name="self">Self.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void DoParentEventSystemHandler<T>(this Component self, Action<T> action) where T : IEventSystemHandler
        {
            Transform parent = self.transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>())
                {
                    if (component is T)
                        action((T)(IEventSystemHandler)component);
                }
                parent = parent.parent;
            }
        }



        public static void AddPosition(this Component self, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = self.transform.position;
            self.transform.position = new Vector3(v.x + (x ?? 0), v.y + (y ?? 0), v.z + (z ?? 0));
        }


        public static void AddLocalPosition(this Component self, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = self.transform.localPosition;
            self.transform.localPosition = new Vector3(v.x + (x ?? 0), v.y + (y ?? 0), v.z + (z ?? 0));
        }

        public static void SetPosition(this Component self, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = self.transform.position;
            self.transform.position = new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }


        public static void SetLocalPosition(this Component self, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = self.transform.localPosition;
            self.transform.localPosition = new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }


        public static void ResetTransform(this Component self)
        {
            self.transform.localPosition = Vector3.zero;
            self.transform.localScale = Vector3.one;
            self.transform.localRotation = Quaternion.identity;
        }
    }
}