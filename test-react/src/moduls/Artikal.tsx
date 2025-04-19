import itemDto from "../Models/ItemDto";
import "../style/Artikal.css";
//import { ReactNode } from "react"
function Artikal(item: itemDto) {
  return (
    <>
      <div className="card shadow-lg rounded-2xl kartica">
        <h2 className="card-titel">{item.naziv}</h2>
        <p className="text-gray-700">
          Brend: <span className="font-semibold">{item.brend}</span>
        </p>
        <p className="text-gray-700">
          Grama: <span className="font-semibold">{item.grama}g</span>
        </p>
        <p className="text-gray-700">
          Cena: <span className="font-semibold">{item.cena} RSD</span>
        </p>
        <p
          className={
            item.dostupnaKolicina > 0 ? "text-green-600" : "text-red-600"
          }
        >
          Dostupno:{" "}
          {item.dostupnaKolicina > 0
            ? `${item.dostupnaKolicina} kom`
            : "Nema na stanju"}
        </p>
        <button
          className={`mt-4 px-4 py-2 rounded-lg text-black ${
            item.dostupnaKolicina > 0
              ? "bg-blue-500 hover:bg-blue-600"
              : "bg-gray-400 cursor-not-allowed"
          }`}
          disabled={item.dostupnaKolicina === 0}
        >
          Dodaj u korpu
        </button>
      </div>
    </>
  );
}

export default Artikal;
