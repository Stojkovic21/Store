import itemDto from "../../DTOs/ItemDto";
import "./Artikal.css";

function Artikal(item: itemDto) {
  return (
    <>
      <div className="card shadow-lg rounded-2xl kartica">
        <h2 className="card-titel">{item.name}</h2>
        {/* <p className="text-gray-700">
          Brend: <span className="font-semibold">{item.brend}</span>
        </p> */}
        <p className="text-gray-700">
          Grama: <span className="font-semibold">{item.netoQuantity}g</span>
        </p>
        <p className="text-gray-700">
          Cena: <span className="font-semibold">{item.price} RSD</span>
        </p>
        <p
          className={
            item.availableQuantity > 0 ? "text-green-600" : "text-red-600"
          }
        >
          Dostupno:{" "}
          {item.availableQuantity > 0
            ? `${item.availableQuantity} kom`
            : "Nema na stanju"}
        </p>
        <button
          className={`mt-4 px-4 py-2 rounded-lg text-black ${
            item.availableQuantity > 0
              ? "bg-blue-500 hover:bg-blue-600"
              : "bg-gray-400 cursor-not-allowed"
          }`}
          disabled={item.availableQuantity === 0}
        >
          Dodaj u korpu
        </button>
      </div>
    </>
  );
}

export default Artikal;
