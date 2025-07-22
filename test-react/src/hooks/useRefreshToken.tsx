import axios from "../api/axios";
import useAuth from "./useAuth";

const useRefreshToken = () => {
  const { handleSignIn, accessToken } = useAuth();

  const refresh = async () => {
    const response = await axios.get("customer/refresh-token", {
      withCredentials: true,
    });
    console.log(accessToken);
    console.log(response.data.accessToken);

    handleSignIn(response.data.accessToken);
    return response.data.accessToken;
  };
  return refresh;
};

export default useRefreshToken;
