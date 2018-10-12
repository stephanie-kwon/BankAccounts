using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class User
{
    [Key]
    public int idUsers {get;set;}

    [Required]
    [MinLength(3)]
    public string FirstName {get;set;}

    [Required]
    [MinLength(3)]
    public string LastName {get;set;}

    [Required]
    [EmailAddress]
    public string Email {get;set;}

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
    public string Password {get;set;}

    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string Confirm {get;set;}

    public List <Transaction> transactions {get;set;}

}

public class LoginUser
{
    public string Email {get;set;}
    public string Password {get;set;}
}

public class Transaction 
{
    [Key]
    public int idTransactions {get;set;}
    
    [Required]
    public Decimal Amount {get;set;}

    public DateTime Created_at {get;set;} = DateTime.Now;
    
    public int Users_idUsers {get;set;}

    [ForeignKey("Users_idUsers")]
    public User creator {get;set;}
    
    
}