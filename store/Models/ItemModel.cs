namespace Item.Models;

public class ItemModel
{
    public int Id { get; set; }
    public string Naziv { get; set; }
    public double Cena { get; set; }
    public string Brend { get; set; }
    public int Grama { get; set; }
    public int DostupnaKolicina { get; set; }
}
