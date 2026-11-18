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
    [DefaultExecutionOrder(12500)]
    public sealed class RealmLegacyLabel : MonoBehaviour
    {
        public FoundationTheme theme;
        public UnityEngine.UI.Text source;
        TMP_Text label;
        void Start()
        {
            label=new FoundationFactory(theme).Text("Phase7Typography",transform,"",source.fontSize,source.alignment==TextAnchor.MiddleCenter?TextAlignmentOptions.Center:TextAlignmentOptions.MidlineLeft);
            RpgUI.Fill(label.rectTransform);label.enableAutoSizing=true;label.fontSizeMin=Mathf.Min(10,source.fontSize);label.fontSizeMax=source.fontSize;label.overflowMode=TextOverflowModes.Ellipsis;source.enabled=false;Refresh();
        }
        void LateUpdate(){Refresh();}
        void Refresh(){if(label==null)return;label.text=RealmCopy.Plain(source.text);label.color=source.transform.parent.name=="BtnOpen"||source.transform.parent.name=="GoldSweetSpot"?theme.ink:source.color.grayscale>.8f?theme.ivory:theme.gold;}
    }
}
