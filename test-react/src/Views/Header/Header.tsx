import { Link, NavLink } from "react-router-dom";
import "./Header.css";
import "../style/Visibility.css";
import useAuth from "../../hooks/useAuth";
import axios from "../../api/axios";
import Card from "./CartIcon";
export default function main() {
  const { isAuthenticated, handleSignOut } = useAuth();
  return (
    <>
      <header className="main-header">
        <div className="logo">
          <NavLink
            className="link-offset-2 link-secondary link-underline link-underline-opacity-0"
            to={"/"}
          >
            MySite
          </NavLink>
        </div>
        <nav className="nav-links">
          <Link to="/newsupplier">New supplier</Link>
          <Link to="/newitem">New item</Link>
          <Link className="visible" to="/newcategory">
            New category
          </Link>
          {isAuthenticated ? (
            <Link
              to="/login"
              onClick={async () => {
                handleSignOut();
                axios.get("/customer/signout", { withCredentials: true });
              }}
            >
              SignOut
            </Link>
          ) : (
            <>
              <Link to="/login">Login</Link>
              <Link to="/signup">Sign In</Link>
            </>
          )}
        </nav>
        <Card />
      </header>
    </>
  );
}
