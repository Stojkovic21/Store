import axios from "../api/axios";
import useAuth from "./useAuth";

const useRefreshToken = () => {
  const { handleSignIn } = useAuth();

  const refresh = async () => {
    const response = await axios.get("customer/refresh-token", {
      withCredentials: true,
    });
    handleSignIn(
      response.data.accessToken,
      response.data.userId,
      response.data.role
    );
    // console.log(accessToken);
    // console.log(response.data.accessToken);

    return response.data.accessToken;
  };
  return refresh;
};

export default useRefreshToken;
