import { use } from "react";
import AuthProviderContext from "../context/AuthContrext";
const useAuth = () => {
  const context = use(AuthProviderContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

export default useAuth;
