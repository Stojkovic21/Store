import itemDto from "../../DTOs/ItemDto";
import Artikal from "../Artikal/Artikal";
import "./Polica.css";
import useAxiosPrivate from "../../hooks/useAxiosPrivate";
import { useEffect, useState } from "react";
//nisam siguran sto sam ga napravio ali neka ga mozda skuzim
// let id:string="0";
// const fetchData=async()=>{
//   try {
//     const response = await axios.get(`http://localhost:5057/item/get/id:${id}`);
//     console.log(response.data.item);
//   } catch (error) {
//     console.log(error);
//   }
// }

// useEffect(() => {
//   const fetchItems = async () => {
//     try {
//       const response = await axios.get("http://localhost:5057/Item/Get/all");
//       setItems(response.data);
//     } catch (err) {
//     } finally {
//     }
//   };

//   fetchItems();
// }, []);

function Polica() {
  const [items, setItems] = useState<itemDto[]>([]);
  const axiosPrivate = useAxiosPrivate();
  useEffect(() => {
    const fetchItems = async () => {
      try {
        const response = await axiosPrivate.get("/item/Get/all");
        setItems(response.data.items);
      } catch (err) {
      } finally {
      }
    };

    fetchItems();
  }, []);

  // items: itemModul[] = [item1, item];
  return (
    <>
      <div className="polica">
        {items
          ? items.map((item) => <Artikal key={item.id} {...item} />)
          : null}
      </div>

      {/*<button onClick={fetchData}> dugme </button>*/}
    </>
  );
}

export default Polica;
