import { createContext, useState, PropsWithChildren } from "react";
import customerDto from "../DTOs/CustomerDto";

// interface AuthData {
//   user?: string;
//   accessToken?: string;
// }

// export interface AuthContextType {
//   auth: AuthData;
//   setAuth: Dispatch<SetStateAction<AuthData>>;
// }
const AuthProviderContext = createContext<AuthProviderContextValue | undefined>(
  undefined
);

export type AuthProviderContextValue = {
  isAuthenticated: boolean;
  accessToken: string | null | undefined;
  //user: customerDto | null;
  handleSignIn: (accessToken: string) => void;
  handleSignOut: () => void;
};

type AuthProviderProps = PropsWithChildren;

export function AuthContext({ children }: AuthProviderProps) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [accessToken, setAccessToken] = useState<string | null | undefined>(
    null
  );

  function handleSignIn(accessToken: string) {
    setIsAuthenticated(true);
    setAccessToken(accessToken);
  }

  function handleSignOut() {
    setIsAuthenticated(false);
    setAccessToken(null);
  }

  return (
    <AuthProviderContext
      value={{
        isAuthenticated,
        accessToken: accessToken,
        handleSignIn,
        handleSignOut,
      }}
    >
      {children}
    </AuthProviderContext>
  );
}

export default AuthProviderContext;
