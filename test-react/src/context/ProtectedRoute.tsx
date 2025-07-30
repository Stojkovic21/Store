import { PropsWithChildren } from "react";
import customerDto from "../DTOs/CustomerDto";
import useAuth from "../hooks/useAuth";

type ProtectedRouteProps = PropsWithChildren & {
  allowRoles?: customerDto["role"][];
};
export default function ProtectedRoute({
  allowRoles,
  children,
}: ProtectedRouteProps) {
  const { role } = useAuth();

  if (role === undefined) {
    return <div>Loading...</div>;
  }
  if (allowRoles && !allowRoles.includes(role)) {
    return <div>Permission denied</div>;
  }

  return children;
}
//Treba da se promeni u context authProvider da postoji user
