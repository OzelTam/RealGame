using RealGame.GameEngine.Entities.Interfaces;
using SFML.Graphics;

namespace RealGame.GameEngine.Entities.EntityObjects
{
    public class GameTexture : Texture, IIdentifiable
    {
        public GameTexture(Texture copy, string? id = null) : base(copy)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(uint width, uint height, string? id = null) : base(width, height)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(string filename, bool srgb = false, string? id = null) : base(filename, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(Stream stream, bool srgb = false, string? id = null) : base(stream, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(Image image, bool srgb = false, string? id = null) : base(image, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(byte[] bytes, bool srgb = false, string? id = null) : base(bytes, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(string filename, IntRect area, bool srgb = false, string? id = null) : base(filename, area, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(Stream stream, IntRect area, bool srgb = false, string? id = null) : base(stream, area, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(Image image, IntRect area, bool srgb = false, string? id = null) : base(image, area, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public GameTexture(byte[] bytes, IntRect area, bool srgb = false, string? id = null) : base(bytes, area, srgb)
        {
            Id = string.IsNullOrEmpty(id) ? Id : id;
        }

        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Tag { get; init; } = string.Empty;
    }
}
