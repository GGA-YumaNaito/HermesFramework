using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSample2 : Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>オプション</summary>
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
