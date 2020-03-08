using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Lines
{
    public class DrawableEditorLineScrollVelocity : DrawableEditorLine
    {
        private SliderVelocityInfo ScrollVelocity { get; }

        public DrawableEditorLineScrollVelocity(EditorPlayfield playfield, SliderVelocityInfo sv) : base(playfield)
        {
            ScrollVelocity = sv;
        }

        public override Color GetColor() => ColorHelper.HexToColor("#56FE6E");

        public override string GetValue() => "";

        public override int GetTime() => (int) Math.Round(ScrollVelocity.StartTime, MidpointRounding.AwayFromZero);

        public override void SetSize()
        {
            var multiplier = Math.Abs(ScrollVelocity.Multiplier);
            var size = MathHelper.Clamp(DefaultSize.X.Value * multiplier, 10, 150);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != size)
                Width = size;
        }
    }
}