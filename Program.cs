using Newtonsoft.Json.Linq; // Importing Newtonsoft.Json for parsing JSON data

class Program
{
    static async Task Main()
    {
        // API URL to fetch order data in JSON format, filtering orders from 1997 onwards
        string requestUrl = "https://services.odata.org/V4/Northwind/Northwind.svc/Orders?$filter=OrderDate ge 1997-01-01T00:00:00Z&$format=json";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(requestUrl); // Sending request to the API

                // If response was successful, read the response content and convert from JSON to CSV
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Data fetched successfully.");

                    SaveToCsv(data, "orders.csv");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}"); // Print error if request fails
                }
            }
        }
        catch (HttpRequestException ex) // Handle network-related errors
        {
            Console.WriteLine($"Network error: {ex.Message}");
        }
        catch (Exception ex) // Handle other unexpected errors
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    // Method to convert JSON data to CSV and save it to a file
    static void SaveToCsv(string jsonData, string filePath)
    {
        JObject jsonObject = JObject.Parse(jsonData); // Parse JSON response
        JArray orders = (JArray)jsonObject["value"]; // Extract "value" array containing order data

        if (orders == null || !orders.Any()) // Check if no data is returned
        {
            Console.WriteLine("No data found.");
            return;
        }

        // Get all field names except '@odata.etag', which is metadata
        var allFields = orders[0].Children<JProperty>()
                               .Select(prop => prop.Name)
                               .Where(name => name != "@odata.etag")
                               .ToList();

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath)) // Open file for writing
            {
                // Write CSV header with column names
                writer.WriteLine(string.Join(",", allFields));

                // Write data rows for each order
                foreach (var order in orders)
                {
                    var values = allFields.Select(field =>
                    {
                        var value = order[field]?.ToString() ?? ""; // Get value, replace null with empty string
                        return value.Contains(",") ? $"\"{value}\"" : value; // Wrap values containing commas in quotes
                    });
                    writer.WriteLine(string.Join(",", values)); // Write the row to the CSV file
                }
            }
            Console.WriteLine($"Data successfully saved to {filePath}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File write error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error while writing CSV: {ex.Message}");
        }
    }
}
