import { createContext, useState, PropsWithChildren } from "react";

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
  // userId: number | undefined;
  // role: string | null;
  handleSignIn: (accessToken: string) => void;
  handleSignOut: () => void;
};

type AuthProviderProps = PropsWithChildren;

export function AuthContext({ children }: AuthProviderProps) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [accessToken, setAccessToken] = useState<string | null | undefined>(
    null
  );
  // const [role, setRole] = useState("Ghues");
  // const [userId, setUserID] = useState<number>();

  function handleSignIn(accessToken: string) {
    setIsAuthenticated(true);
    setAccessToken(accessToken);
    // setRole(role);
    // setUserID(userId);
  }

  function handleSignOut() {
    setIsAuthenticated(false);
    setAccessToken(null);
  }

  return (
    <AuthProviderContext
      value={{
        isAuthenticated,
        accessToken,
        handleSignIn,
        handleSignOut,
        // role,
        // userId,
      }}
    >
      {children}
    </AuthProviderContext>
  );
}

export default AuthProviderContext;
