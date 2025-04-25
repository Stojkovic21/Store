import { Link } from "react-router-dom";
import "../style/Header.css";

export default function main() {
    
    return (<>

  <header className="main-header">
  <div className="logo">MySite</div>
  <nav className="nav-links">
    <Link to="/login">Login</Link>
    <Link to="/signup">Sign In</Link>
  </nav>
</header>
    </>);
}