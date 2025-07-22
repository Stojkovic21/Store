import { get, SubmitHandler, useForm } from "react-hook-form"; //zod
import "bootstrap/dist/css/bootstrap.min.css";
import axios from "../../api/axios";
import Header from "../Header/Header";
import "../style/Card.css";
import LoginDTO from "../../DTOs/LoginDTO";
import { useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";
import useRefreshToken from "../../hooks/useRefreshToken";
import AuthProvider from "../Auth/AuthProvider";
import useAxiosPrivate from "../../hooks/useAxiosPrivate";

function LoginPage() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginDTO>();
  const { isAuthenticated, handleSignIn, handleSignOut, accessToken } =
    useAuth();
  const refresh = useRefreshToken();
  const navigate = useNavigate();
  const axiosPrivate = useAxiosPrivate();
  const onSubmit: SubmitHandler<LoginDTO> = async (data) => {
    await new Promise((resolve) => setTimeout(resolve, 100));
    const response = axios
      .post("/customer/login", data, {
        withCredentials: true,
      })
      .then((response) => {
        console.log(response.data);
        handleSignIn(response.data.accessToken);
        return response.status;
      })
      .catch((error) => {
        if (error.response.status === 400) {
          console.log("sifra ili email nisu dobri");
        }
      });
    //console.log("Response from login page " + response);
    //handleSignIn(response.accessToken);
    //navigate("/");
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
              {isSubmitting && isAuthenticated ? "Loading..." : "Submit"}
            </button>
          </form>

          <button
            onClick={async () => {
              const getMe = await axiosPrivate.get("/customer/get/all", {
                withCredentials: true,
              });
              console.log(getMe.data);
            }}
          >
            refresh
          </button>
        </div>
      </div>
    </>
  );
}

export default LoginPage;
