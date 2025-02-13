# NLPHelpDesk

Ticket management system using Natural Language Process.  
[Click to move NLP Help Desk deploying in Azure App Service.](https://nlphelpdesk-v-1.azurewebsites.net/) 

## Application overview

This project aims to develop a web-based application, the "NLP Help Desk," to streamline help desk operations.  
The objective is to reduce the overhead on help desk technicians by automating the categorization and prioritization of incoming tickets using Natural Language Processing (NLP) and Term Frequency-Inverse Document Frequency (TF-IDF) techniques.  
This automated system will analyze ticket descriptions, classify them into relevant help desk categories, and prioritize them, enabling technicians to focus on critical issues and respond to threats more effectively.  
The application will provide distinct user roles (Technician and End-User) with tailored functionalities, a user-friendly interface, and a built-in chat function to facilitate communication and improve overall help desk efficiency.  

This project is structured into four main components:

*   **NLPHelpDesk (ASP.NET Core Web Application):** This is the main web application that handles user interaction, ticket management, product management, and communication. It triggers the Azure Function for ticket categorization and prioritization.
*   **NLPHelpDesk.Data (Class Library):** This library contains the data models, enums, and the `ApplicationDbContext` (Entity Framework Core context) used by the web application.
*   **NLPHelpDesk.Function (Azure Functions):** This Azure Functions is triggered by the web application. It executes the category and priority prediction using NLP and TF-IDF with pre-trained ML.NET models.
*   **NLPHelpDesk.Train (Console Application):** This console application is used for training the ML.NET models locally. It reads a CSV file containing training data, trains separate models for category and priority prediction, and then stores these trained models in the `NLPHelpDesk.Function` project for deployment to Azure.


## Tech Stack

* C#
* ASP.NET Core
* MVC (Model-View-Controller)
* Entity Framework Core
* Identity Framework
* ML.NET (Machine learning framework for the .NET)
* MicroSoft SQL Server
* JavaScript
* HTML5
* CSS3
* Boostrap
* Azure App Service
* Azure SQL Database
* Azure Storage (Blob and Queue)
* Azure Functions

## Functionality

*   **User Authentication:** Secure, role-based access control (Technician and End-User) with dynamic sign-up forms.
*   **Ticket Management:** Create, view, edit, and track tickets. Automated categorization and prioritization of tickets based on the ticket description using NLP. Assignment of tickets to specialized technicians. Real-time communication between users and technicians via a comment system.
*   **NLP-powered Categorization and Prioritization:** Automated identification of help desk category and priority within ticket descriptions using pre-trained NLP and TF-IDF models within an Azure Function.
*   **Product Management:** Create, view, edit, and manage products.
*   **Model Training (NLPHelpDesk.Train):**  A separate console application for training the ML.NET models used for ticket categorization and prioritization. This allows for offline model training and avoids resource constraints on the web application or Azure Function.

## Deployment

*   **Azure App Service:** Hosts the ASP.NET Core web application.
*   **Azure SQL Database:** Provides persistent data storage.
*   **Azure Blob Storage:** Stores and retrieves image files and text files related to comments.
*   **Azure Queue Storage:** Used for asynchronous communication and processing, potentially for tasks like handling large volumes of incoming tickets or background processing of NLP tasks.
*   **Azure Functions:** Hosts the NLP prediction function.  The trained models are deployed as part of this function.
