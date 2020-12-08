namespace ArkivaMVCOnly
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TJ_Users
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string username { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string password { get; set; }

        [Key]
        [Column(Order = 2)]
        public bool isActive { get; set; }
    }
}
