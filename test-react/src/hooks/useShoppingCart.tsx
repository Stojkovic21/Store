import { use } from "react";
import ShoppingCartContext from "../context/ShoppingCartContext";

export default function useShoppingCart() {
  const context = use(ShoppingCartContext);
  if (context === undefined) {
    throw new Error("useShoppingCart must be used within an AuthProvider");
  }
  return context;
}
