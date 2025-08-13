// using System.Data.SqlClient;
// using Azure.Identity;
// using Azure.Security.KeyVault.Secrets;
// using Microsoft.AspNetCore.Builder;

// var builder = WebApplication.CreateBuilder(args);

// // 🔐 Get password from Azure Key Vault
// string keyVaultUrl = "https://azurekeyva1.vault.azure.net/";
// var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

// KeyVaultSecret secret = secretClient.GetSecret("SqlDbPassword");
// string dbPassword = secret.Value;

// // 🛠️ Build connection string
// string connectionString = $"Server=tcp:app-sql-server.database.windows.net,1433;" +
//                           $"Initial Catalog=databaseN;" +
//                           $"User ID=systemadmin;" +
//                           $"Password={dbPassword};" +
//                           $"Encrypt=True;Connection Timeout=30;";

// // Store in config
// builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// var app = builder.Build();

// // Route: GET /
// app.MapGet("/", () => "Welcome");

// // Route: GET /data
// app.MapGet("/data", async (HttpContext context) =>
// {
//     var connStr = context.RequestServices
//                          .GetRequiredService<IConfiguration>()
//                          .GetConnectionString("DefaultConnection");

//     using var connection = new SqlConnection(connStr);
//     await connection.OpenAsync();

//     using var command = new SqlCommand("SELECT * FROM student_info", connection);
//     using var reader = await command.ExecuteReaderAsync();

//     var results = new List<object>();
//     while (await reader.ReadAsync())
//     {
//         var row = new Dictionary<string, object>();
//         for (int i = 0; i < reader.FieldCount; i++)
//         {
//             row[reader.GetName(i)] = reader.GetValue(i);
//         }
//         results.Add(row);
//     }

//     return Results.Json(results);
// });

// // 🔒 Use HSTS in production
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error");
//     app.UseHsts();
// }

// app.Run();


using System.Data.SqlClient;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Get password from Azure Key Vault
string keyVaultUrl = "https://azurekeyva1.vault.azure.net/";
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

KeyVaultSecret secret = secretClient.GetSecret("SqlDbPassword");
string dbPassword = secret.Value;

// 🛠️ Build connection string
string connectionString = $"Server=tcp:app-sql-server.database.windows.net,1433;" +
                          $"Initial Catalog=databaseN;" +
                          $"User ID=systemadmin;" +
                          $"Password={dbPassword};" +
                          $"Encrypt=True;Connection Timeout=30;";

// Store in config
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

var app = builder.Build();

// Route: GET /
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>Welcome Page</title>
    <style>
        body {
            background-color: #f0f8ff;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }
        .welcome-box {
            text-align: center;
            padding: 40px;
            background-color: #ffffff;
            border: 2px solid #007acc;
            border-radius: 10px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
        }
        h1 {
            color: #007acc;
        }
        button {
            margin-top: 20px;
            padding: 10px 20px;
            background-color: #007acc;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }
        button:hover {
            background-color: #005b99;
        }
        #data-container {
            margin-top: 20px;
            text-align: left;
            max-height: 300px;
            overflow-y: auto;
        }
        table {
            border-collapse: collapse;
            width: 100%;
        }
        th, td {
            border: 1px solid #ccc;
            padding: 8px;
        }
        th {
            background-color: #e6f2ff;
        }
    </style>
</head>
<body>
    <div class='welcome-box'>
        <h1>Welcome to Azure Keyvault Project</h1>
        <button onclick='loadData()'>Show Data</button>
        <div id='data-container'></div>
    </div>

    <script>
        async function loadData() {
            const response = await fetch('/data');
            const data = await response.json();
            const container = document.getElementById('data-container');

            if (data.length === 0) {
                container.innerHTML = '<p>No data found.</p>';
                return;
            }

            let table = '<table><thead><tr>';
            for (let key in data[0]) {
                table += `<th>${key}</th>`;
            }
            table += '</tr></thead><tbody>';

            data.forEach(row => {
                table += '<tr>';
                for (let key in row) {
                    table += `<td>${row[key]}</td>`;
                }
                table += '</tr>';
            });

            table += '</tbody></table>';
            container.innerHTML = table;
        }
    </script>
</body>
</html>
", "text/html"));


// Route: GET /data
app.MapGet("/data", async (HttpContext context) =>
{
    var connStr = context.RequestServices
                         .GetRequiredService<IConfiguration>()
                         .GetConnectionString("DefaultConnection");

    using var connection = new SqlConnection(connStr);
    await connection.OpenAsync();

    using var command = new SqlCommand("SELECT * FROM student_info", connection);
    using var reader = await command.ExecuteReaderAsync();

    var results = new List<object>();
    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row[reader.GetName(i)] = reader.GetValue(i);
        }
        results.Add(row);
    }

    return Results.Json(results);
});

// 🔒 Use HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Run();
