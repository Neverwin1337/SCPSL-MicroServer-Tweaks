using System;
using System.Collections.Generic;
using DisplayKit.Elements;
using DisplayKit.Enums;
using PlayerRoles;
using UnityEngine;
using UnityEngine.UIElements;

namespace SCPSL_MicroServer_Tweaks
{
    public sealed class VotingUI
    {
        private readonly VotingController _controller;
        private DisplayCanvas _canvas;
        private readonly Dictionary<RoleTypeId, DisplayText> _roleCountTexts = new Dictionary<RoleTypeId, DisplayText>();
        private readonly Dictionary<RoleTypeId, DisplayElement> _roleBars = new Dictionary<RoleTypeId, DisplayElement>();
        private DisplayText _footerText;

        private static readonly (RoleTypeId role, string name, string item, Color color)[] Roles = {
            (RoleTypeId.Scp049, "SCP", "SCP-018", new Color(1f, 0.2f, 0.2f)),
            (RoleTypeId.Scientist, "科學家", "對講機", new Color(0.2f, 0.6f, 1f)),
            (RoleTypeId.ClassD, "D級人員", "鎖", new Color(1f, 0.8f, 0.2f)),
            (RoleTypeId.FacilityGuard, "安保人員", "鑰匙卡", new Color(0.2f, 1f, 0.4f)),
        };

        public VotingUI(VotingController controller)
        {
            _controller = controller;
        }

        public void Show()
        {
            if (_canvas != null)
                return;

            _canvas = DisplayCanvas.Create();
            _canvas.DefaultVisibility = CanvasVisibility.Visible;
            _canvas.SortOrder = 100;
            _canvas.Flex.Grow = 1f;
            _canvas.Background.Color = new Color(0, 0, 0, 0.7f);
            _canvas.Align.AlignItems = Align.Center;
            _canvas.Align.JustifyContent = Justify.Center;
            _canvas.Text.Color = Color.white;

            DisplayElement container = _canvas.AddElement();
            container.Size.Width = 500f;
            container.Flex.Direction = FlexDirection.Column;
            container.Align.AlignItems = Align.Center;
            container.Background.Color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            container.Border.Color = Color.gray;
            container.Border.Width = 2f;
            container.Border.Radius = 10f;
            container.Spacing.PaddingAll = 20f;

            DisplayText titleText = container.AddText("🗳️ 身份投票");
            titleText.Text.FontSize = 28f;
            titleText.Text.Font = FontType.RobotoBold;
            titleText.Text.Color = Color.yellow;
            titleText.Spacing.MarginBottom = 15f;

            foreach (var (role, name, item, color) in Roles)
            {
                DisplayElement row = container.AddElement();
                row.Flex.Direction = FlexDirection.Row;
                row.Align.AlignItems = Align.Center;
                row.Spacing.MarginBottom = 8f;
                row.Size.Width = Length.Percent(100f);

                DisplayText label = row.AddText($"[{GetIndex(role)}] {name} ({item})");
                label.Text.FontSize = 18f;
                label.Text.Color = color;
                label.Text.Font = FontType.RobotoBold;
                label.Size.Width = 180f;

                DisplayElement barBg = row.AddElement();
                barBg.Size.Width = 200f;
                barBg.Size.Height = 20f;
                barBg.Background.Color = new Color(0.3f, 0.3f, 0.3f);
                barBg.Border.Radius = 3f;

                DisplayElement barFill = barBg.AddElement();
                barFill.Size.Width = 0f;
                barFill.Size.Height = Length.Percent(100f);
                barFill.Background.Color = color;
                barFill.Border.Radius = 3f;

                _roleBars[role] = barFill;

                DisplayText countText = row.AddText("0 票");
                countText.Text.FontSize = 16f;
                countText.Text.Font = FontType.RobotoMonoBold;
                countText.Text.Color = Color.white;
                countText.Size.Width = 60f;
                countText.Text.Align = TextAnchor.MiddleRight;

                _roleCountTexts[role] = countText;
            }

            _footerText = container.AddText("");
            _footerText.Text.FontSize = 16f;
            _footerText.Text.Color = Color.gray;
            _footerText.Spacing.MarginTop = 15f;

            DisplayText instruction = container.AddText("輸入 .1 .2 .3 .4 投票");
            instruction.Text.FontSize = 14f;
            instruction.Text.Color = new Color(0.7f, 0.7f, 0.7f);
            instruction.Spacing.MarginTop = 5f;

            _canvas.Spawn();
            UpdateDisplay();
        }

        public void Hide()
        {
            _canvas?.Destroy();
            _canvas = null;
        }

        public void UpdateDisplay()
        {
            if (_canvas == null || !_controller.IsActive)
                return;

            int maxVotes = 1;
            foreach (int count in _controller.VoteCounts.Values)
            {
                if (count > maxVotes)
                    maxVotes = count;
            }

            foreach (var (role, _, _, _) in Roles)
            {
                _controller.VoteCounts.TryGetValue(role, out int count);
                _roleCountTexts[role].Content = $"{count} 票";

                float barWidth = (float)count / maxVotes * 200f;
                _roleBars[role].Size.Width = barWidth;
            }

            int remaining = (int)Math.Ceiling(_controller.TimeRemaining);
            _footerText.Content = $"剩餘時間: {remaining}s  ·  已投票: {_controller.TotalVoters}/{_controller.TotalPlayers}";
        }

        private static string GetIndex(RoleTypeId role)
        {
            return role switch
            {
                RoleTypeId.Scp049 => "1",
                RoleTypeId.Scientist => "2",
                RoleTypeId.ClassD => "3",
                RoleTypeId.FacilityGuard => "4",
                _ => "?"
            };
        }
    }
}