namespace UnityEngine
{
    /// <summary>
    /// RectTransformExtentions.
    /// </summary>
    public static partial class RectTransformExtentions
    {
        /// <summary>
        /// FitByParent
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parent"></param>
        public static void FitByParent(this RectTransform self, Transform parent)
        {
            self.SetParent(parent);
            self.FitByParent();
        }

        /// <summary>
        /// FitByParent
        /// </summary>
        /// <param name="self"></param>
        public static void FitByParent(this RectTransform self)
        {
            self.localPosition = Vector3.zero;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;

            self.anchorMax = Vector2.one;
            self.anchorMin = Vector2.zero;
            self.sizeDelta = Vector2.zero;
            self.offsetMax = Vector2.zero;
            self.offsetMin = Vector2.zero;
            self.anchoredPosition = Vector2.zero;
            self.pivot = Vector2.one / 2;
        }

        static Vector3[] wcSelf = new Vector3[4];
        static Vector3[] wcTarget = new Vector3[4];
        /// <summary>
        /// Overlap
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns>bool</returns>
        public static bool Overlap(this RectTransform self, RectTransform target)
        {
            if (!self || !target)
                return false;

            target.GetWorldCorners(wcTarget);
            float xMinTarget = Mathf.Min(wcTarget[0].x, wcTarget[1].x, wcTarget[2].x, wcTarget[3].x);
            float xMaxTarget = Mathf.Max(wcTarget[0].x, wcTarget[1].x, wcTarget[2].x, wcTarget[3].x);
            float yMinTarget = Mathf.Min(wcTarget[0].y, wcTarget[1].y, wcTarget[2].y, wcTarget[3].y);
            float yMaxTarget = Mathf.Max(wcTarget[0].y, wcTarget[1].y, wcTarget[2].y, wcTarget[3].y);

            self.GetWorldCorners(wcSelf);
            float xMinSelf = Mathf.Min(wcSelf[0].x, wcSelf[1].x, wcSelf[2].x, wcSelf[3].x);
            float xMaxSelf = Mathf.Max(wcSelf[0].x, wcSelf[1].x, wcSelf[2].x, wcSelf[3].x);
            float yMinSelf = Mathf.Min(wcSelf[0].y, wcSelf[1].y, wcSelf[2].y, wcSelf[3].y);
            float yMaxSelf = Mathf.Max(wcSelf[0].y, wcSelf[1].y, wcSelf[2].y, wcSelf[3].y);

            return xMinTarget <= xMaxSelf && xMinSelf <= xMaxTarget
                && yMinTarget <= yMaxSelf && yMinSelf <= yMaxTarget;
        }

        /// <summary>
        /// CopyFrom
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        public static void CopyFrom(this RectTransform self, RectTransform target)
        {
            if (!self || !target)
                return;

            self.SetParent(target.parent);
            self.anchorMax = target.anchorMax;
            self.anchorMin = target.anchorMin;
            self.pivot = target.pivot;
            self.sizeDelta = target.sizeDelta;
            self.anchoredPosition3D = target.anchoredPosition3D;
            self.localRotation = target.localRotation;
            self.localScale = target.localScale;
        }
    }
}