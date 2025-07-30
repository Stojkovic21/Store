import { createContext, PropsWithChildren, useState } from "react";
import ShoppingCart from "../Views/ShoppingCart/ShoppingCart";
import { AuthProvider } from "./AuthContrext";

const ShoppingCartContext = createContext<
  ShoppingCartProviderValue | undefined
>(undefined);

type ShoppingCartProps = PropsWithChildren;

export type ShoppingCartProviderValue = {
  openCart: () => void;
  closeCart: () => void;
  getItemQuantity: (id: number) => number;
  increaseCartQuantity: (id: number) => void;
  decreaseCartQuantity: (id: number) => void;
  removeFromCart: (id: number) => void;
  cartQuantity: number;
  cartItems: CartItem[];
};
type CartItem = {
  id: number;
  quantity: number;
};
export function ShoppingCardProvider({ children }: ShoppingCartProps) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [isOpen, setIsOpen] = useState(false);

  const cartQuantity = cartItems.reduce(
    (quantity, item) => item.quantity + quantity,
    0
  );
  function openCart() {
    setIsOpen(true);
  }
  function closeCart() {
    setIsOpen(false);
  }

  function getItemQuantity(id: number) {
    return cartItems.find((item) => item.id === id)?.quantity || 0;
  }
  function increaseCartQuantity(id: number) {
    setCartItems((currItem) => {
      if (currItem.find((item) => item.id === id) == null) {
        return [...currItem, { id, quantity: 1 }];
      } else {
        return currItem.map((item) => {
          if (item.id === id) {
            return { ...item, quantity: item.quantity + 1 };
          } else return item;
        });
      }
    });
  }
  function decreaseCartQuantity(id: number) {
    setCartItems((currItem) => {
      if (currItem.find((item) => item.id === id)?.quantity === 1) {
        return currItem.filter((item) => item.id !== id);
      } else {
        return currItem.map((item) => {
          if (item.id === id) {
            return { ...item, quantity: item.quantity - 1 };
          } else return item;
        });
      }
    });
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
        cartQuantity,
        cartItems,
      }}
    >
      {children}
      <AuthProvider>
        <ShoppingCart isOpen={isOpen} />
      </AuthProvider>
    </ShoppingCartContext>
  );
}

export default ShoppingCartContext;
