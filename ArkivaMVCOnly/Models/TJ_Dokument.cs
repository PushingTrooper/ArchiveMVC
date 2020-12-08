namespace ArkivaMVCOnly.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TJ_Dokument
    {
        public TJ_Dokument(string emri_dokumentit, string emri_subjektit, string nr_inspektimi,
            string lloji, string fusha_indeksimit, string vend_zyra, string vend_nr_kutise,
            string vend_rafti, DateTime date_regjistrimi, byte[] dokumenti, string CreatedBy)
        {
            this.emri_dokumentit = emri_dokumentit;
            this.emri_subjektit = emri_subjektit;
            this.nr_inspektimi = nr_inspektimi;
            this.lloji = lloji;
            this.fusha_indeksimit = fusha_indeksimit;
            this.vend_zyra = vend_zyra;
            this.vend_nr_kutise = vend_nr_kutise;
            this.vend_rafti = vend_rafti;
            this.date_regjistrimi = date_regjistrimi;
            this.dokumenti = dokumenti;
            this.CreatedBy = CreatedBy;
        }

        public TJ_Dokument() { }

        [Key]
        public int IDTJ_Dokument { get; set; }

        [Required]
        [StringLength(500)]
        public string emri_dokumentit { get; set; }

        [Required]
        [StringLength(50)]
        public string emri_subjektit { get; set; }

        [StringLength(50)]
        public string nr_inspektimi { get; set; }

        [Required]
        [StringLength(50)]
        public string lloji { get; set; }

        [Required]
        [StringLength(50)]
        public string fusha_indeksimit { get; set; }

        [StringLength(50)]
        public string vend_zyra { get; set; }

        [StringLength(50)]
        public string vend_nr_kutise { get; set; }

        [StringLength(50)]
        public string vend_rafti { get; set; }

        public DateTime date_regjistrimi { get; set; }

        [Required]
        public byte[] dokumenti { get; set; }

        
        [StringLength(50)]
        public string CreatedBy { get; set; }
    }
}
