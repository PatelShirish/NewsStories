The API is built using .Net 7 (Visual Studio 2022), it can either be run using Swagger UI or directly on the browser using the example url 
https://localhost:7087/HackerNews/GetBestStories?count=10. (Nuget package "AutoMapper.Extensions.Microsoft.DependencyInjection" will required to be downloaded)

Execution flow
--------------
1. Fetch all IDs.
2. Get Details for each Story.
3. Sort the Stories according to their score and store it in the cache.
4. For every request, first check the cache and if its empty then reload the data.

Stats
-----
1. Cache is set to refresh (if ideal) every 3 hours and mandatorily refresh every 24 hours. (HackerService.cs)
2. Number of maximum user requests or Rate limit is set to 50 / minute. (Program.cs)

Possible Enhancements
---------------------
1. The data size of all the given stories combined is reasonable enough for memory caching, other options such as caching only frequently requested data or 
an external caching can be considered based on data size and server infrastructure.

2. Implementaion of Validations, Logging, Application wide Exceptions handling, Unit tests etc. depending on the requirements.
