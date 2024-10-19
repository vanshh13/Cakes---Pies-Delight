# Cakes-pie-delights 🍰🥧

**Cakes-pie-delights** is an online shop built using ASP.NET Core where users can browse, order, and enjoy a variety of cakes and pies. This web application includes user authentication, a shopping cart, and an order checkout process.

## Features

- User Authentication (Registration and Login)
- Browse cakes and pies by category
- Add items to the shopping cart
- Order checkout process with pre-filled user details
- View past orders
- Admin capabilities (manage pies and orders)

## Technologies Used

- **ASP.NET Core MVC** for server-side logic and rendering
- **Entity Framework Core** for database access
- **Microsoft SQL Server** for data storage
- **Bootstrap** for responsive design and styling
- **Identity** for user management and authentication

## Installation and Setup

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- A code editor like [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Steps

1. Clone the repository:
    ```bash
    git clone https://github.com/vanshh13/Cakes-pie-delights.git
    ```
   
2. Navigate to the project directory:
    ```bash
    cd Cakes-pie-delights
    ```

3. Update `appsettings.json` with your SQL Server connection string:
    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;;Database=BethanysPieShop;Trusted_Connection=True;"
    }
    ```

4. Run the following commands to restore the packages and build the project:
    ```bash
    dotnet restore
    dotnet build
    ```

5. Apply migrations and update the database:
    ```bash
    dotnet ef database update
    ```

6. Run the application:
    ```bash
    dotnet run
    ```

7. Open a browser and navigate to `https://localhost:44323` to view the site.

## Usage

### Browse Products
- Users can browse a list of available cakes and pies.
- Products are categorized, and users can filter items by category.

### Shopping Cart
- Users can add cakes and pies to their cart.
- The cart shows the list of selected items and total price.
  
### Checkout Process
- Users can proceed to checkout where they fill in or update their details.
- The order is processed, and a confirmation message is displayed.

### Customer Orders
- Users can view their past orders after logging in.

## Screenshots

### Home Page
![Home Page](screenshots/home-page.jpg)

### Product Page
![Product Page](screenshots/product-page.jpg)

### Shopping Cart
![Shopping Cart](screenshots/shopping-cart.jpg)

## Contributing

If you'd like to contribute, please fork the repository and use a feature branch. Pull requests are warmly welcome.

1. Fork the repository.
2. Create your feature branch:
   ```bash
   git checkout -b feature/YourFeature
