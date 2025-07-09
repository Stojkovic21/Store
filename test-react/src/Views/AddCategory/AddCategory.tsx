import { SubmitHandler, useForm } from "react-hook-form";
import Header from "../Header/Header";
import "../style/Card.css";
import { useEffect, useState } from "react";
import axios from "axios";
import CategoryDTO from "../../DTOs/CategoryDTO";

export default function AddCategoty() {
  const [getAllCategorys, setAllCategorys] = useState<CategoryDTO[]>([]);
  useEffect(() => {
    const FetchItem = async () => {
      try {
        const allCategorys = await axios.get(
          "http://localhost:5057/category/get/all"
        );
        setAllCategorys(allCategorys.data.categorys);
      } catch (error) {}
    };
    FetchItem();
  }, []);
  const onSubmit: SubmitHandler<CategoryDTO> = async (data) => {
    data.id=getAllCategorys.length;
    await axios.post("http://localhost:5057/category/add",data).catch();
    window.location.reload();
  };
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, errors },
  } = useForm<CategoryDTO>();

  return (
    <>
      <Header />
      <div className="card-container">
        <div className="card">
          <h2>Add New Category</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input
                {...register("name", { required: "*Name is required" })}
                placeholder="Enter category name"
                name="name"
                className="form-control"
                type="text"
              />
              {errors.name && (
                <div className="redError">{errors.name.message}</div>
              )}
            </div>
            <button
              type="submit"
              className="btn btn-prinary"
              disabled={isSubmitting}
            >
              {" "}
              {isSubmitting ? "Loading..." : "Submit"}
            </button>
          </form>
        </div>
      </div>
    </>
  );
}
