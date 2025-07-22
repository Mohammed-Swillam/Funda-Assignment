# Funda Makelaars Analyzer

A .NET 8 console application that analyzes Amsterdam real estate data from the Funda API to determine 
which makelaars have the most properties listed for sale.

## Business Features

- Analyzes Two types of properties in Amsterdam: All properties and properties with gardens
- Displayes the top 10 Maklaars in both types


## Technical Features
- Uses Polly for retry policies and rate limiting
- Displayes Real-time progress updates during data fetching
- Separation of concerns with dependency injection


## Requirements

- .NET 8.0 Runtime

## Installation & Usage

1. **Clone or download the repository**:
   ```bash
   git clone https://github.com/Mohammed-Swillam/Funda-Assignment.git
   ```
2. **Open a terminal inside the root folder and navigate to the project directory**:
   ```bash
   cd FundaMakelaarsAnalyzer   
   ```
3. **Restore packages**:
   ```bash
   dotnet restore
   ```
4. **Run the application**:   
    1. Execute the following command to run the application:
        
    ```bash
    dotnet run .
    ```
    2. If the previous command failed, try to explicitly provide the project name
    ```bash
    dotnet run --project FundaMakelaarsAnalyzer
    ```



## Technical Details

### Architecture
- **Models**: API response DTOs and business entities
- **Services**: API client and business logic
- **Configuration**: API settings and rate limiting parameters

### Rate Limiting
- Implemented using Polly's Retry policy
- API Requests uses a default of 630ms delay (via configuration) between requests to stay below the 100 requests/minute limit

  *Api limit of 100 requests
  per minute. 1 minute => 60000ms / 95 requests (saftey buffer of 5 requests) = 631ms per request (rounded to 630ms for simplicity)*
- Retry logic could be tested by setting up a lower delay in the configuration file. ```appsettings.json => RequestDelayMilliseconds``` => (ex:100ms)
### Resilience
- Polly Retry policy with up to 10 retries in case the app hits the API rate limit.
- Comprehensive error handling for network issues

### Dependencies
- Polly 8.2.0 - to implement retry policies
- System.Text.Json 8.0.5 - JSON serialization


### Notes
- While investigating the API, I noticed that changing the page size to a  value lower than 25 works fine as expected
- When I tried to change it to a higher value than 25, I found that the api doesn't respect that newer value and still returns 25 results per page, but **it mistakenly  affects the ```AantalPaginas``` value**

### AI Usages
- Generated DTOs from API response (I used Insomnia, a tool similar to Postman, to fetch a sample of the api response, and provided it to CoPilot to quickly extract DTOs from the response)
- I took the following suggestions from the AI and applied them using GitHub Copilot:
  - Added more Exception handling cases inside `FundaApiClient.cs`  
  - Added ```TableFormatter``` class to display results when the app fetches and analyzes requested data
  - Added friendly progress reporting during data fetching
  - Although I used Polly to implement a retry policy if the app hits the API rate limit, the AI further suggested the logic for setting up a delay between requests to avoid hitting the api rate limit. I added that and used the suggested calculation to set the delay value
  - Applied a few suggested ```Console.WriteLine()``` statements to improve the user experience






