using SFML.Graphics;

namespace RealGame.GameEngine.ConfigurationObjects
{
    public class DrawingOptions
    {
        public Color WindowClearColor { get; set; } = Color.Black;
        public bool ShowBoundaryBox { get; set; }
        public Color BoundaryBoxColor { get; set; } = Color.Magenta;
        public float BoundaryBoxThickness { get; set; } = 1;
        public bool ShowOriginPoint { get; set; }
        public Color OriginPointColor { get; set; } = Color.Blue;
        public float OriginPointRadius { get; set; } = 5;
        public bool ShowCenterPoint { get; set; }
        public Color CenterPointColor { get; set; } = Color.Green;
        public float CenterPointThickness { get; set; } = 5;

        public bool ShowCollisionBox { get; set; }
        public Color CollisionBoxColor { get; set; } = Color.Red;
        public float CollisionBoxThickness { get; set; } = 2;

        public bool ShowIds { get; set; }
        public bool ShowTags { get; set; }
        public bool ShowAnimation { get; set; }
        public bool ShowCurrentState { get; set; }
        public Color TextColor { get; set; } = Color.Black;
        public uint TextSize { get; set; } = 12;
        public bool ShowCollidedIds { get; set; }
        public bool ShowFPS { get; set; }
        public int UseAverageFPS { get; set; }
        public bool ShowViewMoved { get; set; }
        public bool ShowViewSize { get; set; }
        public bool ShowViewCenter { get; set; }
        public bool ShowDeltaTime { get; set; }
        public int UseAverageDeltaTime { get; set; }
        public bool ShowMousePosition { get; set; }
        public bool ShowPositions { get; set; }



    }
}
