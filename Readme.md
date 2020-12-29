# PigeonMessenger

Welcome to PigeonMessenger, an API created by Brian Plantico as an at home engineering project during an interview for a Back End Engineering position. PigeonMessenger is built using Azure's cloud native Functions and also utilizes the cloud database provider Snowflake for persistent data storage. The endpoints which you can interact with are outlined below with examples of successful requests and successful responses, however, if you have any questions about this API, please reach out to me directly.

Deployed Link:
https://pigeonmessengerapp.azurewebsites.net/api/v1/messages

#### Brian Plantico: https://github.com/bplantico

### Schema
PigeonMessenger's database schema currently has one table, Messages, though additional tables to persist Users and a corresponding joins table for UsersMessages would also make sense to implement soon with more time.

![PigeonMessenger's DB Schema](https://user-images.githubusercontent.com/43261385/103309281-ee4dca00-49d1-11eb-8443-332199460d1d.png)

### CI/CD and Monitoring

In order to expedite integrating and deployments, a small CI/CD pipeline has been set up for the application. Currently the pipeline runs when changes are committed to a remote branch, which triggers the test suite to run, the code to be built, and (if both previous steps are successful) for the code to be deployed to either the develop or production environment, depending on the target branch.
![PigeonMessenger CI/CD Pipeline](https://user-images.githubusercontent.com/43261385/103312596-c7e05c80-49da-11eb-8fc7-df90d760e6fa.png)

To make monitoring the application quick and easy, I've also created an ApplicationInsights resource in order to monitor traffic and dependency calls for the program.
![PigeonMessenger ApplicationInsights Resource](https://user-images.githubusercontent.com/43261385/103312543-a2ebe980-49da-11eb-863c-73f011fe6b3c.png)

### Testing
Tests have been set up to run against a seperate test schema. The username and password set as environment varibles within the tests should provide access, however, the credentials run against a trial Snowflake instance so will expire around the end of January 2021. That being said, happy paths have been tested for each function being invoked by the controller on the database service layer, and with more time, tests would be written for additional scenarios.

### Setup and Configuration


PigeonMessenger provides three endpoints to interact with:
#### Messages
+ [Create Message request](#create_message)
+ [Messages Between Parties request](#between_parties)
+ [Messages for all senders request](#all_senders)

# Configuration
It's not necessary to install and run PigeonMessenger on your local machine since the application's AzureFunctions are deployed and publicly accessible, however, if you're interested in working on the code base the following instructions will help you get started:

Fork and/or clone this repository to your local machine.

Open PigeonMessenger.sln in Visual Studio (or IDE/code editor of your choice that supports running C#/.NET Core code).
The Target Framework used is .Net Core 3.1 (Microsoft.NETCore.App).
More information on getting your environment set up to run AzureFunctions locally can be found in Microsoft's documentation, for example here's how to run AzureFunctions using Visual Studio Code: https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp#run-the-function-locally

Please message bplantico@gmail.com for credentials to connect to the Snowflake DB.

Copy the local.settings.EXAMPLE.json file and rename it to local.settings.json. In your new file you will need to include values for the keys listed below:

```
"AzureWebJobsStorage": "UseDevelopmentStorage=true",
"FUNCTIONS_WORKER_RUNTIME": "dotnet",
"SnowflakeAccount": "<Snowflake Acct>",
"SnowflakeUser": "<Snowflake Username>",
"SnowflakePassword": "<Snowflake Password>",
"SnowflakeDb": "<Snowflake Db>",
"SnowflakeSchema": "<Snowflake Schema>",
"DefaultResultsLimit": 100
```

You're ready to go! The simplest way to run the code is to use Visual Studio's 'Start' feature, either with debugging enabled (shortcut is to use 'F5'), or without debugging enabled ('Ctrl + F5')

# <a name="create_message"></a>Create Message request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages`

A POST request to the messages endpoint returns the id (guid, as a string with hyphens removed) of the newly created Message.

Example Request
```
POST https://pigeonmessengerapp.azurewebsites.net/api/v1/messages

{
    "sender": "elmer",
    "recipient": "bugs",
    "body": "Be vewy, vewy quiet. I'm hunting wabbits.",
    "isPublic": true
}
```
Example Response
```
Status: 201 CREATED

784205cca9f64317993093e972d10be7
```

# <a name="between_parties"></a>Messages Between Parties request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages/{recipient}/{sender}`

A GET request to the messages endpoint takes path parameters of a `{recipient}` and `{sender}` and returns the messages between those two parties, limited to the 100 most recent messages by default. The endpoint also accepts an optional query parameter of `since_days_ago`, an integer i.e. `?since_days_ago=2` that is used to filter how many days back you'd like to see messages between the two parties for, with a max of 30 days. If an integer is supplied that's greater than 30, 30 days of messages will be returned. Results are ordered in descending order by when they were created.

Example Request
```
GET https://pigeonmessengerapp.azurewebsites.net/api/v1/messages/elmer/bugs?since_days_ago=2
```
Example Response
```
Status: 200 OK
[
    {
        "id": "784205cca9f64317993093e972d10be7",
        "sender": "elmer",
        "recipient": "bugs",
        "body": "Be vewy, vewy quiet. I'm hunting wabbits.",
        "isPublic": true,
        "createdAt": "2020-12-29T19:55:42",
        "updatedAt": "2020-12-29T19:55:42"
    }
]
```

# <a name="all_senders"></a>Messages for all senders request
`https://pigeonmessengerapp.azurewebsites.net/api/v1/messages`

A GET request to the messages endpoint returns the messages sent from any/all senders, limited to the 100 most recent by default. The endpoint also accepts an optional query parameter of `since_days_ago`, an integer i.e. `?since_days_ago=2` that is used to filter how many days back you'd like to see messages from all senders for, up to a maximum of 30 days. If an integer is supplied that's greater than 30, 30 days of messages will be returned. Results are ordered in descending order by when they were created.

Example Request
```
GET https://pigeonmessengerapp.azurewebsites.net/api/v1/messages
```
Example response
```
Status: 200 OK
[
    {
        "id": "1daad338d3f94cf18e678ab7dc3516a5",
        "sender": "brian",
        "recipient": "bash",
        "body": "I thought it was funny.",
        "isPublic": true,
        "createdAt": "2020-12-29T07:25:46",
        "updatedAt": "2020-12-29T07:25:46"
    },
    {
        "id": "7a3a3814ec06481c8e712616faf85166",
        "sender": "lola",
        "recipient": "bash",
        "body": "We're not friends anymore.",
        "isPublic": true,
        "createdAt": "2020-12-29T06:45:30",
        "updatedAt": "2020-12-29T06:45:30"
    },
    {
        "id": "8246a8bef4e84020806cb7711aa3d729",
        "sender": "lola",
        "recipient": "bash",
        "body": "...smh",
        "isPublic": true,
        "createdAt": "2020-12-29T06:40:48",
        "updatedAt": "2020-12-29T06:40:48"
    },
    {
        "id": "6930206f0f994824b5d66b25be809d82",
        "sender": "bash",
        "recipient": "lola",
        "body": "Not much. What's up with you?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:39:50",
        "updatedAt": "2020-12-29T06:39:50"
    },
    {
        "id": "774615f7e58f4059bd36951f2cdfa60a",
        "sender": "lola",
        "recipient": "bash",
        "body": "Huh? What's updog?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:39:01",
        "updatedAt": "2020-12-29T06:39:01"
    },
    {
        "id": "673480fe1f4442c3bdd609c978c35df6",
        "sender": "bash",
        "recipient": "lola",
        "body": "Have you seen my updog?",
        "isPublic": true,
        "createdAt": "2020-12-29T06:38:25",
        "updatedAt": "2020-12-29T06:38:25"
    }
]
```


### Contstraints, Design Decisions, and Process
#### High Level Requirements
```
"Build an api that supports a web-application to enable two users to send short text messages to each other, like Facebook Messages app or Google Chat.
Ignore Authorization, Authentication and Registration. Assume this is a global api that can be leveraged by anyone, security is not a concern.
1. A short text message can be sent from one user (the sender) to another (the recipient).
2. Recent messages can be requested for a recipient from a specific sender
- with a limit of 100 messages or all messages in last 30 days.
3. Recent messages can be requested from all senders - with a limit of 100 messages or all messages in last 30 days.
4. Document your api like you would be presenting to a web team for use.
5. Show us how you would test your api.
6. Ensure we can start / invoke your api.

Other Considerations
We're only expecting a couple hours to a half day of effort, though you're free to spend more if inspired.
```
Due to the time constraints, both stated in the prompt and obligations to my current role/family/holiday celebrations, I knew going in I'd want to get an MVP deployed quick, then add to it as time allowed. I initially considered at minimum implementing multiple entities/models that would've been set up as Messages, Users, and a corresponding model UsersMessages (a joins table), where a User would have many Messages, and a Message would have many Users (i.e. a Message would belong to both Sender and Recipient), however, realizing that in this instnace, the Message was the resource being affected the most, the instruction to `Ignore Authorization, Authentication and Registration`, wanting to have a larger discussion about the vision for Users/Identity within the system, and knowing there were skills that I wanted to demonstrate outside of setting up the model relationships all led me to the decision to focus on Messages only and circle back to implement Users and the corresponding UserMessages resources as a later feature.

Getting into what I considered the three base features of the API, the first was relatively straightforward -- The ability to create a Message. The second and third allowed for more interpretation in my opinion. For example, I initially interpretted "2. Recent messages can be requested for a recipient from a specific sender" as meaning the result set were only messages where the recipient was X and the sender was Y, essentially just one side of a conversation. Upon implementing that interpretation, I imagined myself in the roles of both the end users as well as a front end developer. Is one side of a conversation useful to either of those roles? It didn't seem like it. I reconsidered and figured that another interpretation would be to return messages between two parties, regardless of who was the sender or reciever. Doing so returns the entire conversation, instead of just half of it.

The constraint on both of the High Level Requirements 2 and 3, i.e. `with a limit of 100 messages or all messages in last 30 days.` also left room for interpretation and would be an opportunity to clarify with a Product Manager or other business stakeholder. Specifically, what about the case where more than 100 messages have been sent in the last 30 days? For an extreme example, what if 100,000 messages have been exchanged between two parties in the last 30 days? Should the result set be limited to the most recent 100 within the last 30 days, or should all 100,000 messages be returned? Commonly, this sort of limit might be imposed as part of a pagination/scroll function, so how useful would it be to receive a result set of 100,000 messages? In this case, I made the decision to keep the filters separated. By default, results would be limited to 100 messages (or a different amount that's configurable upon Startup), or every single message within a specific number of days back, up to 30.

Regarding potential interpretations of the third feature, `Recent messages can be requested from all senders`, is this result set the 100 most recent messages regardless of who sent them (i.e. one User is accountable for sending the last 100 messages), or if 99 other users have each sent at least one message recently, then should one message be returned for each sender (and the 99 other Messages from the Message happy user be ignored to allow seeing messages from other senders)? In this case, I decided to implement the most recent 100 messages, regardless of the frequency of the sender appearing, or every message in the last x days (again allowing for user input to filter up to 30 days using a query param).

As next steps, I'd prioritize implementing a Users model to at least provide for the ability to more easily store and query against a table of UserMessages. Additionally, I wanted to demonstrate the ability to get the application deployed so you would not have to run it locally, including tests for the database layer function calls, showing CI/CD pipeline, and having a way to monitor the application. I met those goals, however, with more time I'd also want to implement a more robust, automated documentation, specifically through OpenAPI/Swagger. My current implementation would also allow for rather easy authentication between a FE, but would also expect to provide more functionality around User authentication/authorization, and could do so with a 3P service like Auth0 or Microsoft's inhouse ActiveDirectory service. Finally, I'd also explore introducing an ORM layer (likely Entity Framework) to handle calls to retrieve results and give the ability to work with them in code as C# objects. While sticking with parameterized queries to call the Snowflake instance made for relatively quick development, the calls feel more fragile in the long run and might be harder to maintain in the long run should another developer (or myself in a year) have to revisit the calls and unpack what they're doing. Additionally, switching to use an ORM would also make changing to a different database easier, with the goal that the code wouldn't need to change, only the underlying DB while the ORM handles converting the code to the proper queries based on the new SQL engine. Finally, I could see a more robust response object being returned from the requests to get messages, something that might include the number of messages returned, oldest message timestamp, newest message timestamp, paging information, etc. This could lead to smarter rendering on the FE and overall a better user experience.
