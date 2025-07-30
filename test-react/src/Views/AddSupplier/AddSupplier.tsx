import { SubmitHandler, useForm } from "react-hook-form";
import Header from "../Header/Header";
import "../style/Card.css";
import { useEffect, useState } from "react";
import SupplierDTO from "../../DTOs/SuppliersDTO";
import { axiosPrivate } from "../../api/axios";

export default function AddSupplier() {
  const [getAllSuppliers, setAllSuppliers] = useState<SupplierDTO[]>([]);
  useEffect(() => {
    const FetchItem = async () => {
      try {
        const allSuppliers = await axiosPrivate.get("/supplier/get/all");
        setAllSuppliers(allSuppliers.data.suppliers);
      } catch (error) {}
    };
    FetchItem();
  }, []);
  const onSubmit: SubmitHandler<SupplierDTO> = async (data) => {
    data.id = getAllSuppliers.length;
    await axiosPrivate.post("/supplier/add", data).catch();
    window.location.reload();
  };
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, errors },
  } = useForm<SupplierDTO>();

  return (
    <>
      <Header />
      <div className="card-container">
        <div className="card">
          <h2>Add New Supplier</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input
                {...register("name", { required: "*Name is required" })}
                placeholder="Enter supplier name"
                name="name"
                className="form-control"
                type="text"
              />
              {errors.name && (
                <div className="redError">{errors.name.message}</div>
              )}
            </div>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input
                {...register("email", { required: "*Email is required" })}
                placeholder="Enter supplier email"
                name="email"
                className="form-control"
                type="text"
              />
              {errors.email && (
                <div className="redError">{errors.email.message}</div>
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
