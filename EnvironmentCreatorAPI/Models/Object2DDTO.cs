using System.ComponentModel.DataAnnotations;

namespace EnvironmentCreatorAPI.Models
{
    public class Object2DDto
    {
        [Required]
        public int EnvironmentId { get; set; } // Only send the ID

        [Required]
        public int PrefabId { get; set; }

        [Required]
        public float PositionX { get; set; }

        [Required]
        public float PositionY { get; set; }

        [Required]
        public float ScaleX { get; set; }

        [Required]
        public float ScaleY { get; set; }

        [Required]
        public float RotationZ { get; set; }

        [Required]
        public int SortingLayer { get; set; }
    }

}
