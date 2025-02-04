using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EnvironmentCreatorAPI.Models
{
    public class Object2D
    {
        [Key]
        public int ObjectId { get; set; }

        [Required, ForeignKey("Environment2D")]
        public int EnvironmentId { get; set; }
        public Environment2D Environment { get; set; }

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
