// import { set } from "react-hook-form";
// import axios from "../../api/axios";
// import { createContext, PropsWithChildren, useEffect, useLayoutEffect, useState } from "react";
// import useAuth from "../../hooks/useAuth";

// type AuthProviderProps=PropsWithChildren;

// const AuthProvider = ({ children }:AuthProviderProps) => {
//   const { accessToken, handleSignIn, handleSignOut } = useAuth();

//   useEffect(() => {
//     const fetchMe = async () => {
//       try {
//         const response = await axios.get("/customer/me");
//         handleSignIn(response.data.accessToken);
//       } catch {
//         handleSignIn(undefined);
//       }
//     };
//     fetchMe();
//   });

//   //useLeyoutEffect da bi bili sigurni da ce da se izvrsi pre nego sto komponenta trigeruje request
//   useLayoutEffect(() => {
//     const authInteceptor = axios.interceptors.request.use((config) => {
//       config.headers.Authorization =
//         !config._retry && accessToken
//           ? `Bearer ${accessToken}`
//           : config.headers.Authorization;
//       return config;
//     });
//     return () => {
//       axios.interceptors.request.eject(authInteceptor);
//     };
//   }, [accessToken]);

//   useLayoutEffect(() => {
//     const refreshInterceptor = axios.interceptors.response.use(
//       (response) => response,
//       async (error) => {
//         const origibalRequest = error.config;
//         if (
//           error.response.status === 403 &&
//           error.response.data.message === "Unauthorized"
//         ) {
//           try {
//             const response = await axios.get("/customer/refresh-token");
//             handleSignIn(response.data.accessToken);

//             origibalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
//             origibalRequest._retry = true;

//             return axios(origibalRequest);
//           } catch {
//             handleSignOut();
//           }
//         }
//         return Promise.reject(error);
//       }
//     );
//     return () => {
//       axios.interceptors.response.eject(refreshInterceptor);
//     };
//   }, []);
// };

// export default AuthProvider;
