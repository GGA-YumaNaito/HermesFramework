using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Version : MonoBehaviour
{
    /// <summary>
    /// バージョン表示するクラス
    /// </summary>
    void Start()
    {
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = Application.version;
    }

}
