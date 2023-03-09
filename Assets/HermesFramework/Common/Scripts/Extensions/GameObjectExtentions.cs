using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    /// <summary>
    /// Component extentions.
    /// </summary>
    public static partial class GameObjectExtentions
    {
        /// <summary>
        /// Returns the components of T in the its parents without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its parent's component.</param>
        public static IEnumerable<T> GetComponentsOnlyInParent<T>(this GameObject target, bool includeInactive = false, bool onlyItsParent = false) where T : Component
        {
            Transform parent = target.transform.parent;
            if (!parent)
                yield break;

            int parentId = parent.gameObject.GetInstanceID();
            int inctinceId = target.gameObject.GetInstanceID();
            int componetId;
            foreach (T component in target.GetComponentsInParent<T>(includeInactive))
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
        public static T GetComponentOnlyInParent<T>(this GameObject target, bool includeInactive = false, bool onlyItsParent = false) where T : Component
        {
            return target.GetComponentsOnlyInParent<T>(includeInactive, onlyItsParent).FirstOrDefault();
        }

        /// <summary>
        /// Returns the components of T in the its children without itself.
        /// </summary>
        /// <param name="includeInactive">Should inactive Components be included in the found set?</param>
        /// <param name="onlyItsParent">Only in its children's component.</param>
        public static IEnumerable<T> GetComponentsOnlyInChildren<T>(this GameObject target, bool includeInactive = false, bool onlyItsChildren = false) where T : Component
        {
            int inctinceId = target.gameObject.GetInstanceID();
            int componetId;

            foreach (T component in target.GetComponentsInChildren<T>(includeInactive))
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
        public static T GetComponentOnlyInChildren<T>(this GameObject target, bool includeInactive = false, bool onlyItsChildren = false) where T : Component
        {
            return target.GetComponentsOnlyInChildren<T>(includeInactive, onlyItsChildren).FirstOrDefault();
        }

        /// <summary>
        /// Returns the component if the gameObject has one attached, or added one.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject target) where T : Component
        {
            return target.GetComponent<T>() ?? target.AddComponent<T>();
        }

        /// <summary>
        /// Returns the components if the gameObject has some attached, or added one.
        /// </summary>
        public static IEnumerable<T> GetOrAddComponents<T>(this GameObject target) where T : Component
        {
            T[] components = target.GetComponents<T>();
            foreach (T component in components)
                yield return component;

            if (components.Length == 0)
                yield return target.AddComponent<T>();
            yield break;
        }

        public static void Attach(this GameObject target, GameObject parent, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
        {
            target.transform.Attach(parent ? parent.transform : null, position, rotation, scale);
        }

        public static void AddPosition(this GameObject target, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = target.transform.position;
            target.transform.position = new Vector3(v.x + (x ?? 0), v.y + (y ?? 0), v.z + (z ?? 0));
        }

        public static void AddLocalPosition(this GameObject target, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = target.transform.localPosition;
            target.transform.localPosition = new Vector3(v.x + (x ?? 0), v.y + (y ?? 0), v.z + (z ?? 0));
        }

        public static void SetPosition(this GameObject target, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = target.transform.position;
            target.transform.position = new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        public static void SetLocalPosition(this GameObject target, float? x = null, float? y = null, float? z = null)
        {
            Vector3 v = target.transform.localPosition;
            target.transform.localPosition = new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
        }

        public static void ResetTransform(this GameObject target)
        {
            target.transform.localPosition = Vector3.zero;
            target.transform.localScale = Vector3.one;
            target.transform.localRotation = Quaternion.identity;
        }
    }
}