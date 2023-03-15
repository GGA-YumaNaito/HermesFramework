namespace UnityEngine.UI
{
    /// <summary>
    /// LayoutElementEx
    /// </summary>
    public class LayoutElementEx : LayoutElement
    {
        /// <summary>preferredの代わりとなるRectTransform</summary>
        public RectTransform preferredRectTransform = null;

        public override float preferredWidth
        {
            get
            {
                if (preferredRectTransform)
                    return preferredRectTransform.rect.width;
                return base.preferredWidth;
            }
            set => base.preferredWidth = value;
        }
        public override float preferredHeight
        {
            get
            {
                if (preferredRectTransform)
                    return preferredRectTransform.rect.height;
                return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }
    }
}