import { createContext, PropsWithChildren, useState } from "react";
import ShoppingCart from "../Views/ShoppingCart/ShoppingCart";
import axios from "../api/axios";

const ShoppingCartContext = createContext<
  ShoppingCartProviderValue | undefined
>(undefined);

type ShoppingCartProps = PropsWithChildren;

export type ShoppingCartProviderValue = {
  openCart: () => void;
  closeCart: () => void;
  getItemQuantity: (id: number) => number;
  increaseCartQuantity: (newItem: CartItem) => void;
  decreaseCartQuantity: (id: number) => void;
  removeFromCart: (id: number) => void;
  previusItemsInCart: (itemsInCart: CartItem[]) => void;
  //cartQuantity: number;
  cartItems: CartItem[];
};
type CartItem = {
  id: number;
  name: string;
  price: number;
  quantity: number;
};
export function ShoppingCardProvider({ children }: ShoppingCartProps) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [isOpen, setIsOpen] = useState(false);

  // const cartQuantity = cartItems.reduce(
  //   (quantity, item) => item.quantity + quantity,
  //   0
  // );
  function openCart() {
    setIsOpen(true);
  }
  function closeCart() {
    setIsOpen(false);
  }
  function previusItemsInCart(itemsInCart: CartItem[]) {
    setCartItems(itemsInCart);
  }
  function getItemQuantity(id: number) {
    return cartItems.find((item) => item.id === id)?.quantity || 0;
  }
  function increaseCartQuantity(newItem: CartItem) {
    setCartItems((currItem) => {
      if (currItem.find((item) => item.id === newItem.id) == null) {
        axios.post("/cart/new", newItem);
        return [
          ...currItem,
          {
            id: newItem.id,
            name: newItem.name,
            price: newItem.price,
            quantity: 1,
          },
        ];
      } else {
        return currItem.map((item) => {
          if (item.id === newItem.id) {
            axios.patch("/cart/inc/itemId=" + newItem.id + "/inc=1");
            return { ...item, quantity: item.quantity + 1 };
          } else return item;
        });
      }
    });
  }
  function decreaseCartQuantity(id: number) {
    setCartItems((currItem) => {
      if (currItem.find((item) => item.id === id)?.quantity === 1) {
        axios.delete("/cart/Remove/id=" + id);
        return currItem.filter((item) => item.id !== id);
      } else {
        return currItem.map((item) => {
          if (item.id === id) {
            axios.patch("/cart/inc/" + id + "/-1");
            return { ...item, quantity: item.quantity - 1 };
          } else return item;
        });
      }
    });
    //console.log(cartItems);
  }
  function removeFromCart(id: number) {
    setCartItems((currItem) => {
      return currItem.filter((item) => item.id !== id);
    });
  }
  return (
    <ShoppingCartContext
      value={{
        openCart,
        closeCart,
        getItemQuantity,
        increaseCartQuantity,
        decreaseCartQuantity,
        removeFromCart,
        //cartQuantity,
        cartItems,
        previusItemsInCart,
      }}
    >
      {children}
      <ShoppingCart isOpen={isOpen} />
    </ShoppingCartContext>
  );
}

export default ShoppingCartContext;
