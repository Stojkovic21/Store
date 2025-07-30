import { SubmitHandler, useForm } from "react-hook-form"; //zod
import "bootstrap/dist/css/bootstrap.min.css";
import Header from "../Header/Header";
import "../style/Card.css";
import LoginDTO from "../../DTOs/LoginDTO";
import { useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";
import useRefreshToken from "../../hooks/useRefreshToken";
import axios from "../../api/axios";
import { useEffect } from "react";

function LoginPage() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginDTO>();
  const { isAuthenticated, handleSignIn } = useAuth();
  const refresh = useRefreshToken();
  const navigate = useNavigate();
  //const axiosPrivate = useAxiosPrivate();
  useEffect(() => {
    refresh();
    isAuthenticated ? navigate("/") : null;
  }, []);
  const onSubmit: SubmitHandler<LoginDTO> = async (data) => {
    axios
      .post("/customer/login", data, {
        withCredentials: true,
      })
      .then((response) => {
        handleSignIn(
          response.data.accessToken,
          response.data.userId,
          response.data.role
        );
        navigate("/");
        return response.status;
      })
      .catch((error) => {
        if (error.response.status === 400) {
          console.log("sifra ili email nisu dobri");
        }
      });
    await new Promise((resolve) => setTimeout(resolve, 1000));
  };
  return (
    <>
      <Header />
      <div className="card-container">
        <div className="card">
          <h2>Login</h2>
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
                    message: "Invalid email format",
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
            <button type="submit" disabled={isSubmitting} className="btn w-100">
              {isSubmitting ? "Loading..." : "Submit"}
            </button>
          </form>
          {
            // <ProtectedRoute allowRoles={["Admin"]}>
            //   <button
            //     onClick={async () => {
            //       await axiosPrivate.get("/customer/get/all", {
            //         withCredentials: true,
            //       });
            //     }}
            //   >
            //     refresh
            //   </button>
            // </ProtectedRoute>
          }
        </div>
      </div>
    </>
  );
}

export default LoginPage;
