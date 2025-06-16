// Interfaccia che rappresenta un ordine
export interface OrderItemDto {
    productId: string;
    quantity: number;
    unitPrice: number;
}

//Intefaccia che rappresenta i dati necessari per creare un ordine
export interface OrderCreate{
    customerId: string;
    items: OrderItemDto[];
}



//Questa interfaccia rappresenta la risposta dell'API quando si crea un ordine
export interface OrderResponse {
    mesage: string;
    orderId: string;
}
