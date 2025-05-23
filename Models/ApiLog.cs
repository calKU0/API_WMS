﻿using APIWMS.Data.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace APIWMS.Models
{
    public class ApiLog
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int EntityWmsId { get; set; }
        public EntityType EntityWmsType { get; set; }
        public int EntityErpId { get; set; }
        public EntityType EntityErpType { get; set; }
        public required LogFlow Flow { get; set; }
        public bool MailSent { get; set; }
        [Required]
        public required string Action { get; set; }
        [Required]
        public required bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
