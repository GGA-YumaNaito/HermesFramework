using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSample3 : Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>ƒIƒvƒVƒ‡ƒ“</summary>
        public class Options
        {
            public string sumpleText;
        }
        public override void OnLoad(object options)
        {
            base.OnLoad(options);
            var op = options as Options;
            if (op != null)
            {
                Debug.Log(op.sumpleText);
            }
        }
    }
}
