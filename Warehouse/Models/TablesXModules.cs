﻿
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("tablesxmodules")] // Maps the class to the "tablesxmodules" table in the database
    public class TablesXModules
    {
        [Key] // Marks this property as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indicates that the value is auto-generated by the database
        [Column("Id")] // Maps to the "Id" column in the table
        public int Id { get; set; }

        [StringLength(30)] // Limits the length of the string to 30 characters
        [Column("name")] // Maps to the "name" column in the table
        public string Name { get; set; }

        [StringLength(30)] // Limits the length of the string to 30 characters
        [Column("namespanish")] // Maps to the "namespanish" column in the table
        public string NameSpanish { get; set; }
        

        [StringLength(30)] // Limits the length of the string to 30 characters
        [Column("table")] // Maps to the "table" column in the table
        public string Table { get; set; }

        [StringLength(30)] // Limits the length of the string to 30 characters
        [Column("sections")] // Maps to the "namespanish" column in the table
        public string Sections { get; set; }

        [Column("active")] // Maps to the "active" column in the table
        public bool Active { get; set; } = true; // Default value is true (1 in SQL)
    }
}