import axios from "../../api/axios";
import useShoppingCart from "../../hooks/useShoppingCart";
import "./ShopingCart.css";
import { useEffect, useState } from "react";
type ShoppingCartProps = {
  isOpen: boolean;
};
type CartItem = {
  id: number;
  name: string;
  price: number;
  quantity: number;
};
function ShoppingCart({ isOpen }: ShoppingCartProps) {
  const { closeCart, cartItems, previusItemsInCart, decreaseCartQuantity } =
    useShoppingCart();

  //const [separateItems, setSeparateItem] = useState<CartItem[]>([]);
  const [currItems, setCurrItem] = useState<CartItem[]>([]);
  useEffect(() => {
    const itemsInCart = async () => {
      try {
        const results = await axios.get("/cart/get");
        setCurrItem(results.data);
      } catch (error) {}
    };
    itemsInCart();
  }, []);

  useEffect(() => {
    const pomNiz = [];
    for (var item in currItems) {
      pomNiz.push(currItems[item]);
    }
    previusItemsInCart(pomNiz);
  }, [currItems]);

  return (
    <>
      <div className={`cart-drawer ${isOpen ? "open" : ""}`}>
        <div className="cart-header">
          <h2>Your Cart</h2>
          <button
            className="close-button"
            onClick={() => {
              decreaseCartQuantity(1);
            }}
          >
            ×
          </button>
        </div>
        <form>
          <div className="cart-items">
            {cartItems.length === 0 ? (
              <p className="empty">Cart is empty.</p>
            ) : (
              cartItems.map((item, i) => (
                <div key={i} className="cart-item">
                  <div>
                    <strong>{item.name}</strong>
                    <p>
                      {item.quantity} × {item.price} din.
                    </p>
                  </div>
                  <div className="item-total">
                    {item.quantity * item.price} din.
                  </div>
                </div>
              ))
            )}
          </div>

          <div className="cart-footer">
            <strong>Total:</strong> {1350} din.
            <button className="checkout-button" onClick={() => console.log("")}>
              Checkout
            </button>
          </div>
        </form>
      </div>

      {isOpen && <div className="overlay" onClick={() => closeCart()} />}
    </>
  );
}

export default ShoppingCart;
