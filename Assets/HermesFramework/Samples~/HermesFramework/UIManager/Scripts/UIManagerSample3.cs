﻿using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSample3 : Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>オプション</summary>
        public class Options
        {
            public string sumpleText;
        }
        public override async UniTask OnLoad(object options)
        {
            var op = options as Options;
            if (op != null)
            {
                Debug.Log(op.sumpleText);
            }
            await UniTask.CompletedTask;
        }
    }
}
