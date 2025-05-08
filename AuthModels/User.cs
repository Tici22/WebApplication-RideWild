using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adventure19.AuthModels;

public partial class User
{
    [Key]
    public int Id { get; set; } // PK 

    [Column("Full_Name")]
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!; //Crypted password + Salt
}
