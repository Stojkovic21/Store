import { Link, NavLink } from "react-router-dom";
import "./Header.css";

export default function main() {
    
  return (<>
    <header className="main-header">
    <div className="logo">
      <NavLink className="link-offset-2 link-secondary link-underline link-underline-opacity-0" to={"/"}>
        MySite
        </NavLink>
      </div>
    <nav className="nav-links">
      <Link to="/newitem">New item</Link>
      <Link to ="/newcategory">New category</Link>
      <Link to="/login">Login</Link>
      <Link to="/signup">Sign In</Link>
    </nav>
    </header>
  </>);
}