import { useForm } from "react-hook-form";
import "../style/Register.css";
import { useEffect, useState } from "react";
import axios from "axios";
import customerModel from "../Models/CustomerModel";
// Define the TypeScript interface for your model
type FormFields = {
  id: number;
  email: string;
  password: string;
  ime: string;
  prezime: string;
  brTel: string;
};

function Registe() {
  const [customers, setCustomers] = useState<customerModel[]>([]);
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const result = await axios.get(
          "http://localhost:5057/Customer/Get/all"
        );
        setCustomers(result.data.customer);
      } catch (error) {
        console.log("Doslo do pucanja get all");
      }
    };

    fetchUser();
  }, []);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormFields>();

  const onSubmit = async (data: FormFields) => {

    data.id = 4;
    const response = await axios.post(
      "http://localhost:5057/Customer/Dodaj",
      data
    );
    await new Promise((responce) => setTimeout(responce, 1000));
    console.log("Form Data Submitted:", response.data);
  };

  return (
    <div className="form-wrapper">
      <h2 className="form-title">User Registration</h2>
      <form onSubmit={handleSubmit(onSubmit)} className="form-container">
        <div className="form-group">
          <label>Email</label>
          <input
            type="email"
            placeholder="Enter your Email"
            {...register("email", {
              required: true,
              pattern: {
                value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                message: "Invalid email address",
              },
            })}
            className="form-input"
          />
          {errors.email && <span className="error">Email is required</span>}
        </div>
        <div className="form-group">
          <label>Password</label>
          <input
            type="password"
            placeholder="Enter your password"
            {...register("password", {
              required: true,
              pattern: {
                value: /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$/,
                message:
                  "Password must be at least 8 characters long and include letters and number",
              },
            })}
            className="form-input"
          />
          {errors.password && (
            <span className="error">Password is required</span>
          )}
        </div>

        <div className="form-group">
          <label>Name</label>
          <input
            type="text"
            placeholder="Enter your Name"
            {...register("ime", { required: true })}
            className="form-input"
          />
          {errors.ime && <span className="error">Ime is required</span>}
        </div>

        <div className="form-group">
          <label>Lastname</label>
          <input
            type="text"
            placeholder="Enter yout Lastname"
            {...register("prezime", { required: true })}
            className="form-input"
          />
          {errors.prezime && <span className="error">Prezime is required</span>}
        </div>

        <div className="form-group">
          <label>Phone number</label>
          <input
            type="text"
            placeholder="Enter your phone number"
            {...register("brTel", { required: true })}
            className="form-input"
          />
          {errors.brTel && <span className="error">BrTel is required</span>}
        </div>

        <button type="submit" className="submit-button" disabled={isSubmitting}>
          {isSubmitting ? "Loading..." : "Submit"}
        </button>
      </form>
    </div>
  );
}

export default Registe;
