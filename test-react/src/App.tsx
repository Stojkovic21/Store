import { createBrowserRouter, RouterProvider } from "react-router-dom";
import "./App.css";
import HomePage from "./moduls/HomePage";
import LoginPage from "./moduls/Login";
import SignUp from "./moduls/SignUp";

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
