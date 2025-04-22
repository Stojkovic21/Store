import "../style/Header.css";
import { Link } from 'react-router-dom'
function main() {
    
    return (<>
       <header className="main-header">
  <div className="logo">MySite</div>
  <nav className="nav-links">
    
    <a href="/Login">About</a>
    <a href="/services">Services</a>
    <a href="/contact">Contact</a>
  </nav>
</header>


    </>);
}

export default main;