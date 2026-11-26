using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public static class RemakeDrawerViews
    {
        private static int _uiStageTabDiff = -1;
        private static int _uiStageTabAct = -1;
        private static int _uiAlchemyPillIdx = -1;
        private static int _uiAlchemySelectedRealm = -1;
        private static RemakePillQuality _uiAlchemySelectedQuality = RemakePillQuality.HaPham;
        private static bool _uiAlchemyShowAllPills = true;
        private static bool _useForgeTalisman = false;
        private static string _selectedDaoVanId = "core_daotam";
        private static string _selectedGearUid = "";
        private static int _selectedFormationIdx = -1;
        private static int _selectedSkillSlot = 0;
        private static int _selectedBreakthroughPill = -1; // -1 = none, 0..4 = RemakePillQuality
        private static int _uiSecretRealmKind = 0;
        private static int _uiSecretRealmTier = 1;
        private static int _uiLbTab = 0;

        // FIFA Online 1 Main + up to 5 Fodder Sword Merge UI State
        private static bool _fifaMainFromEquipped = true;
        private static int _fifaMainIndex = 0;
        private static readonly List<int> _fifaFodderBagIndices = new List<int>();
        private static int _swordBagTapMode = 0; // 0 = Them Phoi, 1 = Chon Chinh, 2 = Xuat Tran

        // =========================================================
        // 1. KIEM LO — FIFA ONLINE 1 MAIN + 5 FODDER FORGE & GRIDS
        // =========================================================
        public static void BuildSwordPath(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            void Refresh()
            {
                body.Clear();
                BuildSwordPath(body);
            }

            SanitizeFifaSelection(save);

            int mainLevel = GetSelectedMainSwordLevel(save);
            var fodderLevels = new List<int>(_fifaFodderBagIndices.Count);
            for (int i = 0; i < _fifaFodderBagIndices.Count; i++)
            {
                int bi = _fifaFodderBagIndices[i];
                if (bi >= 0 && bi < save.swordInventoryLevels.Count)
                    fodderLevels.Add(save.swordInventoryLevels[bi]);
            }

            float chance = mainLevel > 0 && mainLevel < 10
                ? engine.ComputeFifaSwordMergeChance(mainLevel, fodderLevels, _useForgeTalisman, out float fodderSum, out float bonusSum)
                : 0f;
            engine.ComputeFifaSwordMergeChance(Mathf.Clamp(mainLevel, 1, 9), fodderLevels, _useForgeTalisman, out float fSumDisp, out float bSumDisp);
            bool isTutorialFree = !save.tutorialCompleted && save.totalMerges == 0 && mainLevel == 1;
            double mergeCost = isTutorialFree ? 0 : CultivationTable.GetSwordMergeCost(Mathf.Clamp(mainLevel, 1, 9));
            float pity = mainLevel >= 1 && mainLevel <= 9 ? save.swordPityByLevel[mainLevel - 1] : 0f;

            // HERO FIFA 1+5 FORGE STAGE
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            hero.style.alignItems = Align.Stretch;

            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var stageArt = new VisualElement { style = { width = 62, height = 62, flexShrink = 0 } };
            SetBorderAndRadius(stageArt, new Color(0.84f, 0.60f, 0.16f, 0.72f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(stageArt, "Stages/stage_sword_forge", "[Kiếm Đài]");
            topRow.Add(stageArt);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 8, flexShrink = 1 } };
            info.Add(new Label("Cổ Kiếm Trủng · Rèn Kiếm 1 Chính + 5 Phôi") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            info.Add(new Label($"Tổng Công: {CultivationTable.FormatNumber(engine.GetTotalSwordAttackPower())} · Xuất trận: {save.equippedSwordLevels.Count}/{CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex)}") { style = { fontSize = 10, marginTop = 2 } });
            info.Add(new Label($"Phù Rèn (+10%): {save.forgeTalismanCount} · Tổng lượt rèn: {save.totalMerges}") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f), marginTop = 1 } });
            topRow.Add(info);
            hero.Add(topRow);

            // MAIN SWORD -> TARGET PREVIEW
            var mainStageRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.Center, marginTop = 8 } };

            var mainSlot = new VisualElement();
            mainSlot.AddToClassList("fifa-main-slot");
            if (mainLevel > 0)
            {
                mainSlot.Add(new Label("KIẾM CHÍNH") { style = { fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.72f, 0.44f, 0.04f), flexShrink = 0 } });
                var mIco = new VisualElement { style = { width = 34, height = 34, minWidth = 34, minHeight = 34, flexShrink = 0, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(mainLevel)) } };
                mIco.AddToClassList("icon-fixed");
                mainSlot.Add(mIco);
                mainSlot.Add(new Label($"Lv.{mainLevel}") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
            }
            else
            {
                mainSlot.Add(new Label("Chọn\nKiếm") { style = { fontSize = 9, unityTextAlign = TextAnchor.MiddleCenter } });
            }
            mainStageRow.Add(mainSlot);

            mainStageRow.Add(new Label("  + Phôi  →  ") { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.24f, 0.36f, 0.32f) } });

            var nextSlot = new VisualElement();
            nextSlot.AddToClassList("fifa-main-slot");
            SetBorderAndRadius(nextSlot, new Color(0.14f, 0.60f, 0.46f), 2f, 12f);
            if (mainLevel > 0 && mainLevel < 10)
            {
                nextSlot.Add(new Label("MỤC TIÊU") { style = { fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.11f, 0.48f, 0.36f), flexShrink = 0 } });
                var nIco = new VisualElement { style = { width = 34, height = 34, minWidth = 34, minHeight = 34, flexShrink = 0, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(mainLevel + 1)) } };
                nIco.AddToClassList("icon-fixed");
                nextSlot.Add(nIco);
                nextSlot.Add(new Label($"Lv.{mainLevel + 1}") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.11f, 0.48f, 0.36f), flexShrink = 0 } });
            }
            else
            {
                nextSlot.Add(new Label("MAX\nLv.10") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter } });
            }
            mainStageRow.Add(nextSlot);
            hero.Add(mainStageRow);

            // 5 FODDER SLOTS (FIFA ONLINE STYLE)
            var fodderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center, marginTop = 7 } };
            for (int slotIdx = 0; slotIdx < 5; slotIdx++)
            {
                int capSlot = slotIdx;
                bool hasFodder = slotIdx < _fifaFodderBagIndices.Count;
                var fBtn = new Button(() =>
                {
                    if (capSlot < _fifaFodderBagIndices.Count)
                    {
                        _fifaFodderBagIndices.RemoveAt(capSlot);
                        Refresh();
                    }
                });
                fBtn.AddToClassList("fifa-fodder-slot");
                if (!hasFodder)
                {
                    fBtn.AddToClassList("fifa-fodder-slot-empty");
                    fBtn.Add(new Label($"Phôi {slotIdx + 1}") { pickingMode = PickingMode.Ignore, style = { fontSize = 8, color = new Color(0.45f, 0.52f, 0.49f), flexShrink = 0 } });
                    fBtn.Add(new Label("+") { pickingMode = PickingMode.Ignore, style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.45f, 0.52f, 0.49f), flexShrink = 0 } });
                }
                else
                {
                    int bIdx = _fifaFodderBagIndices[slotIdx];
                    int flv = save.swordInventoryLevels[bIdx];
                    int contribPct = Mathf.RoundToInt(engine.GetSingleFodderMergeContribution(Mathf.Clamp(mainLevel, 1, 9), flv) * 100f);
                    var fIco = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 26, height = 26, minWidth = 26, minHeight = 26, flexShrink = 0, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(flv)) } };
                    fIco.AddToClassList("icon-fixed");
                    fBtn.Add(fIco);
                    fBtn.Add(new Label($"Lv.{flv}") { pickingMode = PickingMode.Ignore, style = { fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
                    fBtn.Add(new Label($"+{contribPct}%") { pickingMode = PickingMode.Ignore, style = { fontSize = 8, color = new Color(0.11f, 0.52f, 0.38f), unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
                }
                fodderRow.Add(fBtn);
            }
            hero.Add(fodderRow);

            // SUCCESS PROBABILITY GAUGE
            var rateTrack = new VisualElement { style = { width = Length.Percent(100), height = 10, marginTop = 6 } };
            rateTrack.AddToClassList("bar-track");
            Color gaugeColor = chance >= 0.80f ? new Color(0.14f, 0.68f, 0.48f) : chance >= 0.40f ? new Color(0.88f, 0.60f, 0.14f) : new Color(0.82f, 0.28f, 0.20f);
            var rateFill = new VisualElement { style = { width = Length.Percent(Mathf.Clamp01(chance) * 100f), height = Length.Percent(100), backgroundColor = gaugeColor } };
            rateTrack.Add(rateFill);
            hero.Add(rateTrack);

            string rateSummary = mainLevel >= 10
                ? "Kiếm đã đạt cấp tối đa (Lv.10)! Hãy chọn thanh kiếm khác để rèn."
                : _fifaFodderBagIndices.Count == 0
                    ? $"Chọn 1–5 phôi kiếm ở lưới Túi bên dưới (Chúc Phúc +{Mathf.RoundToInt(pity * 100)}%)"
                    : $"Tỉ lệ thành công: {Mathf.RoundToInt(chance * 100)}% (Phôi +{Mathf.RoundToInt(fSumDisp * 100)}% · Chúc Phúc/Phù +{Mathf.RoundToInt(bSumDisp * 100)}%) · Phí: {(isTutorialFree ? "Miễn Phí" : CultivationTable.FormatNumber(mergeCost) + " LT")}";
            hero.Add(new Label(rateSummary) { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = TextAnchor.MiddleCenter, whiteSpace = WhiteSpace.Normal, marginTop = 4 } });

            // ACTION BUTTONS
            var actionRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center, marginTop = 7 } };
            var forgeBtn = new Button(() =>
            {
                engine.TryMergeSwordFifa(_fifaMainFromEquipped, _fifaMainIndex, _fifaFodderBagIndices, _useForgeTalisman);
                _fifaFodderBagIndices.Clear();
                Refresh();
            })
            {
                name = mainLevel == 1 ? "btn-sword-merge-1" : "btn-sword-merge-fifa",
                text = _fifaFodderBagIndices.Count > 0 ? $"Rèn Hợp Nhất ({Mathf.RoundToInt(chance * 100)}%)" : "Rèn Hợp Nhất"
            };
            forgeBtn.AddToClassList("btn-primary");
            forgeBtn.SetEnabled(mainLevel >= 1 && mainLevel < 10 && _fifaFodderBagIndices.Count > 0);
            forgeBtn.style.marginRight = 5; forgeBtn.style.marginBottom = 4;
            actionRow.Add(forgeBtn);

            var autoFillBtn = new Button(() =>
            {
                AutoFillFifaFodders(save, engine, mainLevel);
                Refresh();
            })
            { name = "btn-sword-autofill", text = "Tự Điền Phôi" };
            autoFillBtn.AddToClassList("btn-secondary");
            autoFillBtn.SetEnabled(mainLevel >= 1 && mainLevel < 10 && save.swordInventoryLevels.Count > 0);
            autoFillBtn.style.marginRight = 4; autoFillBtn.style.marginBottom = 4;
            actionRow.Add(autoFillBtn);

            var clearBtn = new Button(() => { _fifaFodderBagIndices.Clear(); Refresh(); }) { text = "Xóa Ô" };
            clearBtn.AddToClassList("btn-compact");
            clearBtn.SetEnabled(_fifaFodderBagIndices.Count > 0);
            clearBtn.style.marginRight = 4; clearBtn.style.marginBottom = 4;
            actionRow.Add(clearBtn);

            var taliBtn = new Button(() => { _useForgeTalisman = !_useForgeTalisman; Refresh(); })
            {
                text = _useForgeTalisman ? $"Phù +10%: BẬT ({save.forgeTalismanCount})" : $"Phù +10% ({save.forgeTalismanCount})"
            };
            taliBtn.AddToClassList("btn-compact");
            if (_useForgeTalisman) taliBtn.AddToClassList("selected");
            taliBtn.style.marginRight = 4; taliBtn.style.marginBottom = 4;
            actionRow.Add(taliBtn);

            var quickBtn = new Button(() => { engine.TryQuickMergeSwords(3); _fifaFodderBagIndices.Clear(); Refresh(); }) { text = "Ghép Nhanh Lv.1–3" };
            quickBtn.AddToClassList("btn-compact");
            quickBtn.style.marginBottom = 4;
            actionRow.Add(quickBtn);

            hero.Add(actionRow);
            body.Add(hero);

            // SWORD STYLE
            var styleCard = new VisualElement();
            styleCard.AddToClassList("card-light");
            styleCard.style.marginBottom = 8;
            AddChoiceRow(styleCard, "Kiếm Pháp:", new[] { "Kiếm Xuyên (+28% Tốc)", "Phá Giáp (+60% Xuyên)" }, save.swordStyle, idx =>
            {
                engine.SetSwordStyle((RemakeSwordStyle)idx);
                Refresh();
            });
            body.Add(styleCard);

            // EQUIPPED SWORDS GRID (TAP TO SELECT AS MAIN)
            var eqCard = new VisualElement();
            eqCard.AddToClassList("card-light");
            eqCard.style.marginBottom = 8;
            eqCard.Add(new Label($"Phi Kiếm Đang Xuất Trận ({save.equippedSwordLevels.Count}/{CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex)}) — Chạm để đặt làm Kiếm Chính")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, whiteSpace = WhiteSpace.Normal, marginBottom = 6 }
            });
            var eqGrid = new VisualElement();
            eqGrid.AddToClassList("grid-wrap");
            for (int i = 0; i < save.equippedSwordLevels.Count; i++)
            {
                int idx = i;
                int lv = save.equippedSwordLevels[i];
                bool isMain = _fifaMainFromEquipped && _fifaMainIndex == i;
                var tile = new Button(() =>
                {
                    if (lv >= 10)
                    {
                        BootOverlayController.Instance?.ShowToast("Kiếm Cấp 10 đã đạt đỉnh phong!");
                        return;
                    }
                    _fifaMainFromEquipped = true;
                    _fifaMainIndex = idx;
                    Refresh();
                })
                {
                    style = { width = 62, height = 74, marginRight = 5, marginBottom = 5 }
                };
                tile.AddToClassList("grid-tile");
                if (isMain) tile.AddToClassList("grid-tile-selected");
                if (isMain)
                    tile.Add(new Label("CHÍNH") { pickingMode = PickingMode.Ignore, style = { fontSize = 7, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.75f, 0.44f, 0.04f), flexShrink = 0 } });
                var eqIco = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 30, height = 30, minWidth = 30, minHeight = 30, flexShrink = 0, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lv)) } };
                eqIco.AddToClassList("icon-fixed");
                tile.Add(eqIco);
                tile.Add(new Label($"Lv.{lv}") { pickingMode = PickingMode.Ignore, style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, flexShrink = 0 } });
                tile.Add(new Label(CultivationTable.FormatNumber(CultivationTable.CalculateSwordDamage(lv))) { pickingMode = PickingMode.Ignore, style = { fontSize = 8, color = new Color(0.16f, 0.56f, 0.44f), flexShrink = 0 } });
                eqGrid.Add(tile);
            }
            eqCard.Add(eqGrid);
            body.Add(eqCard);

            // SWORD BAG GRID (TAP TO ADD FODDER / PICK MAIN / EQUIP)
            var bagCard = new VisualElement();
            bagCard.AddToClassList("card-light");
            var bagHead = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 6 } };
            bagHead.Add(new Label($"Túi Phôi Kiếm ({save.swordInventoryLevels.Count})") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            var modeRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            string[] modeNames = { "+Phôi", "Làm Chính", "Xuất Trận" };
            for (int m = 0; m < 3; m++)
            {
                int capM = m;
                var mb = new Button(() => { _swordBagTapMode = capM; Refresh(); }) { text = modeNames[m] };
                mb.AddToClassList("btn-compact");
                if (_swordBagTapMode == m) mb.AddToClassList("selected");
                mb.style.fontSize = 9; mb.style.paddingLeft = 6; mb.style.paddingRight = 6; mb.style.marginLeft = 3;
                modeRow.Add(mb);
            }
            bagHead.Add(modeRow);
            bagCard.Add(bagHead);

            if (save.swordInventoryLevels.Count == 0)
            {
                bagCard.Add(new Label("Túi phôi kiếm đang trống. Treo máy hoặc vào Kiếm Mộ Cổ để săn phôi!") { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, color = new Color(0.45f, 0.50f, 0.47f) } });
            }
            else
            {
                var bagGrid = new VisualElement();
                bagGrid.AddToClassList("grid-wrap");
                for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                {
                    int idx = i;
                    int lv = save.swordInventoryLevels[i];
                    bool isMain = !_fifaMainFromEquipped && _fifaMainIndex == i;
                    int fodderPos = _fifaFodderBagIndices.IndexOf(i);
                    bool isFodder = fodderPos >= 0;
                    int contribPct = mainLevel >= 1 && mainLevel < 10
                        ? Mathf.RoundToInt(engine.GetSingleFodderMergeContribution(mainLevel, lv) * 100f)
                        : 0;

                    var tile = new Button(() =>
                    {
                        if (_swordBagTapMode == 2)
                        {
                            _fifaFodderBagIndices.Clear();
                            engine.TryEquipFromBag(idx);
                            Refresh();
                        }
                        else if (_swordBagTapMode == 1)
                        {
                            if (lv >= 10)
                            {
                                BootOverlayController.Instance?.ShowToast("Kiếm Cấp 10 đã đạt đỉnh phong!");
                                return;
                            }
                            _fifaFodderBagIndices.Remove(idx);
                            _fifaMainFromEquipped = false;
                            _fifaMainIndex = idx;
                            Refresh();
                        }
                        else
                        {
                            if (isMain)
                            {
                                BootOverlayController.Instance?.ShowToast("Thanh kiếm này đang là Kiếm Chính!");
                                return;
                            }
                            if (isFodder)
                                _fifaFodderBagIndices.Remove(idx);
                            else if (_fifaFodderBagIndices.Count < 5)
                                _fifaFodderBagIndices.Add(idx);
                            else
                                BootOverlayController.Instance?.ShowToast("Đã đầy 5 ô Phôi Kiếm!");
                            Refresh();
                        }
                    })
                    {
                        style = { width = 58, height = 72, marginRight = 4, marginBottom = 4 }
                    };
                    tile.AddToClassList("grid-tile");
                    if (isMain) tile.AddToClassList("grid-tile-selected");
                    else if (isFodder)
                    {
                        tile.style.backgroundColor = new Color(0.86f, 0.96f, 0.91f);
                        SetBorderAndRadius(tile, new Color(0.14f, 0.58f, 0.44f), 2f, 10f);
                    }

                    string tagText = isMain ? "CHÍNH" : isFodder ? $"PHÔI {fodderPos + 1}" : $"+{contribPct}%";
                    Color tagColor = isMain ? new Color(0.75f, 0.44f, 0.04f) : isFodder ? new Color(0.10f, 0.48f, 0.36f) : new Color(0.36f, 0.48f, 0.44f);
                    tile.Add(new Label(tagText) { pickingMode = PickingMode.Ignore, style = { fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = tagColor, flexShrink = 0 } });
                    var bIco = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 28, height = 28, minWidth = 28, minHeight = 28, flexShrink = 0, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lv)) } };
                    bIco.AddToClassList("icon-fixed");
                    tile.Add(bIco);
                    tile.Add(new Label($"Lv.{lv}") { pickingMode = PickingMode.Ignore, style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
                    bagGrid.Add(tile);
                }
                bagCard.Add(bagGrid);
            }
            body.Add(bagCard);
        }

        private static void SanitizeFifaSelection(RemakeSaveData save)
        {
            bool validMain = _fifaMainFromEquipped
                ? (_fifaMainIndex >= 0 && _fifaMainIndex < save.equippedSwordLevels.Count && save.equippedSwordLevels[_fifaMainIndex] < 10)
                : (_fifaMainIndex >= 0 && _fifaMainIndex < save.swordInventoryLevels.Count && save.swordInventoryLevels[_fifaMainIndex] < 10);

            if (!validMain)
            {
                bool found = false;
                for (int i = 0; i < save.equippedSwordLevels.Count; i++)
                {
                    if (save.equippedSwordLevels[i] < 10)
                    {
                        _fifaMainFromEquipped = true;
                        _fifaMainIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                    {
                        if (save.swordInventoryLevels[i] < 10)
                        {
                            _fifaMainFromEquipped = false;
                            _fifaMainIndex = i;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    _fifaMainFromEquipped = true;
                    _fifaMainIndex = 0;
                }
            }

            for (int i = _fifaFodderBagIndices.Count - 1; i >= 0; i--)
            {
                int bi = _fifaFodderBagIndices[i];
                if (bi < 0 || bi >= save.swordInventoryLevels.Count || (!_fifaMainFromEquipped && bi == _fifaMainIndex))
                    _fifaFodderBagIndices.RemoveAt(i);
            }

            // Guarantee smooth 1-tap tutorial flow on very first merge
            if (!save.tutorialCompleted && save.totalMerges == 0 && _fifaFodderBagIndices.Count == 0 && save.swordInventoryLevels.Count > 0)
            {
                for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                {
                    if (!_fifaMainFromEquipped && i == _fifaMainIndex) continue;
                    _fifaFodderBagIndices.Add(i);
                    break;
                }
            }
        }

        private static int GetSelectedMainSwordLevel(RemakeSaveData save)
        {
            if (_fifaMainFromEquipped)
                return (_fifaMainIndex >= 0 && _fifaMainIndex < save.equippedSwordLevels.Count) ? save.equippedSwordLevels[_fifaMainIndex] : 0;
            return (_fifaMainIndex >= 0 && _fifaMainIndex < save.swordInventoryLevels.Count) ? save.swordInventoryLevels[_fifaMainIndex] : 0;
        }

        private static void AutoFillFifaFodders(RemakeSaveData save, GameplayEngine engine, int mainLevel)
        {
            _fifaFodderBagIndices.Clear();
            if (mainLevel < 1 || mainLevel >= 10) return;

            var candidates = new List<int>();
            for (int i = 0; i < save.swordInventoryLevels.Count; i++)
            {
                if (!_fifaMainFromEquipped && i == _fifaMainIndex) continue;
                candidates.Add(i);
            }
            // Prefer closest level to mainLevel (same level first, then slightly lower/higher)
            candidates.Sort((a, b) =>
            {
                int da = Mathf.Abs(save.swordInventoryLevels[a] - mainLevel);
                int db = Mathf.Abs(save.swordInventoryLevels[b] - mainLevel);
                if (da != db) return da.CompareTo(db);
                return save.swordInventoryLevels[b].CompareTo(save.swordInventoryLevels[a]);
            });

            var lvls = new List<int>(5);
            for (int i = 0; i < candidates.Count && _fifaFodderBagIndices.Count < 5; i++)
            {
                int bi = candidates[i];
                _fifaFodderBagIndices.Add(bi);
                lvls.Add(save.swordInventoryLevels[bi]);
                float ch = engine.ComputeFifaSwordMergeChance(mainLevel, lvls, _useForgeTalisman, out _, out _);
                if (ch >= 0.999f) break;
            }
        }

        // =========================================================
        // 2. DAO VAN TREE — FOG OF WAR, TRUE SCALE ZOOM & RED DOTS
        // =========================================================
        public static bool IsDaoVanNodeVisible(GameplayEngine engine, DaoVanNodeSpec node)
        {
            if (node == null || engine == null) return false;
            if (string.IsNullOrEmpty(node.parentId)) return true;
            if (engine.GetDaoVanLevel(node.id) > 0) return true;
            return engine.GetDaoVanLevel(node.parentId) > 0;
        }

        public static void BuildDaoVanTree(VisualElement body, Vector2 pan, float zoom, Action<Vector2, float> onViewportChanged)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            bool treeOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage);
            float clampedZoom = Mathf.Clamp(zoom, 0.48f, 1.75f);

            var visibleNodes = new List<DaoVanNodeSpec>();
            for (int i = 0; i < RemakeCatalogs.StarterDaoVanNodes.Count; i++)
            {
                var n = RemakeCatalogs.StarterDaoVanNodes[i];
                if (IsDaoVanNodeVisible(engine, n))
                    visibleNodes.Add(n);
            }

            var sel = FindDaoVanSpec(_selectedDaoVanId);
            if (sel == null || !IsDaoVanNodeVisible(engine, sel))
            {
                sel = RemakeCatalogs.StarterDaoVanNodes[0];
                _selectedDaoVanId = sel.id;
            }

            // HEADER BAR
            var topBar = new VisualElement();
            topBar.AddToClassList("card-light");
            topBar.style.marginBottom = 6;
            topBar.Add(new Label($"Điểm Đạo Vận: {save.daoPoints} · Linh Thạch: {CultivationTable.FormatNumber(save.linhThach)} · Khai phá: {visibleNodes.Count}/{RemakeCatalogs.StarterDaoVanNodes.Count}")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, whiteSpace = WhiteSpace.Normal }
            });
            if (!treeOpen)
                topBar.Add(new Label($"Khóa: Cần {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.DaoVanTree)} để khai mở cây Đạo Vận.") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginTop = 2 } });
            else if (visibleNodes.Count < RemakeCatalogs.StarterDaoVanNodes.Count)
                topBar.Add(new Label("Các nhánh ẩn sẽ tự động hiện ra khi bạn khai mở linh mạch tiền đề!") { style = { fontSize = 9, color = new Color(0.18f, 0.54f, 0.42f), marginTop = 2 } });
            body.Add(topBar);

            // REVEALED BRANCH CHIPS (ONLY VISIBLE BRANCHES + RED DOTS FOR UPGRADEABLE)
            var branchDefs = new (string label, string rootId, string prefix)[]
            {
                ("Đạo Tâm", "core_daotam", "core_"),
                ("Kiếm Đạo", "branch_sword", "sword"),
                ("Thể Phách", "branch_body", "body"),
                ("Tu Luyện", "branch_cultivation", "cult"),
                ("Tài Lộc", "branch_wealth", "wealth"),
                ("Bách Nghệ", "branch_arts", "arts")
            };
            var chipRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6 } };
            foreach (var bDef in branchDefs)
            {
                var cap = bDef;
                bool branchRevealed = false;
                bool branchHasUpgrade = false;
                for (int i = 0; i < visibleNodes.Count; i++)
                {
                    var vn = visibleNodes[i];
                    if (vn.id == cap.rootId || vn.id.Contains(cap.prefix))
                    {
                        branchRevealed = true;
                        if (engine.CanUnlockOrUpgradeNode(vn)) branchHasUpgrade = true;
                    }
                }
                if (!branchRevealed) continue;

                var nodeSpec = FindDaoVanSpec(cap.rootId);
                bool chipSel = _selectedDaoVanId == cap.rootId || _selectedDaoVanId.Contains(cap.prefix);
                var b = new Button(() =>
                {
                    _selectedDaoVanId = cap.rootId;
                    float focusZoom = Mathf.Max(1.08f, clampedZoom);
                    Vector2 targetPan = nodeSpec != null ? -nodeSpec.normalizedPos * focusZoom : Vector2.zero;
                    onViewportChanged?.Invoke(targetPan, focusZoom);
                });
                b.AddToClassList("dao-chip-btn");
                if (chipSel) b.AddToClassList("selected");
                b.Add(new Label(cap.label)
                {
                    pickingMode = PickingMode.Ignore,
                    style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0, color = chipSel ? Color.white : new Color(0.09f, 0.36f, 0.29f) }
                });
                if (branchHasUpgrade)
                {
                    var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                    dot.AddToClassList("badge-dot");
                    dot.style.width = 9; dot.style.height = 9; dot.style.right = -3; dot.style.top = -3;
                    b.Add(dot);
                }
                chipRow.Add(b);
            }
            body.Add(chipRow);

            // ZOOM & FIT-ALL CONTROLS
            var zoomRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 6 } };
            zoomRow.Add(new Label($"Kéo để di chuyển · Zoom {Mathf.RoundToInt(clampedZoom * 100)}%") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f) } });
            var zBtns = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var bOut = new Button(() => onViewportChanged?.Invoke(pan * 0.82f, Mathf.Max(0.48f, clampedZoom - 0.22f))) { text = "Thu -" };
            var bFit = new Button(() =>
            {
                ComputeDaoVanFitViewport(visibleNodes, out Vector2 fitPan, out float fitZoom);
                onViewportChanged?.Invoke(fitPan, fitZoom);
            })
            { text = "Toàn Cảnh" };
            var bIn = new Button(() => onViewportChanged?.Invoke(pan * 1.15f, Mathf.Min(1.75f, clampedZoom + 0.22f))) { text = "Phóng +" };
            bOut.AddToClassList("btn-compact"); bFit.AddToClassList("btn-compact"); bIn.AddToClassList("btn-compact");
            bOut.style.marginRight = 4; bFit.style.marginRight = 4;
            bOut.style.fontSize = 10; bFit.style.fontSize = 10; bIn.style.fontSize = 10;
            zBtns.Add(bOut); zBtns.Add(bFit); zBtns.Add(bIn);
            zoomRow.Add(zBtns);
            body.Add(zoomRow);

            // INTERACTIVE CONSTELLATION CANVAS (DRAG PAN + TRUE NODE SCALING)
            var canvas = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    height = 286,
                    overflow = Overflow.Hidden,
                    backgroundColor = new Color(0.95f, 0.96f, 0.93f),
                    marginBottom = 8
                }
            };
            SetBorderAndRadius(canvas, new Color(0.75f, 0.65f, 0.35f, 0.65f), 1.5f, 12f);
            var bgTex = CodexArtLoader.LoadCodexTexture("DaoVan/daovan_bg_canvas");
            if (bgTex != null) canvas.style.backgroundImage = new StyleBackground(bgTex);

            var graph = new DaoVanGraphLayer();
            graph.AddToClassList("full-screen");
            graph.Bind(RemakeCatalogs.StarterDaoVanNodes, engine.GetDaoVanLevel, pan, clampedZoom, n => IsDaoVanNodeVisible(engine, n));
            canvas.Add(graph);

            var anchor = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style = { position = Position.Absolute, left = Length.Percent(50), top = Length.Percent(50), width = 0, height = 0, overflow = Overflow.Visible }
            };
            canvas.Add(anchor);

            // Pointer drag panning on constellation background
            bool isDragging = false;
            Vector2 dragStart = Vector2.zero;
            Vector2 livePan = pan;
            canvas.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                isDragging = true;
                dragStart = evt.position;
                livePan = pan;
            });
            canvas.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isDragging) return;
                Vector2 delta = (Vector2)evt.position - dragStart;
                if (delta.sqrMagnitude < 25f) return;
                livePan = pan + delta;
                livePan.x = Mathf.Clamp(livePan.x, -420f, 420f);
                livePan.y = Mathf.Clamp(livePan.y, -420f, 420f);
                graph.Bind(RemakeCatalogs.StarterDaoVanNodes, engine.GetDaoVanLevel, livePan, clampedZoom, n => IsDaoVanNodeVisible(engine, n));
                anchor.style.translate = new Translate(livePan.x - pan.x, livePan.y - pan.y);
            });
            canvas.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!isDragging) return;
                isDragging = false;
                if ((livePan - pan).sqrMagnitude >= 25f)
                    onViewportChanged?.Invoke(livePan, clampedZoom);
            });

            // RENDER ONLY VISIBLE NODES WITH SCALED DIMENSIONS, CRISP ICONS & RED DOTS
            float nodeSize = Mathf.Round(Mathf.Clamp(52f * clampedZoom, 32f, 84f));
            float halfNode = nodeSize * 0.5f;
            float iconSize = Mathf.Round(nodeSize * 0.54f);
            int lvFont = Mathf.Clamp(Mathf.RoundToInt(8.5f * clampedZoom), 7, 13);
            int dotSize = Mathf.Clamp(Mathf.RoundToInt(11f * clampedZoom), 9, 15);
            bool showNodeTitle = clampedZoom >= 1.02f;

            for (int i = 0; i < visibleNodes.Count; i++)
            {
                var n = visibleNodes[i];
                var capNode = n;
                int lv = engine.GetDaoVanLevel(n.id);
                bool selected = _selectedDaoVanId == n.id;
                bool unlocked = lv > 0;
                bool canUp = engine.CanUnlockOrUpgradeNode(n);

                var nodeWrap = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        position = Position.Absolute,
                        left = pan.x + n.normalizedPos.x * clampedZoom - halfNode,
                        top = pan.y + n.normalizedPos.y * clampedZoom - halfNode,
                        alignItems = Align.Center,
                        overflow = Overflow.Visible
                    }
                };

                var nodeBtn = new Button(() =>
                {
                    if (_selectedDaoVanId != capNode.id)
                    {
                        _selectedDaoVanId = capNode.id;
                        body.Clear();
                        BuildDaoVanTree(body, pan, clampedZoom, onViewportChanged);
                    }
                    else if (engine.TryUpgradeDaoVanNode(capNode.id))
                    {
                        body.Clear();
                        BuildDaoVanTree(body, pan, clampedZoom, onViewportChanged);
                    }
                })
                {
                    style =
                    {
                        width = nodeSize,
                        height = nodeSize,
                        marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 0,
                        paddingTop = 2, paddingBottom = 2, paddingLeft = 2, paddingRight = 2,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                        overflow = Overflow.Visible,
                        backgroundColor = unlocked ? new Color(1f, 0.97f, 0.88f) : new Color(0.92f, 0.95f, 0.93f)
                    }
                };
                Color border = selected ? new Color(0.92f, 0.58f, 0.08f) : unlocked ? new Color(0.16f, 0.58f, 0.46f) : canUp ? new Color(0.84f, 0.66f, 0.22f) : new Color(0.68f, 0.72f, 0.70f);
                SetBorderAndRadius(nodeBtn, border, selected ? 3f : 1.5f, halfNode);

                var ico = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    style = { width = iconSize, height = iconSize, minWidth = iconSize, minHeight = iconSize, flexShrink = 0 }
                };
                ico.AddToClassList("icon-fixed");
                CodexArtLoader.ApplyIconOrPlaceholder(ico, n.codexIconKey, "", n.reusedIconKey);
                nodeBtn.Add(ico);

                if (clampedZoom >= 0.56f)
                {
                    nodeBtn.Add(new Label($"{lv}/{n.maxLevel}")
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { fontSize = lvFont, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.12f, 0.16f, 0.14f), flexShrink = 0, marginTop = -1 }
                    });
                }

                // Red Notification Dot on EVERY upgradeable node!
                if (canUp)
                {
                    var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                    dot.AddToClassList("badge-dot");
                    dot.style.width = dotSize;
                    dot.style.height = dotSize;
                    dot.style.borderTopLeftRadius = dotSize * 0.5f;
                    dot.style.borderTopRightRadius = dotSize * 0.5f;
                    dot.style.borderBottomLeftRadius = dotSize * 0.5f;
                    dot.style.borderBottomRightRadius = dotSize * 0.5f;
                    dot.style.right = -2;
                    dot.style.top = -2;
                    nodeBtn.Add(dot);
                }

                nodeWrap.Add(nodeBtn);

                if (showNodeTitle)
                {
                    var titlePill = new Label(n.branchName)
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            marginTop = 2,
                            fontSize = Mathf.Clamp(Mathf.RoundToInt(8.5f * clampedZoom), 9, 12),
                            unityFontStyleAndWeight = FontStyle.Bold,
                            color = new Color(0.14f, 0.24f, 0.20f),
                            backgroundColor = new Color(0.99f, 0.98f, 0.94f, 0.90f),
                            paddingLeft = 4, paddingRight = 4, paddingTop = 1, paddingBottom = 1,
                            borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5
                        }
                    };
                    nodeWrap.Add(titlePill);
                }

                anchor.Add(nodeWrap);
            }
            body.Add(canvas);

            // SELECTED NODE INSPECTOR CARD
            int selLv = engine.GetDaoVanLevel(sel.id);
            double selCost = engine.GetDaoVanUpgradeCost(sel, selLv);
            bool selMax = selLv >= sel.maxLevel;
            bool selCan = engine.CanUnlockOrUpgradeNode(sel);

            var detail = new VisualElement();
            detail.AddToClassList("stage-hero-box");
            detail.style.alignItems = Align.Stretch;
            var dHead = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var dIcon = new VisualElement { style = { width = 40, height = 40, flexShrink = 0 } };
            SetBorderAndRadius(dIcon, new Color(0.84f, 0.60f, 0.16f), 1.5f, 20f);
            CodexArtLoader.ApplyIconOrPlaceholder(dIcon, sel.codexIconKey, "", sel.reusedIconKey);
            dHead.Add(dIcon);

            var dMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 8, flexShrink = 1 } };
            dMeta.Add(new Label($"[{sel.branchName}] {sel.title} (Cấp {selLv}/{sel.maxLevel})") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            dMeta.Add(new Label(sel.effectDesc) { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            dHead.Add(dMeta);
            detail.Add(dHead);

            var dAct = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginTop = 7 } };
            string costStr = selMax ? "Đã đạt Đỉnh Phong" : sel.costsDaoPoints ? $"Tiêu hao: {selCost:0} Điểm Đạo Vận" : $"Tiêu hao: {CultivationTable.FormatNumber(selCost)} Linh Thạch";
            dAct.Add(new Label(costStr) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });
            var upBtn = new Button(() =>
            {
                if (engine.TryUpgradeDaoVanNode(sel.id))
                {
                    body.Clear();
                    BuildDaoVanTree(body, pan, clampedZoom, onViewportChanged);
                }
            })
            { text = selMax ? "Đã Tối Đa" : (selLv == 0 ? "Khai Mở" : "Nâng Cấp") };
            upBtn.AddToClassList("btn-primary");
            upBtn.SetEnabled(selCan);
            dAct.Add(upBtn);
            detail.Add(dAct);
            body.Add(detail);
        }

        private static void ComputeDaoVanFitViewport(List<DaoVanNodeSpec> visibleNodes, out Vector2 pan, out float zoom)
        {
            if (visibleNodes == null || visibleNodes.Count <= 1)
            {
                pan = Vector2.zero;
                zoom = 1.05f;
                return;
            }
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < visibleNodes.Count; i++)
            {
                Vector2 p = visibleNodes[i].normalizedPos;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }
            float spanX = Mathf.Max(140f, maxX - minX + 120f);
            float spanY = Mathf.Max(140f, maxY - minY + 140f);
            zoom = Mathf.Clamp(Mathf.Min(310f / spanX, 215f / spanY), 0.48f, 0.98f);
            Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            pan = -center * zoom;
        }

        private static DaoVanNodeSpec FindDaoVanSpec(string id)
        {
            for (int i = 0; i < RemakeCatalogs.StarterDaoVanNodes.Count; i++)
                if (RemakeCatalogs.StarterDaoVanNodes[i].id == id) return RemakeCatalogs.StarterDaoVanNodes[i];
            return null;
        }

        // =========================================================
        // 3. LUYEN DAN — % SUCCESS, 3.5S SLIDE BAR, SMART GRID & SINKS
        // =========================================================
        public static void BuildAlchemy(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            if (_uiAlchemyPillIdx < 0) _uiAlchemyPillIdx = Mathf.Clamp(save.realmIndex, 0, 7);

            void Refresh()
            {
                body.Clear();
                BuildAlchemy(body);
            }

            bool unlocked = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, save.realmIndex, save.subStage);
            int herbTier = CultivationTable.GetPillRequiredHerbTier(_uiAlchemyPillIdx);
            int reqHerbs = CultivationTable.GetPillRequiredHerbCount(_uiAlchemyPillIdx);
            int ownedHerbs = save.herbsByTier[Mathf.Clamp(herbTier - 1, 0, 5)];
            double ltCost = CultivationTable.GetPillBrewLinhThachCost(_uiAlchemyPillIdx);
            string pillName = CultivationTable.GetPillNameForRealmBreakthrough(_uiAlchemyPillIdx);
            int brewChancePct = Mathf.RoundToInt(engine.GetAlchemyBrewSuccessChance(_uiAlchemyPillIdx) * 100f);
            engine.GetAlchemyQualityOdds(out int haPct, out int trungPct, out int thuongPct, out int cucPct);

            // HERO CAULDRON STAGE
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            hero.style.alignItems = Align.Stretch;

            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var cauldronWrap = new VisualElement { style = { width = 76, height = 76, flexShrink = 0 } };
            SetBorderAndRadius(cauldronWrap, new Color(0.86f, 0.56f, 0.14f, 0.72f), 1.5f, 12f);
            CodexArtLoader.ApplyIconOrPlaceholder(cauldronWrap, "Stages/stage_alchemy_cauldron", "[Luyện Đan Lô]");
            var flameBadge = new VisualElement { style = { position = Position.Absolute, right = 2, bottom = 2, width = 26, height = 26, backgroundColor = new Color(1f, 0.96f, 0.88f, 0.95f) } };
            SetBorderAndRadius(flameBadge, new Color(0.88f, 0.42f, 0.14f), 1.5f, 13f);
            CodexArtLoader.ApplyIconOrPlaceholder(flameBadge, CultivationTable.GetSpiritFlameCodexKey(save.spiritFlameTier), "[Hỏa]");
            cauldronWrap.Add(flameBadge);
            topRow.Add(cauldronWrap);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 8, flexShrink = 1 } };
            info.Add(new Label("Đan Các · Cửu Long Luyện Đan Lô") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            info.Add(new Label($"Đan Sư: {CultivationTable.GetAlchemistRankName(save.alchemistRank)} (XP {save.alchemistXp})") { style = { fontSize = 10, marginTop = 2 } });
            info.Add(new Label($"Chân Hỏa: {CultivationTable.GetSpiritFlameName(save.spiritFlameTier)} ({save.spiritFlameXp}/{CultivationTable.GetSpiritFlameNextTierXp(save.spiritFlameTier)} XP)") { style = { fontSize = 10, color = new Color(0.72f, 0.34f, 0.10f), marginTop = 1 } });
            info.Add(new Label($"Dược Trần: {save.herbDust} · {ActStageDatabaseSO.GetHerbName(herbTier)}: {ownedHerbs}/{reqHerbs}") { style = { fontSize = 10, color = new Color(0.16f, 0.56f, 0.44f), marginTop = 1 } });
            topRow.Add(info);
            hero.Add(topRow);

            if (!unlocked)
                hero.Add(new Label($"Mở khóa Luyện Đan tại {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.Alchemy)}.") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginTop = 4 } });

            // 4x2 RECIPE GRID
            var pillTabs = new VisualElement();
            pillTabs.AddToClassList("grid-wrap");
            pillTabs.style.justifyContent = Justify.SpaceBetween;
            pillTabs.style.marginTop = 7;
            for (int p = 0; p < 8; p++)
            {
                int capP = p;
                var b = new Button(() => { _uiAlchemyPillIdx = capP; Refresh(); })
                {
                    text = CultivationTable.GetPillNameForRealmBreakthrough(p).Replace(" Đan", ""),
                    style =
                    {
                        width = Length.Percent(23.8f),
                        marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 4,
                        fontSize = 9, paddingLeft = 3, paddingRight = 3, paddingTop = 4, paddingBottom = 4
                    }
                };
                b.AddToClassList("btn-compact");
                if (p == _uiAlchemyPillIdx) b.AddToClassList("selected");
                pillTabs.Add(b);
            }
            hero.Add(pillTabs);

            // BREW SUCCESS % & QUALITY ODDS PREVIEW
            var oddsBox = new VisualElement { style = { paddingTop = 5, paddingBottom = 5, paddingLeft = 8, paddingRight = 8, marginTop = 3, backgroundColor = new Color(0.96f, 0.98f, 0.95f) } };
            SetBorderAndRadius(oddsBox, new Color(0.18f, 0.56f, 0.44f, 0.45f), 1f, 8f);
            oddsBox.Add(new Label($"Tỉ lệ thành đan: {brewChancePct}%  ·  Cần: {reqHerbs} {ActStageDatabaseSO.GetHerbName(herbTier)} + {CultivationTable.FormatNumber(ltCost)} LT")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, color = new Color(0.11f, 0.48f, 0.36f), whiteSpace = WhiteSpace.Normal }
            });
            oddsBox.Add(new Label($"Xác suất phẩm chất: Hạ {haPct}% · Trung {trungPct}% · Thượng {thuongPct}% · Cực {cucPct}%")
            {
                style = { fontSize = 9, color = new Color(0.35f, 0.44f, 0.40f), marginTop = 2, whiteSpace = WhiteSpace.Normal }
            });
            hero.Add(oddsBox);

            // LIVE 3.5s BREW COUNTDOWN SLIDE BAR
            var brewBox = new VisualElement { style = { marginTop = 6 } };
            string brewLabelTxt = engine.IsBrewingPill
                ? $"Đang luyện {CultivationTable.GetPillNameForRealmBreakthrough(engine.BrewingTargetRealm)}... còn {engine.BrewTimeRemaining:0.0}s ({Mathf.RoundToInt(engine.BrewProgress01 * 100)}%)"
                : $"Sẵn sàng khai lò {pillName} · Thời gian ngưng đan: {GameplayEngine.BrewDuration:0.0}s";
            brewBox.Add(new Label(brewLabelTxt) { name = "alchemy-brew-label", style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.72f, 0.36f, 0.06f), marginBottom = 3 } });
            var brewTrack = new VisualElement { style = { width = Length.Percent(100), height = 11 } };
            brewTrack.AddToClassList("bar-track");
            var brewFill = new VisualElement { name = "alchemy-brew-fill", style = { width = Length.Percent(engine.BrewProgress01 * 100f) } };
            brewFill.AddToClassList("bar-fill-alchemy");
            brewTrack.Add(brewFill);
            brewBox.Add(brewTrack);
            hero.Add(brewBox);

            // BREW CTA ROW
            var recipeRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginTop = 7 } };
            var leftP = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexShrink = 1 } };
            var pillIcon = new VisualElement { style = { width = 30, height = 30, marginRight = 6, flexShrink = 0 } };
            CodexArtLoader.ApplyIconOrPlaceholder(pillIcon, CultivationTable.GetPillCodexKey(_uiAlchemyPillIdx), "[Đan]");
            leftP.Add(pillIcon);
            leftP.Add(new Label($"{pillName} ({brewChancePct}%)") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
            recipeRow.Add(leftP);

            var brewBtn = new Button(() => { engine.TryStartBrewPill(_uiAlchemyPillIdx); Refresh(); })
            {
                text = engine.IsBrewingPill ? "Đang Ngưng Đan..." : $"Khai Lò ({GameplayEngine.BrewDuration:0.0}s)"
            };
            brewBtn.AddToClassList("btn-primary");
            brewBtn.SetEnabled(unlocked && !engine.IsBrewingPill);
            recipeRow.Add(brewBtn);
            hero.Add(recipeRow);
            body.Add(hero);

            // SMART OWNED PILL INVENTORY GRID + EXCESS PILL SINKS
            var ownedStacks = new List<RemakePillStackEntry>();
            int totalPills = 0;
            for (int i = 0; i < save.pills.Count; i++)
            {
                var st = save.pills[i];
                if (st == null || st.count <= 0) continue;
                totalPills += st.count;
                if (_uiAlchemyShowAllPills || st.realmTarget == _uiAlchemyPillIdx)
                    ownedStacks.Add(st);
            }
            ownedStacks.Sort((a, b) =>
            {
                if (a.realmTarget != b.realmTarget) return b.realmTarget.CompareTo(a.realmTarget);
                return ((int)b.quality).CompareTo((int)a.quality);
            });

            RemakePillStackEntry selStack = null;
            for (int i = 0; i < ownedStacks.Count; i++)
                if (ownedStacks[i].realmTarget == _uiAlchemySelectedRealm && ownedStacks[i].quality == _uiAlchemySelectedQuality)
                { selStack = ownedStacks[i]; break; }
            if (selStack == null && ownedStacks.Count > 0)
            {
                selStack = ownedStacks[0];
                _uiAlchemySelectedRealm = selStack.realmTarget;
                _uiAlchemySelectedQuality = selStack.quality;
            }

            var invCard = new VisualElement();
            invCard.AddToClassList("card-light");

            var invHead = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 6 } };
            invHead.Add(new Label($"Kho Đan Thông Minh ({totalPills} viên)") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            var invBtns = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var filterBtn = new Button(() => { _uiAlchemyShowAllPills = !_uiAlchemyShowAllPills; Refresh(); })
            { text = _uiAlchemyShowAllPills ? "Đang xem: Tất Cả" : "Đang xem: Đan Chọn" };
            filterBtn.AddToClassList("btn-compact");
            filterBtn.style.fontSize = 9; filterBtn.style.marginRight = 4;
            invBtns.Add(filterBtn);

            var mergeAllBtn = new Button(() => { engine.TryMergeAllPills(_uiAlchemyPillIdx); Refresh(); }) { text = "Hợp Nhanh 3→1" };
            mergeAllBtn.AddToClassList("btn-compact");
            mergeAllBtn.SetEnabled(unlocked);
            mergeAllBtn.style.fontSize = 9;
            invBtns.Add(mergeAllBtn);
            invHead.Add(invBtns);
            invCard.Add(invHead);

            if (ownedStacks.Count == 0)
            {
                invCard.Add(new Label("Chưa có đan dược nào trong kho. Hãy Khai Lò Luyện Đan phía trên!") { style = { fontSize = 11, color = new Color(0.45f, 0.50f, 0.47f) } });
            }
            else
            {
                var pillGrid = new VisualElement();
                pillGrid.AddToClassList("grid-wrap");
                for (int i = 0; i < ownedStacks.Count; i++)
                {
                    var st = ownedStacks[i];
                    bool isSel = selStack != null && st.realmTarget == selStack.realmTarget && st.quality == selStack.quality;
                    var tile = new Button(() =>
                    {
                        _uiAlchemySelectedRealm = st.realmTarget;
                        _uiAlchemySelectedQuality = st.quality;
                        Refresh();
                    })
                    {
                        style = { width = 72, height = 78, marginRight = 5, marginBottom = 5 }
                    };
                    tile.AddToClassList("grid-tile");
                    if (isSel) tile.AddToClassList("grid-tile-selected");

                    Color qColor = GetPillQualityColor(st.quality);
                    var cntLbl = new Label($"x{st.count}")
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { position = Position.Absolute, right = 3, top = 2, fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.14f, 0.24f, 0.20f) }
                    };
                    tile.Add(cntLbl);

                    var pIco = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 30, height = 30, minWidth = 30, minHeight = 30, flexShrink = 0, marginTop = 6 } };
                    pIco.AddToClassList("icon-fixed");
                    CodexArtLoader.ApplyIconOrPlaceholder(pIco, CultivationTable.GetPillCodexKey(st.realmTarget), "[Đan]");
                    tile.Add(pIco);
                    tile.Add(new Label(CultivationTable.GetPillNameForRealmBreakthrough(st.realmTarget).Replace(" Đan", "")) { pickingMode = PickingMode.Ignore, style = { fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
                    tile.Add(new Label(GetShortPillQuality(st.quality)) { pickingMode = PickingMode.Ignore, style = { fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = qColor, flexShrink = 0 } });
                    pillGrid.Add(tile);
                }
                invCard.Add(pillGrid);
            }

            // SELECTED PILL SMART ACTIONS (SYNTHESIZE / FLAME / ABSORB TU VI / REFINE LT)
            if (selStack != null)
            {
                string sName = CultivationTable.GetPillNameForRealmBreakthrough(selStack.realmTarget);
                string sQual = CultivationTable.GetPillQualityName(selStack.quality);
                int sBonus = Mathf.RoundToInt(CultivationTable.GetPillBreakthroughBonus(selStack.quality) * 100f);

                var actBox = new VisualElement { style = { marginTop = 6, paddingTop = 7, paddingBottom = 7, paddingLeft = 8, paddingRight = 8, backgroundColor = new Color(0.98f, 0.97f, 0.92f) } };
                SetBorderAndRadius(actBox, new Color(0.84f, 0.62f, 0.16f, 0.68f), 1.5f, 10f);
                actBox.Add(new Label($"{sName} ({sQual}) — Đang có: {selStack.count} · Đột phá +{sBonus}%")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, whiteSpace = WhiteSpace.Normal }
                });
                actBox.Add(new Label("Dùng đan dư để Hợp Đan phẩm cao, Tế Chân Hỏa, Hấp Thu Tu Vi hoặc Luyện Hóa Linh Thạch:")
                {
                    style = { fontSize = 9, color = new Color(0.32f, 0.42f, 0.37f), whiteSpace = WhiteSpace.Normal, marginTop = 2 }
                });

                var bRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginTop = 6 } };
                if (selStack.quality == RemakePillQuality.HaPham || selStack.quality == RemakePillQuality.TrungPham)
                {
                    var syn = new Button(() => { engine.TryMergePills(selStack.realmTarget, selStack.quality); Refresh(); }) { text = "Hợp 3→1" };
                    syn.AddToClassList("btn-secondary");
                    syn.SetEnabled(unlocked && selStack.count >= 3);
                    syn.style.marginRight = 4; syn.style.marginBottom = 4; syn.style.fontSize = 10;
                    bRow.Add(syn);
                }

                if (save.spiritFlameTier < 4)
                {
                    var sac = new Button(() => { engine.TrySacrificePillToFlame(selStack.realmTarget, selStack.quality); Refresh(); }) { text = "Tế Chân Hỏa" };
                    sac.AddToClassList("btn-compact");
                    sac.SetEnabled(unlocked);
                    sac.style.marginRight = 4; sac.style.marginBottom = 4; sac.style.fontSize = 10;
                    bRow.Add(sac);
                }

                var tuVi1 = new Button(() => { engine.TryConsumePillForTuVi(selStack.realmTarget, selStack.quality, 1); Refresh(); }) { text = "Hấp Thu Tu Vi (x1)" };
                tuVi1.AddToClassList("btn-compact");
                tuVi1.style.marginRight = 4; tuVi1.style.marginBottom = 4; tuVi1.style.fontSize = 10;
                bRow.Add(tuVi1);

                if (selStack.count >= 5)
                {
                    var tuVi5 = new Button(() => { engine.TryConsumePillForTuVi(selStack.realmTarget, selStack.quality, 5); Refresh(); }) { text = "Hấp Thu (x5)" };
                    tuVi5.AddToClassList("btn-compact");
                    tuVi5.style.marginRight = 4; tuVi5.style.marginBottom = 4; tuVi5.style.fontSize = 10;
                    bRow.Add(tuVi5);
                }

                var ref1 = new Button(() => { engine.TryRefinePillToLinhThach(selStack.realmTarget, selStack.quality, 1); Refresh(); }) { text = "Luyện Hóa LT (x1)" };
                ref1.AddToClassList("btn-compact");
                ref1.style.marginRight = 4; ref1.style.marginBottom = 4; ref1.style.fontSize = 10;
                bRow.Add(ref1);

                if (selStack.count >= 5)
                {
                    var ref5 = new Button(() => { engine.TryRefinePillToLinhThach(selStack.realmTarget, selStack.quality, 5); Refresh(); }) { text = "Luyện Hóa (x5)" };
                    ref5.AddToClassList("btn-compact");
                    ref5.style.marginBottom = 4; ref5.style.fontSize = 10;
                    bRow.Add(ref5);
                }

                actBox.Add(bRow);
                invCard.Add(actBox);
            }

            if (save.herbDust > 0 && save.spiritFlameTier < 4)
            {
                var dustBtn = new Button(() => { engine.TrySacrificePillToFlame(_uiAlchemyPillIdx, RemakePillQuality.PheDan); Refresh(); })
                { text = $"Tế Dược Trần Lên Chân Hỏa ({save.herbDust})" };
                dustBtn.AddToClassList("btn-compact");
                dustBtn.style.marginTop = 6;
                invCard.Add(dustBtn);
            }
            body.Add(invCard);
        }

        private static string GetShortPillQuality(RemakePillQuality q) => q switch
        {
            RemakePillQuality.PheDan => "Phế Đan",
            RemakePillQuality.HaPham => "Hạ Phẩm",
            RemakePillQuality.TrungPham => "Trung",
            RemakePillQuality.ThuongPham => "Thượng",
            _ => "Cực Phẩm"
        };

        private static Color GetPillQualityColor(RemakePillQuality q) => q switch
        {
            RemakePillQuality.PheDan => new Color(0.55f, 0.45f, 0.42f),
            RemakePillQuality.HaPham => new Color(0.24f, 0.56f, 0.42f),
            RemakePillQuality.TrungPham => new Color(0.16f, 0.48f, 0.78f),
            RemakePillQuality.ThuongPham => new Color(0.58f, 0.26f, 0.78f),
            _ => new Color(0.84f, 0.52f, 0.06f)
        };

        // =========================================================
        // 4. TRANG BI — 3-COL PAPERDOLL + LIVE COMPARE + GRID BAG
        // =========================================================
        public static void BuildEquipment(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            void Refresh()
            {
                body.Clear();
                BuildEquipment(body);
            }

            bool unlocked = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, save.realmIndex, save.subStage);

            // HERO 3-COLUMN PAPERDOLL
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            hero.style.alignItems = Align.Stretch;

            var hdr = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 6 } };
            hdr.Add(new Label($"Tướng Tinh 6 Ô ({save.equipmentBag.Count}/40 món)") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            var autoBtn = new Button(() => { engine.TryAutoEquipBestGear(); Refresh(); }) { text = "Mặc Nhanh (Tối Ưu)" };
            autoBtn.AddToClassList("btn-primary");
            autoBtn.style.paddingTop = 4; autoBtn.style.paddingBottom = 4; autoBtn.style.paddingLeft = 8; autoBtn.style.paddingRight = 8; autoBtn.style.fontSize = 10;
            autoBtn.SetEnabled(unlocked && save.equipmentBag.Count > 0);
            hdr.Add(autoBtn);
            hero.Add(hdr);

            if (!unlocked)
                hero.Add(new Label($"Mở khóa Trang Bị tại {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.EquipmentLoot)}.") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginBottom = 5 } });

            VisualElement BuildSlotTile(RemakeEquipSlot slot)
            {
                int slotIdx = (int)slot;
                var eq = engine.GetEquippedItem(slot);
                int enh = save.equipSlotEnhanceLevels[slotIdx];
                var tile = new VisualElement { style = { paddingTop = 4, paddingBottom = 4, paddingLeft = 4, paddingRight = 4, marginBottom = 5, backgroundColor = new Color(0.99f, 0.98f, 0.94f), alignItems = Align.Center } };
                Color bc = eq != null ? new Color(0.86f, 0.62f, 0.14f) : new Color(0.75f, 0.78f, 0.76f);
                SetBorderAndRadius(tile, bc, 1.5f, 8f);

                var topSlot = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                var frame = new VisualElement { style = { width = 28, height = 28, alignItems = Align.Center, justifyContent = Justify.Center } };
                if (eq != null) CodexArtLoader.ApplyIconOrPlaceholder(frame, CultivationTable.GetRarityFrameCodexKey(eq.rarity), "");
                var sIcon = new VisualElement { style = { width = 22, height = 22 } };
                var (cKey, rKey) = CultivationTable.GetEquipSlotIcons(slot);
                CodexArtLoader.ApplyIconOrPlaceholder(sIcon, cKey, "", rKey);
                frame.Add(sIcon);
                topSlot.Add(frame);
                topSlot.Add(new Label($" +{enh}") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });
                tile.Add(topSlot);

                tile.Add(new Label(eq != null ? $"{GetShortEquipSlot(slot)}·T{eq.tier}" : GetShortEquipSlot(slot)) { style = { fontSize = 9, marginTop = 2 } });

                var eBtn = new Button(() => { engine.TryEnhanceEquipSlot(slotIdx); Refresh(); }) { text = $"+Cường ({CultivationTable.GetEquipSlotEnhanceCostOre(enh)} Q)" };
                eBtn.AddToClassList("btn-compact");
                eBtn.SetEnabled(unlocked);
                eBtn.style.fontSize = 8; eBtn.style.paddingTop = 2; eBtn.style.paddingBottom = 2; eBtn.style.marginTop = 2;
                tile.Add(eBtn);
                return tile;
            }

            var paperRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween } };
            var leftCol = new VisualElement { style = { width = 94 } };
            leftCol.Add(BuildSlotTile(RemakeEquipSlot.Crown));
            leftCol.Add(BuildSlotTile(RemakeEquipSlot.Robe));
            leftCol.Add(BuildSlotTile(RemakeEquipSlot.Amulet));

            var centerCol = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center, paddingLeft = 4, paddingRight = 4 } };
            var portrait = new VisualElement { style = { width = 66, height = 66, marginBottom = 4, backgroundColor = new Color(0.99f, 0.96f, 0.88f) } };
            SetBorderAndRadius(portrait, new Color(0.86f, 0.62f, 0.14f), 2f, 33f);
            CodexArtLoader.ApplyIconOrPlaceholder(portrait, "", "[Tu Sĩ]", "CharacterPortrait");
            centerCol.Add(portrait);

            float totalScore = 0f;
            for (int i = 0; i < 6; i++)
                totalScore += CultivationTable.CalculateEquipItemScore(engine.GetEquippedItem((RemakeEquipSlot)i), save.realmIndex);
            centerCol.Add(new Label($"Lực TB: {totalScore:0}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, color = new Color(0.11f, 0.48f, 0.36f) } });
            centerCol.Add(new Label($"Quặng: {save.ore} · Đá Tẩy: {save.reforgeStoneCount}") { style = { fontSize = 9, color = new Color(0.32f, 0.42f, 0.37f), marginTop = 2, unityTextAlign = TextAnchor.MiddleCenter } });
            centerCol.Add(new Label($"Thanh Vân {engine.GetEquipSetCount(RemakeEquipSet.ThanhVan)}/6 · Huyền Vũ {engine.GetEquipSetCount(RemakeEquipSet.HuyenVu)}/6 · Đan Tông {engine.GetEquipSetCount(RemakeEquipSet.DanTong)}/6")
            {
                style = { fontSize = 9, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter, marginTop = 4 }
            });

            var rightCol = new VisualElement { style = { width = 94 } };
            rightCol.Add(BuildSlotTile(RemakeEquipSlot.Bracer));
            rightCol.Add(BuildSlotTile(RemakeEquipSlot.Ring));
            rightCol.Add(BuildSlotTile(RemakeEquipSlot.Boots));

            paperRow.Add(leftCol);
            paperRow.Add(centerCol);
            paperRow.Add(rightCol);
            hero.Add(paperRow);
            body.Add(hero);

            // AUTO-DISMANTLE ROW
            var filterCard = new VisualElement();
            filterCard.AddToClassList("card-light");
            filterCard.style.marginBottom = 8;
            AddChoiceRow(filterCard, "Tự Phân Rã:", new[] { "Tắt", "≤Phàm", "≤Linh", "≤Huyền" }, Mathf.Clamp(save.autoDismantleMaxRarity + 1, 0, 3), idx =>
            {
                engine.SetAutoDismantleMaxRarity(idx - 1);
                Refresh();
            });
            body.Add(filterCard);

            // RESOLVE SELECTED BAG ITEM & LIVE COMPARISON CARD
            RemakeEquipmentItem selectedItem = null;
            for (int i = 0; i < save.equipmentBag.Count; i++)
                if (save.equipmentBag[i].uid == _selectedGearUid) { selectedItem = save.equipmentBag[i]; break; }
            if (selectedItem == null && save.equipmentBag.Count > 0)
            {
                selectedItem = save.equipmentBag[0];
                _selectedGearUid = selectedItem.uid;
            }

            if (selectedItem != null)
            {
                var equippedInSlot = engine.GetEquippedItem(selectedItem.slot);
                float selScore = CultivationTable.CalculateEquipItemScore(selectedItem, save.realmIndex);
                float eqScore = CultivationTable.CalculateEquipItemScore(equippedInSlot, save.realmIndex);
                float diff = selScore - eqScore;

                var cmp = new VisualElement();
                cmp.AddToClassList("card-light");
                cmp.style.marginBottom = 8;
                SetBorderAndRadius(cmp, new Color(0.18f, 0.56f, 0.44f, 0.72f), 1.5f, 10f);

                var cHead = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center } };
                string setStr = selectedItem.setId != RemakeEquipSet.None ? $" · {CultivationTable.GetEquipSetName(selectedItem.setId)}" : "";
                cHead.Add(new Label($"{CultivationTable.GetRarityName(selectedItem.rarity)} {CultivationTable.GetEquipSlotName(selectedItem.slot)} Bậc {selectedItem.tier}{setStr}")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, whiteSpace = WhiteSpace.Normal, flexShrink = 1 }
                });
                var deltaLbl = new Label(diff >= 0 ? $"▲ +{diff:0}" : $"▼ {diff:0}") { style = { fontSize = 12, flexShrink = 0, marginLeft = 6 } };
                deltaLbl.AddToClassList(diff >= 0 ? "stat-up" : "stat-down");
                cHead.Add(deltaLbl);
                cmp.Add(cHead);

                cmp.Add(new Label(equippedInSlot != null
                    ? $"Đang mặc: {CultivationTable.GetRarityName(equippedInSlot.rarity)} T{equippedInSlot.tier} ({eqScore:0}) vs Chọn ({selScore:0})"
                    : $"Ô {CultivationTable.GetEquipSlotName(selectedItem.slot)} đang trống (Điểm {selScore:0})")
                {
                    style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f), marginTop = 2 }
                });

                for (int s = 0; s < selectedItem.substats.Count; s++)
                {
                    var st = selectedItem.substats[s];
                    float eqVal = 0f;
                    if (equippedInSlot != null)
                        for (int k = 0; k < equippedInSlot.substats.Count; k++)
                            if (equippedInSlot.substats[k].statType == st.statType) eqVal += equippedInSlot.substats[k].value;
                    float sd = st.value - eqVal;
                    var sLbl = new Label($"• {CultivationTable.GetEquipStatLabel(st.statType, st.value)} ({(sd >= 0 ? "+" : "")}{sd:0.#})") { style = { fontSize = 10, marginTop = 2 } };
                    sLbl.AddToClassList(sd >= 0 ? "stat-up" : "stat-down");
                    cmp.Add(sLbl);
                }

                var btns = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginTop = 7 } };
                var eqBtn = new Button(() => { engine.TryEquipGear(selectedItem.uid); Refresh(); }) { text = "Mặc" };
                eqBtn.AddToClassList("btn-primary");
                eqBtn.SetEnabled(unlocked);
                eqBtn.style.marginRight = 4; eqBtn.style.marginBottom = 3;
                btns.Add(eqBtn);

                var lockBtn = new Button(() => { engine.TryToggleGearLock(selectedItem.uid); Refresh(); }) { text = selectedItem.isLocked ? "Mở Khóa" : "Khóa" };
                lockBtn.AddToClassList("btn-compact");
                if (selectedItem.isLocked) lockBtn.AddToClassList("selected");
                lockBtn.style.marginRight = 4; lockBtn.style.marginBottom = 3;
                btns.Add(lockBtn);

                var refBtn = new Button(() => { engine.TryReforgeGear(selectedItem.uid); Refresh(); }) { text = "Tẩy Luyện" };
                refBtn.AddToClassList("btn-secondary");
                refBtn.SetEnabled(unlocked && save.reforgeStoneCount > 0);
                refBtn.style.marginRight = 4; refBtn.style.marginBottom = 3;
                btns.Add(refBtn);

                var disBtn = new Button(() => { engine.TryDismantleGear(selectedItem.uid); Refresh(); }) { text = selectedItem.isLocked ? "Đã Khóa" : "Phân Rã" };
                disBtn.AddToClassList("btn-compact");
                disBtn.SetEnabled(unlocked && !selectedItem.isLocked);
                disBtn.style.marginBottom = 3;
                btns.Add(disBtn);

                cmp.Add(btns);
                body.Add(cmp);
            }

            // EQUIPMENT BAG AS COMPACT MULTI-COLUMN GRID
            var bagCard = new VisualElement();
            bagCard.AddToClassList("card-light");
            bagCard.Add(new Label($"Kho Đồ Trang Bị ({save.equipmentBag.Count}/40) — Chạm ô lưới để So Sánh") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, marginBottom = 6 } });

            if (save.equipmentBag.Count == 0)
            {
                bagCard.Add(new Label("Kho trang bị đang trống. Đánh quái hoặc Boss để nhặt pháp khí!") { style = { fontSize = 11, color = new Color(0.45f, 0.50f, 0.47f) } });
            }
            else
            {
                var bagGrid = new VisualElement();
                bagGrid.AddToClassList("grid-wrap");
                for (int i = 0; i < save.equipmentBag.Count; i++)
                {
                    var item = save.equipmentBag[i];
                    bool isSel = item.uid == _selectedGearUid;
                    float score = CultivationTable.CalculateEquipItemScore(item, save.realmIndex);
                    float diff = score - CultivationTable.CalculateEquipItemScore(engine.GetEquippedItem(item.slot), save.realmIndex);

                    var tile = new Button(() => { _selectedGearUid = item.uid; Refresh(); })
                    {
                        style = { width = 62, height = 74, marginRight = 5, marginBottom = 5 }
                    };
                    tile.AddToClassList("grid-tile");
                    if (isSel) tile.AddToClassList("grid-tile-selected");

                    if (item.isLocked)
                    {
                        tile.Add(new Label("K")
                        {
                            pickingMode = PickingMode.Ignore,
                            style = { position = Position.Absolute, left = 3, top = 2, fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.78f, 0.48f, 0.06f) }
                        });
                    }
                    var arrow = new Label(diff > 0.1f ? "▲" : diff < -0.1f ? "▼" : "=")
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { position = Position.Absolute, right = 3, top = 2, fontSize = 9 }
                    };
                    arrow.AddToClassList(diff >= 0 ? "stat-up" : "stat-down");
                    tile.Add(arrow);

                    var iconWrap = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 34, height = 34, minWidth = 34, minHeight = 34, flexShrink = 0, marginTop = 6, alignItems = Align.Center, justifyContent = Justify.Center } };
                    iconWrap.AddToClassList("icon-fixed");
                    CodexArtLoader.ApplyIconOrPlaceholder(iconWrap, CultivationTable.GetRarityFrameCodexKey(item.rarity), "");
                    var innerIcon = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 24, height = 24, minWidth = 24, minHeight = 24, flexShrink = 0 } };
                    innerIcon.AddToClassList("icon-fixed");
                    var (ck, rk) = CultivationTable.GetEquipSlotIcons(item.slot);
                    CodexArtLoader.ApplyIconOrPlaceholder(innerIcon, ck, "", rk);
                    iconWrap.Add(innerIcon);
                    tile.Add(iconWrap);

                    tile.Add(new Label($"T{item.tier}·{GetShortEquipSlot(item.slot)}") { pickingMode = PickingMode.Ignore, style = { fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 2, flexShrink = 0 } });
                    bagGrid.Add(tile);
                }
                bagCard.Add(bagGrid);
            }
            body.Add(bagCard);
        }

        private static string GetShortEquipSlot(RemakeEquipSlot slot) => slot switch
        {
            RemakeEquipSlot.Crown => "Mũ",
            RemakeEquipSlot.Robe => "Áo",
            RemakeEquipSlot.Amulet => "Phù",
            RemakeEquipSlot.Bracer => "Tay",
            RemakeEquipSlot.Ring => "Nhẫn",
            _ => "Giày"
        };

        // =========================================================
        // 5. TRAN PHAP — 2-COLUMN RESPONSIVE GRID (ZERO OVERFLOW!)
        // =========================================================
        public static void BuildFormation(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            bool formOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, save.realmIndex, save.subStage);
            bool subOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.FormationSubSlot, save.realmIndex, save.subStage);

            if (_selectedFormationIdx < 0 || _selectedFormationIdx >= RemakeCatalogs.Formations.Count)
                _selectedFormationIdx = save.activeMainFormation >= 0 ? save.activeMainFormation : 0;

            void Refresh()
            {
                body.Clear();
                BuildFormation(body);
            }

            var top = new VisualElement();
            top.AddToClassList("card-light");
            top.style.marginBottom = 8;
            string mainName = save.activeMainFormation >= 0 ? RemakeCatalogs.Formations[save.activeMainFormation].name : "Chưa khai trận";
            string subName = save.activeSubFormation >= 0 ? RemakeCatalogs.Formations[save.activeSubFormation].name : (subOpen ? "Chưa chọn" : "Mở ở Hóa Thần (50% hiệu lực)");
            top.Add(new Label($"Trận Chính: {mainName}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, whiteSpace = WhiteSpace.Normal } });
            top.Add(new Label($"Ô Trận Phụ: {subName}") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            body.Add(top);

            // 2-COLUMN GRID OF ALL 6 FORMATIONS
            var grid = new VisualElement();
            grid.AddToClassList("grid-wrap");
            grid.style.justifyContent = Justify.SpaceBetween;
            grid.style.marginBottom = 8;

            for (int i = 0; i < RemakeCatalogs.Formations.Count; i++)
            {
                var f = RemakeCatalogs.Formations[i];
                bool fUnlocked = formOpen && save.realmIndex >= f.minRealmIndex;
                bool isMain = save.activeMainFormation == f.index;
                bool isSub = save.activeSubFormation == f.index;
                bool isSel = _selectedFormationIdx == f.index;

                var tile = new Button(() => { _selectedFormationIdx = f.index; Refresh(); })
                {
                    style =
                    {
                        width = Length.Percent(48.5f),
                        minHeight = 96,
                        marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 6,
                        paddingTop = 7, paddingBottom = 7, paddingLeft = 6, paddingRight = 6,
                        alignItems = Align.Center,
                        justifyContent = Justify.FlexStart,
                        overflow = Overflow.Hidden
                    }
                };
                tile.AddToClassList("grid-tile");
                if (isSel) tile.AddToClassList("grid-tile-selected");
                else if (isMain || isSub)
                {
                    tile.style.backgroundColor = new Color(0.90f, 0.97f, 0.94f);
                    SetBorderAndRadius(tile, new Color(0.16f, 0.58f, 0.44f), 2f, 10f);
                }

                var headRow = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.Center, marginBottom = 4 } };
                var icon = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 34, height = 34, marginRight = 6, flexShrink = 0 } };
                icon.AddToClassList("art-slot");
                CodexArtLoader.ApplyIconOrPlaceholder(icon, f.codexIconKey, "[Trận]", f.reusedIconKey);
                headRow.Add(icon);

                string statusTag = isMain ? "CHÍNH" : isSub ? "PHỤ 50%" : fUnlocked ? "ĐÃ MỞ" : "KHÓA";
                Color tagBg = isMain ? new Color(0.84f, 0.56f, 0.10f) : isSub ? new Color(0.16f, 0.56f, 0.44f) : fUnlocked ? new Color(0.28f, 0.62f, 0.50f) : new Color(0.62f, 0.66f, 0.64f);
                headRow.Add(new Label(statusTag)
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white,
                        backgroundColor = tagBg, paddingLeft = 5, paddingRight = 5, paddingTop = 2, paddingBottom = 2,
                        borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5
                    }
                });
                tile.Add(headRow);

                tile.Add(new Label(f.name)
                {
                    pickingMode = PickingMode.Ignore,
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter }
                });
                tile.Add(new Label(fUnlocked ? f.effectDesc : $"Mở: {f.realmReqText}")
                {
                    pickingMode = PickingMode.Ignore,
                    style = { fontSize = 9, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter, color = fUnlocked ? new Color(0.24f, 0.36f, 0.32f) : new Color(0.72f, 0.28f, 0.18f), marginTop = 2 }
                });
                grid.Add(tile);
            }
            body.Add(grid);

            // SELECTED FORMATION DETAIL & ACTIVATION CARD
            var sel = RemakeCatalogs.Formations[_selectedFormationIdx];
            bool selUnlocked = formOpen && save.realmIndex >= sel.minRealmIndex;
            bool selIsMain = save.activeMainFormation == sel.index;
            bool selIsSub = save.activeSubFormation == sel.index;

            var detail = new VisualElement();
            detail.AddToClassList("stage-hero-box");
            detail.style.alignItems = Align.Stretch;
            var dRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var dIco = new VisualElement { style = { width = 44, height = 44, marginRight = 8, flexShrink = 0 } };
            dIco.AddToClassList("art-slot");
            CodexArtLoader.ApplyIconOrPlaceholder(dIco, sel.codexIconKey, "[Trận]", sel.reusedIconKey);
            dRow.Add(dIco);
            var dCol = new VisualElement { style = { flexGrow = 1, flexShrink = 1 } };
            dCol.Add(new Label(sel.name) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            dCol.Add(new Label($"{sel.effectDesc} · Yêu cầu: {sel.realmReqText}") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            dRow.Add(dCol);
            detail.Add(dRow);

            var dBtns = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.FlexEnd, marginTop = 8 } };
            var btnMain = new Button(() => { engine.TryActivateFormation(sel.index, false); Refresh(); })
            { text = selIsMain ? "Đang Trận Chính" : "Khai Trận Chính" };
            btnMain.AddToClassList("btn-primary");
            btnMain.SetEnabled(selUnlocked && !selIsMain);
            btnMain.style.marginRight = 6;
            dBtns.Add(btnMain);

            if (subOpen)
            {
                var btnSub = new Button(() => { engine.TryActivateFormation(sel.index, true); Refresh(); })
                { text = selIsSub ? "Tắt Trận Phụ" : "Đặt Trận Phụ (50%)" };
                btnSub.AddToClassList("btn-secondary");
                btnSub.SetEnabled(selUnlocked && !selIsMain);
                dBtns.Add(btnSub);
            }
            detail.Add(dBtns);
            body.Add(detail);
        }

        // =========================================================
        // 6. KY NANG — 2x2 GRID + DETAIL CARD
        // =========================================================
        public static void BuildSkills(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            _selectedSkillSlot = Mathf.Clamp(_selectedSkillSlot, 0, 3);

            void Refresh()
            {
                body.Clear();
                BuildSkills(body);
            }

            var scrollBar = new VisualElement();
            scrollBar.AddToClassList("card-light");
            scrollBar.style.marginBottom = 8;
            scrollBar.style.flexDirection = FlexDirection.Row;
            scrollBar.style.alignItems = Align.Center;
            var scIcon = new VisualElement { style = { width = 26, height = 26, marginRight = 8, flexShrink = 0 } };
            scIcon.AddToClassList("art-slot");
            CodexArtLoader.ApplyIconOrPlaceholder(scIcon, "Equipment/mat_skill_scroll", "");
            scrollBar.Add(scIcon);
            scrollBar.Add(new Label($"Tàn Quyển Công Pháp: {save.skillScrollCount} (ưu tiên dùng khi nâng · Thức tỉnh Lv.10 / 20 / 30)")
            {
                style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, flexGrow = 1, flexShrink = 1 }
            });
            body.Add(scrollBar);

            (string name, string req, string reused, string desc)[] skills =
            {
                ("1. Lôi Phạt", "Luyện Khí Tầng 4", "SkillThunder", "Giáng thiên lôi AoE diệt quái diện rộng."),
                ("2. Âm Dương Trận", "Trúc Cơ Sơ Kỳ", "SkillYinYang", "Tỏa thái cực quanh thân sát thương toàn bộ quái gần."),
                ("3. Vạn Kiếm Quy Tông", "Kim Đan Sơ Kỳ", "SkillSword", "Triệu kiếm trận giáng mưa kiếm liên hoàn."),
                ("4. Ấn Chưởng", "Hóa Thần Sơ Kỳ", "SkillPalm", "Đại Thủ Ấn giáng sát thương cực hạn + đẩy lùi.")
            };

            // 2x2 GRID
            var grid = new VisualElement();
            grid.AddToClassList("grid-wrap");
            grid.style.justifyContent = Justify.SpaceBetween;
            grid.style.marginBottom = 8;

            for (int s = 0; s < 4; s++)
            {
                int slot = s;
                bool unlocked = engine.IsSkillUnlocked(s);
                int lv = engine.GetSkillLevel(s);
                bool autoOn = engine.GetSkillAuto(s);
                bool isSel = _selectedSkillSlot == s;

                var tile = new Button(() => { _selectedSkillSlot = slot; Refresh(); })
                {
                    style =
                    {
                        width = Length.Percent(48.5f),
                        minHeight = 88,
                        marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 6,
                        paddingTop = 7, paddingBottom = 7, paddingLeft = 6, paddingRight = 6,
                        alignItems = Align.Center,
                        overflow = Overflow.Hidden
                    }
                };
                tile.AddToClassList("grid-tile");
                if (isSel) tile.AddToClassList("grid-tile-selected");

                var ico = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 36, height = 36, minWidth = 36, minHeight = 36, flexShrink = 0, marginBottom = 4 } };
                ico.AddToClassList("icon-fixed");
                CodexArtLoader.ApplyIconOrPlaceholder(ico, "", "[Skill]", skills[s].reused);
                tile.Add(ico);

                tile.Add(new Label($"{skills[s].name}") { pickingMode = PickingMode.Ignore, style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, whiteSpace = WhiteSpace.Normal, unityTextAlign = TextAnchor.MiddleCenter } });
                tile.Add(new Label(unlocked ? $"Cấp {lv} · Auto {(autoOn ? "Bật" : "Tắt")}" : $"Mở: {skills[s].req}")
                {
                    pickingMode = PickingMode.Ignore,
                    style = { fontSize = 9, color = unlocked ? new Color(0.14f, 0.52f, 0.40f) : new Color(0.72f, 0.28f, 0.18f), marginTop = 2 }
                });
                grid.Add(tile);
            }
            body.Add(grid);

            // SELECTED SKILL DETAIL CARD
            int selSlot = _selectedSkillSlot;
            bool selUnlocked = engine.IsSkillUnlocked(selSlot);
            int selLv = engine.GetSkillLevel(selSlot);
            bool selAuto = engine.GetSkillAuto(selSlot);
            double ltCost = Math.Round((180 + selSlot * 320) * Math.Pow(1.22, selLv - 1));

            var detail = new VisualElement();
            detail.AddToClassList("stage-hero-box");
            detail.style.alignItems = Align.Stretch;
            detail.Add(new Label($"{skills[selSlot].name} (Cấp {selLv}) — {(selUnlocked ? "Đã khai mở" : "Yêu cầu: " + skills[selSlot].req)}")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal }
            });
            detail.Add(new Label(skills[selSlot].desc) { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 3 } });

            var acts = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginTop = 8 } };
            var btnAuto = new Button(() => { engine.SetSkillAuto(selSlot, !selAuto); Refresh(); }) { text = selAuto ? "Tự Động: BẬT" : "Tự Động: TẮT" };
            btnAuto.AddToClassList("btn-compact");
            btnAuto.EnableInClassList("selected", selAuto);
            btnAuto.SetEnabled(selUnlocked);
            acts.Add(btnAuto);

            string upLbl = save.skillScrollCount > 0 ? "Nâng (1 Tàn Quyển)" : $"Nâng ({CultivationTable.FormatNumber(ltCost)} LT)";
            var btnUp = new Button(() => { engine.TryUpgradeSkill(selSlot, true); Refresh(); }) { text = upLbl };
            btnUp.AddToClassList("btn-primary");
            btnUp.SetEnabled(selUnlocked && (save.skillScrollCount > 0 || save.linhThach >= ltCost));
            acts.Add(btnUp);
            detail.Add(acts);
            body.Add(detail);
        }

        // =========================================================
        // 7. BAN DO VUC-AI (ALREADY GRID!)
        // =========================================================
        public static void BuildStages(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            if (_uiStageTabDiff < 0) _uiStageTabDiff = save.activeDifficulty;
            if (_uiStageTabAct < 0) _uiStageTabAct = save.activeAct;

            var curStage = engine.GetCurrentStageSpec();
            var preview = new VisualElement();
            preview.AddToClassList("card-light");
            preview.style.marginBottom = 8;
            preview.Add(new Label($"Ải Đang Treo: {ActStageDatabaseSO.GetDifficultyLabel(curStage.difficulty)} · {curStage.stageName}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            preview.Add(new Label($"Thấu Thị Rơi Đồ: Tu Vi {CultivationTable.FormatNumber(curStage.tuViPerKill)} · LT {CultivationTable.FormatNumber(curStage.linhThachPerKill)} · Kiếm Lv.{curStage.swordDropMinLevel}-{curStage.swordDropMaxLevel} ({curStage.swordDropRate * 100f:0.0}%) · {ActStageDatabaseSO.GetHerbName(curStage.herbTier)}")
            {
                style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 3 }
            });
            var bossCtrlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginTop = 6 } };
            var autoBossBtn = new Button(() =>
            {
                engine.SetAutoChallengeBoss(!save.autoChallengeBoss);
                body.Clear();
                BuildStages(body);
            })
            {
                text = save.autoChallengeBoss ? "Tự Tiến Thủ Lĩnh X-10: BẬT" : "Tự Tiến Thủ Lĩnh X-10: TẮT (Đang treo X-9)"
            };
            autoBossBtn.AddToClassList("btn-compact");
            if (save.autoChallengeBoss) autoBossBtn.AddToClassList("selected");
            autoBossBtn.style.fontSize = 10;
            bossCtrlRow.Add(autoBossBtn);
            if (engine.CanChallengeActBossNow())
            {
                var chalBtn = new Button(() => engine.TryChallengeCurrentActBoss()) { text = "Khiêu Chiến Boss!" };
                chalBtn.AddToClassList("btn-primary");
                chalBtn.style.paddingTop = 3; chalBtn.style.paddingBottom = 3; chalBtn.style.paddingLeft = 8; chalBtn.style.paddingRight = 8;
                chalBtn.style.fontSize = 10;
                bossCtrlRow.Add(chalBtn);
            }
            preview.Add(bossCtrlRow);
            body.Add(preview);

            var diffRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6 } };
            for (int d = 0; d < 4; d++)
            {
                int capD = d;
                var b = new Button(() => { _uiStageTabDiff = capD; body.Clear(); BuildStages(body); })
                { text = ActStageDatabaseSO.GetDifficultyLabel((RemakeDifficulty)d) };
                b.AddToClassList("btn-compact");
                if (d == _uiStageTabDiff) b.AddToClassList("selected");
                b.style.flexGrow = 1; b.style.marginRight = d < 3 ? 4 : 0; b.style.fontSize = 10;
                diffRow.Add(b);
            }
            body.Add(diffRow);

            var actRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 8 } };
            for (int a = 0; a < 3; a++)
            {
                int capA = a;
                var b = new Button(() => { _uiStageTabAct = capA; body.Clear(); BuildStages(body); })
                { text = a == 0 ? "Vực 1: Đồng Cỏ" : a == 1 ? "Vực 2: Hoang Mạc" : "Vực 3: Tiên Sơn" };
                b.AddToClassList("btn-compact");
                if (a == _uiStageTabAct) b.AddToClassList("selected");
                b.style.flexGrow = 1; b.style.marginRight = a < 2 ? 4 : 0; b.style.fontSize = 10;
                actRow.Add(b);
            }
            body.Add(actRow);

            var stageGrid = new VisualElement();
            stageGrid.AddToClassList("grid-wrap");
            for (int s = 1; s <= 10; s++)
            {
                int stageNum = s;
                var spec = engine.GetStageSpec((RemakeDifficulty)_uiStageTabDiff, _uiStageTabAct, s);
                bool unlocked = spec.LinearIndex <= save.highestUnlockedLinearStage;
                bool active = save.activeDifficulty == _uiStageTabDiff && save.activeAct == _uiStageTabAct && save.activeStage == s;

                var btn = new Button(() =>
                {
                    if (engine.TrySelectStage((RemakeDifficulty)_uiStageTabDiff, _uiStageTabAct, stageNum))
                    {
                        body.Clear();
                        BuildStages(body);
                    }
                })
                { text = spec.isBossStage ? $"Boss {spec.stageCode}\nThủ Lĩnh" : $"Ải {spec.stageCode}" };
                btn.AddToClassList("btn-secondary");
                if (active) btn.AddToClassList("selected");
                btn.SetEnabled(unlocked);
                btn.style.width = 86;
                btn.style.height = 46;
                btn.style.marginRight = 5;
                btn.style.marginBottom = 5;
                btn.style.fontSize = 10;
                stageGrid.Add(btn);
            }
            body.Add(stageGrid);
        }

        // =========================================================
        // 8. DOT PHA — PILL GRID + 15-SECOND TRIBULATION SECLUSION
        // =========================================================
        public static void BuildBreakthrough(VisualElement body, Action onTribulationStarted = null)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            void Refresh()
            {
                body.Clear();
                BuildBreakthrough(body, onTribulationStarted);
            }

            double reqTuVi = engine.GetRequiredTuViForCurrent();
            bool isMajor = save.subStage >= 10;
            int nextSub = isMajor ? 1 : save.subStage + 1;
            int nextRealm = isMajor ? Mathf.Min(8, save.realmIndex + 1) : save.realmIndex;

            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            hero.style.alignItems = Align.Stretch;
            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var altar = new VisualElement { style = { width = 84, height = 84, flexShrink = 0 } };
            SetBorderAndRadius(altar, new Color(0.84f, 0.60f, 0.16f), 1.5f, 12f);
            CodexArtLoader.ApplyIconOrPlaceholder(altar, "Stages/stage_tribulation_altar", "[Độ Kiếp Đài]");
            topRow.Add(altar);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 10, flexShrink = 1 } };
            info.Add(new Label($"{CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)} → {CultivationTable.GetRealmDisplayName(nextRealm, nextSub)}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            info.Add(new Label($"Tu Vi: {CultivationTable.FormatNumber(save.tuVi)} / {CultivationTable.FormatNumber(reqTuVi)} ({Mathf.RoundToInt(Mathf.Clamp01((float)(save.tuVi / Math.Max(1, reqTuVi))) * 100)}%)") { style = { fontSize = 11, marginTop = 3 } });
            info.Add(new Label($"Nghi thức Độ Kiếp: Bế quan đón 9 đạo Thiên Lôi trong 15 giây!") { style = { fontSize = 10, color = new Color(0.68f, 0.34f, 0.08f), whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            topRow.Add(info);
            hero.Add(topRow);

            var helperRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginTop = 7 } };
            helperRow.Add(new Label($"Thất bại liên tiếp: {save.consecutiveBreakthroughFails} · Hộ Pháp: +{save.hoPhapBonusSteps}%") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f) } });
            var incBtn = new Button(() => { if (engine.TryUseCalmIncense()) Refresh(); })
            { text = $"Tĩnh Tâm Hương ({save.calmIncenseCount})" };
            incBtn.AddToClassList("btn-compact");
            incBtn.SetEnabled(save.calmIncenseCount > 0 && save.consecutiveBreakthroughFails > 0);
            helperRow.Add(incBtn);
            hero.Add(helperRow);
            body.Add(hero);

            RemakePillQuality? selectedPill = null;
            if (isMajor)
            {
                if (_selectedBreakthroughPill >= 0 && _selectedBreakthroughPill <= 4)
                {
                    var q = (RemakePillQuality)_selectedBreakthroughPill;
                    if (engine.GetPillCount(save.realmIndex, q) > 0) selectedPill = q;
                    else _selectedBreakthroughPill = -1;
                }

                string pillName = CultivationTable.GetPillNameForRealmBreakthrough(save.realmIndex);
                string pillCodex = CultivationTable.GetPillCodexKey(save.realmIndex);

                var gridCard = new VisualElement();
                gridCard.AddToClassList("card-light");
                gridCard.style.marginBottom = 8;
                gridCard.Add(new Label($"Chọn {pillName} Trợ Lực Độ Kiếp:") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, marginBottom = 6 } });

                var optGrid = new VisualElement();
                optGrid.AddToClassList("grid-wrap");
                optGrid.style.justifyContent = Justify.FlexStart;

                var choices = new List<RemakePillQuality?> { null, RemakePillQuality.HaPham, RemakePillQuality.TrungPham, RemakePillQuality.ThuongPham, RemakePillQuality.CucPham };
                for (int i = 0; i < choices.Count; i++)
                {
                    var opt = choices[i];
                    int cnt = opt.HasValue ? engine.GetPillCount(save.realmIndex, opt.Value) : -1;
                    bool canPick = !opt.HasValue || cnt > 0;
                    bool isSel = (!opt.HasValue && _selectedBreakthroughPill == -1) || (opt.HasValue && _selectedBreakthroughPill == (int)opt.Value);
                    int chPct = Mathf.RoundToInt(engine.ComputeBreakthroughChance(opt) * 100f);

                    var tile = new Button(() =>
                    {
                        if (!canPick) return;
                        _selectedBreakthroughPill = opt.HasValue ? (int)opt.Value : -1;
                        Refresh();
                    })
                    {
                        style =
                        {
                            width = Length.Percent(31.5f),
                            minHeight = 82,
                            marginLeft = 0, marginRight = (i % 3 < 2) ? Length.Percent(2.5f) : 0, marginTop = 0, marginBottom = 6,
                            paddingTop = 5, paddingBottom = 5, alignItems = Align.Center
                        }
                    };
                    tile.AddToClassList("grid-tile");
                    if (isSel) tile.AddToClassList("grid-tile-selected");
                    tile.SetEnabled(canPick);

                    var pIco = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 30, height = 30, minWidth = 30, minHeight = 30, flexShrink = 0 } };
                    pIco.AddToClassList("icon-fixed");
                    CodexArtLoader.ApplyIconOrPlaceholder(pIco, opt.HasValue ? pillCodex : "HUD/nav_breakthrough", "[Đan]");
                    tile.Add(pIco);
                    tile.Add(new Label(opt.HasValue ? $"{GetShortPillQuality(opt.Value)} (x{cnt})" : "Vượt Chay") { pickingMode = PickingMode.Ignore, style = { fontSize = 9, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 2, flexShrink = 0 } });
                    tile.Add(new Label($"Tỉ lệ {chPct}%") { pickingMode = PickingMode.Ignore, style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.11f, 0.48f, 0.36f), flexShrink = 0 } });
                    optGrid.Add(tile);
                }
                gridCard.Add(optGrid);
                body.Add(gridCard);
            }

            float finalChance = isMajor ? engine.ComputeBreakthroughChance(selectedPill) : 1.0f;
            var ctaCard = new VisualElement();
            ctaCard.AddToClassList("card-light");
            ctaCard.style.alignItems = Align.Center;
            ctaCard.Add(new Label($"Tổng xác suất Độ Kiếp: {Mathf.RoundToInt(finalChance * 100)}%")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13, color = new Color(0.11f, 0.48f, 0.36f), marginBottom = 6 }
            });

            var startBtn = new Button(() =>
            {
                if (engine.TryStartTribulation(selectedPill))
                    onTribulationStarted?.Invoke();
            })
            {
                name = "btn-start-tribulation",
                text = engine.IsInTribulation ? "Đang Bế Quan Đón Thiên Kiếp..." : "Bế Quan Đón Thiên Kiếp (15s)"
            };
            startBtn.AddToClassList("btn-primary");
            startBtn.SetEnabled(engine.IsBreakthroughReady && !engine.IsInTribulation);
            ctaCard.Add(startBtn);
            body.Add(ctaCard);
        }

        // =========================================================
        // 9. NHAT KY TIEN LO — 2-COLUMN GRID
        // =========================================================
        public static void BuildPioneerDiary(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            var top = new VisualElement();
            top.AddToClassList("card-light");
            top.style.marginBottom = 8;
            top.Add(new Label("Lộ Trình Chuẩn Thương Mại: 30 Ngày 24/7 (720 Giờ) → Đại Thừa") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            top.Add(new Label($"Cảnh giới: {CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)} · Tiến độ ải: {save.highestUnlockedLinearStage + 1}/120") { style = { fontSize = 11, marginTop = 2 } });
            body.Add(top);

            var grid = new VisualElement();
            grid.AddToClassList("grid-wrap");
            grid.style.justifyContent = Justify.SpaceBetween;

            foreach (var m in RemakeCatalogs.PioneerMilestones)
            {
                var capM = m;
                bool claimed = (save.claimedPioneerMilestonesMask & (1 << m.index)) != 0;
                bool ready = engine.IsPioneerMilestoneReady(m);

                var card = new VisualElement
                {
                    style =
                    {
                        width = Length.Percent(48.5f),
                        marginLeft = 0, marginRight = 0, marginTop = 0, marginBottom = 6,
                        paddingTop = 7, paddingBottom = 7, paddingLeft = 7, paddingRight = 7,
                        justifyContent = Justify.SpaceBetween,
                        overflow = Overflow.Hidden
                    }
                };
                card.AddToClassList("grid-tile");
                card.style.alignItems = Align.Stretch;
                if (ready) card.AddToClassList("grid-tile-selected");

                card.Add(new Label(m.title) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, whiteSpace = WhiteSpace.Normal } });
                card.Add(new Label($"Đạt: {m.targetHint}") { style = { fontSize = 9, color = new Color(0.18f, 0.50f, 0.40f), whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
                card.Add(new Label($"Quà: {CultivationTable.FormatNumber(m.rewardLinhThach)} LT · +{m.rewardDaoPoints} ĐV · +{m.rewardForgeTalismans} Phù") { style = { fontSize = 9, whiteSpace = WhiteSpace.Normal, marginTop = 2, marginBottom = 5 } });

                var btn = new Button(() => { engine.TryClaimPioneerMilestone(capM.index); body.Clear(); BuildPioneerDiary(body); })
                { text = claimed ? "Đã Nhận" : ready ? "Nhận Quà" : "Chưa Đạt" };
                btn.AddToClassList(ready ? "btn-primary" : "btn-compact");
                btn.style.fontSize = 10; btn.style.paddingTop = 3; btn.style.paddingBottom = 3; btn.style.minWidth = 0;
                btn.SetEnabled(ready);
                card.Add(btn);
                grid.Add(card);
            }
            body.Add(grid);
        }

        // =========================================================
        // 10. SETTINGS, LEADERBOARD, WORLD CHAT, FRIENDS, IMMORTAL GATE
        // =========================================================
        public static void BuildSettings(VisualElement body)
        {
            var g = SaveService.Current.graphics;
            var svc = GraphicsSettingsService.Instance;

            var cardGfx = new VisualElement();
            cardGfx.AddToClassList("card-light");
            cardGfx.style.marginBottom = 8;
            cardGfx.Add(new Label("Cài Đặt Đồ Họa 3D & Tối Ưu Pin (WebGL / iPhone):") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 6 } });

            AddChoiceRow(cardGfx, "Mức tổng thể:", new[] { "Thấp", "Trung", "Cao" }, (int)g.preset, idx => svc?.ApplyPreset((RemakeGraphicsPreset)idx));
            AddChoiceRow(cardGfx, "Render Scale:", new[] { "70%", "85%", "100%" }, g.renderScaleIndex, idx => svc?.SetRenderScaleIndex(idx));
            AddChoiceRow(cardGfx, "Bóng đổ 3D:", new[] { "Tắt (Đĩa)", "Thấp", "Cao" }, (int)g.shadowMode, idx => svc?.SetShadowMode((RemakeShadowMode)idx));
            AddChoiceRow(cardGfx, "Mật độ VFX:", new[] { "Tối giản", "Đầy đủ" }, (int)g.vfxDensity, idx => svc?.SetVfxDensity((RemakeVfxDensity)idx));
            AddChoiceRow(cardGfx, "Giới hạn FPS:", new[] { "30 FPS", "60 FPS" }, g.targetFps <= 35 ? 0 : 1, idx => svc?.SetTargetFps(idx == 0 ? 30 : 60));
            AddChoiceRow(cardGfx, "Hiện bộ đếm FPS:", new[] { "Tắt", "Bật" }, g.showFpsCounter ? 1 : 0, idx => svc?.SetShowFpsCounter(idx == 1));
            AddChoiceRow(cardGfx, "Âm thanh SFX:", new[] { "Tắt", "Bật" }, g.soundEnabled ? 1 : 0, idx => svc?.SetAudioEnabled(idx == 1, g.musicEnabled));
            AddChoiceRow(cardGfx, "Nhạc nền:", new[] { "Tắt", "Bật" }, g.musicEnabled ? 1 : 0, idx => svc?.SetAudioEnabled(g.soundEnabled, idx == 1));
            body.Add(cardGfx);

            CodexArtLoader.GetCodexProgress(out int ready, out int total);
            var codexCard = new VisualElement();
            codexCard.AddToClassList("card-light");
            codexCard.Add(new Label($"Tiến Độ Hình Ảnh Codex: {ready}/{total} ảnh đã nhận (Thư mục: Assets/_Game/Resources/CodexDrop/)")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal }
            });
            codexCard.Add(new Label("Đã tích hợp bản bake seamless & 9-slice trong BakedMaps/ cùng 47 ảnh GameIcons tái sử dụng.")
            {
                style = { fontSize = 11, marginTop = 3, whiteSpace = WhiteSpace.Normal }
            });
            body.Add(codexCard);
        }

        public static void BuildLeaderboard(VisualElement body)
        {
            var svc = RemakeOnlineService.Instance;
            var statusCard = new VisualElement();
            statusCard.AddToClassList("card-light");
            statusCard.style.marginBottom = 8;
            statusCard.style.flexDirection = FlexDirection.Row;
            statusCard.style.justifyContent = Justify.SpaceBetween;
            statusCard.style.alignItems = Align.Center;
            statusCard.Add(new Label($"Trạng thái: {(svc != null ? svc.StatusText : "Ngoại tuyến")}") { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
            var btnRefresh = new Button(() => svc?.SyncAllOnlineData()) { text = svc != null && svc.IsConnected ? "Làm Mới" : "Kết Nối LAN" };
            btnRefresh.AddToClassList("btn-compact");
            statusCard.Add(btnRefresh);
            body.Add(statusCard);

            string[] ids = { "thien_kieu", "dan_tong", "tram_yeu" };
            string[] names = { "Thiên Kiêu (Cảnh Giới)", "Đan Tông (Đan Sư)", "Trảm Yêu (Ải/Quái)" };
            var tabs = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 8 } };
            for (int i = 0; i < 3; i++)
            {
                int cap = i;
                var b = new Button(() => { _uiLbTab = cap; svc?.RefreshLeaderboard(ids[cap]); body.Clear(); BuildLeaderboard(body); }) { text = names[i] };
                b.AddToClassList("btn-compact");
                b.EnableInClassList("selected", i == _uiLbTab);
                b.style.flexGrow = 1; b.style.marginRight = i < 2 ? 4 : 0; b.style.fontSize = 10;
                tabs.Add(b);
            }
            body.Add(tabs);

            var listCard = new VisualElement();
            listCard.AddToClassList("card-light");
            string boardId = ids[Mathf.Clamp(_uiLbTab, 0, 2)];
            if (svc == null || !svc.IsConnected || !svc.Leaderboards.TryGetValue(boardId, out var rows) || rows.Count == 0)
            {
                listCard.Add(new Label(svc != null && svc.IsConnected ? "Chưa có thông tin hạng — bấm Làm Mới để đồng bộ." : "Hãy chạy Tools/LocalServer/server.mjs để đồng bộ BXH LAN Nakama.")
                {
                    style = { fontSize = 11, whiteSpace = WhiteSpace.Normal }
                });
            }
            else
            {
                foreach (var r in rows)
                {
                    bool isMe = r.ownerId == svc.MyUserId;
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 4, paddingBottom = 4, borderBottomWidth = 1, borderBottomColor = new Color(0.86f, 0.89f, 0.86f) } };
                    row.Add(new Label($"#{r.rank} · {r.displayName}{(isMe ? " (Bạn)" : "")} [{r.realmMeta}]")
                    {
                        style = { fontSize = 11, unityFontStyleAndWeight = isMe ? FontStyle.Bold : FontStyle.Normal, color = isMe ? new Color(0.10f, 0.48f, 0.36f) : new Color(0.18f, 0.24f, 0.22f) }
                    });
                    string val = _uiLbTab == 0 ? $"Lực: {CultivationTable.FormatNumber(r.subscore)}" : _uiLbTab == 1 ? $"Đan XP: {r.score % 100000}" : $"Ải {(r.score / 1000000)} · {r.subscore} Quái";
                    row.Add(new Label(val) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });
                    listCard.Add(row);
                }
            }
            body.Add(listCard);
        }

        public static void BuildWorldChat(VisualElement body)
        {
            var svc = RemakeOnlineService.Instance;
            var statusCard = new VisualElement();
            statusCard.AddToClassList("card-light");
            statusCard.style.marginBottom = 8;
            statusCard.style.flexDirection = FlexDirection.Row;
            statusCard.style.justifyContent = Justify.SpaceBetween;
            statusCard.style.alignItems = Align.Center;
            statusCard.Add(new Label($"Kênh Thế Giới · {(svc != null ? svc.StatusText : "Ngoại tuyến")}") { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
            var btnConn = new Button(() => svc?.SyncAllOnlineData()) { text = "Đồng Bộ" };
            btnConn.AddToClassList("btn-compact");
            statusCard.Add(btnConn);
            body.Add(statusCard);

            var feed = new VisualElement();
            feed.AddToClassList("card-light");
            feed.style.minHeight = 200;
            feed.style.marginBottom = 8;
            if (svc == null || svc.WorldChat.Count == 0)
            {
                feed.Add(new Label("Chưa có tin nhắn thế giới. Mọi tin Đột Phá Đại Thừa, Cực Phẩm Đan Văn & Kiếm Lv.9–10 sẽ tự động thông báo tại đây!")
                {
                    style = { fontSize = 11, whiteSpace = WhiteSpace.Normal }
                });
            }
            else
            {
                int start = Mathf.Max(0, svc.WorldChat.Count - 25);
                for (int i = start; i < svc.WorldChat.Count; i++)
                {
                    var c = svc.WorldChat[i];
                    bool isAnnounce = c.kind == "announce";
                    var lbl = new Label(isAnnounce ? c.message : $"{c.daoName}: {c.message}")
                    {
                        style =
                        {
                            fontSize = 11, whiteSpace = WhiteSpace.Normal, marginBottom = 4,
                            color = isAnnounce ? new Color(0.68f, 0.40f, 0.04f) : new Color(0.16f, 0.24f, 0.22f),
                            unityFontStyleAndWeight = isAnnounce ? FontStyle.Bold : FontStyle.Normal
                        }
                    };
                    feed.Add(lbl);
                }
            }
            body.Add(feed);

            var inputCard = new VisualElement();
            inputCard.AddToClassList("card-light");
            inputCard.style.flexDirection = FlexDirection.Row;
            inputCard.style.alignItems = Align.Center;
            var tf = new TextField { value = "" };
            tf.style.flexGrow = 1; tf.style.height = 32; tf.style.marginRight = 6; tf.style.backgroundColor = Color.white;
            inputCard.Add(tf);

            var btnTelex = new Button(() =>
            {
                BootOverlayController.Instance?.PromptTextInput("Chat Thế Giới (Telex iOS / Web)", tf.value, 160, txt => svc?.SendWorldChat(txt));
            }) { text = "Telex" };
            btnTelex.AddToClassList("btn-compact");
            btnTelex.style.marginRight = 4;
            inputCard.Add(btnTelex);

            var btnSend = new Button(() => { svc?.SendWorldChat(tf.value); tf.value = ""; }) { text = "Gửi" };
            btnSend.AddToClassList("btn-primary");
            btnSend.style.minWidth = 72; btnSend.style.paddingLeft = 14; btnSend.style.paddingRight = 14;
            inputCard.Add(btnSend);
            body.Add(inputCard);
        }

        public static void BuildFriends(VisualElement body)
        {
            var svc = RemakeOnlineService.Instance;
            var save = SaveService.Current;

            var meCard = new VisualElement();
            meCard.AddToClassList("card-light");
            meCard.style.marginBottom = 8;
            meCard.Add(new Label($"Mã Tài Khoản LAN: {(svc != null && !string.IsNullOrEmpty(svc.MyUsername) ? svc.MyUsername : "(Chưa kết nối)")}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
            meCard.Add(new Label($"Hộ Pháp đang chờ hỗ trợ đột phá: +{save.hoPhapBonusSteps}/3% · Dùng Truyền Công +5% Tu Vi 1 lần/ngày.") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            var addRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 6 } };
            var tfFriend = new TextField { value = "" };
            tfFriend.style.flexGrow = 1; tfFriend.style.height = 30; tfFriend.style.marginRight = 6; tfFriend.style.backgroundColor = Color.white;
            addRow.Add(tfFriend);
            var btnAdd = new Button(() => svc?.AddFriendByUsernameOrId(tfFriend.value)) { text = "Kết Bạn" };
            btnAdd.AddToClassList("btn-compact");
            addRow.Add(btnAdd);
            meCard.Add(addRow);
            body.Add(meCard);

            var listCard = new VisualElement();
            listCard.AddToClassList("card-light");
            listCard.style.marginBottom = 8;
            listCard.Add(new Label($"Danh Sách Đạo Hữu ({(svc != null ? svc.Friends.Count : 0)}):") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 } });

            if (svc == null || svc.Friends.Count == 0)
            {
                listCard.Add(new Label("Chưa có bạn bè nào. Nhập Username bạn cùng mạng LAN để kết bạn, Truyền Công, Hộ Pháp hoặc Nhắn Riêng!")
                {
                    style = { fontSize = 11, whiteSpace = WhiteSpace.Normal }
                });
            }
            else
            {
                foreach (var f in svc.Friends)
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginBottom = 6, paddingBottom = 5, borderBottomWidth = 1, borderBottomColor = new Color(0.86f, 0.89f, 0.86f) } };
                    string stLbl = f.state == 0 ? (f.online ? "Online" : "Offline") : f.state == 2 ? "Chờ chấp nhận" : "Đã gửi lời mời";
                    row.Add(new Label($"{f.displayName} ({stLbl})") { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });

                    var acts = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    if (f.state == 2)
                    {
                        var btnAcc = new Button(() => svc.AddFriendByUsernameOrId(f.userId)) { text = "Chấp Nhận" };
                        btnAcc.AddToClassList("btn-compact");
                        acts.Add(btnAcc);
                    }
                    else if (f.state == 0)
                    {
                        var bTc = new Button(() => svc.TryTruyenCongWithFriend(f.displayName)) { text = "Truyền Công" };
                        var bHp = new Button(() => svc.SendHoPhapAssist(f.userId, f.displayName)) { text = "Hộ Pháp" };
                        var bDm = new Button(() => svc.OpenDmConversation(f.userId)) { text = "Nhắn" };
                        foreach (var b in new[] { bTc, bHp, bDm }) { b.AddToClassList("btn-compact"); b.style.marginLeft = 3; b.style.fontSize = 9; acts.Add(b); }
                    }
                    row.Add(acts);
                    listCard.Add(row);
                }
            }
            body.Add(listCard);

            if (svc != null && !string.IsNullOrEmpty(svc.ActiveDmFriendId))
            {
                var dmCard = new VisualElement();
                dmCard.AddToClassList("card-light");
                dmCard.Add(new Label("Nhắn Riêng Đạo Hữu:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 } });
                foreach (var dm in svc.ActiveDmHistory)
                    dmCard.Add(new Label($"{dm.fromName}: {dm.message}") { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginBottom = 2 } });

                var dmRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 6 } };
                var tfDm = new TextField { value = "" };
                tfDm.style.flexGrow = 1; tfDm.style.height = 30; tfDm.style.marginRight = 6; tfDm.style.backgroundColor = Color.white;
                dmRow.Add(tfDm);
                var btnDmSend = new Button(() => { svc.SendDm(svc.ActiveDmFriendId, tfDm.value); tfDm.value = ""; }) { text = "Gửi DM" };
                btnDmSend.AddToClassList("btn-compact");
                dmRow.Add(btnDmSend);
                dmCard.Add(dmRow);
                body.Add(dmCard);
            }
        }

        public static void BuildImmortalGate(VisualElement body, Action onOpenPvpPreview)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            engine.EnsureDailyChallengeReset();

            void Refresh()
            {
                body.Clear();
                BuildImmortalGate(body, onOpenPvpPreview);
            }

            bool towerOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, save.realmIndex, save.subStage);
            int nextFloor = Mathf.Clamp(save.demonTowerHighestFloor + 1, 1, 100);
            var tSpec = CultivationTable.GetDemonTowerFloorSpec(nextFloor);

            var towerCard = new VisualElement();
            towerCard.AddToClassList("stage-hero-box");
            towerCard.style.alignItems = Align.Stretch;
            var tTop = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var tArt = new VisualElement { style = { width = 72, height = 72, flexShrink = 0 } };
            SetBorderAndRadius(tArt, new Color(0.84f, 0.60f, 0.16f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(tArt, "Stages/mode_demon_tower", "[Vạn Yêu Tháp]");
            tTop.Add(tArt);
            var tMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10, flexShrink = 1 } };
            tMeta.Add(new Label($"Vạn Yêu Tháp · Tầng {save.demonTowerHighestFloor}/100") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } });
            tMeta.Add(new Label(towerOpen
                ? $"Thử thách Tầng {nextFloor} (45s) · Thưởng +{CultivationTable.FormatNumber(tSpec.rewardLt)} LT & Đá Tẩy Luyện"
                : $"Mở khóa tại {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.ImmortalTower)}")
            {
                style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2, color = towerOpen ? new Color(0.20f, 0.28f, 0.24f) : new Color(0.78f, 0.25f, 0.14f) }
            });
            tTop.Add(tMeta);
            towerCard.Add(tTop);

            var tBtns = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 7 } };
            var salBtn = new Button(() => { engine.TryClaimTowerDailySalary(); Refresh(); }) { text = "Nhận Bổng Lộc Ngày" };
            salBtn.AddToClassList("btn-secondary");
            salBtn.SetEnabled(towerOpen && save.demonTowerHighestFloor > 0);
            salBtn.style.marginRight = 6;
            tBtns.Add(salBtn);
            var enterTowerBtn = new Button(() => engine.TryStartDemonTowerFloor(nextFloor)) { text = $"Khiêu Chiến Tầng {nextFloor}" };
            enterTowerBtn.AddToClassList("btn-primary");
            enterTowerBtn.SetEnabled(towerOpen);
            tBtns.Add(enterTowerBtn);
            towerCard.Add(tBtns);
            body.Add(towerCard);

            bool realmOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, save.realmIndex, save.subStage);
            var kind = (RemakeSecretRealmKind)_uiSecretRealmKind;
            var rSpec = CultivationTable.GetSecretRealmTierSpec(kind, _uiSecretRealmTier);
            int clearedTier = save.secretRealmClearedTier[_uiSecretRealmKind];

            var realmCard = new VisualElement();
            realmCard.AddToClassList("stage-hero-box");
            realmCard.style.alignItems = Align.Stretch;
            var rTop = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var rArt = new VisualElement { style = { width = 72, height = 72, flexShrink = 0 } };
            SetBorderAndRadius(rArt, new Color(0.18f, 0.56f, 0.44f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(rArt, "Stages/mode_secret_realm", "[Thái Hư Bí Cảnh]");
            rTop.Add(rArt);
            var rMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10, flexShrink = 1 } };
            rMeta.Add(new Label($"Thái Hư Bí Cảnh · Lượt hôm nay: {save.secretRealmDailyRemaining}/3") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } });
            rMeta.Add(new Label(realmOpen
                ? $"{rSpec.title} · Chọn Cổng & Độ Khó T1–T5 (Càn Quét khi đã vượt)"
                : $"Mở khóa tại {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.SecretRealm)}")
            {
                style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2, color = realmOpen ? new Color(0.20f, 0.28f, 0.24f) : new Color(0.78f, 0.25f, 0.14f) }
            });
            rTop.Add(rMeta);
            realmCard.Add(rTop);

            var gateRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 6 } };
            string[] gateNames = { "Kiếm Mộ Cổ", "Bách Thảo Viên", "Tinh Thiết Động" };
            for (int g = 0; g < 3; g++)
            {
                int capG = g;
                var gb = new Button(() => { _uiSecretRealmKind = capG; Refresh(); }) { text = gateNames[g] };
                gb.AddToClassList("btn-compact");
                if (_uiSecretRealmKind == g) gb.AddToClassList("selected");
                gb.style.flexGrow = 1; gb.style.marginRight = g < 2 ? 4 : 0; gb.style.fontSize = 10;
                gateRow.Add(gb);
            }
            realmCard.Add(gateRow);

            var tierRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 5 } };
            for (int t = 1; t <= 5; t++)
            {
                int capT = t;
                var tb = new Button(() => { _uiSecretRealmTier = capT; Refresh(); }) { text = t <= clearedTier ? $"T{t}*" : $"T{t}" };
                tb.AddToClassList("btn-compact");
                if (_uiSecretRealmTier == t) tb.AddToClassList("selected");
                tb.style.flexGrow = 1; tb.style.marginRight = t < 5 ? 4 : 0; tb.style.fontSize = 10;
                tierRow.Add(tb);
            }
            realmCard.Add(tierRow);

            var rBtns = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 7 } };
            var sweepBtn = new Button(() => { engine.TrySweepSecretRealm(kind, _uiSecretRealmTier); Refresh(); }) { text = "Càn Quét Nhanh" };
            sweepBtn.AddToClassList("btn-secondary");
            sweepBtn.SetEnabled(realmOpen && save.secretRealmDailyRemaining > 0 && clearedTier >= _uiSecretRealmTier);
            sweepBtn.style.marginRight = 6;
            rBtns.Add(sweepBtn);
            var enterBtn = new Button(() => engine.TryEnterSecretRealm(kind, _uiSecretRealmTier)) { text = "Vào Bí Cảnh" };
            enterBtn.AddToClassList("btn-primary");
            enterBtn.SetEnabled(realmOpen && save.secretRealmDailyRemaining > 0);
            rBtns.Add(enterBtn);
            realmCard.Add(rBtns);
            body.Add(realmCard);

            var pvpCard = new VisualElement();
            pvpCard.AddToClassList("card-light");
            pvpCard.style.flexDirection = FlexDirection.Row;
            pvpCard.style.alignItems = Align.Center;
            var pArt = new VisualElement { style = { width = 62, height = 62, flexShrink = 0 } };
            SetBorderAndRadius(pArt, new Color(0.84f, 0.56f, 0.14f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(pArt, "Stages/mode_pvp_arena", "[Luận Kiếm Đài]");
            pvpCard.Add(pArt);

            var pMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10, flexShrink = 1 } };
            var pTitleRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, alignItems = Align.Center } };
            pTitleRow.Add(new Label("Luận Kiếm Đài · Liên Đấu") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, marginRight = 6 } });
            var csBadge = new Label("COMING SOON");
            csBadge.AddToClassList("coming-soon-badge");
            pTitleRow.Add(csBadge);
            pMeta.Add(pTitleRow);
            pMeta.Add(new Label("Dự kiến Nguyên Anh T5 · So tài Phi Kiếm Bất Đồng Bộ qua Nakama.") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            pvpCard.Add(pMeta);

            var pvBtn = new Button(() => onOpenPvpPreview?.Invoke()) { text = "Xem Trước" };
            pvBtn.AddToClassList("btn-compact");
            pvpCard.Add(pvBtn);
            body.Add(pvpCard);
        }

        public static void BuildKyNgo(VisualElement body, Action onClose)
        {
            var engine = GameplayEngine.Instance;
            int herbTier = engine.GetCurrentStageSpec().herbTier;
            double ltReward = Math.Round(engine.GetCurrentStageSpec().linhThachPerKill * 18);

            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            var orb = new VisualElement { style = { width = 82, height = 82 } };
            CodexArtLoader.ApplyIconOrPlaceholder(orb, "Stages/event_fortuitous_orb", "[Kỳ Ngộ]");
            hero.Add(orb);
            hero.Add(new Label($"Cơ Duyên Linh Cơ Xuất Hiện ({Mathf.CeilToInt(engine.KyNgoTimeRemaining)}s)") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13, marginTop = 6 } });
            hero.Add(new Label("Chọn một trong hai cơ duyên trước khi Linh Cơ tiêu tán:") { style = { fontSize = 11, marginTop = 2 } });
            body.Add(hero);

            var c0 = new VisualElement();
            c0.AddToClassList("card-light");
            c0.style.marginBottom = 8;
            c0.Add(new Label("1. Linh Tuyền Khai Ngộ") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            c0.Add(new Label($"Nhân Đôi Tu Vi (x2 Tu Vi) trong 90 giây + {CultivationTable.FormatNumber(ltReward)} Linh Thạch.") { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 3, marginBottom = 6 } });
            var b0 = new Button(() => { engine.TryClaimKyNgoChoice(0); onClose?.Invoke(); }) { text = "Hấp Thu Linh Tuyền" };
            b0.AddToClassList("btn-primary");
            c0.Add(b0);
            body.Add(c0);

            var c1 = new VisualElement();
            c1.AddToClassList("card-light");
            c1.Add(new Label("2. Tàng Bảo Động Phủ") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            c1.Add(new Label($"Nhận +4 {ActStageDatabaseSO.GetHerbName(herbTier)} + 1 Phù Rèn Kiếm + {CultivationTable.FormatNumber(ltReward)} Linh Thạch.") { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 3, marginBottom = 6 } });
            var b1 = new Button(() => { engine.TryClaimKyNgoChoice(1); onClose?.Invoke(); }) { text = "Thu Nhận Cơ Duyên" };
            b1.AddToClassList("btn-secondary");
            c1.Add(b1);
            body.Add(c1);
        }

        public static void BuildPvpPreview(VisualElement body)
        {
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            var art = new VisualElement { style = { width = 120, height = 120 } };
            SetBorderAndRadius(art, new Color(0.84f, 0.60f, 0.16f), 2f, 14f);
            CodexArtLoader.ApplyIconOrPlaceholder(art, "Stages/mode_pvp_arena", "[Luận Kiếm Đài]");
            hero.Add(art);
            var badge = new Label("COMING SOON · DỰ KIẾN NGUYÊN ANH T5") { style = { marginTop = 8 } };
            badge.AddToClassList("coming-soon-badge");
            hero.Add(badge);
            hero.Add(new Label("Luận Kiếm Đài — Liên Đấu Phi Kiếm") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginTop = 6 } });
            body.Add(hero);

            string[] bullets =
            {
                "• So tài Bất Đồng Bộ 24/7 theo đội hình Phi Kiếm & Trận Pháp lưu trên máy chủ Nakama.",
                "• Quy đổi Điểm Kiếm Vọng lấy Phù Rèn, Đá Tẩy Luyện & Khung Avatar Danh Hiệu.",
                "• Xếp hạng Mùa Tuần toàn máy chủ LAN / Online — sẽ mở ở bản cập nhật kế tiếp!"
            };
            foreach (var bText in bullets)
            {
                var c = new VisualElement();
                c.AddToClassList("card-light");
                c.style.marginBottom = 6;
                c.Add(new Label(bText) { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal } });
                body.Add(c);
            }
        }

        private static void SetBorderAndRadius(VisualElement ve, Color c, float w, float r)
        {
            ve.style.borderTopWidth = w; ve.style.borderRightWidth = w;
            ve.style.borderBottomWidth = w; ve.style.borderLeftWidth = w;
            ve.style.borderTopColor = c; ve.style.borderRightColor = c;
            ve.style.borderBottomColor = c; ve.style.borderLeftColor = c;
            ve.style.borderTopLeftRadius = r; ve.style.borderTopRightRadius = r;
            ve.style.borderBottomLeftRadius = r; ve.style.borderBottomRightRadius = r;
        }

        private static void AddChoiceRow(VisualElement parent, string title, string[] labels, int selectedIdx, Action<int> onPick)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginBottom = 6 } };
            row.Add(new Label(title) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, width = 104 } });
            var btnGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, flexGrow = 1, justifyContent = Justify.FlexEnd } };
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                var b = new Button(() => onPick?.Invoke(idx)) { text = labels[i] };
                b.AddToClassList("btn-compact");
                if (i == selectedIdx) b.AddToClassList("selected");
                b.style.marginLeft = 4; b.style.marginBottom = 2;
                b.style.fontSize = 10;
                btnGroup.Add(b);
            }
            row.Add(btnGroup);
            parent.Add(row);
        }
    }
}
