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
        public double PositionX { get; set; }

        [Required]
        public double PositionY { get; set; }

        [Required]
        public double ScaleX { get; set; }

        [Required]
        public double ScaleY { get; set; }

        [Required]
        public double RotationZ { get; set; }

        [Required]
        public int SortingLayer { get; set; }
    }

}
