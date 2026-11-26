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
        private static bool _useForgeTalisman = false;
        private static string _selectedDaoVanId = "core_daotam";
        private static string _selectedGearUid = "";
        private static int _uiSecretRealmKind = 0;
        private static int _uiSecretRealmTier = 1;

        public static void BuildSwordPath(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            void Refresh()
            {
                body.Clear();
                BuildSwordPath(body);
            }

            int[] counts = new int[11];
            for (int i = 0; i < save.equippedSwordLevels.Count; i++)
                counts[Mathf.Clamp(save.equippedSwordLevels[i], 1, 10)]++;
            for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                counts[Mathf.Clamp(save.swordInventoryLevels[i], 1, 10)]++;

            int lowestMergeLv = -1;
            bool hasQuickMerge = false;
            for (int lv = 1; lv < 10; lv++)
            {
                if (counts[lv] >= 2)
                {
                    if (lowestMergeLv < 0) lowestMergeLv = lv;
                    if (lv <= 3) hasQuickMerge = true;
                }
            }

            // HERO V2 FORGE STAGE
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");

            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, width = Length.Percent(100) } };
            var stageArt = new VisualElement { style = { width = 92, height = 92, flexShrink = 0 } };
            SetBorderAndRadius(stageArt, new Color(0.84f, 0.60f, 0.16f, 0.72f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(stageArt, "Stages/stage_sword_forge", "[Kiếm Đài]");
            topRow.Add(stageArt);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
            info.Add(new Label("Cổ Kiếm Trủng · Đúc & Hợp Nhất Phi Kiếm") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } });
            info.Add(new Label($"Tổng Công Phi Kiếm: {CultivationTable.FormatNumber(engine.GetTotalSwordAttackPower())} · Kiếm Mặc: {save.equippedSwordLevels.Count}/{CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex)}") { style = { fontSize = 11, marginTop = 3 } });
            info.Add(new Label($"Phù Rèn (+10% tỉ lệ): {save.forgeTalismanCount} · Tổng lượt ghép: {save.totalMerges}") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f), marginTop = 2 } });
            topRow.Add(info);
            hero.Add(topRow);

            if (lowestMergeLv > 0)
            {
                float pity = save.swordPityByLevel[lowestMergeLv - 1];
                float daoFortune = engine.GetDaoVanLevel("wealth_fortune") * 0.02f;
                float taliBonus = (_useForgeTalisman && save.forgeTalismanCount > 0) ? 0.10f : 0f;
                bool isTutorialFree = !save.tutorialCompleted && save.totalMerges == 0 && lowestMergeLv == 1;
                float effChance = isTutorialFree ? 1.0f : Mathf.Clamp01(CultivationTable.GetSwordMergeEffectiveChance(lowestMergeLv, pity + daoFortune + taliBonus));
                double cost = isTutorialFree ? 0 : CultivationTable.GetSwordMergeCost(lowestMergeLv);

                var slotPair = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.Center, marginTop = 8 } };
                var leftIcon = new VisualElement { style = { width = 38, height = 38, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lowestMergeLv)) } };
                SetBorderAndRadius(leftIcon, new Color(0.75f, 0.65f, 0.45f), 1.5f, 8f);
                var rightIcon = new VisualElement { style = { width = 44, height = 44, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lowestMergeLv + 1)) } };
                SetBorderAndRadius(rightIcon, new Color(0.88f, 0.62f, 0.12f), 2f, 8f);
                slotPair.Add(leftIcon);
                slotPair.Add(new Label($" + Lv.{lowestMergeLv}  →  ") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, marginLeft = 4, marginRight = 4 } });
                slotPair.Add(rightIcon);
                slotPair.Add(new Label($" Lv.{lowestMergeLv + 1}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, color = new Color(0.11f, 0.48f, 0.36f), marginLeft = 4 } });
                hero.Add(slotPair);

                var pityTrack = new VisualElement { style = { width = Length.Percent(100), marginTop = 6 } };
                pityTrack.AddToClassList("bar-track");
                var pityFill = new VisualElement { style = { width = Length.Percent(Mathf.Clamp01(pity) * 100f) } };
                pityFill.AddToClassList("bar-fill-tuvi");
                pityTrack.Add(pityFill);
                hero.Add(pityTrack);

                hero.Add(new Label($"Tỉ lệ: {Mathf.RoundToInt(effChance * 100)}% (Chúc Phúc +{Mathf.RoundToInt(pity * 100)}%) · Phí: {(isTutorialFree ? "Miễn Phí" : CultivationTable.FormatNumber(cost) + " LT")}")
                {
                    style = { fontSize = 10, marginTop = 3, unityTextAlign = TextAnchor.MiddleCenter }
                });

                var actionRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center, marginTop = 6 } };
                var mainMergeBtn = new Button(() =>
                {
                    engine.TryMergeSwords(lowestMergeLv, _useForgeTalisman);
                    Refresh();
                })
                {
                    name = $"btn-sword-merge-{lowestMergeLv}",
                    text = $"Ghép Lv.{lowestMergeLv} → Lv.{lowestMergeLv + 1}"
                };
                mainMergeBtn.AddToClassList("btn-primary");
                mainMergeBtn.style.marginRight = 5;
                mainMergeBtn.style.marginBottom = 4;
                actionRow.Add(mainMergeBtn);

                var quickBtn = new Button(() =>
                {
                    engine.TryQuickMergeSwords(3);
                    Refresh();
                })
                {
                    text = "Ghép Nhanh Lv.1–3"
                };
                quickBtn.AddToClassList("btn-secondary");
                quickBtn.SetEnabled(hasQuickMerge);
                quickBtn.style.marginRight = 5;
                quickBtn.style.marginBottom = 4;
                actionRow.Add(quickBtn);

                var taliBtn = new Button(() =>
                {
                    _useForgeTalisman = !_useForgeTalisman;
                    Refresh();
                })
                {
                    text = _useForgeTalisman ? $"Phù Rèn: BẬT ({save.forgeTalismanCount})" : $"Dùng Phù Rèn ({save.forgeTalismanCount})"
                };
                taliBtn.AddToClassList("btn-compact");
                if (_useForgeTalisman) taliBtn.AddToClassList("selected");
                taliBtn.style.marginBottom = 4;
                actionRow.Add(taliBtn);
                hero.Add(actionRow);
            }
            else
            {
                hero.Add(new Label("Chưa đủ cặp Phi Kiếm cùng cấp để ghép. Treo máy hoặc vào Kiếm Mộ Cổ để săn thêm phôi kiếm!")
                {
                    style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 6, color = new Color(0.32f, 0.42f, 0.37f) }
                });
            }
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

            // EQUIPPED SWORDS
            var eqCard = new VisualElement();
            eqCard.AddToClassList("card-light");
            eqCard.style.marginBottom = 8;
            eqCard.Add(new Label($"Phi Kiếm Đang Xuất Trận ({save.equippedSwordLevels.Count}/{CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex)})") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, marginBottom = 6 } });
            var eqRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            for (int i = 0; i < save.equippedSwordLevels.Count; i++)
            {
                int lv = save.equippedSwordLevels[i];
                var slot = new VisualElement { style = { width = 58, alignItems = Align.Center, paddingTop = 4, paddingBottom = 4, paddingLeft = 4, paddingRight = 4, marginRight = 5, marginBottom = 5, backgroundColor = new Color(0.98f, 0.96f, 0.90f) } };
                SetBorderAndRadius(slot, new Color(0.84f, 0.62f, 0.16f), 1.5f, 8f);
                var icon = new VisualElement { style = { width = 30, height = 30, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lv)) } };
                slot.Add(icon);
                slot.Add(new Label($"Lv.{lv}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, marginTop = 2 } });
                slot.Add(new Label(CultivationTable.FormatNumber(CultivationTable.CalculateSwordDamage(lv))) { style = { fontSize = 9, color = new Color(0.16f, 0.56f, 0.44f) } });
                eqRow.Add(slot);
            }
            eqCard.Add(eqRow);
            body.Add(eqCard);

            // BAG SWORDS
            var bagCard = new VisualElement();
            bagCard.AddToClassList("card-light");
            bagCard.style.marginBottom = 8;
            bagCard.Add(new Label($"Túi Phôi Kiếm ({save.swordInventoryLevels.Count}) — Bấm Để Trang Bị") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, marginBottom = 6 } });
            if (save.swordInventoryLevels.Count == 0)
            {
                bagCard.Add(new Label("Túi phôi kiếm đang trống.") { style = { fontSize = 11, color = new Color(0.45f, 0.50f, 0.47f) } });
            }
            else
            {
                var bagGrid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
                for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                {
                    int idx = i;
                    int lv = save.swordInventoryLevels[i];
                    var b = new Button(() => { engine.TryEquipFromBag(idx); Refresh(); })
                    {
                        style = { width = 54, height = 56, alignItems = Align.Center, justifyContent = Justify.Center, marginRight = 4, marginBottom = 4, paddingTop = 3, paddingBottom = 3, paddingLeft = 3, paddingRight = 3 }
                    };
                    b.AddToClassList("btn-secondary");
                    b.Add(new VisualElement { style = { width = 26, height = 26, backgroundImage = new StyleBackground(CodexArtLoader.LoadReusedSwordTexture(lv)) } });
                    b.Add(new Label($"Lv.{lv}") { style = { fontSize = 10, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
                    bagGrid.Add(b);
                }
                bagCard.Add(bagGrid);
            }
            body.Add(bagCard);

            // MERGE PAIRS LIST (ALL LEVELS)
            for (int lv = 1; lv < 10; lv++)
            {
                if (counts[lv] < 2) continue;
                int capLv = lv;
                float pity = save.swordPityByLevel[capLv - 1];
                float chance = Mathf.Clamp01(CultivationTable.GetSwordMergeEffectiveChance(capLv, pity + engine.GetDaoVanLevel("wealth_fortune") * 0.02f + ((_useForgeTalisman && save.forgeTalismanCount > 0) ? 0.10f : 0f)));
                double cost = CultivationTable.GetSwordMergeCost(capLv);

                var row = new VisualElement();
                row.AddToClassList("card-light");
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 5;

                var left = new VisualElement();
                left.Add(new Label($"Ghép Cặp Lv.{capLv} ({counts[capLv]} thanh) → Lv.{capLv + 1}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
                left.Add(new Label($"Tỉ lệ: {Mathf.RoundToInt(chance * 100)}% (Pity {Mathf.RoundToInt(pity * 100)}%) · {CultivationTable.FormatNumber(cost)} LT") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f) } });
                row.Add(left);

                var mBtn = new Button(() => { engine.TryMergeSwords(capLv, _useForgeTalisman); Refresh(); }) { text = "Ghép" };
                if (capLv != lowestMergeLv) mBtn.name = $"btn-sword-merge-{capLv}";
                mBtn.AddToClassList("btn-primary");
                row.Add(mBtn);
                body.Add(row);
            }
        }

        public static void BuildDaoVanTree(VisualElement body, Vector2 pan, float zoom, Action<Vector2, float> onViewportChanged)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            bool treeOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage);

            var topBar = new VisualElement();
            topBar.AddToClassList("card-light");
            topBar.style.marginBottom = 6;
            topBar.Add(new Label($"Điểm Đạo Vận: {save.daoPoints} · Linh Thạch: {CultivationTable.FormatNumber(save.linhThach)}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            if (!treeOpen)
                topBar.Add(new Label($"Khóa: Cần {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.DaoVanTree)} để khai mở cây Đạo Vận.") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginTop = 2 } });
            body.Add(topBar);

            // 5 BRANCH QUICK FOCUS CHIPS
            var branchChips = new (string label, string nodeId)[]
            {
                ("Đạo Tâm", "core_daotam"),
                ("Kiếm Đạo", "branch_sword"),
                ("Thể Phách", "branch_body"),
                ("Tu Luyện", "branch_cultivation"),
                ("Tài Lộc", "branch_wealth"),
                ("Bách Nghệ", "branch_arts")
            };
            var chipRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6 } };
            foreach (var chip in branchChips)
            {
                var c = chip;
                var nodeSpec = FindDaoVanSpec(c.nodeId);
                var b = new Button(() =>
                {
                    _selectedDaoVanId = c.nodeId;
                    Vector2 targetPan = nodeSpec != null ? -nodeSpec.normalizedPos * Mathf.Max(0.82f, zoom) : Vector2.zero;
                    onViewportChanged?.Invoke(targetPan, Mathf.Max(0.82f, zoom));
                })
                { text = c.label };
                b.AddToClassList("btn-compact");
                if (_selectedDaoVanId == c.nodeId) b.AddToClassList("selected");
                b.style.marginRight = 4; b.style.marginBottom = 4; b.style.fontSize = 10;
                chipRow.Add(b);
            }
            body.Add(chipRow);

            // ZOOM CONTROLS
            var zoomRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 6 } };
            zoomRow.Add(new Label("Chạm Node để xem · Chạm lại để Nâng Cấp") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f) } });
            var zBtns = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var bOut = new Button(() => onViewportChanged?.Invoke(pan, Mathf.Max(0.65f, zoom - 0.14f))) { text = "-" };
            var bReset = new Button(() => { _selectedDaoVanId = "core_daotam"; onViewportChanged?.Invoke(Vector2.zero, 0.78f); }) { text = "Tâm" };
            var bIn = new Button(() => onViewportChanged?.Invoke(pan, Mathf.Min(1.45f, zoom + 0.14f))) { text = "+" };
            bOut.AddToClassList("btn-compact"); bReset.AddToClassList("btn-compact"); bIn.AddToClassList("btn-compact");
            bOut.style.marginRight = 4; bReset.style.marginRight = 4;
            zBtns.Add(bOut); zBtns.Add(bReset); zBtns.Add(bIn);
            zoomRow.Add(zBtns);
            body.Add(zoomRow);

            // CONSTELLATION CANVAS
            var canvas = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100),
                    height = 268,
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
            graph.Bind(RemakeCatalogs.StarterDaoVanNodes, engine.GetDaoVanLevel, pan, zoom);
            canvas.Add(graph);

            var anchor = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style = { position = Position.Absolute, left = Length.Percent(50), top = Length.Percent(50), width = 0, height = 0, overflow = Overflow.Visible }
            };
            canvas.Add(anchor);

            foreach (var n in RemakeCatalogs.StarterDaoVanNodes)
            {
                var capNode = n;
                int lv = engine.GetDaoVanLevel(n.id);
                bool selected = _selectedDaoVanId == n.id;
                bool unlocked = lv > 0;
                bool canUp = engine.CanUnlockOrUpgradeNode(n);

                var nodeBtn = new Button(() =>
                {
                    if (_selectedDaoVanId != capNode.id)
                    {
                        _selectedDaoVanId = capNode.id;
                        body.Clear();
                        BuildDaoVanTree(body, pan, zoom, onViewportChanged);
                    }
                    else if (engine.TryUpgradeDaoVanNode(capNode.id))
                    {
                        body.Clear();
                        BuildDaoVanTree(body, pan, zoom, onViewportChanged);
                    }
                })
                {
                    style =
                    {
                        position = Position.Absolute,
                        left = pan.x + n.normalizedPos.x * zoom - 21f,
                        top = pan.y + n.normalizedPos.y * zoom - 21f,
                        width = 42,
                        height = 42,
                        paddingTop = 2,
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                        backgroundColor = unlocked ? new Color(1f, 0.97f, 0.88f) : new Color(0.92f, 0.94f, 0.93f)
                    }
                };
                Color border = selected ? new Color(0.90f, 0.62f, 0.10f) : unlocked ? new Color(0.18f, 0.58f, 0.46f) : canUp ? new Color(0.84f, 0.68f, 0.24f) : new Color(0.68f, 0.72f, 0.70f);
                SetBorderAndRadius(nodeBtn, border, selected ? 3f : 1.5f, 21f);

                var ico = new VisualElement { pickingMode = PickingMode.Ignore, style = { width = 22, height = 22 } };
                CodexArtLoader.ApplyIconOrPlaceholder(ico, n.codexIconKey, "", n.reusedIconKey);
                nodeBtn.Add(ico);
                nodeBtn.Add(new Label($"{lv}/{n.maxLevel}") { pickingMode = PickingMode.Ignore, style = { fontSize = 8, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.12f, 0.16f, 0.14f) } });
                anchor.Add(nodeBtn);
            }
            body.Add(canvas);

            // COMPACT SINGLE SELECTED-NODE INSPECTOR (NO REPEATED LONG LIST!)
            var sel = FindDaoVanSpec(_selectedDaoVanId) ?? RemakeCatalogs.StarterDaoVanNodes[0];
            int selLv = engine.GetDaoVanLevel(sel.id);
            double selCost = engine.GetDaoVanUpgradeCost(sel, selLv);
            bool selMax = selLv >= sel.maxLevel;
            bool parentOk = string.IsNullOrEmpty(sel.parentId) || engine.GetDaoVanLevel(sel.parentId) > 0;
            bool selCan = engine.CanUnlockOrUpgradeNode(sel);

            var detail = new VisualElement();
            detail.AddToClassList("stage-hero-box");
            detail.style.alignItems = Align.Stretch;
            var dHead = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var dIcon = new VisualElement { style = { width = 40, height = 40, flexShrink = 0 } };
            SetBorderAndRadius(dIcon, new Color(0.84f, 0.60f, 0.16f), 1.5f, 20f);
            CodexArtLoader.ApplyIconOrPlaceholder(dIcon, sel.codexIconKey, "", sel.reusedIconKey);
            dHead.Add(dIcon);

            var dMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 8 } };
            dMeta.Add(new Label($"[{sel.branchName}] {sel.title} (Cấp {selLv}/{sel.maxLevel})") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            dMeta.Add(new Label(sel.effectDesc) { style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
            dHead.Add(dMeta);
            detail.Add(dHead);

            if (!parentOk)
            {
                var pSpec = FindDaoVanSpec(sel.parentId);
                detail.Add(new Label($"Yêu cầu khai mở trước: {(pSpec != null ? pSpec.title : sel.parentId)}") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginTop = 5 } });
            }

            var dAct = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginTop = 7 } };
            string costStr = selMax ? "Đã đạt Đỉnh Phong" : sel.costsDaoPoints ? $"Tiêu hao: {selCost:0} Điểm Đạo Vận" : $"Tiêu hao: {CultivationTable.FormatNumber(selCost)} Linh Thạch";
            dAct.Add(new Label(costStr) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });
            var upBtn = new Button(() =>
            {
                if (engine.TryUpgradeDaoVanNode(sel.id))
                {
                    body.Clear();
                    BuildDaoVanTree(body, pan, zoom, onViewportChanged);
                }
            })
            {
                text = selMax ? "Đã Tối Đa" : (selLv == 0 ? "Khai Mở" : "Nâng Cấp")
            };
            upBtn.AddToClassList("btn-primary");
            upBtn.SetEnabled(selCan);
            dAct.Add(upBtn);
            detail.Add(dAct);
            body.Add(detail);
        }

        private static DaoVanNodeSpec FindDaoVanSpec(string id)
        {
            for (int i = 0; i < RemakeCatalogs.StarterDaoVanNodes.Count; i++)
                if (RemakeCatalogs.StarterDaoVanNodes[i].id == id) return RemakeCatalogs.StarterDaoVanNodes[i];
            return null;
        }

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

            // HERO CAULDRON STAGE
            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, width = Length.Percent(100) } };
            var cauldronWrap = new VisualElement { style = { width = 98, height = 98, flexShrink = 0 } };
            SetBorderAndRadius(cauldronWrap, new Color(0.86f, 0.56f, 0.14f, 0.72f), 1.5f, 12f);
            CodexArtLoader.ApplyIconOrPlaceholder(cauldronWrap, "Stages/stage_alchemy_cauldron", "[Cửu Long Lô]");
            var flameBadge = new VisualElement { style = { position = Position.Absolute, right = 4, bottom = 4, width = 32, height = 32, backgroundColor = new Color(1f, 0.96f, 0.88f, 0.95f) } };
            SetBorderAndRadius(flameBadge, new Color(0.88f, 0.42f, 0.14f), 1.5f, 16f);
            CodexArtLoader.ApplyIconOrPlaceholder(flameBadge, CultivationTable.GetSpiritFlameCodexKey(save.spiritFlameTier), "[Hỏa]");
            cauldronWrap.Add(flameBadge);
            topRow.Add(cauldronWrap);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
            info.Add(new Label("Đan Các · Cửu Long Luyện Đan Lô") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } });
            info.Add(new Label($"Đan Sư: {CultivationTable.GetAlchemistRankName(save.alchemistRank)} (+{Mathf.RoundToInt(CultivationTable.GetAlchemistQualityBonus(save.alchemistRank) * 100)}% phẩm · XP {save.alchemistXp})") { style = { fontSize = 11, marginTop = 3 } });
            info.Add(new Label($"Chân Hỏa: {CultivationTable.GetSpiritFlameName(save.spiritFlameTier)} (XP {save.spiritFlameXp}/{CultivationTable.GetSpiritFlameNextTierXp(save.spiritFlameTier)})") { style = { fontSize = 10, color = new Color(0.72f, 0.34f, 0.10f), marginTop = 2 } });
            info.Add(new Label($"Dược Trần: {save.herbDust} · {ActStageDatabaseSO.GetHerbName(herbTier)}: {ownedHerbs}/{reqHerbs}") { style = { fontSize = 10, color = new Color(0.16f, 0.56f, 0.44f), marginTop = 2 } });
            topRow.Add(info);
            hero.Add(topRow);

            if (!unlocked)
                hero.Add(new Label($"Mở khóa Luyện Đan tại {CultivationTable.GetFeatureUnlockRequirementText(RemakeFeatureId.Alchemy)}.") { style = { fontSize = 10, color = new Color(0.78f, 0.25f, 0.14f), marginTop = 5 } });

            var pillTabs = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.Center, marginTop = 7 } };
            for (int p = 0; p < 8; p++)
            {
                int capP = p;
                var b = new Button(() => { _uiAlchemyPillIdx = capP; Refresh(); }) { text = CultivationTable.GetPillNameForRealmBreakthrough(p).Replace(" Đan", "") };
                b.AddToClassList("btn-compact");
                if (p == _uiAlchemyPillIdx) b.AddToClassList("selected");
                b.style.marginRight = 3; b.style.marginBottom = 3; b.style.fontSize = 9;
                pillTabs.Add(b);
            }
            hero.Add(pillTabs);

            var recipeRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, width = Length.Percent(100), marginTop = 6 } };
            var pillIcon = new VisualElement { style = { width = 34, height = 34, marginRight = 6 } };
            CodexArtLoader.ApplyIconOrPlaceholder(pillIcon, CultivationTable.GetPillCodexKey(_uiAlchemyPillIdx), "[Đan]");
            recipeRow.Add(pillIcon);
            var recTxt = new VisualElement { style = { flexGrow = 1 } };
            recTxt.Add(new Label(pillName) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            recTxt.Add(new Label($"Cần: {reqHerbs} {ActStageDatabaseSO.GetHerbName(herbTier)} + {CultivationTable.FormatNumber(ltCost)} LT") { style = { fontSize = 10 } });
            recipeRow.Add(recTxt);
            var brewBtn = new Button(() => { engine.TryBrewPill(_uiAlchemyPillIdx); Refresh(); }) { text = "Khai Lò Luyện Đan" };
            brewBtn.AddToClassList("btn-primary");
            brewBtn.SetEnabled(unlocked);
            recipeRow.Add(brewBtn);
            hero.Add(recipeRow);
            body.Add(hero);

            // PILL INVENTORY, SYNTHESIS 3->1 & SACRIFICE TO FLAME
            var invCard = new VisualElement();
            invCard.AddToClassList("card-light");
            invCard.Add(new Label($"Kho {pillName} — Hợp Đan 3→1 & Tế Chân Hỏa") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, marginBottom = 6 } });
            for (int qIdx = 0; qIdx <= 4; qIdx++)
            {
                var q = (RemakePillQuality)qIdx;
                int count = engine.GetPillCount(_uiAlchemyPillIdx, q);
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginBottom = 5 } };
                row.Add(new Label($"{CultivationTable.GetPillQualityName(q)} (+{Mathf.RoundToInt(CultivationTable.GetPillBreakthroughBonus(q) * 100)}%): {count}") { style = { fontSize = 11, flexGrow = 1 } });

                if (q < RemakePillQuality.CucPham)
                {
                    var syn = new Button(() => { engine.TryMergePills(_uiAlchemyPillIdx, q); Refresh(); }) { text = "Hợp 3→1" };
                    syn.AddToClassList("btn-secondary");
                    syn.SetEnabled(unlocked && count >= 3);
                    syn.style.paddingTop = 3; syn.style.paddingBottom = 3; syn.style.fontSize = 10; syn.style.marginRight = 4;
                    row.Add(syn);
                }
                if (q == RemakePillQuality.PheDan || q == RemakePillQuality.HaPham)
                {
                    var sac = new Button(() => { engine.TrySacrificePillToFlame(_uiAlchemyPillIdx, q); Refresh(); }) { text = "Tế Hỏa" };
                    sac.AddToClassList("btn-compact");
                    sac.SetEnabled(unlocked && count >= 1);
                    sac.style.fontSize = 10;
                    row.Add(sac);
                }
                invCard.Add(row);
            }
            body.Add(invCard);
        }

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
            hdr.Add(new Label($"Tướng Tinh 6 Ô ({save.equipmentBag.Count} món túi)") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
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

                tile.Add(new Label(eq != null ? $"{CultivationTable.GetEquipSlotName(slot)}·T{eq.tier}" : CultivationTable.GetEquipSlotName(slot)) { style = { fontSize = 9, marginTop = 2 } });

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
            var portrait = new VisualElement { style = { width = 68, height = 68, marginBottom = 4, backgroundColor = new Color(0.99f, 0.96f, 0.88f) } };
            SetBorderAndRadius(portrait, new Color(0.86f, 0.62f, 0.14f), 2f, 34f);
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
                cHead.Add(new Label($"So Sánh: {CultivationTable.GetRarityName(selectedItem.rarity)} · {CultivationTable.GetEquipSlotName(selectedItem.slot)} Bậc {selectedItem.tier}")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 }
                });
                var deltaLbl = new Label(diff >= 0 ? $"▲ +{diff:0}" : $"▼ {diff:0}") { style = { fontSize = 12 } };
                deltaLbl.AddToClassList(diff >= 0 ? "stat-up" : "stat-down");
                cHead.Add(deltaLbl);
                cmp.Add(cHead);

                cmp.Add(new Label(equippedInSlot != null
                    ? $"Đang mặc cùng ô: {CultivationTable.GetRarityName(equippedInSlot.rarity)} Bậc {equippedInSlot.tier} (Điểm {eqScore:0}) vs Mới (Điểm {selScore:0})"
                    : $"Ô {CultivationTable.GetEquipSlotName(selectedItem.slot)} đang trống (Điểm mới {selScore:0})")
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
                    var sLbl = new Label($"• {CultivationTable.GetEquipStatLabel(st.statType, st.value)}  ({(sd >= 0 ? "+" : "")}{sd:0.#})") { style = { fontSize = 10, marginTop = 2 } };
                    sLbl.AddToClassList(sd >= 0 ? "stat-up" : "stat-down");
                    cmp.Add(sLbl);
                }

                var btns = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginTop = 7 } };
                var eqBtn = new Button(() => { engine.TryEquipGear(selectedItem.uid); Refresh(); }) { text = "Mặc" };
                eqBtn.AddToClassList("btn-primary");
                eqBtn.SetEnabled(unlocked);
                eqBtn.style.marginRight = 4;
                btns.Add(eqBtn);

                var lockBtn = new Button(() => { engine.TryToggleGearLock(selectedItem.uid); Refresh(); }) { text = selectedItem.isLocked ? "Mở Khóa" : "Khóa" };
                lockBtn.AddToClassList("btn-compact");
                if (selectedItem.isLocked) lockBtn.AddToClassList("selected");
                lockBtn.style.marginRight = 4;
                btns.Add(lockBtn);

                var refBtn = new Button(() => { engine.TryReforgeGear(selectedItem.uid); Refresh(); }) { text = "Tẩy Luyện" };
                refBtn.AddToClassList("btn-secondary");
                refBtn.SetEnabled(unlocked && save.reforgeStoneCount > 0);
                refBtn.style.marginRight = 4;
                btns.Add(refBtn);

                var disBtn = new Button(() => { engine.TryDismantleGear(selectedItem.uid); Refresh(); }) { text = selectedItem.isLocked ? "Đã Khóa" : "Phân Rã" };
                disBtn.AddToClassList("btn-compact");
                disBtn.SetEnabled(unlocked && !selectedItem.isLocked);
                btns.Add(disBtn);

                cmp.Add(btns);
                body.Add(cmp);
            }

            // BAG LIST
            for (int i = 0; i < save.equipmentBag.Count; i++)
            {
                var item = save.equipmentBag[i];
                bool isSel = item.uid == _selectedGearUid;
                float score = CultivationTable.CalculateEquipItemScore(item, save.realmIndex);
                var row = new Button(() => { _selectedGearUid = item.uid; Refresh(); })
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginBottom = 5, paddingTop = 7, paddingBottom = 7, paddingLeft = 7, paddingRight = 7, unityTextAlign = TextAnchor.MiddleLeft }
                };
                row.AddToClassList("card-light");
                if (isSel) SetBorderAndRadius(row, new Color(0.88f, 0.62f, 0.12f), 2f, 9f);

                var left = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexGrow = 1 } };
                var iconWrap = new VisualElement { style = { width = 34, height = 34, marginRight = 8, alignItems = Align.Center, justifyContent = Justify.Center } };
                CodexArtLoader.ApplyIconOrPlaceholder(iconWrap, CultivationTable.GetRarityFrameCodexKey(item.rarity), "");
                var innerIcon = new VisualElement { style = { width = 24, height = 24 } };
                var (ck, rk) = CultivationTable.GetEquipSlotIcons(item.slot);
                CodexArtLoader.ApplyIconOrPlaceholder(innerIcon, ck, "", rk);
                iconWrap.Add(innerIcon);
                left.Add(iconWrap);

                var meta = new VisualElement { style = { flexGrow = 1 } };
                string lockTag = item.isLocked ? " [Khóa]" : "";
                string setTag = item.setId != RemakeEquipSet.None ? $" · {CultivationTable.GetEquipSetName(item.setId)}" : "";
                meta.Add(new Label($"{CultivationTable.GetRarityName(item.rarity)} {CultivationTable.GetEquipSlotName(item.slot)} T{item.tier}{setTag}{lockTag}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
                meta.Add(new Label($"Điểm: {score:0} · {item.substats.Count} dòng phụ") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f) } });
                left.Add(meta);
                row.Add(left);

                var cmpEq = engine.GetEquippedItem(item.slot);
                float diff = score - CultivationTable.CalculateEquipItemScore(cmpEq, save.realmIndex);
                var badge = new Label(diff >= 0 ? $"▲ +{diff:0}" : $"▼ {diff:0}") { style = { fontSize = 10 } };
                badge.AddToClassList(diff >= 0 ? "stat-up" : "stat-down");
                row.Add(badge);
                body.Add(row);
            }
        }
        public static void BuildFormation(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            bool formOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, save.realmIndex, save.subStage);
            bool subOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.FormationSubSlot, save.realmIndex, save.subStage);

            var top = new VisualElement();
            top.AddToClassList("card-light");
            top.style.marginBottom = 8;
            string mainName = save.activeMainFormation >= 0 ? RemakeCatalogs.Formations[save.activeMainFormation].name : "Chưa khai trận";
            string subName = save.activeSubFormation >= 0 ? RemakeCatalogs.Formations[save.activeSubFormation].name : (subOpen ? "Chưa chọn" : "Mở ở Hóa Thần (50% hiệu lực)");
            top.Add(new Label($"Trận Chính: {mainName}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            top.Add(new Label($"Ô Trận Phụ: {subName}") { style = { fontSize = 11, marginTop = 2 } });
            body.Add(top);

            foreach (var f in RemakeCatalogs.Formations)
            {
                bool fUnlocked = formOpen && save.realmIndex >= f.minRealmIndex;
                bool isMain = save.activeMainFormation == f.index;
                bool isSub = save.activeSubFormation == f.index;

                var card = new VisualElement();
                card.AddToClassList("card-light");
                card.style.flexDirection = FlexDirection.Row;
                card.style.alignItems = Align.Center;
                card.style.justifyContent = Justify.SpaceBetween;
                card.style.marginBottom = 6;

                var left = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexGrow = 1, marginRight = 6 } };
                var icon = new VisualElement { style = { width = 42, height = 42, marginRight = 8 } };
                icon.AddToClassList("art-slot");
                CodexArtLoader.ApplyIconOrPlaceholder(icon, f.codexIconKey, "[Trận]", f.reusedIconKey);
                left.Add(icon);

                var col = new VisualElement { style = { flexGrow = 1 } };
                col.Add(new Label(f.name) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
                col.Add(new Label($"{f.effectDesc} · Mở: {f.realmReqText}") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal } });
                left.Add(col);
                card.Add(left);

                var btnCol = new VisualElement { style = { alignItems = Align.FlexEnd } };
                var btnMain = new Button(() => engine.TryActivateFormation(f.index, false)) { text = isMain ? "Đang Trận Chính" : "Trận Chính" };
                btnMain.AddToClassList("btn-compact");
                btnMain.EnableInClassList("selected", isMain);
                btnMain.SetEnabled(fUnlocked && !isMain);
                btnCol.Add(btnMain);

                if (subOpen)
                {
                    var btnSub = new Button(() => engine.TryActivateFormation(f.index, true)) { text = isSub ? "Tắt Trận Phụ" : "Đặt Phụ 50%" };
                    btnSub.AddToClassList("btn-compact");
                    btnSub.EnableInClassList("selected", isSub);
                    btnSub.style.marginTop = 3;
                    btnSub.SetEnabled(fUnlocked && !isMain);
                    btnCol.Add(btnSub);
                }
                card.Add(btnCol);
                body.Add(card);
            }
        }

        public static void BuildSkills(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            var scrollBar = new VisualElement();
            scrollBar.AddToClassList("card-light");
            scrollBar.style.marginBottom = 8;
            scrollBar.style.flexDirection = FlexDirection.Row;
            scrollBar.style.alignItems = Align.Center;
            var scIcon = new VisualElement { style = { width = 26, height = 26, marginRight = 8 } };
            scIcon.AddToClassList("art-slot");
            CodexArtLoader.ApplyIconOrPlaceholder(scIcon, "Equipment/mat_skill_scroll", "");
            scrollBar.Add(scIcon);
            scrollBar.Add(new Label($"Tàn Quyển Công Pháp hiện có: {save.skillScrollCount} (tự dùng ưu tiên khi nâng Kỹ Năng · Thức tỉnh Lv.10 / 20 / 30)")
            {
                style = { fontSize = 11, whiteSpace = WhiteSpace.Normal, flexGrow = 1 }
            });
            body.Add(scrollBar);

            (string name, string req, string reused, string desc)[] skills =
            {
                ("1. Lôi Phạt", "Luyện Khí Tầng 4", "SkillThunder", "Giáng thiên lôi AoE diệt quái diện rộng."),
                ("2. Âm Dương Trận", "Trúc Cơ Sơ Kỳ", "SkillYinYang", "Tỏa thái cực quanh thân sát thương toàn bộ quái gần."),
                ("3. Vạn Kiếm Quy Tông", "Kim Đan Sơ Kỳ", "SkillSword", "Triệu kiếm trận giáng mưa kiếm liên hoàn."),
                ("4. Ấn Chưởng", "Hóa Thần Sơ Kỳ", "SkillPalm", "Đại Thủ Ấn giáng sát thương cực hạn + đẩy lùi.")
            };

            for (int s = 0; s < 4; s++)
            {
                int slot = s;
                bool unlocked = engine.IsSkillUnlocked(s);
                int lv = engine.GetSkillLevel(s);
                bool autoOn = engine.GetSkillAuto(s);
                double ltCost = Math.Round((180 + s * 320) * Math.Pow(1.22, lv - 1));

                var card = new VisualElement();
                card.AddToClassList("card-light");
                card.style.flexDirection = FlexDirection.Row;
                card.style.alignItems = Align.Center;
                card.style.justifyContent = Justify.SpaceBetween;
                card.style.marginBottom = 6;

                var left = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexGrow = 1, marginRight = 6 } };
                var icon = new VisualElement { style = { width = 42, height = 42, marginRight = 8 } };
                icon.AddToClassList("art-slot");
                CodexArtLoader.ApplyIconOrPlaceholder(icon, "", "[Skill]", skills[s].reused);
                left.Add(icon);

                var info = new VisualElement { style = { flexGrow = 1 } };
                info.Add(new Label($"{skills[s].name} (Lv.{lv}) — {(unlocked ? "Đã mở" : "Mở: " + skills[s].req)}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
                info.Add(new Label(skills[s].desc) { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal } });
                left.Add(info);
                card.Add(left);

                var acts = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                var btnAuto = new Button(() => engine.SetSkillAuto(slot, !autoOn)) { text = autoOn ? "Auto: Bật" : "Auto: Tắt" };
                btnAuto.AddToClassList("btn-compact");
                btnAuto.EnableInClassList("selected", autoOn);
                btnAuto.style.marginRight = 4;
                btnAuto.SetEnabled(unlocked);

                string upLbl = save.skillScrollCount > 0 ? "Nâng (1 Quyển)" : $"Nâng ({CultivationTable.FormatNumber(ltCost)} LT)";
                var btnUp = new Button(() => engine.TryUpgradeSkill(slot, true)) { text = upLbl };
                btnUp.AddToClassList("btn-compact");
                btnUp.SetEnabled(unlocked && (save.skillScrollCount > 0 || save.linhThach >= ltCost));
                acts.Add(btnAuto);
                acts.Add(btnUp);
                card.Add(acts);
                body.Add(card);
            }
        }

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
            var bossCtrlRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, marginTop = 6 } };
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

            // 4 Difficulty Tabs
            var diffRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6 } };
            for (int d = 0; d < 4; d++)
            {
                int capD = d;
                var b = new Button(() => { _uiStageTabDiff = capD; body.Clear(); BuildStages(body); })
                {
                    text = ActStageDatabaseSO.GetDifficultyLabel((RemakeDifficulty)d)
                };
                b.AddToClassList("btn-compact");
                if (d == _uiStageTabDiff) b.AddToClassList("selected");
                b.style.flexGrow = 1; b.style.marginRight = d < 3 ? 4 : 0; b.style.fontSize = 10;
                diffRow.Add(b);
            }
            body.Add(diffRow);

            // 3 Act World Tabs
            var actRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 8 } };
            for (int a = 0; a < 3; a++)
            {
                int capA = a;
                var b = new Button(() => { _uiStageTabAct = capA; body.Clear(); BuildStages(body); })
                {
                    text = a == 0 ? "Vực 1: Đồng Cỏ" : a == 1 ? "Vực 2: Hoang Mạc" : "Vực 3: Tiên Sơn"
                };
                b.AddToClassList("btn-compact");
                if (a == _uiStageTabAct) b.AddToClassList("selected");
                b.style.flexGrow = 1; b.style.marginRight = a < 2 ? 4 : 0; b.style.fontSize = 10;
                actRow.Add(b);
            }
            body.Add(actRow);

            // 10 Stages in Selected Difficulty + Act (120 Stages Total!)
            var stageGrid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
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
                {
                    text = spec.isBossStage ? $"Boss {spec.stageCode}\nThủ Lĩnh" : $"Ải {spec.stageCode}"
                };
                btn.AddToClassList("btn-secondary");
                if (active) btn.AddToClassList("selected");
                btn.SetEnabled(unlocked);
                btn.style.width = 88;
                btn.style.height = 46;
                btn.style.marginRight = 5;
                btn.style.marginBottom = 5;
                btn.style.fontSize = 10;
                stageGrid.Add(btn);
            }
            body.Add(stageGrid);
        }

        public static void BuildBreakthrough(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            double reqTuVi = engine.GetRequiredTuViForCurrent();
            float baseChance = CultivationTable.GetBaseBreakthroughChance(save.realmIndex);
            int nextSub = save.subStage < 9 ? save.subStage + 1 : 1;
            int nextRealm = save.subStage < 9 ? save.realmIndex : Mathf.Min(8, save.realmIndex + 1);

            var hero = new VisualElement();
            hero.AddToClassList("stage-hero-box");
            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, width = Length.Percent(100) } };
            var altar = new VisualElement { style = { width = 94, height = 94, flexShrink = 0 } };
            SetBorderAndRadius(altar, new Color(0.84f, 0.60f, 0.16f), 1.5f, 12f);
            CodexArtLoader.ApplyIconOrPlaceholder(altar, "Stages/stage_tribulation_altar", "[Độ Kiếp Đài]");
            topRow.Add(altar);

            var info = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
            info.Add(new Label($"Độ Kiếp Đài: {CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)} → {CultivationTable.GetRealmDisplayName(nextRealm, nextSub)}") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, whiteSpace = WhiteSpace.Normal } });
            info.Add(new Label($"Tu Vi: {CultivationTable.FormatNumber(save.tuVi)} / {CultivationTable.FormatNumber(reqTuVi)} ({Mathf.RoundToInt(Mathf.Clamp01((float)(save.tuVi / Math.Max(1, reqTuVi))) * 100)}%)") { style = { fontSize = 11, marginTop = 3 } });
            info.Add(new Label($"Thất bại liên tiếp: {save.consecutiveBreakthroughFails} · Hộ Pháp LAN: +{save.hoPhapBonusSteps}%") { style = { fontSize = 10, color = new Color(0.32f, 0.42f, 0.37f), marginTop = 2 } });
            topRow.Add(info);
            hero.Add(topRow);

            var helperRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.SpaceBetween, width = Length.Percent(100), marginTop = 7 } };
            helperRow.Add(new Label($"Tỉ lệ nền: {Mathf.RoundToInt(baseChance * 100)}%") { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });
            var incBtn = new Button(() =>
            {
                if (engine.TryUseCalmIncense())
                {
                    body.Clear();
                    BuildBreakthrough(body);
                }
            })
            {
                text = $"Tĩnh Tâm Hương ({save.calmIncenseCount})"
            };
            incBtn.AddToClassList("btn-secondary");
            incBtn.SetEnabled(save.calmIncenseCount > 0 && save.consecutiveBreakthroughFails > 0);
            helperRow.Add(incBtn);
            hero.Add(helperRow);
            body.Add(hero);

            var options = new List<RemakePillQuality?> { null };
            for (int q = 0; q <= 4; q++)
                options.Add((RemakePillQuality)q);

            string pillName = CultivationTable.GetPillNameForRealmBreakthrough(save.realmIndex);
            string pillCodex = CultivationTable.GetPillCodexKey(save.realmIndex);

            foreach (var opt in options)
            {
                if (opt.HasValue && engine.GetPillCount(save.realmIndex, opt.Value) <= 0 && opt.Value != RemakePillQuality.HaPham)
                    continue;

                float chance = engine.ComputeBreakthroughChance(opt);
                int count = opt.HasValue ? engine.GetPillCount(save.realmIndex, opt.Value) : -1;

                var card = new VisualElement();
                card.AddToClassList("card-light");
                card.style.flexDirection = FlexDirection.Row;
                card.style.justifyContent = Justify.SpaceBetween;
                card.style.alignItems = Align.Center;
                card.style.marginBottom = 6;

                var left = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexGrow = 1 } };
                var pIcon = new VisualElement { style = { width = 32, height = 32, marginRight = 8 } };
                CodexArtLoader.ApplyIconOrPlaceholder(pIcon, opt.HasValue ? pillCodex : "HUD/nav_breakthrough", "[Đan]");
                left.Add(pIcon);

                var txt = new VisualElement { style = { flexGrow = 1 } };
                string label = opt.HasValue ? $"{pillName} ({CultivationTable.GetPillQualityName(opt.Value)} · Có {count})" : "Vượt Chay (Không Dùng Đan)";
                txt.Add(new Label(label) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
                txt.Add(new Label($"Tổng xác suất: {Mathf.RoundToInt(chance * 100)}% (Hộ Pháp LAN: +{save.hoPhapBonusSteps}%)") { style = { fontSize = 11 } });
                left.Add(txt);
                card.Add(left);

                var btn = new Button(() => engine.TryAttemptBreakthrough(opt)) { text = "Ứng Kiếp" };
                btn.AddToClassList("btn-primary");
                btn.SetEnabled(engine.IsBreakthroughReady && (!opt.HasValue || count > 0));
                card.Add(btn);
                body.Add(card);
            }
        }

        public static void BuildPioneerDiary(VisualElement body)
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;

            var top = new VisualElement();
            top.AddToClassList("card-light");
            top.style.marginBottom = 8;
            top.Add(new Label("Lộ Trình Chuẩn Thương Mại: 30 Ngày 24/7 (720 Giờ) → Đại Thừa") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } });
            top.Add(new Label($"Cảnh giới hiện tại: {CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)} · Tiến độ ải: {save.highestUnlockedLinearStage + 1}/120") { style = { fontSize = 11, marginTop = 2 } });
            body.Add(top);

            foreach (var m in RemakeCatalogs.PioneerMilestones)
            {
                bool claimed = (save.claimedPioneerMilestonesMask & (1 << m.index)) != 0;
                bool ready = engine.IsPioneerMilestoneReady(m);

                var card = new VisualElement();
                card.AddToClassList("card-light");
                card.style.flexDirection = FlexDirection.Row;
                card.style.alignItems = Align.Center;
                card.style.justifyContent = Justify.SpaceBetween;
                card.style.marginBottom = 6;

                var col = new VisualElement { style = { flexGrow = 1, marginRight = 6 } };
                col.Add(new Label(m.title) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
                col.Add(new Label($"Mục tiêu: {m.targetHint} | Quà: {CultivationTable.FormatNumber(m.rewardLinhThach)} LT, +{m.rewardDaoPoints} Đạo Vận, +{m.rewardForgeTalismans} Phù, +{m.rewardSkillScrolls} Quyển")
                {
                    style = { fontSize = 10, whiteSpace = WhiteSpace.Normal }
                });
                card.Add(col);

                var btn = new Button(() => engine.TryClaimPioneerMilestone(m.index))
                {
                    text = claimed ? "Đã Nhận" : ready ? "Nhận" : "Chưa Đạt"
                };
                btn.AddToClassList(ready ? "btn-primary" : "btn-compact");
                btn.SetEnabled(ready);
                card.Add(btn);
                body.Add(card);
            }
        }

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

        private static int _uiLbTab = 0;

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
                listCard.Add(new Label(svc != null && svc.IsConnected ? "Chưa có dữ liệu hạng — bấm Làm Mới để đồng bộ." : "Hãy chạy Tools/LocalServer/server.mjs để đồng bộ BXH LAN Nakama.")
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
                        var btnAcc = new Button(() => svc.AddFriendByUsernameOrId(f.userId)) { text = "Đồng Ý" };
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

            // 1. VAN YEU THAP (100 FLOORS)
            bool towerOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, save.realmIndex, save.subStage);
            int nextFloor = Mathf.Clamp(save.demonTowerHighestFloor + 1, 1, 100);
            var tSpec = CultivationTable.GetDemonTowerFloorSpec(nextFloor);

            var towerCard = new VisualElement();
            towerCard.AddToClassList("stage-hero-box");
            towerCard.style.alignItems = Align.Stretch;
            var tTop = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var tArt = new VisualElement { style = { width = 76, height = 76, flexShrink = 0 } };
            SetBorderAndRadius(tArt, new Color(0.84f, 0.60f, 0.16f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(tArt, "Stages/mode_demon_tower", "[Vạn Yêu Tháp]");
            tTop.Add(tArt);
            var tMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
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

            // 2. THAI HU BI CANH (3 GATES / DAY + QUICK SWEEP)
            bool realmOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, save.realmIndex, save.subStage);
            var kind = (RemakeSecretRealmKind)_uiSecretRealmKind;
            var rSpec = CultivationTable.GetSecretRealmTierSpec(kind, _uiSecretRealmTier);
            int clearedTier = save.secretRealmClearedTier[_uiSecretRealmKind];

            var realmCard = new VisualElement();
            realmCard.AddToClassList("stage-hero-box");
            realmCard.style.alignItems = Align.Stretch;
            var rTop = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var rArt = new VisualElement { style = { width = 76, height = 76, flexShrink = 0 } };
            SetBorderAndRadius(rArt, new Color(0.18f, 0.56f, 0.44f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(rArt, "Stages/mode_secret_realm", "[Thái Hư Bí Cảnh]");
            rTop.Add(rArt);
            var rMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
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

            // 3. LUAN KIEM DAI (COMING SOON)
            var pvpCard = new VisualElement();
            pvpCard.AddToClassList("card-light");
            pvpCard.style.flexDirection = FlexDirection.Row;
            pvpCard.style.alignItems = Align.Center;
            var pArt = new VisualElement { style = { width = 64, height = 64, flexShrink = 0 } };
            SetBorderAndRadius(pArt, new Color(0.84f, 0.56f, 0.14f), 1.5f, 10f);
            CodexArtLoader.ApplyIconOrPlaceholder(pArt, "Stages/mode_pvp_arena", "[Luận Kiếm Đài]");
            pvpCard.Add(pArt);

            var pMeta = new VisualElement { style = { flexGrow = 1, marginLeft = 10 } };
            var pTitleRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            pTitleRow.Add(new Label("Luận Kiếm Đài · Liên Đấu") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12, marginRight = 6 } });
            var csBadge = new Label("COMING SOON");
            csBadge.AddToClassList("coming-soon-badge");
            pTitleRow.Add(csBadge);
            pMeta.Add(pTitleRow);
            pMeta.Add(new Label("Dự kiến Nguyên Anh T5 · So tài Phi Kiếm Bảng Bất Đồng Bộ qua Nakama.") { style = { fontSize = 10, whiteSpace = WhiteSpace.Normal, marginTop = 2 } });
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
            row.Add(new Label(title) { style = { fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, width = 108 } });
            var btnGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1, justifyContent = Justify.FlexEnd } };
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                var b = new Button(() => onPick?.Invoke(idx)) { text = labels[i] };
                b.AddToClassList("btn-compact");
                if (i == selectedIdx) b.AddToClassList("selected");
                b.style.marginLeft = 4;
                b.style.fontSize = 10;
                btnGroup.Add(b);
            }
            row.Add(btnGroup);
            parent.Add(row);
        }
    }
}
