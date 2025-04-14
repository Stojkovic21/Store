//import { FormEvent, useCallback, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import "../style/AddItem.css"
import axios from "axios";
import { useEffect, useState } from "react";
import itemModel from "../Models/ItemModel";

type FormFields = {
  id:number
  naziv: string;
  cena: number;
  brend: string;
  grama: number;
  dostupnaKolicina: number;
};

function ItemForm() {
  //   const handleSubmit = useCallback((e: FormEvent<any>) => {
  //   console.log(e);
  //   e.preventDefault();
  // }, []);
  //in form on/Submit={handleSubmit}
  const [items, setItems]= useState<itemModel[]>([]);
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

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormFields>();


  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    data.id=items.length;
    const response = await axios.post("http://localhost:5057/Item/Dodaj",data)
    await new Promise((resolve) => setTimeout(resolve, 1000));
    console.log(response.data);
  };
  return (
    <div className="container mt-4">
      <h2>Add New Item</h2>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="mb-3">
          <label className="form-label">Naziv</label>
          <input
            {...register("naziv", { required: "naziv fali" })}
            type="text"
            className="form-control"
            name="naziv"
          />
        </div>
        {errors.naziv && (
          <div className="redError">
            {errors.naziv.message}
          </div>
        )}
        <div className="mb-3">
          <label className="form-label">Cena</label>
          <input
            {...register("cena")}
            type="number"
            className="form-control"
            name="cena"
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Brend</label>
          <input
            {...register("brend")}
            type="text"
            className="form-control"
            name="brend"
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Grama</label>
          <input
            {...register("grama")}
            type="number"
            className="form-control"
            name="grama"
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Dostupna Količina</label>
          <input
            {...register("dostupnaKolicina")}
            type="number"
            className="form-control"
            name="dostupnaKolicina"
          />
        </div>

        <button
          disabled={isSubmitting}
          type="submit"
          className="btn btn-primary"
        >
          {isSubmitting ? "Loading..." : "Submit"}
        </button>
      </form>
    </div>
  );
}

export default ItemForm;
