# Online Book Shop Web Application

This project is an online book shop web application built to provide a seamless shopping experience for book enthusiasts. It incorporates full CRUD (Create, Read, Update, Delete) operations for managing book inventory, comprehensive order management, secure authentication using ASP.NET Identity with external login support, and a user-friendly admin panel for efficient user management.

## Live Production
View the live production version of the application [here](https://bookcorner.runasp.net/).


## Features

### Book Management
- **Create:** Add new books to the inventory with details such as title, author, genre, price, and quantity.
- **Read:** Browse through the available books, view detailed information, and explore various genres.
- **Update:** Modify book details, including price, quantity, and other attributes as needed.
- **Delete:** Remove books from the inventory that are no longer available.

### Order Management
- **Place Orders:** Users can add books to their shopping cart and proceed to checkout to place orders.
- **View Orders:** Users can view their order history and track the status of their orders.
- **Update Orders:** Admins can update the status of orders, such as marking them as shipped or delivered.

### Payment Gateway Integration
- **Secure Transactions:** Integration with a secure payment gateway ensures safe and reliable transactions.
- **Multiple Payment Methods:** Support for various payment methods, including credit/debit cards, PayPal, and more.

### Security and Authentication
- **ASP.NET Identity:** Utilizes ASP.NET Identity for robust authentication and authorization mechanisms.
- **User Registration:** Users can register for an account, providing secure access to the platform.
- **External Login:** Supports external login providers such as Google, Facebook, and Microsoft for seamless authentication.
- **Role-Based Access Control:** Differentiates between admin and regular user roles, each with specific permissions and access levels.

### Admin Panel
- **User Management:** Admins can manage user accounts, including creating, updating, and deleting accounts.
- **Book Inventory Management:** Admins have full control over the book inventory, including adding new books, updating details, and removing obsolete ones.
- **Order Management:** Admins can view and manage orders, update their status, and track shipments.

## Technologies Used
ASP.NET Core, Entity Framework Core, Bootstrap, Payment Gateway Integration, ASP.NET Identity, External Login Providers

## Getting Started
To run the application locally, follow these steps:
1. Clone the repository to your local machine.
2. Set up the database by running migrations (`dotnet ef database update`).
3. Configure the necessary settings, including connection strings and authentication providers.
4. Build and run the application using Visual Studio or the .NET CLI (`dotnet run`).

## Contributors

- [Ahmed Elghobary ](https://www.linkedin.com/in/ahmed-elghobary/): Backend Development & Database Management

 Happy coding! 📚💻
