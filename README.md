# Telegram Bot for Drink Sales with MAUI Admin App

This project is a **Telegram bot** designed for selling drinks, paired with a **MAUI application** for administration. Users can place orders for drinks via the Telegram bot, while administrators can manage inventory and orders through the MAUI app.

## Functionality

### Telegram Bot
- Browse available drinks
- Place orders by selecting drinks from the menu
- Receive updates on order status
- Interact with users through Telegram messages

### MAUI Admin App
- Manage drink inventory (add, update, delete drinks)
- View current stock levels
- Process orders and manage order status
- User-friendly interface for administrators

## Technologies
- C#
- .NET 8
- Telegram.Bot
- Entity Framework
- ASP.NET Web API
- MAUI

## Installation
1. Clone the repository.
2. Configure the connection string to the database in the MAUI app by modifying the `appsettings.json` and `admappsettings.json` files. Replace `x` with your actual connection settings.
3. Obtain a token for the Telegram bot from BotFather
4. Set up the bot using the obtained token.
5. Compile and run the web API application.
6. Launch the MAUI admin application on your device or emulator.
7. Start the Telegram bot to handle user interactions.

## Getting Started
To run the application:
1. Open the source code in your preferred IDE.
2. Modify the `appsettings.json` and `admappsettings.json` files with your connection details.
3. Compile the project.
4. Run the web API application.
5. Launch the MAUI app on your device, and everything should work seamlessly.
