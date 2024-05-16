# Gentle.Provider.MySQL.Interceptor

## How it works

Library modifies `SHOW COLLATION` query using `BaseCommandInterceptor` to prevent fetching collations with NULL Id's.
There is also small piece of code which append `utf8mb3` charset to MySql.Data's internal mapping Dictionary allowing
to read fields with utf8mb3 collations.

## Usage

1. Append following line to your connection string

    `;commandinterceptors=Gentle.Provider.MySQL.Interceptor.Interceptor,Gentle.Provider.MySQL.Interceptor`

2. Call `Utf8mb3.Enable()` before opening first MySql connection.
