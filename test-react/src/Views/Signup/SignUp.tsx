import { useForm } from "react-hook-form";
import "../style/Card.css";
import { useEffect, useState } from "react";
import customerDto from "../../DTOs/CustomerDto";
import Header from "../Header/Header";
import axios from "../../api/axios";

function SignUp() {
  const [customers, setCustomers] = useState<customerDto[]>([]);
  useEffect(() => {
    const fetchUser = async () => {
      try {
        const result = await axios.get("/customer/get/all");
        setCustomers(result.data.customer);
      } catch (error) {}
    };
    fetchUser();
  }, []);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<customerDto>();

  const onSubmit = async (data: customerDto) => {
    data.id = customers.length;
    data.role = "User";
    await axios.post("/customer/signup", data);
    await new Promise((responce) => setTimeout(responce, 1000));
    console.log("Form Data Submitted:", data);
  };

  return (
    <>
      <Header />
      <div className="card-container">
        <div className="card">
          <h2>User Registration</h2>
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input
                className="form-control"
                type="email"
                placeholder="Enter your Email"
                {...register("email", {
                  required: true,
                  // pattern: {
                  //   value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                  //   message: "Invalid email address",
                  // },
                })}
              />
              {errors.email && (
                <span className="text-danger">*Email is required</span>
              )}
            </div>
            <div className="mb-3">
              <label className="form-label">Password</label>
              <input
                type="password"
                placeholder="Enter your password"
                className="form-control"
                {...register("password", {
                  required: true,
                  // pattern: {
                  //   value: /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$/,
                  //   message:
                  //     "Password must be at least 8 characters long and include letters and number",
                  // },
                })}
              />
              {errors.password && (
                <span className="text-danger">*Password is required</span>
              )}
            </div>

            <div className="mb-3">
              <label className="form-label">Name</label>
              <input
                type="text"
                className="form-control"
                placeholder="Enter your Name"
                {...register("ime", { required: true })}
              />
              {errors.ime && (
                <span className="text-danger">*Name is required</span>
              )}
            </div>

            <div className="mb-3">
              <label className="form-label">Lastname</label>
              <input
                type="text"
                className="form-control"
                placeholder="Enter yout Lastname"
                {...register("prezime", { required: true })}
              />
              {errors.prezime && (
                <span className="text-danger">*Lastname is required</span>
              )}
            </div>

            <div className="mb-3">
              <label className="form-label">Phone number</label>
              <input
                type="text"
                className="form-control"
                placeholder="Enter your phone number"
                {...register("brTel", { required: true })}
              />
              {errors.brTel && (
                <span className="text-danger">*Phone number is required</span>
              )}
            </div>

            <button type="submit" className="btn w-100" disabled={isSubmitting}>
              {isSubmitting ? "Loading..." : "Submit"}
            </button>
          </form>
        </div>
      </div>
    </>
  );
}

export default SignUp;
