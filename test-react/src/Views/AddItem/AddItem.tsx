//import { FormEvent, useCallback, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import "./AddItem.css";
import { useEffect, useState } from "react";
import itemDTo from "../../DTOs/ItemDto";
import "../style/Card.css";
import Header from "../Header/Header";
import BrendDTO from "../../DTOs/BrendDTO";
import RelationshipDTO from "../../DTOs/RelationshipDTO";
import CategoryDTO from "../../DTOs/CategoryDTO";
import useAxiosPrivate from "../../hooks/useAxiosPrivate";

function Additem() {
  const [items, setItems] = useState<itemDTo[]>([]);
  const [getAllBrends, setAllBrends] = useState<BrendDTO[]>([]);
  const [brend, setBrend] = useState<BrendDTO>();
  const [getAllCategorys, setAllCategorys] = useState<CategoryDTO[]>([]);
  const [category, setCategory] = useState<CategoryDTO>();
  const axiosPrivate = useAxiosPrivate();
  useEffect(() => {
    const fetchItems = async () => {
      try {
        const responseItem = await axiosPrivate.get("/item/get/all");
        setItems(responseItem.data.items);
        const responceBrend = await axiosPrivate.get("/supplier/get/all");
        setAllBrends(responceBrend.data.suppliers);
        const responceCategory = await axiosPrivate.get("/category/get/all");
        setAllCategorys(responceCategory.data.categorys);
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
  } = useForm<itemDTo>();

  const onSubmit: SubmitHandler<itemDTo> = async (data) => {
    data.id = items.length;
    const brendConnection: RelationshipDTO = {
      sourceId: data.id,
      targetId: Number(brend?.id),
      relationshipType: "Supplies",
    };

    const categoryConnection: RelationshipDTO = {
      sourceId: data.id,
      targetId: Number(category?.id),
      relationshipType: "Is",
    };

    await axiosPrivate.post("/item/add", data);
    await new Promise((resolve) => setTimeout(resolve, 1000));
    await axiosPrivate.post("/relationship/supplier/connect", brendConnection);
    await axiosPrivate.post(
      "/relationship/category/connect",
      categoryConnection
    );
    window.location.reload();
  };

  return (
    <>
      <Header />
      <div className="card-container">
        <div className="card">
          <h2>Add New Item</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input
                {...register("name", { required: "*Name is required" })}
                type="text"
                className="form-control"
                name="name"
                placeholder="Enter item name"
              />
            </div>
            {errors.name && (
              <div className="redError">{errors.name.message}</div>
            )}
            <div className="mb-3">
              <label className="form-label">Price</label>
              <input
                {...register("price", { required: "*Proce is required" })}
                type="number"
                className="form-control"
                name="price"
                placeholder="Enter item price"
              />
            </div>
            {errors.price && (
              <div className="redError">{errors.price.message}</div>
            )}

            <div className="mb-3">
              <label className="form-label">Brend</label>

              <select
                className="form-control"
                onChange={(e) => {
                  const b = getAllBrends?.find(
                    ({ id }) => id === Number(e.target.value)
                  );
                  setBrend(b);
                }}
              >
                <option>Choose a item brend</option>
                {getAllBrends
                  ? getAllBrends.map((brend) => {
                      return (
                        <option key={brend.id} value={brend.id}>
                          {brend.name}
                        </option>
                      );
                    })
                  : null}
              </select>
            </div>
            {/* {errors.brend && (
              <div className="redError">{errors.brend.message}</div>
            )} */}

            <div className="mb-3">
              <label className="form-label">Neto quantity</label>
              <input
                {...register("netoQuantity", {
                  required: "*NetoQuantity is require",
                })}
                type="number"
                className="form-control"
                name="netoQuantity"
                placeholder="Enter neto quantity here"
              />
            </div>
            {errors.netoQuantity && (
              <div className="redError">{errors.netoQuantity.message}</div>
            )}

            <div className="mb-3">
              <label className="form-label">Available quantity</label>
              <input
                {...register("availableQuantity", {
                  required: "Available quantity is require",
                })}
                type="number"
                className="form-control"
                name="availableQuantity"
                placeholder="Enter available quantity here"
              />
            </div>
            {errors.availableQuantity && (
              <div className="redError">{errors.availableQuantity.message}</div>
            )}

            <div className="mb-3">
              <label className="form-label">Item category</label>
              <select
                className="form-control"
                onChange={(e) => {
                  const c = getAllCategorys?.find(
                    ({ id }) => id === Number(e.target.value)
                  );
                  setCategory(c);
                }}
              >
                <option>Choose item category</option>
                {getAllCategorys
                  ? getAllCategorys.map((category) => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))
                  : null}
              </select>
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
      </div>
    </>
  );
}

export default Additem;
