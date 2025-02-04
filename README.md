# NLPHelpDesk
Ticket management system using Natural Language Process 
[Click to move NLP Help Desk deploying in Azure App Service.]() 

## Application overview
NLP Help Desk is a web-based application designed to streamline help desk operations by automating the categorization of incoming tickets and facilitating efficient communication between users and support staff.
Leveraging Natural Language Processing (NLP) and Term Frequency-Inverse Document Frequency (TF-IDF) techniques, the system analyzes ticket descriptions to identify help desk category and its priority. 
The application provides distinct user roles (Technician and End-User) with role-specific functionalities for managing tickets, products, and user assignments.

## Tech Stack
- C#
- ASP.NET Core
- MVC (Model-View-Controller)
- Entity Framework Core
- Identity Framework
- ML.NET (Machine learning framework for the .NET)
- MicroSoft SQL Server
- JavaScript
- HTML5
- CSS3
- Boostrap
- Azure App Service
- Azure SQL Database
- Azure Blob Storage

## Functionality
- **User Authentication:** Secure, role-based access control (Technician and End-User) with dynamic sign-up forms.
- **Ticket Management:**  Create, view, edit, and track tickets. Automated categorization and prioritization of tickets based on the ticket description using NLP. Assignment of tickets to specialized technicians.  Real-time communication between users and technicians via a comment system.
- **NLP-powered Categorization:**  Automated identification of help desk category within ticket descriptions using NLP and TF-IDF.
- **Product Management:** Create, view, edit, and manage products.

## Deployment
- **Azure App Service:** Hosts the ASP.NET Core application.
- **Azure SQL Database:** Provides persistent data storage.
- **Azure Blob Storage:** Stores and retrieves image files and text files on the comment function.
