import useShoppingCart from "../../hooks/useShoppingCart";
import "./ShopingCart.css";
type ShoppingCartProps = {
  isOpen: boolean;
};
function ShoppingCart({ isOpen }: ShoppingCartProps) {
  const { closeCart, cartItems } = useShoppingCart();
  //const totalPrice = 0;
  return (
    <>
      <div className={`cart-drawer ${isOpen ? "open" : ""}`}>
        <div className="cart-header">
          <h2>Your Cart</h2>
          <button className="close-button" onClick={() => closeCart()}>
            ×
          </button>
        </div>

        <div className="cart-items">
          {cartItems.length === 0 ? (
            <p className="empty">Cart is empty.</p>
          ) : (
            cartItems.map((item, i) => (
              <div key={i} className="cart-item">
                <div>
                  <strong>{"Pepsi"}</strong>
                  <p>
                    {item.quantity} × {135} din.
                  </p>
                </div>
                <div className="item-total">{10 * 135} din.</div>
              </div>
            ))
          )}
        </div>

        <div className="cart-footer">
          <strong>Total:</strong> {1350} din.
          <button className="checkout-button">Checkout</button>
        </div>
      </div>

      {isOpen && <div className="overlay" onClick={() => closeCart()} />}
    </>
  );
}

export default ShoppingCart;
