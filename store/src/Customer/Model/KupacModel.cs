using Isopoh.Cryptography.Argon2;
using Neo4j.Driver;

public class KupacModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public string Ime { get; set; }
    public string Prezime { get; set; }
    public string BrTel { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenTimeExpire { get; set; }
    public void Print()
    {
        Console.WriteLine(Email + "\n" + Role + "\n" + Ime + "\n" + Prezime + "\n" + BrTel + "\n" + RefreshToken + "\n" + RefreshTokenTimeExpire);
    }
}