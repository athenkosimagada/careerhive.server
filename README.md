# CareerHive

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Connecting Talent with Opportunity**

CareerHive is a platform designed to connect job seekers with relevant opportunities posted by other users.  Built with a focus on community and collaboration, CareerHive empowers individuals to share job postings they've discovered online, creating a centralized hub for job seekers.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Folder Structure](#folder-structure)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [API Usage](#api-usage)
- [Contact](#contact)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Introduction

In today's competitive job market, finding the right opportunity can be a daunting task. CareerHive simplifies this process by providing a platform where users can share job postings they've found across the web.  Whether you're actively searching for a new role or simply exploring your options, CareerHive helps you discover opportunities you might otherwise miss.

## Features

- **Job Posting:** Users can easily submit job postings with details like title, description, company, location, and a link to the original posting.
- **Search and Filtering:**  Robust search and filtering options allow users to quickly find jobs that match their skills and interests.
- **User Profiles:** Users can create profiles to showcase their experience and connect with other members of the CareerHive community.
- **Community Focus:** CareerHive is built on the principle of community support.  Users are encouraged to share their job search experiences and help others in their job search journey.
- **API (Available with Commercial License):**  A powerful API is available for those who wish to integrate CareerHive's job data into their own applications.

## Tech Stack

- **Backend:** ASP.NET Core Web API
- **Database:** [Specify your database, e.g., SQL Server, PostgreSQL]
- **Frontend:** [Specify your frontend technology, e.g., React, Angular, Vue.js, or leave blank if it's a backend-only project]
- **Other Technologies:** [List any other important technologies, e.g., AutoMapper, Entity Framework Core, JWT for authentication]

## Folder Structure

careerhive/
├── src/
│   ├── careerhive.api/        # ASP.NET Core Web API project
│   ├── careerhive.application/ # Application logic layer
│   ├── careerhive.domain/      # Domain model layer
│   └── careerhive.infrastructure/ # Infrastructure layer
├── .gitignore          # Git ignore file
├── .editorconfig      # Editor configuration
├── careerhive.sln   # Visual Studio solution file
├── LICENSE            # Project license
├── NOTICE             # Notices
├── README.md          # Main project documentation
└── tests/             # Directory for test projects

## Installation

1. Clone the repository: `git clone https://github.com/athenkosimagada/careerhive.git`
2. Navigate to the `careerhive.api` directory: `cd careerhive/src/careerhive.api`
3. Restore NuGet packages: `dotnet restore`
4. Configure user secrets (for local development):  See the [API Usage](#api-usage) section for details.
5. Apply database migrations: `dotnet ef database update`
6. Run the application: `dotnet run`

## Usage

[Provide clear and concise instructions on how to use your application. Include examples and screenshots if helpful.  For example:]

1. Register an account.
2. Browse job postings.
3. Post a new job opportunity.
4. Search for jobs based on keywords, location, etc.

## Contributing

Contributions are welcome! Please see the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines.

## API Usage

The CareerHive API is intended for non-commercial use.  For commercial licensing inquiries, please contact Athenkosi Magada at [Your Email Address].

For local development, you'll need to configure user secrets. Navigate to the `careerhive.api` project root and run the following commands, **replacing the placeholder values with your actual secrets**:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=CareerHiveDb;Trusted_Connection=True;MultipleActiveResultSets=true"
dotnet user-secrets set "JwtSettings:SecretKey" "a_long_and_random_jwt_secret_key_that_you_generate_and_keep_safe"  # **REPLACE THIS WITH A REAL SECRET**
dotnet user-secrets set "JwtSettings:Issuer" "https://localhost:7264" # Or your API's URL
dotnet user-secrets set "JwtSettings:Audience" "https://localhost:6001" # Or your client's URL
dotnet user-secrets set "JwtSettings:AccessTokenExpirationMinutes" "30"
dotnet user-secrets set "JwtSettings:RefreshTokenExpirationDays" "7"
dotnet user-secrets set "EmailSettings:SmtpHost" "smtp.gmail.com" # Or your email provider's SMTP host
dotnet user-secrets set "EmailSettings:SmtpPort" "587" # Or the appropriate port
dotnet user-secrets set "EmailSettings:SmtpUser" "your_actual_email@gmail.com" # Your actual email
dotnet user-secrets set "EmailSettings:SmtpPass" "your_actual_email_password_or_app_password" # Your email password or app password (Gmail)
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

