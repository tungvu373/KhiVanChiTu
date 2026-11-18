using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.SecretRealm
{
    public sealed class RealmCard : MonoBehaviour
    {
        public SecretRealmType type;
        public UnityEngine.UI.Image artwork;
        public TMP_Text title,mode,description,reward,condition,record;
        public FoundationAction enter;
        void LateUpdate()
        {
            var body=title.transform.parent as RectTransform;
            if(body==null)return;
            float height=Mathf.Max(660,UnityEngine.UI.LayoutUtility.GetPreferredHeight(body)+200);
            var layout=GetComponent<UnityEngine.UI.LayoutElement>();
            if(layout!=null&&Mathf.Abs(layout.preferredHeight-height)>1)layout.minHeight=layout.preferredHeight=height;
        }
    }
}
