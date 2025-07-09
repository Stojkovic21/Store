import { createBrowserRouter, RouterProvider } from "react-router-dom";
import "./App.css";
import HomePage from "./Views/HomePage/HomePage";
import LoginPage from "./Views/Login/Login";
import SignUp from "./Views/Signup/SignUp";
import Additem from "./Views/AddItem/AddItem";
import AddCategoty from "./Views/AddCategory/AddCategory";

const router = createBrowserRouter([ //razlika izmedju link i a je sto a refresuje ceo html i js a link samo prosledi na tu stranicu
  {
    path: "/",
    element: <HomePage />,
    errorElement:<div>404 not found</div>,
  },
  {
    path: "/login",
    element: <LoginPage />,
    errorElement:<div>404 not found</div>,
  },
  {
    path:"/signup",
    element:<SignUp/>,
    errorElement:<div>404 not found</div>,
  },
  {
    path:"/newitem",
    element:<Additem/>,
    errorElement:<div>404 not found</div>,
  },
  {
    path:"/newcategory",
    element:<AddCategoty/>,
    errorElement:<div>404 not found</div>
  }
  //profiles
// {
//   path:'profiles/:profileId',
//   element: <ProfilePage>>
// }
]);

function App() {
  return (   
    <>
      <RouterProvider router={router}/>
    </>
  );
}

export default App;
