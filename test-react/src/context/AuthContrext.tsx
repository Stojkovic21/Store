import { createContext, useState, PropsWithChildren } from "react";

const AuthProviderContext = createContext<AuthProviderContextValue | undefined>(
  undefined
);

export type AuthProviderContextValue = {
  isAuthenticated: boolean;
  accessToken?: string | null;
  userId?: number | null;
  role: string;
  handleSignIn: (accessToken: string, userId: number, role: string) => void;
  handleSignOut: () => void;
};

type AuthProviderProps = PropsWithChildren;

export function AuthProvider({ children }: AuthProviderProps) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [accessToken, setAccessToken] = useState<string | null | undefined>(
    null
  );
  const [role, setRole] = useState("Guest");
  const [userId, setUserID] = useState<number | null | undefined>();

  function handleSignIn(accessToken: string, userId: number, role: string) {
    setIsAuthenticated(true);
    setAccessToken(accessToken);
    setRole(role);
    setUserID(userId);
  }

  function handleSignOut() {
    setIsAuthenticated(false);
    setAccessToken(null);
    setRole("Guest");
    setUserID(null);
  }

  return (
    <AuthProviderContext
      value={{
        isAuthenticated,
        accessToken,
        handleSignIn,
        handleSignOut,
        role,
        userId,
      }}
    >
      {children}
    </AuthProviderContext>
  );
}

export default AuthProviderContext;
