import React from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import "bootstrap/dist/css/bootstrap.min.css";
import axios from "axios";
type FormFields = {
  email: string;
  password: string;
};

function LoginPage() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormFields>();

  const onSubmit: SubmitHandler<FormFields> = async (data) => {
    await new Promise((resolve) => setTimeout(resolve, 1000));
    const response = await axios
      .post("http://localhost:5057/Customer/login", data)
      .then((response) => {
        return response.data;
      })  
      .catch(function (error) {
        if (error.response.status==400) {
          // The request was made and the server responded with a status code
          // that falls out of the range of 2xx
          return "sifra ili email nisu dobri";
        }
      });

    console.log("Form Data Submitted:", response);
  };

  return (
    <div className="d-flex justify-content-center align-items-center vh-100 bg-light">
      <div className="card p-4 shadow-sm" style={{ width: "22rem" }}>
        <h2 className="text-center mb-4">Login</h2>
        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="mb-3">
            <label className="form-label">Email</label>
            <input
              type="email"
              className="form-control"
              placeholder="Enter your email"
              {...register("email", {
                required: "Email is required",
                pattern: {
                  value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                  message: "ss",
                },
              })}
            />
            {errors.email && (
              <small className="text-danger">{errors.email.message}</small>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Password</label>
            <input
              type="password"
              className="form-control"
              placeholder="Enter your password"
              {...register("password", {
                required: "Password is required",
              })}
            />
            {errors.password && (
              <small className="text-danger">{errors.password.message}</small>
            )}
          </div>
          <button
            type="submit"
            disabled={isSubmitting}
            className="btn btn-primary w-100"
          >
            {isSubmitting ? "Loading..." : "Submit"}
          </button>
        </form>
      </div>
    </div>
  );
}

export default LoginPage;
