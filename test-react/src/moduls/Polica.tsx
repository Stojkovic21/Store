import itemModul from "../Models/ItemModel";
import Artikal from "./Artikal";
import "../style/Polica.css";
import axios from "axios";
import { useEffect, useState } from "react";

let id:string="s";
const fetchData=async()=>{
  try {
    const response = await axios.get(`http://localhost:5057/Item/Get/naziv:${id}`);
    console.log(response.data.item);
  } catch (error) {
    console.log(error);
  }
}
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
  const [items, setItems]= useState<itemModul[]>([]);
  useEffect(() => {
    const fetchItems = async () => {
      try {
        const response = await axios.get("http://localhost:5057/Item/Get/all");
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
        {items.map((item) => (
          <Artikal key={item.id}{...item} />
        ))}
      </div>

      <button onClick={fetchData}> dugme </button>
    </>
  );
}

export default Polica;
